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

namespace InfraSetup
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
            get { return NastaveniInfraSetup.FileNameSize; }
            set
            {
                NastaveniInfraSetup.FileNameSize = value;
                SaveHelper.FileNameSize = NastaveniInfraSetup.FileNameSize;
            }
        }
        public string NazevObrazku
        {
            get { return NastaveniInfraSetup.NazevObrazku; }
            set
            {
                NastaveniInfraSetup.NazevObrazku = value;
                //Counter = SaveHelper.GetNumber(Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + NastaveniInfraViewer.FileNameSize + NastaveniInfraViewer.Pripona));
                //CounterRaw = SaveHelper.GetNumber(Path.Combine(cc.VybranaCesta, NastaveniInfraViewer.NazevObrazku + NastaveniInfraViewer.FileNameSize + NastaveniInfraViewer.PriponaRAW));
            }
        }
        public bool SaveRaw = false;
        private string cestaIni = "";
        ApplicHyper.Hyper.HyperCubeManager hcm = null;
        public NastaveniBolometru nb = null;
        ControlAutoGSKpro AutoCorr = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            hl.Title("");
            hl.Init(false, true);

            sh.Init("Settings InfraViewer", NastaveniInfraSetup.Hodnoty, NastaveniInfraSetup.Save);

            Sensor.Set(Sensor.ESensors.PICO640Gen2);

            cco.TextOn = NastaveniInfraSetup.TextOn;
            cco.TextOff = NastaveniInfraSetup.TextOff;

            var BaseCesta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InfraViewer");
            if (!Directory.Exists(BaseCesta)) Directory.CreateDirectory(BaseCesta);
            //var BaseCesta = "";
            string BolInitCesta = Path.Combine(BaseCesta, NastaveniInfraSetup.BolInit);
            if (!Directory.Exists(BolInitCesta)) Directory.CreateDirectory(BolInitCesta);

            cestaIni = Path.Combine(BaseCesta, NastaveniInfraSetup.NastaveniKamery);
            if (!File.Exists(cestaIni)) File.Create(cestaIni).Close();
            
            if (!Directory.Exists(NastaveniInfraSetup.CestaSave))
            {
                    NastaveniInfraSetup.CestaSave = Path.Combine(BaseCesta, NastaveniInfraSetup.Saves);
                    Directory.CreateDirectory(NastaveniInfraSetup.CestaSave);
            }
            //cc.VybranaCesta = NastaveniInfraViewer.CestaSave;
            //cc.ButtonText = NastaveniInfraViewer.ButtonText;
            //cc.PropertyChanged += Cc_PropertyChanged;
            
            Resp = delegate (ClientCommandResponse ccr) { if (ccr.Error) Console.WriteLine("ERR " + ccr.Text); };

            ControlGMS cgms = new ControlGMS();
            controlGain.Init(Sensor.HodnotyGMS);
            controlGain.Init(cgms);

            MojeKamera = new Kamera() { FilterCut = false, MaxImgCnt = 20 };
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


            AutoCorr = new ControlAutoGSKpro();
            AutoCorr.Init(MojeKamera);
            cco.Init(MojeKamera);
            pripojitOdpojit.Init(MojeKamera, iptb);

            ov.Init(MojeKamera.Obarvovac);
            nb = new NastaveniBolometru(BolInitCesta);
            bfs.Init(nb);

            kc.Init(MojeKamera, BolInitCesta);

            MS = new MujServer();
            MS.Init(this);

            Korekce = MojeKamera.MojeHodnoty[CameraValuesBit.NUCcomputeOffset];

            MojeKamera.Connect();

            //hcm = new ApplicHyper.Hyper.HyperCubeManager();
            //HCQ.Init(hcm);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (nb == null) return;
            if (e.AddedItems.Count >= 1)
            {
                spButtons.Children.Clear();
                foreach (var item in nb.Seznam.Skip(4))
                {
                    var b = new Button() {Content = item.Jmeno };
                    b.Click += delegate (object senderB, RoutedEventArgs eB) { bfs.Set(item); };
                    spButtons.Children.Add(b);
                }

                foreach (var item in kc.NMI.Seznam)
                {
                    var b = new Button() { Content = item.Jmeno };
                    b.Click += delegate (object senderB, RoutedEventArgs eB) { kc.KT.Decode(item.Text); };
                    spButtons.Children.Add(b);
                }
            }
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
            if (!MojeKamera.Pripojeno) MessageBox.Show(NastaveniInfraSetup.KameraNepripojena);
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
                string jmeno = cameraQuickSaver.Cesta + Counter.ToString(NastaveniInfraSetup.FileNameSize) + NastaveniInfraSetup.Pripona;
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
                    Vypis.Error(NastaveniInfraSetup.NastaveniIP);
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

        private void SaveCamConf(object sender, RoutedEventArgs e)
        {            
            var CamInit = MojeKamera.MojeHodnoty.Get(CameraValuesByte.SDcameraInit);
            var SDwrite = MojeKamera.MojeHodnoty[CameraValuesBit.SDwriteInitSet];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SET IMS CNA -1");
            sb.AppendLine("SET IMS DEA 0");
            string s = null;
            var ii = new CameraValuesInt[] { CameraValuesInt.BolometerAcqX, CameraValuesInt.BolometerAcqY, CameraValuesInt.BolometerCKDIV, CameraValuesInt.BolometerFirstX, CameraValuesInt.BolometerFirstY, CameraValuesInt.BolometerLastX, CameraValuesInt.BolometerLastY, CameraValuesInt.BolometerGFID, CameraValuesInt.BolometerGMS, CameraValuesInt.BolometerGSK, CameraValuesInt.BolometerINT, CameraValuesInt.ShutterTimeOff, CameraValuesInt.ShutterTimeOn, CameraValuesInt.BolometerVBUS, CameraValuesInt.BolometerVDET };
            foreach (var i in ii)
            {
                s = MojeKamera.MojeHodnoty.GetInitCommand(i);
                if (s != null) sb.AppendLine(s);
            }

            var iii = new CameraValuesBool[] { CameraValuesBool.BolometerEnable, CameraValuesBool.DataComplementEnable, CameraValuesBool.ImageEnableNUC, CameraValuesBool.ShutterPol, CameraValuesBool.ImageOutputPacking, CameraValuesBool.ShutterEnable };
            foreach (CameraValuesBool i in iii)
            {
                s = MojeKamera.MojeHodnoty.GetInitCommand(i);
                if (s != null) sb.AppendLine(s);
            }
            byte[] transfer_data = Encoding.ASCII.GetBytes(sb.ToString());
            CamInit.Hodnota = transfer_data;
            SDwrite.Send(1);
        }

        private void SaveConf(object sender, RoutedEventArgs e)
        {
            var cesta = MojeCesta.VyberCestuFolder();
            if (string.IsNullOrEmpty(cesta)) return;

            var BaseCesta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InfraViewer");
            DirectoryInfo dir = new DirectoryInfo(BaseCesta);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(cesta)) Directory.CreateDirectory(cesta);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(cesta, file.Name);
                file.CopyTo(tempPath, false);
            }
            bool copySubDirs = true;
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(cesta, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {  // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists) { throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName); }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        private void FactoryConf(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Revert to factory settings?","",MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var BaseCesta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InfraViewer");
            if (Directory.Exists(BaseCesta)) Directory.Delete(BaseCesta, true);
            Directory.CreateDirectory(BaseCesta);

            string BolInitCesta = Path.Combine(BaseCesta, NastaveniInfraSetup.BolInit);
            Directory.CreateDirectory(BolInitCesta);

            cestaIni = Path.Combine(BaseCesta, NastaveniInfraSetup.NastaveniKamery);
            File.Create(cestaIni).Close();

            nb.SetDefault();
            nb.Save();
            kc.NMI.SetDefault();
            kc.NMI.Save();
        }

        private void AutoCorrection(object sender, RoutedEventArgs e)
        {
            var len = MojeKamera.MojeHodnoty[CameraValuesBool.ShutterEnable].HodnotaValid;
            MojeKamera.MojeHodnoty[CameraValuesBool.ShutterEnable].Hodnota = false;
            try
            {
                AutoCorr.Compute();
                SpinWait.SpinUntil(() => !AutoCorr.compute, 20000);
            }
            catch { }
            MojeKamera.MojeHodnoty[CameraValuesBool.ShutterEnable].Hodnota = len;
            bolometerControl3.Refresh();
        }
    }
}
