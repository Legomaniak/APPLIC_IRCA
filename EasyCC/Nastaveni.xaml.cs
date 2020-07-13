using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyCC
{
    /// <summary>
    /// Interakční logika pro Nastaveni.xaml
    /// </summary>
    public partial class Nastaveni : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _AdresaIP;

        public string AdresaIP
        {
            get { return _AdresaIP; }
            set { _AdresaIP = value; OnPropertyChanged("AdresaIP"); }
        }
        
        public Nastaveni()
        {
            InitializeComponent();
            Binding b = new Binding("AdresaIP");
            b.Source = this;
            b.Mode = BindingMode.TwoWay;
            tbBaseCesta.SetBinding(TextBox.TextProperty, b);
        }

        public void Init(ApplicInfra.Infra.Kamera K)
        {
            ck.Init(K);
            AdresaIP = AppJCE.Properties.MujSettings.Default.AdresaIP;
        }

        private void BaseCestaClick(object sender, RoutedEventArgs e)
        {
            AppJCE.Properties.MujSettings.Default.AdresaIP = AdresaIP;
            AppJCE.Properties.MujSettings.Default.Save();
        }
    }
}
