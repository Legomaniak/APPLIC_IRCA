using AppJCE;
using AppJCE.Komunikace;
using AppJCE.SplashScreen;
using ApplicInfra.Infra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace EasyCC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Kamera MojeKamera;
        NastaveniMyInit NMI;
        static string BaseCesta = "";
        public ObservableCollection<MyInitScrypt> PodScrypty { get; set; }
        public ObservableCollection<string> Scrypty { get; set; }

        SplasherViewWindow2 MySplasher;
        public bool SaveAll { get; set; }
        public bool SaveAllB { get; set; }
        public int SaveTimeout { get; set; } = 0;
        public string CestaSave { get; set; }
        public Queue<Tuple<byte[], byte[]>> StoreFIFO;
        public int SaveNum { get; set; } = 0;
        public int SaveNumMax { get; set; } = 1000;

        public MainWindow()
        {
            MySplasher = new SplasherViewWindow2();
            try
            {
                if (Properties.Settings.Default.HlaskaOn)
                {
                    var f = File.ReadAllLines("Hlasky.txt");
                    Random r = new Random();
                    var h = f[r.Next(0, f.Length)];
                    MySplasher.Zobraz(h);
                    MySplasher.Show();
                    Thread.Sleep(1000 + h.Length * 30);
                }
                else
                {
                    MySplasher.Zobraz("Naèítám se ...");
                    MySplasher.Show();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            InitializeComponent();
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
            //SplashScreen ss = new SplashScreen("obrazek");
            //ss.Show(true, true);
            WindowState = WindowState.Minimized;
            try
            {
                this.Top = AppJCE.Properties.MujSettings.Default.Top;
                this.Left = AppJCE.Properties.MujSettings.Default.Left;
                this.Height = AppJCE.Properties.MujSettings.Default.Height;
                this.Width = AppJCE.Properties.MujSettings.Default.Width;
            }
            catch
            {
                Console.WriteLine("Invalid windows size or position.");
            }
            AppJCE.Properties.MujSettings.Default.Debug = true;

            Sensor.Set(Sensor.ESensors.PICO640Gen2);
            BaseCesta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasyCC");
            if (!Directory.Exists(BaseCesta)) Directory.CreateDirectory(BaseCesta);
            string initCesta = Path.Combine(BaseCesta, "MyInit");
            if (!Directory.Exists(initCesta)) Directory.CreateDirectory(initCesta);

            NMI = new NastaveniMyInit(initCesta);
            PodScrypty = new ObservableCollection<MyInitScrypt>();
            Scrypty = new ObservableCollection<string>();

            SelectionChanged();

            Binding b = new Binding("Scrypty");
            b.Mode = BindingMode.TwoWay;
            b.Source = this;
            slozkaListBox1.SetBinding(ListBox.ItemsSourceProperty, b);

            b = new Binding("PodScrypty");
            b.Mode = BindingMode.TwoWay;
            b.Source = this;
            slozkaListBox2.SetBinding(ListBox.ItemsSourceProperty, b);
            slozkaListBox2.DisplayMemberPath = "Jmeno";

            slozkaListBox1.SelectionChanged += SlozkaListBox1_SelectionChanged;
            slozkaListBox2.SelectionChanged += SlozkaListBox_SelectionChanged;
            slozkaListBox2.MouseDoubleClick += SlozkaListBox_MouseDoubleClick;

            MojeKamera = new Kamera() { Silent = true, };
            MojeKamera.Connected += MojeKamera_Connected;
            MojeKamera.NewObrazekBol += MojeKamera_NewObrazekBol;
            MojeKamera.NewObrazekBolSource += MojeKamera_NewObrazekBolSource;
            MojeKamera.NewObrazekRaw += MojeKamera_NewObrazekRaw;
            MojeKamera.NewObrazekBolSource4 += MojeKamera_NewObrazekBolSource4;

            MojeKamera.Connect();
            show2D.Init(MojeKamera.Obarvovac);

            cconline.Init(MojeKamera);
            n.Init(MojeKamera);
        }


        KameraConnectWindow kcw = null;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MySplasher?.Close();
            try
            {
                // Very quick and dirty - but it does the job
                if (AppJCE.Properties.MujSettings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            catch { }
            slozkaListBox1.SelectedIndex = 0;

            if (!MojeKamera.Pripojeno)
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
            }
        }

        private void KS_VybranaIP(object sender, IPAddress e)
        {
            if (kcw != null)
            {
                try
                {
                    kcw.DialogResult = true;
                    kcw.Close();

                    AppJCE.Properties.MujSettings.Default.AdresaIP = e.ToString();
                    n.AdresaIP = AppJCE.Properties.MujSettings.Default.AdresaIP;
                    AppJCE.Properties.MujSettings.Default.Save();
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
            MojeKamera.Connect(e);
        }

        private void SelectionChanged()
        {
            var index = slozkaListBox1.SelectedIndex;
            VypisKategorie();
            slozkaListBox1.SelectedIndex = index < 0 ? 0 : index;
        }
        public void VypisKategorie()
        {
            Scrypty.Clear();
            PodScrypty.Clear();
            foreach (var i in (from f in NMI.Seznam select f.Kategorie))
            {
                if (!Scrypty.Contains(i)) Scrypty.Add(i);
            }

            if (Scrypty.Count > 0)
            {
                VypisScrypty(Scrypty[0]);
            }
        }
        private void SlozkaListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = slozkaListBox1.SelectedIndex;
            if (index < 0) return;
            VypisScrypty(Scrypty[index]);
        }
        public void VypisScrypty(string kat)
        {
            var s = (from f in NMI.Seznam where f.Kategorie == kat select f).OrderBy(x => x.Cislo).ToArray();
            PodScrypty.Clear();
            foreach (var i in s)
            {
                PodScrypty.Add(i);
            }
            for (int i = 0; i < PodScrypty.Count; i++)
            {
                PodScrypty[i].Cislo = i;
                NMI.Seznam[NMI.Seznam.IndexOf(PodScrypty[i])].Cislo = i;
            }
            if (PodScrypty.Count > 0)
            {
                slozkaListBox2.SelectedIndex = 0;
                AktivujScrypt(PodScrypty[0]);
            }
        }

        private void MojeKamera_NewObrazekBolSource4(object sender, ImageHeader<uint> e)
        {
            imageInfo.Init(e);
            if (SaveAll)
            {
                //uloz header
                var cesta = Path.Combine(CestaSave, SaveNum + ".h");
                File.WriteAllText(cesta, e.Source);
            }
        }
        private void MojeKamera_NewObrazekBolSource(object sender, ImageHeader<short> e)
        {
            imageInfo.Init(e);
            if (SaveAll)
            {
                //uloz header
                var cesta = Path.Combine(CestaSave, SaveNum + ".h");
                File.WriteAllText(cesta, e.Source);
                //using (StreamWriter sw = new StreamWriter(File.Create(cesta)))
                //{
                //    sw.WriteLine("Average " + e.Average);
                //    sw.WriteLine("BitsPerPixel " + e.BitsPerPixel);
                //    sw.WriteLine("ByteSize " + e.ByteSize);
                //    sw.WriteLine("CameraID " + e.CameraID);
                //    sw.WriteLine("ColoringMaximum " + e.ColoringMaximum);
                //    sw.WriteLine("ColoringMinimum " + e.ColoringMinimum);
                //    sw.WriteLine("Data " + e.Data);
                //    sw.WriteLine("DNA " + e.DNA);
                //    sw.WriteLine("Firmware " + e.Firmware);
                //    sw.WriteLine("Height " + e.Height);
                //    sw.WriteLine("HWResolutionX " + e.HWResolutionX);
                //    sw.WriteLine("HWResolutionY " + e.HWResolutionY);
                //    sw.WriteLine("ImageFlags " + e.ImageFlags);
                //    sw.WriteLine("ImageNumber " + e.ImageNumber);
                //    sw.WriteLine("IsReadOnly " + e.IsReadOnly);
                //    sw.WriteLine("Length " + e.Length);
                //    sw.WriteLine("Maximum " + e.Maximum);
                //    sw.WriteLine("Minimum " + e.Minimum);
                //    sw.WriteLine("PixelByteStride " + e.PixelByteStride);
                //    sw.WriteLine("Popis " + e.Popis);
                //    sw.WriteLine("StartX " + e.StartX);
                //    sw.WriteLine("StartY " + e.StartY);
                //    sw.WriteLine("Sum " + e.Sum);
                //    sw.WriteLine("TemperatureADCBol " + e.TemperatureADCBol);
                //    sw.WriteLine("TemperatureBol " + e.TemperatureBol);
                //    sw.WriteLine("TimeStamp " + e.TimeStamp);
                //    sw.WriteLine("ResolvedTrigger " + e.ResolvedTrigger);
                //    sw.WriteLine("TriggerA " + e.TriggerA);
                //    sw.WriteLine("TriggerB " + e.TriggerB);
                //    sw.WriteLine("Type " + e.Type);
                //    sw.WriteLine("Width " + e.Width);
                //}
            }
        }
        private void MojeKamera_NewObrazekBol(object sender, BitmapSource e)
        {
            show2D.Zobraz(e);

            if (SaveAll)
            {
                //uloz obrazek

                var cesta = Path.Combine(CestaSave, SaveNum + ".bmp");
                BitmapFrame bf = null;
                if (e != null)
                {
                    bf = BitmapFrame.Create(e);
                }
                using (var fileStream = new FileStream(cesta, FileMode.Create))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(bf);
                    encoder.Save(fileStream);
                }
                SaveNum++;
                if (SaveTimeout > 0) Thread.Sleep(SaveTimeout);
                if (SaveNum > SaveNumMax) SaveAll = false;
            }
        }

        private void MojeKamera_NewObrazekRaw(object sender, Tuple<byte[], byte[]> e)
        {
            if (SaveAllB)
            {
                //store images to queue. Having trouble with fucking memory? Fucking buy more, motherfucker
                StoreFIFO.Enqueue(e);
                SaveNum++;
                double done = 100.0 * SaveNum / SaveNumMax;
                Console.Write(done.ToString("F2") + "%" + "\r");
                if (SaveTimeout > 0) Thread.Sleep(SaveTimeout);
                if (SaveNum >= SaveNumMax)
                {
                    int last_fnum = -1;
                    SaveAllB = false;

                    Console.WriteLine("Storing images");
                    // store all the fucking images at once, cause i've crazy motherfucking superfast pc and you have a fucking shit. Deal with it, fucker.
                    int cnt = StoreFIFO.Count();

                    Task[] tasks = new Task[cnt];
                    for (int i = 0; i < cnt; i++)
                    {
                        Tuple<byte[], byte[]> img = StoreFIFO.Dequeue();
                        int imgnum = i;
                        tasks[i] = Task.Factory.StartNew(() =>
                        {
                            SaveImageThreaded(imgnum, CestaSave, img);
                        });
                    }
                    while (tasks.Any(t => !t.IsCompleted)) { } //spin wait

                    Console.WriteLine("Checking images");
                    // check dropped frames, just to be sure, that no fucker escaped
                    bool order_ok = true;
                    for (int i = 0; i < SaveNumMax; i++)
                    {
                        var path = Path.Combine(CestaSave, i.ToString("D6") + ".xml");
                        XmlReader reader = XmlReader.Create(@path);
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                if (reader.Name.ToString() == "FrameNumber")
                                {
                                    string sfn = reader.ReadString();
                                    int fnum;
                                    if (!int.TryParse(sfn, out fnum))
                                    {
                                        Console.WriteLine("Failed to read XML @" + i);
                                        continue;
                                    }
                                    if (last_fnum > 0)
                                    {
                                        if ((fnum - last_fnum) > 1)
                                        {
                                            Console.WriteLine((fnum - last_fnum - 1) + " frame(s) dropped @" + i + " between" + last_fnum + "/" + fnum);
                                        }
                                        if (fnum <= last_fnum)
                                        {
                                            Console.WriteLine("Ordering error @" + i);
                                            order_ok = false;
                                        }
                                    }
                                    last_fnum = fnum;
                                }
                            }
                        }
                    }
                    Console.WriteLine("Check done");
                    if (order_ok)
                    {
                        Console.Beep(440, 1000);
                    }
                    else
                    {
                        Console.Beep(4400, 1000);
                    }
                }
            }
        }

        public static void SaveImageThreaded(int savenum, string basepath, Tuple<byte[], byte[]> e)
        {
            var cesta = Path.Combine(basepath, savenum.ToString("D6") + ".xml");
            File.WriteAllBytes(cesta, e.Item1);
            cesta = Path.Combine(basepath, savenum.ToString("D6") + ".bin");
            File.WriteAllBytes(cesta, e.Item2);
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MojeKamera?.Close();
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                AppJCE.Properties.MujSettings.Default.Top = RestoreBounds.Top;
                AppJCE.Properties.MujSettings.Default.Left = RestoreBounds.Left;
                AppJCE.Properties.MujSettings.Default.Height = RestoreBounds.Height;
                AppJCE.Properties.MujSettings.Default.Width = RestoreBounds.Width;
                AppJCE.Properties.MujSettings.Default.Maximized = true;
            }
            else
            {
                AppJCE.Properties.MujSettings.Default.Top = this.Top;
                AppJCE.Properties.MujSettings.Default.Left = this.Left;
                AppJCE.Properties.MujSettings.Default.Height = this.Height;
                AppJCE.Properties.MujSettings.Default.Width = this.Width;
                AppJCE.Properties.MujSettings.Default.Maximized = false;
            }

            AppJCE.Properties.MujSettings.Default.Save();
        }


        private void MojeKamera_Connected(object sender, bool e)
        {
            //string[] s = NMI._AktivniPolozka?.Text.Split('\n');
            //foreach (var i in s)
            //{
            //    MojeKamera.CC.AddComand(i, Resp);
            //}
        }

        private void SlozkaListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //textBoxIn.Text = NMI._AktivniPolozka.Text;
            AktivujScrypt((MyInitScrypt)slozkaListBox2.SelectedItem);
            Start_Click(null, null);
        }

        private void SlozkaListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (slozkaListBox2.SelectedIndex < 0) return;
            AktivujScrypt((MyInitScrypt)slozkaListBox2.SelectedItem);
        }
        public void AktivujScrypt(MyInitScrypt mis)
        {
            NMI._AktivniPolozka = mis;
            textBoxIn.Text = mis?.Text;
        }


        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (!MojeKamera.Pripojeno)
            {
                Console.WriteLine("Kamera neni pripojena!");
                //MessageBox.Show("Kamera neni pripojena!");
                return;
            }
            //string[] s = NMI._AktivniPolozka?.Text.Split('\n');
            string[] s = textBoxIn.Text.Replace("\r", "").Split('\n');
            foreach (var i in s)
            {
                if (!string.IsNullOrEmpty(i))
                {
                    string[] ss = i.Split(' ');
                    switch (ss[0].ToLower())
                    {
                        case "getb":
                            var hb = new HodnotaByte() { Prikaz = new MyCommand() { Hodnota = string.Join(" ", ss, 2, ss.Length - 3), SubFix = " ", Prikaz = new MyCommand() { Hodnota = ss[1], SubFix = " " } } };
                            hb.CC = MojeKamera.CC;
                            try
                            {
                                var b = hb.HodnotaValid();
                                File.WriteAllBytes(Path.Combine(BaseCesta, ss.Last()), b);
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                        case "setb":
                            hb = new HodnotaByte() { Prikaz = new MyCommand() { Hodnota = string.Join(" ", ss, 2, ss.Length - 3), SubFix = " ", Prikaz = new MyCommand() { Hodnota = ss[1], SubFix = " " } } };
                            hb.CC = MojeKamera.CC;
                            try
                            {
                                var b = File.ReadAllBytes(Path.Combine(BaseCesta, ss.Last()));
                                hb.Hodnota = b;
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                        case "set":
                            try
                            {
                                if (ss.Length == 4 && ss[3].ToLower().Contains('x')) ss[3] = long.Parse(ss[3].Substring(2, ss[3].Length - 2), NumberStyles.HexNumber).ToString();
                                //if (ss.Length > 3 && ss[3].ToLower().Contains('x')) ss[3] = int.Parse(new string(s.Skip(2).ToArray()), NumberStyles.HexNumber).ToString();
                                string sss = string.Join(" ", ss);
                                MojeKamera.CC.AddComand(sss, delegate (ClientCommandResponse ccr) { if (ccr.Error) Console.WriteLine("ERR " + ccr.Text); });
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                        case "get":
                            MojeKamera.CC.AddComand(i, delegate (ClientCommandResponse ccr)
                            {
                                if (ccr.Error) Console.WriteLine("ERR " + ccr.Text);
                                else Console.WriteLine("OK " + ccr.Text);
                            });
                            break;
                        case "save":
                            try
                            {
                                CestaSave = ss[1];
                                if (!Directory.Exists(CestaSave)) Directory.CreateDirectory(CestaSave);
                                SaveNum = 0;
                                SaveNumMax = int.Parse(ss[2]);
                                SaveAll = true;
                                SaveAllB = false;
                                SaveTimeout = int.Parse(ss[3]);
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                        case "saveb":
                            try
                            {
                                CestaSave = ss[1];
                                if (!Directory.Exists(CestaSave)) Directory.CreateDirectory(CestaSave);
                                SaveNum = 0;
                                SaveNumMax = int.Parse(ss[2]);
                                Console.WriteLine("Original maximgcnt: " + MojeKamera.MaxImgCnt.ToString());
                                MojeKamera.MaxImgCnt = 2 * SaveNumMax;
                                Console.WriteLine("New maximgcnt: " + MojeKamera.MaxImgCnt.ToString());
                                SaveAll = false;
                                SaveAllB = true;
                                SaveTimeout = int.Parse(ss[3]);
                                StoreFIFO = new Queue<Tuple<byte[], byte[]>>(SaveNumMax);
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                        case "app":
                            try
                            {
                                switch (ss[1].ToLower())
                                {
                                    case "dmy":
                                        MojeKamera.Dummy = ss[2] == "1";
                                        break;
                                    case "cnv":
                                        MojeKamera.Convert = ss[2] == "1";
                                        break;
                                    case "ad":
                                        MojeKamera.AllData = ss[2] == "1";
                                        break;
                                    case "avg":
                                        MojeKamera.Average = int.Parse(ss[2]);
                                        break;
                                    case "clr":
                                        MojeKamera.Colored = ss[2] == "1";
                                        break;
                                    case "cor"://nic nedela
                                        MojeKamera.Corr = ss[2] == "1";
                                        break;
                                    case "color":
                                        if (uint.TryParse(ss[2], out uint barva))
                                        {
                                            if (MojeKamera.Obarvovac.ColorPalete.Seznam.Count > barva)
                                            {
                                                MojeKamera.Obarvovac.ColorPalete.AktivniPolozka = (int)barva;
                                                //MojeKamera.Obarvovac.SetColoring();//called automaticaly from change of AktivniPolozka
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (MojeKamera.Pripojeno) MojeKamera.Close();
            else MojeKamera.Connect();
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBoxIn.Text = File.ReadAllText(MojeCesta.VyberCestuLoad("Skript|*.init;*.txt|vše|*.*"));
            }
            catch
            {
                Console.WriteLine("Chyba souboru");
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            MyInitScrypt mls = new MyInitScrypt()
            {
                Jmeno = "MyInit" + PodScrypty.Count.ToString("0000"),
                Kategorie = slozkaListBox1.SelectedItem.ToString(),
                Cislo = PodScrypty.Count,
                Text = textBoxIn.Text,
            };
            NMI.Add(mls);
            SlozkaListBox1_SelectionChanged(null, null);
            slozkaListBox2.SelectedIndex = PodScrypty.Count - 1;
        }

        private void Rem_Click(object sender, RoutedEventArgs e)
        {
            NMI.Remove();
            NMI.Save();
            SelectionChanged();
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (slozkaListBox2.SelectedIndex >= 0 && slozkaListBox2.SelectedIndex < slozkaListBox2.Items.Count - 1)
            {
                var s = (from f in NMI.Seznam where f.Kategorie == slozkaListBox1.SelectedItem.ToString() select f).ToArray();
                NMI._AktivniPolozka.Cislo++;
                NMI.Seznam[NMI.Seznam.IndexOf(s[slozkaListBox2.SelectedIndex + 1])].Cislo--;
                PodScrypty.Move(slozkaListBox2.SelectedIndex, slozkaListBox2.SelectedIndex + 1);
                NMI.Save();
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if (slozkaListBox2.SelectedIndex > 0)
            {
                var s = (from f in NMI.Seznam where f.Kategorie == slozkaListBox1.SelectedItem.ToString() select f).ToArray();
                NMI._AktivniPolozka.Cislo--;
                NMI.Seznam[NMI.Seznam.IndexOf(s[slozkaListBox2.SelectedIndex - 1])].Cislo++;
                PodScrypty.Move(slozkaListBox2.SelectedIndex, slozkaListBox2.SelectedIndex - 1);
                NMI.Save();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (slozkaListBox2.SelectedIndex < 0) return;
            var w = new MyInitWindow();
            w.Add(NMI._AktivniPolozka);
            if (w.ShowDialog().Value)
            {
                try
                {
                    NMI._AktivniPolozka.Jmeno = w.Polozka.Jmeno;
                    NMI._AktivniPolozka.Kategorie = w.Polozka.Kategorie;
                    NMI._AktivniPolozka.Text = w.Polozka.Text;
                    NMI.Save();
                }
                catch (Exception ex) { Console.WriteLine("Nepovedlo se aktualizovat hodnotu\n" + ex.Message); }
                SelectionChanged();
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            var u = new ApplicInfra.Infra.Komponenty.CommandSelector();
            w.Content = u;
            u.NewCommand += U_NewCommand;
            u.Init(MojeKamera);
            w.Show();
        }

        private void U_NewCommand(object sender, string e)
        {
            textBoxIn.Text += e + "\n";
        }
    }
}
