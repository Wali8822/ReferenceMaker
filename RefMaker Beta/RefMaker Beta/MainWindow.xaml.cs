using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Reference;
using System.Collections;
using System.Threading;
using System.Windows.Threading;

namespace RefMaker_Beta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window , IProgressable
    {
        Thread timeThread;
        Thread td;
        public MainWindow()
        {
            InitializeComponent();
            listBox1.SelectionMode = SelectionMode.Single;
            this.text1.Text = DateTime.Now.ToLocalTime().ToString("yyyy年MM月dd日 hh:mm:ss");

            timeThread = new Thread(new ThreadStart(DispatcherThread));
            timeThread.Start();

            button2.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = false;
            button5.IsEnabled = false;
            button6.IsEnabled = false;
        }


        public delegate void InvokeDelegate(double t);
        public delegate void AddDelegate(string txt);
        public delegate void buttonDelegate();

        public  void onProcess(int total, int n, string paper_name)
        {
            progressbar1.Dispatcher.BeginInvoke(new InvokeDelegate(UpdateProcessBar),
                                new object[]{ (((double)n+1) / total) });
            progressbar1.Dispatcher.Invoke(new AddDelegate(UpdateStatus),
                            new object[] { paper_name });
            //progressbar1.Value = (int)((((double)n) / total_n) * 100);
        }
        public void UpdateStatus(string papername)
        {
            text2.Content = "正在生成...  " + "进度：" + (int)progressbar1.Value + "%";
            text2.Content += "      当前正在查找文献：" + papername;
        }
        public  void onFinish(ArrayList ref_list)
        {
            foreach (string items in ref_list)
            {
                progressbar1.Dispatcher.Invoke(new AddDelegate(AddListBoxItem),new object[]{items});
            }
            progressbar1.Dispatcher.Invoke(new buttonDelegate(buttonEnable));
            
        }
        public void buttonEnable()
        {
            button1.IsEnabled = true;
            text2.Content = "正在生成...  " + "进度：" + (int)progressbar1.Value + "%";
            text2.Content += "      已完成";

        }

        //Analysis
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (canvas1.Children.Count == 0)
            {
                MessageBox.Show("Null");
                return;

            }
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            button1.IsEnabled = false;
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = false;
            button5.IsEnabled = false;
            button6.IsEnabled = false;

            string[] filenames = new string[canvas1.Children.Count];
            for (int i = 0; i < canvas1.Children.Count; i++)
            {
                filenames[i] = ((Button)canvas1.Children[i]).Content.ToString();

            }

            progressbar1.Value = 0;
            text2.Content = "准备就绪....";

            td = new Thread(new ParameterizedThreadStart(ThreadProgress));
            td.Start(filenames);

            //PaperRefGenerator prg = new PaperRefGenerator();
            //ArrayList al = prg.getPaperReference(filenames, null);
            //foreach (string items in al)
            //{
            //    AddListBoxItem(items);
            //}

            //AddListBoxItem("My Name is Jame Green");
            //AddListBoxItem("Hello World");
            //AddListBoxItem("Hello google Earth");
        }
        public void DispatcherThread()
        {
            //可以通过循环条件来控制UI的更新
            while (true)
            {
                ///线程优先级，最长超时时间，方法委托（无参方法）
                text1.Dispatcher.BeginInvoke(
                   DispatcherPriority.Normal, new Action(UpdateTime));
                Thread.Sleep(1000);
            }
        }
        private void UpdateTime()
        {
            this.text1.Text ="当前时间：" + DateTime.Now.ToLocalTime().ToString("yyyy年MM月dd日 hh:mm:ss");
        }
        public void ThreadProgress(object filenames)
        {
            string[] fn = (string [])filenames;

            PaperRefGenerator prg = new PaperRefGenerator();
            
            prg.setProgressable(this);
            ArrayList papernotfoud = new ArrayList();
            ArrayList al = prg.getPaperReference(fn, papernotfoud);
            foreach (string items in papernotfoud)
            {
                progressbar1.Dispatcher.Invoke(new AddDelegate(AddListBox2Item), new object[] { items });
            }
        }

        public void UpdateProcessBar(double rate)
        {
             progressbar1.Value = (rate * 100);

            // text2.Content = 
        }

        public void btn_Click(Object sender, EventArgs e)
        {
            canvas1.Children.Remove((Button)sender);
            OrderCanrasChild();
        }
        public bool IsRightType(string filename)
        {
            string[] righttype = new string[] { "pdf", ".kdh", ".nh" };
            foreach (string typename in righttype)
            {
                if (typename == filename.Substring(filename.LastIndexOf('.') + 1))
                {
                    return true;
                }
            }
            return false;
        }
        public void OrderCanrasChild()
        {
            double width = this.canvas1.ActualWidth - 5;
            double height = 22.0;

            for (int i = 0; i < canvas1.Children.Count; i++)
            {
                Canvas.SetTop(canvas1.Children[i], (i - 1) * height + height);
                Canvas.SetLeft(canvas1.Children[i], 5);
            }
        }
        public void AddCanrasChild(string filename)
        {
            //canvas1.Children.Clear();
            OrderCanrasChild();
            double height = canvas1.Children.Count * 22;
            Button newbutton = new Button();
            newbutton.Width = this.canvas1.ActualWidth - 5;
            newbutton.Height = 20.0;
            newbutton.Content = filename;
            Canvas.SetTop(newbutton, height);
            Canvas.SetLeft(newbutton, 5);
            newbutton.Style = (Style)this.FindResource("ButtonTemplate2");
            newbutton.MouseDoubleClick += this.btn_Click;
            canvas1.Height = height + 40;
            canvas1.Children.Add(newbutton);


        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "文档文件(.pdf)|*.pdf|所有文件(*.*)|*.*";

            if ((bool)ofd.ShowDialog())
            {
                foreach (string filename in ofd.FileNames)
                {
                    if (IsRightType(filename))
                    {
                        AddCanrasChild(filename.Substring(filename.LastIndexOf('\\') + 1, filename.LastIndexOf('.') - filename.LastIndexOf('\\') - 1));
                    }
                }
            }
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            WriteInfoWin InsertInfo = new WriteInfoWin();
            bool? result = InsertInfo.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.AddCanrasChild(InsertInfo.textBox1.Text.Trim());
                //string results = DialogResult.Value.ToString();
            }
        }
        //Analysis
        public void AddListBoxItem(string txtString)
        {
            if (listBox1.Items.Count == 0)
            {
                button5.IsEnabled = true;
                button6.IsEnabled = true;
            }
            foreach (string name in listBox1.Items)
            {
                if (name == txtString)
                {
                    listBox1.Items.Add("已录入" + listBox1.Items.Count);
                    return;
                }
            }
            listBox1.Items.Add(txtString);


        }
        public void AddListBox2Item(string txtString)
        {
            listBox2.Items.Add(txtString);
        }
        public void DeleteListBoxItem()
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
            if (listBox1.Items.Count == 0)
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button4.IsEnabled = false;
                button5.IsEnabled = false;
                button6.IsEnabled = false;
            }
            else
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button4.IsEnabled = false;
            }
        }
        public void UpMoveListBoxItem()
        {
            int index = listBox1.SelectedIndex;
            string value = listBox1.SelectedItem.ToString();
            if (index > 0)
            {
                //listBox1.Items[index] = listBox1.Items.GetItemAt(index - 1);
                listBox1.Items[index] = listBox1.Items[index - 1];
                listBox1.Items[index - 1] = value;
                listBox1.SelectedIndex = index;
                listBox1.SelectedIndex--;
            }
        }
        public void DownMoveListBoxItem()
        {
            int index = listBox1.SelectedIndex;
            string value = listBox1.SelectedItem.ToString();
            if (index < listBox1.Items.Count)
            {
                listBox1.Items[index] = listBox1.Items[index + 1];
                listBox1.Items[index + 1] = value;
                listBox1.SelectedIndex = index;
                listBox1.SelectedIndex++;
            }
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            button2.IsEnabled = true;
            button3.IsEnabled = true;
            button4.IsEnabled = true;
            if (listBox1.SelectedIndex == 0)
            {
                button2.IsEnabled = false;
            }
            if (listBox1.SelectedIndex == listBox1.Items.Count - 1)
            {
                button3.IsEnabled = false;
            }


        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            UpMoveListBoxItem();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            DownMoveListBoxItem();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            DeleteListBoxItem();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            PreviewWin pw = new PreviewWin();
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                pw.textBox1.Text += "[" + (i + 1) + "]" + listBox1.Items[i].ToString() + "\r\n";
            }
            pw.ShowDialog();
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (td != null && td.IsAlive)
            {
                td.Abort();
            }

            timeThread.Abort();
           // td.Abort();
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = false;
            button5.IsEnabled = false;
            button6.IsEnabled = false;
        }

        private void listBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
