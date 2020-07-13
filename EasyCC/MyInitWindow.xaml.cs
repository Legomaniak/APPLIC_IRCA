using AppJCE.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyCC
{
    /// <summary>
    /// Interaction logic for MyInitWindow.xaml
    /// </summary>
    public partial class MyInitWindow : Window, ISlozkaWindow<MyInitScrypt>
    {
        public MyInitWindow()
        {
            InitializeComponent();
        }

        public MyInitScrypt Polozka { get; set; }

        public void Add(MyInitScrypt polozka)
        {
            var p = new MyInitScrypt();
            p.Jmeno = polozka.Jmeno;
            p.Text = polozka.Text;
            p.Kategorie = polozka.Kategorie;
            DataContext = p;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Polozka = (MyInitScrypt)DataContext;
        }
    }
}