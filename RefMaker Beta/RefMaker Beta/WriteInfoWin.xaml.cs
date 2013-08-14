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
    /// Interaction logic for WriteInfoWin.xaml
    /// </summary>
    public partial class WriteInfoWin : Window
    {
        public WriteInfoWin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Trim() == "")
            {
                MessageBox.Show("Null");
                return;
            }
            MainWindow mw = (MainWindow)this.Owner;
           // mw.titleString = textBox1.Text.Trim();
            //mw.AddCanrasChild(textBox1.Text.Trim());
            this.DialogResult = true;
            this.Hide();  
        }
    }
}
