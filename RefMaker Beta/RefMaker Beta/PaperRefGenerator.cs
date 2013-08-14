using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Nodes;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;


namespace Reference
{
    interface IProgressable
    {
        void onProcess(int total,int n,string paper_name);
        void onFinish(ArrayList ref_list);
    }

    class HttpUtil
    {
        private const string URL_1 = "http://s.wanfangdata.com.cn/Paper.aspx?q=";
        private const string URL_2 = "http://s.wanfangdata.com.cn/Export/Export.aspx?scheme=Reference";

        public static Cookie _AuthCookie = null;

        public HttpUtil()
        {}

        public string getPaperIDHTML(string paper_name)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_1 + paper_name);
            request.CookieContainer = new CookieContainer();

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _AuthCookie = response.Cookies["WFKS.Auth"];

                StreamReader reader = new StreamReader(response.GetResponseStream());

                return reader.ReadToEnd();
            }

            return null;
        }

        public string getPaperReferenceHTML(ArrayList paper_ids)
        {
            if (_AuthCookie == null)
            {
                //Auth Cookie is null
                return null;
            }

            string rs = "|";

            foreach( string p in paper_ids)
            {
                rs = rs + p + "|";
            }

            Cookie rs_cookie = new Cookie("rs", rs, _AuthCookie.Path, _AuthCookie.Domain);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_2);
            request.CookieContainer = new CookieContainer();

            request.CookieContainer.Add(_AuthCookie);
            request.CookieContainer.Add(rs_cookie);

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                return reader.ReadToEnd();
            }

            return null;
        }
    }


    /// <summary>
    /// Filter the PaperID URL
    /// </summary>
    class PaperFilter : NodeFilter
    {
        private string _PaperName = "";

        public PaperFilter(string paper_name)
        {
            _PaperName = paper_name;
        }

        public bool Accept(INode node)
        {
            return (node is IText) && (((IText)node).GetText().Equals(_PaperName));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class PaperRefGenerator
    {
        private IProgressable _Progressable = null;
        private HttpUtil _HttpUtil = new HttpUtil();

        public PaperRefGenerator()
        {}

        public void setProgressable(IProgressable p)
        {
            _Progressable = p;
        }

        protected string getPaperID(string paper_name)
        {
            string html_page = _HttpUtil.getPaperIDHTML(paper_name);

            if (html_page == null || html_page == "")
            {
                return null;
            }

            Parser p = new Parser(new Lexer(html_page));

            TagNameFilter tag_f = new TagNameFilter("A");
            HasAttributeFilter attr_f = new HasAttributeFilter("target", "_blank");
            HasChildFilter child_f = new HasChildFilter(new PaperFilter(paper_name));

            AndFilter af = new AndFilter(tag_f,attr_f);
            AndFilter aff = new AndFilter(af, child_f);

            NodeList childs = p.ExtractAllNodesThatMatch(aff);

            if (childs == null || childs.Count <= 0)
            {
                //Paper not found
                return null;
            }
            //TODO Multi Paper found

            INode node = childs[0];
            if (node is ITag)
            {
                ITag t = node as ITag;

                string href = t.GetAttribute("href");
                
                if (href != null && href != "")
                {
                    string [] sp = href.Split(new char[]{'/'});

                    return sp[sp.Length - 1].Split(new char[]{'.'})[0];
                }
            }
            
            //Not Found
            return null;
        }

        protected ArrayList getPaperReferenceByID(ArrayList paper_id)
        {
            string html_page = _HttpUtil.getPaperReferenceHTML(paper_id);

            if (html_page == null || html_page == "")
            {
                return null;
            }

            Parser p = new Parser(new Lexer(html_page));

            TagNameFilter tag_f = new TagNameFilter("div");
            HasAttributeFilter attr_f = new HasAttributeFilter("id", "export_container");

            AndFilter af = new AndFilter(tag_f, attr_f);

            NodeList childs = p.ExtractAllNodesThatMatch(af);

            if (childs == null || childs.Count <= 0)
            {
                return null;
            }

            INode node = childs[0];

            NodeList ref_childs = node.Children;
            ArrayList ref_list = new ArrayList();

            for (int i = 0; i < ref_childs.Count;++i )
            {
                INode tmp = ref_childs[i];

                if (tmp is ITag)
                {
                    ITag tag = tmp as ITag;

                    string str = tag.ToPlainTextString();

                    str = str.Replace('\r', ' ').Replace('\n',' ');

                    str = str.Substring(str.IndexOf(']') + 1);

                    //str = System.Text.RegularExpressions.Regex.Replace(str, @"^\[*\]$", "");

                    ref_list.Add(str);
                }
            }

            if (_Progressable != null)
            {
                _Progressable.onFinish(ref_list);
            }

            return ref_list;
        }

        public ArrayList getPaperReference(string [] papers,ArrayList paper_not_found)
        {
            ArrayList paper_ids = new ArrayList();

            for (int i=0;i<papers.Length; ++i)
            {
                string one_paper = papers[i];

                string id = getPaperID(one_paper);

                if (_Progressable != null)
                {
                    _Progressable.onProcess(papers.Length,i,one_paper);
                }

                if (id != null && id != "")
                {


                    paper_ids.Add(id);
                }
                else
                {
                    if (paper_not_found != null)
                    {
                        paper_not_found.Add(one_paper);
                    }
                }
            }

            return getPaperReferenceByID(paper_ids);
        }
    }
}
