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

namespace InfraViewer
{
    /// <summary>
    /// Interaction logic for ImageInfo.xaml
    /// </summary>
    public partial class ImageInfo : UserControl
    {
        ImageHeader<short> IH = new ImageHeader<short>();
        public ImageInfo()
        {
            InitializeComponent();
            DataContext = IH;
        }
        public void Set(ImageHeader<short> ih)
        {
            IH.Copy(ih);
            //IH.Average = Math.Round(ih.Sum / ih.Width / ih.Height, 2);
            //IH.Average = ih.Sum / 307200;
        }
    }
}
