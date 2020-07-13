using AppJCE.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraViewer
{
    public static class NastaveniInfraViewer
    {
        static MujInitSoubor Ini = new MujInitSoubor("NastaveniInfraViewer");

        #region Obecne
        static Hodnota _CestaSave = new HodnotaString() { Sekce = "Obecne", Jmeno = "CestaSave", Value = "" };
        public static string CestaSave
        {
            get { return (string)_CestaSave.Value; }
            set
            {
                if ((string)_CestaSave.Value != value)
                {
                    _CestaSave.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _ServerPort = new HodnotaInt() { Sekce = "Obecne", Jmeno = "ServerPort", Value = 36000 };
        public static int ServerPort
        {
            get { return (int)_ServerPort.Value; }
            set
            {
                if ((int)_ServerPort.Value != value)
                {
                    _ServerPort.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _FileNameSize = new HodnotaString() { Sekce = "Obecne", Jmeno = "FileNameSize", Value = "000000" };
        public static string FileNameSize
        {
            get { return (string)_FileNameSize.Value; }
            set
            {
                if ((string)_FileNameSize.Value != value)
                {
                    _FileNameSize.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _NazevObrazku = new HodnotaString() { Sekce = "Obecne", Jmeno = "NazevObrazku", Value = "image" };
        public static string NazevObrazku
        {
            get { return (string)_NazevObrazku.Value; }
            set
            {
                if ((string)_NazevObrazku.Value != value)
                {
                    _NazevObrazku.Value = value;
                    Save();
                }
            }
        }
        #endregion
        #region Interni
        static Hodnota _Pripona = new HodnotaString() { Sekce = "Interni", Jmeno = "Pripona", Value = ".bin" };
        public static string Pripona
        {
            get { return (string)_Pripona.Value; }
            set
            {
                if ((string)_Pripona.Value != value)
                {
                    _Pripona.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _PriponaRAW = new HodnotaString() { Sekce = "Interni", Jmeno = "PriponaRAW", Value = ".raw" };
        public static string PriponaRAW
        {
            get { return (string)_PriponaRAW.Value; }
            set
            {
                if ((string)_PriponaRAW.Value != value)
                {
                    _PriponaRAW.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _TextOn = new HodnotaString() { Sekce = "Interni", Jmeno = "TextOn", Value = "Online:" };
        public static string TextOn
        {
            get { return (string)_TextOn.Value; }
            set
            {
                if ((string)_TextOn.Value != value)
                {
                    _TextOn.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _TextOff = new HodnotaString() { Sekce = "Interni", Jmeno = "TextOff", Value = "Offline:" };
        public static string TextOff
        {
            get { return (string)_TextOff.Value; }
            set
            {
                if ((string)_TextOff.Value != value)
                {
                    _TextOff.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _ButtonText = new HodnotaString() { Sekce = "Interni", Jmeno = "ButtonText", Value = "Select" };
        public static string ButtonText
        {
            get { return (string)_ButtonText.Value; }
            set
            {
                if ((string)_ButtonText.Value != value)
                {
                    _ButtonText.Value = value;
                    Save();
                }
            }
        }
        #endregion
        #region Slozky
        static Hodnota _BolInit = new HodnotaString() { Sekce = "Slozky", Jmeno = "BolInit", Value = "BolInit" };
        public static string BolInit
        {
            get { return (string)_BolInit.Value; }
            set
            {
                if ((string)_BolInit.Value != value)
                {
                    _BolInit.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _NastaveniKamery = new HodnotaString() { Sekce = "Slozky", Jmeno = "NastaveniKamery", Value = "NastaveniKamery.ini" };
        public static string NastaveniKamery
        {
            get { return (string)_NastaveniKamery.Value; }
            set
            {
                if ((string)_NastaveniKamery.Value != value)
                {
                    _NastaveniKamery.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _Saves = new HodnotaString() { Sekce = "Slozky", Jmeno = "Saves", Value = "Saves" };
        public static string Saves
        {
            get { return (string)_Saves.Value; }
            set
            {
                if ((string)_Saves.Value != value)
                {
                    _Saves.Value = value;
                    Save();
                }
            }
        }
        #endregion
        #region Hlasky
        static Hodnota _NastaveniIP = new HodnotaString() { Sekce = "Hlasky", Jmeno = "NastaveniIP", Value = "Chyba nastaveni IP" };
        public static string NastaveniIP
        {
            get { return (string)_NastaveniIP.Value; }
            set
            {
                if ((string)_NastaveniIP.Value != value)
                {
                    _NastaveniIP.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _KameraNepripojena = new HodnotaString() { Sekce = "Hlasky", Jmeno = "KameraNepripojena", Value = "Kamera neni pripojena!" };
        public static string KameraNepripojena
        {
            get { return (string)_KameraNepripojena.Value; }
            set
            {
                if ((string)_KameraNepripojena.Value != value)
                {
                    _KameraNepripojena.Value = value;
                    Save();
                }
            }
        }
        static Hodnota _ServerNepripojen = new HodnotaString() { Sekce = "Hlasky", Jmeno = "ServerNepripojen", Value = "Muj server closed\n" };
        public static string ServerNepripojen
        {
            get { return (string)_ServerNepripojen.Value; }
            set
            {
                if ((string)_ServerNepripojen.Value != value)
                {
                    _ServerNepripojen.Value = value;
                    Save();
                }
            }
        }
        #endregion









        

        /// <summary>
        /// PRIDAT VSECHNY PROMENE .. pro ukladani a nacitani
        /// </summary>
        public static List<Hodnota> Hodnoty = new List<Hodnota>() { _CestaSave, _ServerPort, _FileNameSize, _NazevObrazku, _Pripona, _PriponaRAW, _BolInit, _NastaveniKamery, _Saves, _ButtonText, _TextOff, _TextOn, _NastaveniIP, _KameraNepripojena, _ServerNepripojen };

        /// <summary>
        /// Uloží Hodnoty
        /// </summary>
        public static void Save()
        {
            if (Hodnoty == null) return;
            Nini.Config.IConfigSource source = Ini.InitSoubor();
            source.Configs.Clear();
            foreach (Hodnota h in Hodnoty)
            {
                if (h != null)
                {
                    Nini.Config.IConfig config = source.Configs[h.Sekce];
                    if (config == null) config = source.AddConfig(h.Sekce);
                    if (config != null)
                    {
                        config.Set(h.Jmeno, h.Value);
                    }
                }
            }
            source.Save();
        }
        /// <summary>
        /// Nacte hodnoty ze souboru
        /// </summary>
        public static void Load()
        {
            if (Hodnoty == null) return;
            //nacte hodnoty
            Nini.Config.IConfigSource source = Ini.InitSoubor();
            foreach (Hodnota h in Hodnoty)
            {
                Nini.Config.IConfig config = source.Configs[h.Sekce];
                if (config == null) config = source.AddConfig(h.Sekce);
                if (config != null)
                {
                    if (config.Contains(h.Jmeno))
                    {
                        h.Refresh(config);
                    }
                    else
                    {
                        config.Set(h.Jmeno, h.Value);
                    }
                }
            }
            source.Save();
        }
    }
}
