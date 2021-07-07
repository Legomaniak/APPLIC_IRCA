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
using System.Threading;
using System.Globalization;
using AppJCE.Zobrazeni;

namespace InfraEasySetup
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Kamera MojeKamera;
        Action<ClientCommandResponse> Resp = null;
        KameraConnectWindow kcw;
        public ImageHeader<short> Obrazek = null;
        public CameraCommandBit Korekce = null;
        object MonitorLock = new object();
        public ObrazekView Monitor = null;

        //private string cestaIni = "";
        public MainWindow()
        {
            InitializeComponent();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
            this.Closing += MainWindow_Closing;
            hl.Title(this.Title);
            hl.Init(true, true);
            Button bFull = new Button()
            {
                Content = "FullScreen",
            };
            hl.Init(bFull);
            //AppJCE.Okna.Config.SetScreen(this, AppJCE.Okna.Config.ScreenMode.Industry);
            AppJCE.Okna.Config.SetScreen(this, AppJCE.Okna.Config.ScreenMode.FullScreen);
            bFull.Click += BFull_Click;
            Loaded += MainWindow_Loaded;

            Sensor.Set(Sensor.ESensors.PICO640Gen2);

            cco.TextOn = "Online:";
            cco.TextOff = "Offline:";

            Resp = delegate (ClientCommandResponse ccr) { if (ccr.Error) Console.WriteLine("ERR " + ccr.Text); };
            
            MojeKamera = new Kamera();
            MojeKamera.Connected += MojeKamera_Connected;
            MojeKamera.NewObrazekBol += MojeKamera_NewObrazekBol;
            MojeKamera.NewObrazekBolSource += MojeKamera_NewObrazekBolSource;
            MojeKamera.KontrolniFrekvencePripojeni = 20000;

            var BaseCesta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IRCAapp");
            if (!Directory.Exists(BaseCesta)) Directory.CreateDirectory(BaseCesta);
            string BolInitCesta = Path.Combine(BaseCesta, "BolInit");
            if (!Directory.Exists(BolInitCesta)) Directory.CreateDirectory(BolInitCesta);
            NastaveniBolometru nb = new NastaveniBolometru(BolInitCesta);

            ControlGMS gms = new ControlGMS();

            MojeKamera.Ovladace.Clear();
            MojeKamera.Ovladace.Add(bolometerControl);
            MojeKamera.Ovladace.Add(bolometerFastSetup);
            MojeKamera.Ovladace.Add(gms);
            MojeKamera.Ovladace.Add(corr);

            bolometerFastSetup.Init(nb);
            controlGMSGain.Init(gms);
            controlKameraIQ.Init(MojeKamera);

            MojeKamera.InitOvladace();

            cco.Init(MojeKamera);

            ov.Init(MojeKamera.Obarvovac, controlROI);

            controlROI.NewLines += ControlROI_NewLines;
            graph1d.showName = false;
            graph1d.AutoX = true;
            graph1d.AutoY = true;


            MojeKamera.Connect();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ov.ZmenaVelikosti();
        }

        private void ControlROI_NewLines(object sender, List<double[]> e)
        {
            if (e == null) ThreadSafer.MakeSTA(graph1d, delegate () { graph1d.Clear(); });
            else ThreadSafer.MakeSTA(graph1d, delegate () { graph1d.addData(e.ToArray()); });
        }

        private void BFull_Click(object sender, RoutedEventArgs e)
        {
            lock (MonitorLock)
            {
                if (Monitor == null)
                {
                    Window w = new Window();
                    AppJCE.Okna.Config.SetScreen(w, AppJCE.Okna.Config.ScreenMode.FullScreen);
                    Monitor = new ObrazekView();
                    Monitor.Init(MojeKamera.Obarvovac);
                    w.Content = Monitor;
                    w.Show();
                    Monitor.ZmenaVelikosti();
                    w.KeyDown += delegate (object sender2, KeyEventArgs e2) { if (e2.Key == Key.Escape) w.Close(); };
                    w.Closed += delegate (object sender1, EventArgs e1) { lock (MonitorLock) { Monitor = null; } };
                }
            }
        }
        
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MojeKamera?.Close();
        }

        public void MojeKamera_NewObrazekBol(object sender, BitmapSource e)
        {
            lock (MonitorLock)
            {
                if (Monitor == null) ov.Zobraz(e);
                else Monitor.Zobraz(e);
            }
        }

        public void MojeKamera_NewObrazekBolSource(object sender, ImageHeader<short> e)
        {
            controlImageHeadeMini.Init(e);
            //ii.Set(e);
            Obrazek = e;
            //ThreadSafer.MakeSTA(controlROI, delegate () { controlROI.Init(e); });
            controlROI.Init(e);
        }

        private void KS_VybranaIP(object sender, IPAddress e)
        {
            if (kcw != null)
            {
                try
                {
                    kcw.DialogResult = true;
                    kcw.Close();
                }
                catch
                {
                    Vypis.Error("Chyba nastaveni IP");
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
        public void MojeKamera_Connected(object sender, bool e)
        {
            if (!e)
            {
                MessageBox.Show("Nedostupná kamera!");
                ThreadSafer.MakeMain(() =>
                {
                    kcw = new KameraConnectWindow();
                    kcw.KS.VybranaIP += KS_VybranaIP;
                    kcw.ShowDialog();
                    //if (!kcw.ShowDialog().Value)
                    //{
                    //    Close();
                    //}
                });
                return;
            }
            if (!MojeKamera.Pripojeno)
            {
                MessageBox.Show("Kamera neni připojena");
                Close();
            }
            else
            {
                string file = "Kamera.ini";
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string radek = sr.ReadLine();
                        while (radek != null)
                        {
                            if (!string.IsNullOrEmpty(radek)) MojeKamera.CC.AddComand(radek, Resp);
                            radek = sr.ReadLine();
                        }
                    }
                }
                else Console.WriteLine(file + " not exist");


                Korekce = MojeKamera.MojeHodnoty[CameraValuesBit.NUCcomputeOffset];

                DefaultSettings(null, null);

                controlROI.MaxX = (int)MojeKamera.MojeHodnoty[CameraValuesInt.ImageWidth].HodnotaValid;
                controlROI.MaxY = (int)MojeKamera.MojeHodnoty[CameraValuesInt.ImageHeight].HodnotaValid;
                controlROI.RegenerateMask();

                Korekce.Send(1);
            }
        }
        
        public void Save(object sender, RoutedEventArgs e)
        {
            string cesta = MojeCesta.VyberCestuSave("obrazek", "Bitmap (*.bmp)|*.bmp");
            if (Directory.Exists(cesta))
            {
                var bs = ov.GetImage();
                if (bs == null) throw new Exception("No Image");
                var bf = BitmapFrame.Create(bs);
                using (var fileStream = new FileStream(cesta, FileMode.Create))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(bf);
                    encoder.Save(fileStream);
                }
            }
        }

        public void Correction(object sender, RoutedEventArgs e)
        {
            Korekce?.Send(1);
        }

        private void DefaultSettings(object sender, RoutedEventArgs e)
        {
            string file = "KameraDefault.ini";
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    string radek = sr.ReadLine();
                    while (radek != null)
                    {
                        if (!string.IsNullOrEmpty(radek)) MojeKamera.CC.AddComand(radek, Resp);
                        radek = sr.ReadLine();
                    }
                }
            }
            else Console.WriteLine(file + " not exist");
            
            MojeKamera.AllData = false;
            MojeKamera.Average = 0;
            MojeKamera.Colored = true;
            MojeKamera.Convert = true;
            MojeKamera.Dummy = false;

            MojeKamera.RefreshOvladace();
        }

        private void ButtonS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bs = ov.GetImage();
                string cesta = MojeCesta.VyberCestuSave("obrazek", "Bitmap (*.bmp)|*.bmp");
                if (bs == null) return;
                var bf = BitmapFrame.Create(bs);
                using (var fileStream = new FileStream(cesta, FileMode.Create))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(bf);
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ButtonSB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var Obrazek = ov.Obrazek;
                string cesta = MojeCesta.VyberCestuSave("obrazek", "Binary (*.bin)|*.bin");
                using (var wb = new BinaryWriter(new FileStream(cesta, FileMode.Create)))
                {
                    var X = Obrazek.Width;
                    wb.Write(X);
                    var Y = Obrazek.Height;
                    wb.Write(Y);
                    for (int x = 0; x < X; x++)
                    {
                        for (int y = 0; y < Y; y++)
                        {
                            wb.Write(Obrazek[x, y]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
