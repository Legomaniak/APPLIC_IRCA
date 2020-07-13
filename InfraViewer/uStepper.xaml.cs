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

namespace InfraViewer
{
    /// <summary>
    /// Interakční logika pro uStepper.xaml
    /// </summary>
    public partial class uStepper : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private float _Pozice = 120;
        public float Pozice
        {
            get { return _Pozice; }
            set { _Pozice = value; OnPropertyChanged("Pozice"); }
        }
        private float _Pozice0;
        public float Pozice0
        {
            get { return _Pozice0; }
            set { _Pozice0 = value; OnPropertyChanged("Pozice0"); }
        }
        private float _PoziceM = 270;
        public float PoziceM
        {
            get { return _PoziceM; }
            set { _PoziceM = value; OnPropertyChanged("PoziceM"); }
        }
        private float _PoziceT;
        public float PoziceT
        {
            get { return _PoziceT; }
            set { _PoziceT = value; OnPropertyChanged("PoziceT"); }
        }
        private float _Krok = 10;
        public float Krok
        {
            get { return _Krok; }
            set { _Krok = value; OnPropertyChanged("Krok"); }
        }
        private int _Cas = 1000;
        public int Cas
        {
            get { return _Cas; }
            set { _Cas = value; OnPropertyChanged("Cas"); }
        }
        public AppJCE.Komunikace.ASerial s;


        public uStepper()
        {
            InitializeComponent();
            s = new AppJCE.Komunikace.ASerial("Motor");
            DataContext = this;
            s.ByloPripojeno += S_ByloPripojeno;
        }

        private void S_ByloPripojeno(object sender, EventArgs e)
        {
            if(s.Pripojeno)
            {
                s.Write("SET_NEW_SP 0 \r");
            }
        }

        private void NastavTeleso(object sender, RoutedEventArgs e)
        {
            s?.Write("SET_NEW_SP " + PoziceT.ToString("0.0") + " \r");
        }

        public void SetPozice(float p)
        {
            s?.Write("SET_NEW_SP " + p.ToString("0.0") + " \r");
        }
        private void NastavPolohu(object sender, RoutedEventArgs e)
        {
            s?.Write("SET_NEW_SP " + Pozice.ToString("0.0") + " \r");
        }

        private void NastavSken(object sender, RoutedEventArgs e)
        {
            try
            {
                int k = (int)((PoziceM - Pozice0) / Krok);
                for (int i = 0; i < k; i++)
                {
                    s?.Write("SET_NEW_SP " + (Pozice0 + i * Krok).ToString("0.0") + " \r");
                    System.Threading.SpinWait.SpinUntil(() => false, Cas);
                }
            }
            catch
            {

            }
        }
    }
}
