using ApplicInfra.Infra;
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

namespace EasyCC
{
    /// <summary>
    /// Interaction logic for ImageInfo.xaml
    /// </summary>
    public partial class ImageInfo : UserControl
    {
        ImageHeader<double> IH = new ImageHeader<double>();
        public ImageInfo()
        {
            InitializeComponent();
            DataContext = IH;
        }
        public void Set(ImageHeader<double> ih)
        {
            IH.Copy(ih);
            IH.Average = Math.Round(ih.Sum / ih.Width / ih.Height, 2);
            //IH.Average = ih.Sum / 307200;
        }
    }
}
