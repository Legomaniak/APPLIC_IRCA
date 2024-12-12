using AppJCE;
using AppJCE.Barvy;
using ApplicHyper.Hyper;
using ImageMagick;
using SharpAvi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace InfraViewer
{
    public class MyImage<T>
    {
        public string Name { get; set; }
        public int ResX, ResY;
        public T[,] Data;
    }
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<MyImage<short>> Poles
        {
            get { return _Poles; }
            set { _Poles = value; OnPropertyChanged("Poles"); }
        }
        ObservableCollection<MyImage<short>> _Poles = null;
        Obarvovac O;
        public int FPS
        {
            get { return _FPS; }
            set { _FPS = value; OnPropertyChanged("FPS"); }
        }
        int _FPS = 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            O = new Obarvovac() { AutoColor = true };
            obrazekView.Init(O);
            Poles = new ObservableCollection<MyImage<short>>();
            MouseMove += MainWindow_MouseMove;
            AllowDrop = true;
            Drop += MainWindow_Drop;

            var b = new Binding("FPS") { Mode = BindingMode.TwoWay, Source = this };
            tb.SetBinding(TextBox.TextProperty, b);

            if(Environment.GetCommandLineArgs().Length>0)
            {
                foreach (var f in Environment.GetCommandLineArgs().Skip(1))
                {
                    Load(f, true);
                }
            }
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            foreach (var f in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                Load(f, true);
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            int X = 0, Y = 0;

            var p = obrazekView.GetPoint(e);
            X = p.X;
            Y = p.Y;

            string s = X == -1 ? "" : "X: " + X + " Y: " + Y;
            lCoord.Content = s;
        }

        private void loadBin(object sender, RoutedEventArgs e)
        {
            string[] cesta = MojeCesta.VyberCestuLoadPole("Binarka (*.bin)|*.bin");
            if (cesta == null) return;
            foreach (var c in cesta) Load(c);
            LB.SelectedIndex = Poles.Count()-1;
        }

        private void Load(string cesta, bool autoDecode=false)
        {
            if (string.IsNullOrEmpty(cesta)) return;
            try
            {
                switch (Path.GetExtension(cesta).ToLower())
                {
                    case ".bin":
                        try
                        {
                            var pole = new MyImage<short>();
                            using (var readBinary = new BinaryReader(new FileStream(cesta, FileMode.Open)))
                            {
                                pole.Name = Path.GetFileNameWithoutExtension(cesta);
                                var X = readBinary.ReadInt32();
                                var Y = readBinary.ReadInt32();
                                pole.Data = new short[X, Y];
                                pole.ResX = X;
                                pole.ResY = Y;
                                for (int x = 0; x < X; x++)
                                    for (int y = 0; y < Y; y++)
                                        pole.Data[x, y] = readBinary.ReadInt16();
                                //pole.Data[x, y] = readBinary.ReadDouble();
                            }
                            Poles.Add(pole);
                        }
                        catch
                        {
                            var pole = new MyImage<short>();
                            using (var readBinary = new BinaryReader(new FileStream(cesta, FileMode.Open)))
                            {
                                pole.Name = Path.GetFileNameWithoutExtension(cesta);
                                var ImageNumber = readBinary.ReadInt32();
                                var Y = readBinary.ReadInt32();
                                var X = readBinary.ReadInt32();
                                var TemperatureADCBol = readBinary.ReadInt32();
                                var ImageFlags = readBinary.ReadInt32();
                                pole.Data = new short[X, Y];
                                pole.ResX = X;
                                pole.ResY = Y;
                                for (int y = 0; y < Y; y++)
                                    for (int x = 0; x < X; x++)
                                        pole.Data[x, y] = readBinary.ReadInt16();
                            }
                            Poles.Add(pole);
                        }
                        break;
                    case ".hdr":
                        HyperCubeShort HCS = new HyperCubeShort();
                        HCS.Load(cesta);
                        for (int i = 0; i < HCS.X; i++)
                        {
                            var p = new MyImage<short>();
                            p.Name = HCS.Header.CubeName + i;
                            //TODO:spravny rez?
                            p.Data = HCS.Get(ECutDir.CutDirX, i, false);
                            p.ResX = p.Data.GetLength(0);
                            p.ResY = p.Data.GetLength(1);
                            Poles.Add(p);
                        }
                        break;
                    default:
                        BitmapSource bs = new BitmapImage(new Uri(cesta));
                        byte[] b = new byte[bs.PixelWidth * bs.PixelHeight * 4];
                        bs.CopyPixels(b, bs.PixelWidth * 4, 0);
                        short[,] ObrazekDecoded = null;
                        var w = new ImgLoader();
                        if (autoDecode)
                        {
                            foreach (var CP in w.o.ColorPalete.Seznam)
                            {
                                var res = CP.getColorI(b, bs.PixelWidth, bs.PixelHeight);
                                if(res.Item1)
                                {
                                    w.ObrazekDecoded = res.Item2;
                                    break;
                                }
                            }
                        }
                        if (!autoDecode || ObrazekDecoded == null)
                        {
                            w.Show(b, bs.PixelWidth, bs.PixelHeight);
                            w.ShowDialog();
                        }
                        Poles.Add(new MyImage<short>() { Name = Path.GetFileNameWithoutExtension(cesta), Data = w.ObrazekDecoded, ResX = w.ObrazekDecoded.GetLength(0), ResY = w.ObrazekDecoded.GetLength(1) });
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private void loadCube(object sender, RoutedEventArgs e)
        {
            string[] cesta = MojeCesta.VyberCestuLoadPole("Cube header|*.hdr");
            if (cesta == null) return;
            foreach (var c in cesta) Load(c);
            LB.SelectedIndex = Poles.Count() - 1;
        }

        private void buttonProcessAvg(object sender, RoutedEventArgs e)
        {
            if (LB.SelectedItems.Count < 2) return;
            bool stejne = true;
            for (int i = 1; i < LB.SelectedItems.Count; i++)
            {
                if (!stejne) break;
                if ((LB.SelectedItems[i] as MyImage<short>).ResX != (LB.SelectedItems[0] as MyImage<short>).ResX) stejne = false;
                if ((LB.SelectedItems[i] as MyImage<short>).ResY != (LB.SelectedItems[0] as MyImage<short>).ResY) stejne = false;
            }
            if (!stejne)
            {
                MessageBox.Show("Selected images with different size");
                return;
            }

            var X = (LB.SelectedItems[0] as MyImage<short>).ResX;
            var Y = (LB.SelectedItems[0] as MyImage<short>).ResY;
            var pole = new short[X, Y];
            for (int x = 0; x < X; x++)
                for (int y = 0; y < Y; y++)
                {
                    double s = 0;
                    for (int p = 0; p < LB.SelectedItems.Count; p++)
                    {
                        s += (LB.SelectedItems[p] as MyImage<short>).Data[x, y];
                    }
                    s /= Poles.Count;
                    pole[x, y] = (short)s;
                }
            obrazekView.Zobraz(pole);
        }

        private void buttonProcessCorr(object sender, RoutedEventArgs e)
        {
            //TODO: load corr
            int corrX = 648;
            int corrY = 480;
            var corr = new bool[corrX, corrY];
            int corrType = 0;

            foreach (MyImage<short> mi in LB.SelectedItems)
            {
                if (mi.ResX >= corrX || mi.ResY >= corrY) continue;
                switch (corrType)
                {
                    case 1:
                        //korekce z okoli pixelu
                        for (int x = 0; x < corrX; x++)
                        {
                            for (int y = 0; y < corrY; y++)
                            {
                                if (corr[x, y])
                                {
                                    int nv = 0;
                                    int i = 0;
                                    for (int xx = -1; xx < 1; xx++)
                                    {
                                        for (int yy = -1; yy < 1; yy++)
                                        {
                                            var nx = x + xx;
                                            var ny = y + yy;
                                            if (nx >= 0 && nx < corrX && ny >= 0 && ny < corrY&&nx!=ny)
                                            {
                                                if (!corr[nx, ny])
                                                {
                                                    i++;
                                                    nv += mi.Data[nx, ny];
                                                }
                                            }
                                        }
                                    }
                                    if(i>0)
                                    {
                                        nv /= i;
                                        mi.Data[x, y] = (short)nv;
                                    }
                                }
                            }
                        }
                        break;
                    case 0:
                    default:
                        //predchozi pixel
                        if (corr[0, 0])
                        {
                            mi.Data[0, 0] = mi.Data[1, 0];
                        }
                        for (int y = 1; y < corrY; y++)
                        {
                            if (corr[0, y])
                            {
                                mi.Data[0, y] = mi.Data[0, y - 1];
                            }
                        }
                        for (int x = 1; x < corrX; x++)
                        {
                            for (int y = 0; y < corrY; y++)
                            {
                                if (corr[x, y])
                                {
                                    mi.Data[x, y] = mi.Data[x - 1, y];
                                }
                            }
                        }
                        break;
                }
            }           
        }

        private void buttonSaveCube(object sender, RoutedEventArgs e)
        {
            try
            {
                HyperCubeShort HCS = new HyperCubeShort();
                if (LB.SelectedItems.Count > 1)
                {
                    HCS.DataCopy((from p in LB.SelectedItems as IList<MyImage<short>> select p.Data).ToArray());
                }
                else
                {
                    HCS.DataCopy((from p in Poles select p.Data).ToArray());
                }
                HCS.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void buttonSaveBin(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LB.SelectedItems.Count > 1)
                {
                    string cesta = MojeCesta.VyberCestuFolder();
                    foreach (MyImage<short> o in LB.SelectedItems)
                    {
                        using (var wb = new BinaryWriter(new FileStream(Path.Combine(cesta,o.Name+".bin"), FileMode.Create)))
                        {
                            var X = o.ResX;
                            wb.Write(X);
                            var Y = o.ResY;
                            wb.Write(Y);
                            for (int x = 0; x < X; x++)
                            {
                                for (int y = 0; y < Y; y++)
                                {
                                    wb.Write(o.Data[x, y]);
                                }
                            }
                        }
                    }                    
                }
                else
                {
                    string cesta = MojeCesta.VyberCestuSave("obrazek", "Binary (*.bin)|*.bin");
                    var Obrazek = obrazekView.Obrazek;
                    using (var wb = new BinaryWriter(new FileStream(cesta, FileMode.Create)))
                    {
                        var X = Obrazek.GetLength(0);
                        wb.Write(X);
                        var Y = Obrazek.GetLength(1);
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
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void buttonSaveBmp(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LB.SelectedItems.Count > 1)
                {
                    string cesta = MojeCesta.VyberCestuFolder();
                    foreach (MyImage<short> o in LB.SelectedItems)
                    {
                        var bf = BitmapFrame.Create(O.GetObraz(o.Data));
                        using (var fileStream = new FileStream(Path.Combine(cesta, o.Name + ".bmp"), FileMode.Create))
                        {
                            BitmapEncoder encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(bf);
                            encoder.Save(fileStream);
                        }
                    }
                }
                else
                {
                    string cesta = MojeCesta.VyberCestuSave("obrazek", "Bitmap (*.bmp)|*.bmp");
                    var bs = obrazekView.GetImage();
                    if (bs == null) return;
                    var bf = BitmapFrame.Create(bs);
                    using (var fileStream = new FileStream(cesta, FileMode.Create))
                    {
                        BitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(bf);
                        encoder.Save(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB.SelectedIndex < 0)
            {
                obrazekView.Clear();
                return;
            }
            obrazekView.ZobrazWait(Poles[LB.SelectedIndex].Data);
        }

        private void clear(object sender, RoutedEventArgs e)
        {
            if(LB.SelectedItems.Count>0)
            {
                var p = new List<MyImage<short>>();
                foreach (MyImage<short> i in LB.SelectedItems) p.Add(i);
                foreach (var i in p) Poles.Remove(i);
            }
            else
            {
                Poles.Clear();
            }
        }

        private void loadImg(object sender, RoutedEventArgs e)
        {
            string[] cesta = MojeCesta.VyberCestuLoadPole("Obrázek|*.jpg;*.png;*.bmp;*.jpeg|Vše|*.*");
            if (cesta == null) return;
            foreach (var c in cesta) Load(c);
            LB.SelectedIndex = Poles.Count() - 1;
        }

        private void buttonSaveGif(object sender, RoutedEventArgs e)
        {
            if (LB.SelectedItems.Count > 1)
            {
                var gEnc = new GifBitmapEncoder();
                string cesta = MojeCesta.VyberCestuSave("obrazek", "GIF (*.gif)|*.gif");

                using (MagickImageCollection collection = new MagickImageCollection())
                {
                    foreach (MyImage<short> o in LB.SelectedItems)
                    {
                        var img = new MagickImage(O.GetData(o.Data), new MagickReadSettings() { Format = MagickFormat.Bgra, Width = (uint)o.ResX, Height = (uint)o.ResY });
                        collection.Add(img);
                        collection[collection.Count - 1].AnimationDelay = (uint)(FPS > 0 ? 1000 / FPS : 1000);
                    }

                    // Optionally reduce colors
                    QuantizeSettings settings = new QuantizeSettings();
                    settings.Colors = 256;//TODO: podpora Gray16
                    collection.Quantize(settings);

                    // Optionally optimize the images (images should have the same size).
                    collection.Optimize();

                    collection.Write(cesta);
                }
            }
        }

        private void buttonSaveVideo(object sender, RoutedEventArgs e)
        {
            if (LB.SelectedItems.Count < 1) return;

            string cesta = MojeCesta.VyberCestuSave("video", "AVI (*.avi)|*.avi");
            var aw = new SharpAvi.Output.AviWriter(cesta);
            if (FPS == 0) FPS = 25;
            aw.FramesPerSecond = FPS;
            var aviStream = aw.AddVideoStream();
            var po = LB.SelectedItems[0] as MyImage<short>;
            aviStream.Width = po.ResX;// width;
            aviStream.Height = po.ResY;// height;
                                       // class SharpAvi.KnownFourCCs.Codecs contains FOURCCs for several well-known codecs
                                       // Uncompressed is the default value, just set it for clarity
            //aviStream.Codec = KnownFourCCs.Codecs.Uncompressed;
            aviStream.Codec = CodecIds.Uncompressed;
            // Uncompressed format requires to also specify bits per pixel
            aviStream.BitsPerPixel = BitsPerPixel.Bpp32;
            var i = 0;
            foreach (MyImage<short> o in LB.SelectedItems)
            {
                byte[] data = O.GetData(o.Data);
                //obrátí data pro uložení, aby se zobrazovala správně
                //int a = 0, b = 0;
                //for (int j = 0; j < height; j++)
                //    for (int i = 0; i < width; i++)
                //    {
                //        a = j * width * 4 + i * 4;
                //        b = (height - 1 - j) * width * 4 + i * 4;
                //        data[b] = data2[a];
                //        data[b + 1] = data2[a + 1];
                //        data[b + 2] = data2[a + 2];
                //        data[b + 3] = data2[a + 3];
                //    }
                // write data to a frame
                aviStream.WriteFrame(i % 25 == 0, // is key frame? (many codecs use concept of key frames, for others - all frames are keys)
                                       data, // array with frame data
                                       0, // starting index in the array
                                       data.Length // length of the data
                                       );

            }
            aw.Close();
        }

        private void buttonSaveImg(object sender, RoutedEventArgs e)
        {
            //ulozeni zobrazeneho obrazku
        }
    }
}
