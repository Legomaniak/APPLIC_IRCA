using AppJCE.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCC
{
    public class NastaveniMyInit : SlozkaControlString<MyInitScrypt>
    {
        public NastaveniMyInit(string Cesta) : base("BolInit", ".init", Cesta)
        {

        }
        public override void SetDefault()
        {
        }
    }
}
