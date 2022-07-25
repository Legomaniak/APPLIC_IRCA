using AppJCE.Barvy;
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
using System.Windows.Shapes;

namespace InfraViewer
{
    /// <summary>
    /// Interakční logika pro ImgLoader.xaml
    /// </summary>
    public partial class ImgLoader : Window
    {
        Obarvovac o,oGW;
        byte[] Obrazek = null;
        public short[,] ObrazekDecoded = null;
        int ObrazekX, ObrazekY;
        public ImgLoader()
        {
            InitializeComponent();
            o = new Obarvovac();
            oGW = new Obarvovac() { AutoColor = true };

            foreach (var CP in o.ColorPalete.Seznam)
            {
                int delka = 100;
                int vyska = 25;
                if (delka < 1) return;
                byte[] display_buffer = new byte[delka * 4 * vyska];
                for (int i = 0; i < delka; i++)
                {
                    byte[] resp = CP.getColor((double)i / delka);
                    display_buffer[i * 4] = resp[0];
                    display_buffer[i * 4 + 1] = resp[1];
                    display_buffer[i * 4 + 2] = resp[2];
                    display_buffer[i * 4 + 3] = resp[3];
                    for (int j = 1; j < vyska; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            display_buffer[i * 4 + j * delka * 4 + k] = display_buffer[i * 4 + k];
                        }
                    }
                }
                PixelFormat pixelFormat = PixelFormats.Bgr32;
                int rawStride = (delka * pixelFormat.BitsPerPixel + 7) / 8;

                BitmapSource bmpsrc = WriteableBitmap.Create(delka, vyska, 96, 96, pixelFormat, null, display_buffer, rawStride);
                bmpsrc.Freeze();

                var strip = new Image() { Source = bmpsrc, Stretch = Stretch.Fill, Height = vyska, Width = delka };
                //strip.Source = bmpsrc;
                //strip.Stretch = Stretch.Fill;

                cbColor.Items.Add(strip);
            }
        }
        public void Show(byte[] obrazek, int X, int Y)
        {
            if (obrazek == null) return;
            Obrazek = obrazek;
            ObrazekX = X;
            ObrazekY = Y;

            ObrazekDecoded = o.ColorPalete._AktivniPolozka.getColorI(obrazek, Y, X);
            oGW.PrumerujMinMax(ObrazekDecoded);
            oGW.Obarvy();
            this.obrazek.Source = oGW.GetObraz(ObrazekDecoded);
        }

        private void ComboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbColor.SelectedIndex < 0) return;
            o.ColoringDone = false;
            o.ColorPalete.AktivniPolozka = cbColor.SelectedIndex;
            System.Threading.SpinWait.SpinUntil(() => o.ColoringDone);
            Show(Obrazek, ObrazekX, ObrazekY);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
