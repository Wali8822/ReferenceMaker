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
using System.Windows.Shapes;

namespace RefMaker_Beta
{
    /// <summary>
    /// Interaction logic for PreviewWin.xaml
    /// </summary>
    public partial class PreviewWin : Window
    {
        public PreviewWin()
        {
            InitializeComponent();
            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text != "")
            {
                Clipboard.SetDataObject(textBox1.Text.Trim());
                MessageBox.Show("已复制到剪切板");
            }
        }
    }
}
