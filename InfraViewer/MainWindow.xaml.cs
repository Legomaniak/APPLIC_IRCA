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
using System.Windows.Navigation;
using ApplicInfra;
using AppJCE;
using ApplicInfra.Infra;
using System.Net;
using System.IO;
using ApplicInfra.Infra.Nastaveni;
using ApplicInfra.Save;
using ApplicInfra.Infra.Controls;

namespace InfraViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Kamera MojeKamera;
        MujServer MS;
        Action<ClientCommandResponse> Resp = null;
        KameraConnectWindow kcw;
        public ImageHeader<short> Obrazek = null;
        public CameraCommandBit Korekce = null;

        public int Counter = 0;
        public int CounterRaw = 0;
        public string FileNameSize
        {
            get { return NastaveniInfraViewer.FileNameSize; }
            set
            {
                NastaveniInfraViewer.FileNameSize = value;
                SaveHelper.FileNameSize = NastaveniInfraViewer.FileNameSize;
            }
        }
        public string NazevObrazku
        {
            get { return NastaveniInfraViewer.NazevObrazku; }
            set
            {
                NastaveniInfraViewer.NazevObrazku = value;
                //Counter = SaveHelper.GetNumber(Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + NastaveniInfraViewer.FileNameSize + NastaveniInfraViewer.Pripona));
                //CounterRaw = SaveHelper.GetNumber(Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + NastaveniInfraViewer.FileNameSize + NastaveniInfraViewer.PriponaRAW));
            }
        }
        public bool SaveRaw = false;
        private string cestaIni = "";
        ApplicHyper.Hyper.HyperCubeManager hcm = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            hl.Title("");
            hl.Init(false, true);

            Sensor.Set(Sensor.ESensors.PICO640Gen2);

            cco.TextOn = NastaveniInfraViewer.TextOn;
            cco.TextOff = NastaveniInfraViewer.TextOff;

            var BaseCesta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InfraViewer");
            if (!Directory.Exists(BaseCesta)) Directory.CreateDirectory(BaseCesta);
            //var BaseCesta = "";
            string BolInitCesta = Path.Combine(BaseCesta, NastaveniInfraViewer.BolInit);
            if (!Directory.Exists(BolInitCesta)) Directory.CreateDirectory(BolInitCesta);

            cestaIni = Path.Combine(BaseCesta, NastaveniInfraViewer.NastaveniKamery);
            if (!File.Exists(cestaIni)) File.Create(cestaIni).Close();
            
            if (!Directory.Exists(NastaveniInfraViewer.CestaSave))
            {
                    NastaveniInfraViewer.CestaSave = Path.Combine(BaseCesta, NastaveniInfraViewer.Saves);
                    Directory.CreateDirectory(NastaveniInfraViewer.CestaSave);
            }
            //cc.VybranaCesta = NastaveniInfraViewer.CestaSave;
            //cc.ButtonText = NastaveniInfraViewer.ButtonText;
            //cc.PropertyChanged += Cc_PropertyChanged;
            
            Resp = delegate (ClientCommandResponse ccr) { if (ccr.Error) Console.WriteLine("ERR " + ccr.Text); };

            ControlGMS cgms = new ControlGMS();
            controlGain.Init(Sensor.HodnotyGMS);
            controlGain.Init(cgms);

            MojeKamera = new Kamera() { FilterCut = false, };
            MojeKamera.Connected += MojeKamera_Connected;
            MojeKamera.NewObrazekBol += MojeKamera_NewObrazekBol;
            MojeKamera.NewObrazekBolSource += MojeKamera_NewObrazekBolSource;
            MojeKamera.NewObrazekRaw += MojeKamera_NewObrazekRaw;
            MojeKamera.NewObrazekBolSource4 += MojeKamera_NewObrazekBolSource4;
            controlKamera.Init(MojeKamera);

            MojeKamera.Ovladace.Add(bfs);
            MojeKamera.Ovladace.Add(cgms);
            MojeKamera.Ovladace.Add(corr); 
            MojeKamera.Ovladace.Add(bolometerControl3);


            MojeKamera.InitOvladace();

            cco.Init(MojeKamera);
            pripojitOdpojit.Init(MojeKamera, iptb);

            ov.Init(MojeKamera.Obarvovac);
            NastaveniBolometru nb = new NastaveniBolometru(BolInitCesta);
            bfs.Init(nb);

            kc.Init(MojeKamera, BolInitCesta);

            MS = new MujServer();
            MS.Init(this);

            Korekce = MojeKamera.MojeHodnoty[CameraValuesBit.NUCcomputeOffset];

            MojeKamera.Connect();

            //hcm = new ApplicHyper.Hyper.HyperCubeManager();
            //HCQ.Init(hcm);
        }


        //private void Cc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "VybranaCesta")
        //    {
        //        if (Directory.Exists(cc.VybranaCesta))
        //        {
        //            NastaveniInfraViewer.CestaSave = cc.VybranaCesta;
        //            Counter = SaveHelper.GetNumber(Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + NastaveniInfraViewer.FileNameSize + NastaveniInfraViewer.Pripona));
        //            CounterRaw = SaveHelper.GetNumber(Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + NastaveniInfraViewer.FileNameSize + NastaveniInfraViewer.PriponaRAW));
        //        }
        //    }
        //}

        private void MojeKamera_NewObrazekRaw(object sender, Tuple<byte[], byte[]> e)
        {
            //cameraQuickSaver.Show(e);
            //    if (e == null || !SaveRaw) return;
            //    if (Directory.Exists(cc.VybranaCesta))
            //    {
            //        string jmeno = Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + CounterRaw.ToString(NastaveniInfraViewer.FileNameSize) + NastaveniInfraViewer.PriponaRAW);
            //        using (var fileStream = new FileStream(jmeno, FileMode.Create))
            //        {
            //            BinaryWriter wb = new BinaryWriter(fileStream);
            //            int l1 = e.Item1.Length;
            //            int l2 = e.Item2.Length;
            //            wb.Write(l1);
            //            wb.Write(l2);
            //            wb.Write(e.Item1);
            //            wb.Write(e.Item2);
            //            wb.Close();
            //        }
            //        CounterRaw++;
            //    }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MojeKamera?.Close();
            MS?.Close();
            //motor.s.DisConnect();
        }

        public void MojeKamera_NewObrazekBol(object sender, BitmapSource e)
        {
            ov?.Zobraz(e);
            cameraQuickSaver.Show(e);
        }

        public void MojeKamera_NewObrazekBolSource(object sender, ImageHeader<short> e)
        {
            ii?.Set(e);
            Obrazek = e;
            hcm?.Show(e);
            cameraQuickSaver.Show(e);
        }

        private void MojeKamera_NewObrazekBolSource4(object sender, ImageHeader<uint> e)
        {
            ii?.Set(e);
            cameraQuickSaver.Show(e);
            //Obrazek4 = e;
            //hcm?.Show(e);
        }

        public void MojeKamera_Connected(object sender, bool e)
        {
            if (!e) return;
            if (!MojeKamera.Pripojeno) MessageBox.Show(NastaveniInfraViewer.KameraNepripojena);
            else
            {
                try
                {
                    StreamReader sr = new StreamReader(cestaIni);
                    string radek = sr.ReadLine();
                    while (radek != null)
                    {
                        if (!string.IsNullOrEmpty(radek) || radek.First() != '#') MojeKamera.CC.AddComand(radek, Resp);
                        radek = sr.ReadLine();
                    }
                    sr.Close();
                }
                catch
                {
                    Console.WriteLine("init file error");
                }
            }

            //Korekce = MojeKamera.MojeHodnoty[HodnotyKameryBit.SetOffset];
            //controlGain.Init(Sensor.HodnotyGMS);
            
            //MojeKamera.MojeHodnoty.Get(HodnotyKameryBool.ShutterEna);
        }

        public void connect(object sender, RoutedEventArgs e)
        {
            //MojeKamera.Close();
            MojeKamera.Connect(iptb.GetIP());
        }

        public void search(object sender, RoutedEventArgs e)
        {
            ThreadSafer.MakeMain(() =>
            {
                kcw = new KameraConnectWindow();
                kcw.KS.VybranaIP += KS_VybranaIP;
                kcw.ShowDialog();
            });
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            if (Obrazek == null) return;
            if (Directory.Exists(cameraQuickSaver.Cesta))
            {
                string jmeno = cameraQuickSaver.Cesta + Counter.ToString(NastaveniInfraViewer.FileNameSize) + NastaveniInfraViewer.Pripona;
                using (var fileStream = new FileStream(jmeno, FileMode.Create))
                {
                    BinaryWriter wb = new BinaryWriter(fileStream);
                    wb.Write(Obrazek.ImageNumber);
                    wb.Write(Obrazek.Height);
                    wb.Write(Obrazek.Width);
                    wb.Write(Obrazek.TemperatureADCBol);
                    wb.Write(Obrazek.ImageFlags);
                    for (int i = 0; i < Obrazek.Data.Length; i++)
                    {
                        wb.Write((short)Obrazek.Data[i]);
                    }
                    wb.Close();
                }
                Counter++;
            }
        }

        public void Correction(object sender, RoutedEventArgs e)
        {
            Korekce?.Send(1);
        }

        private void KS_VybranaIP(object sender, IPAddress e)
        {
            if (kcw != null)
            {
                try
                {
                    kcw.DialogResult = true;
                    kcw.Close();
                    iptb.SetIP(e);
                }
                catch
                {
                    Vypis.Error(NastaveniInfraViewer.NastaveniIP);
                }
            }
            if (e == null)//Offline rezim
            {
                return;
            }
            //propojeni kamery
            //MojeKamera.Close();
            MojeKamera.Connect(e);
        }
    }
}
