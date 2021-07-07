using ApplicInfra.Infra;
using ApplicInfra.Save;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfraViewer
{
    public class MujServer
    {
        TcpListener server;
        BackgroundWorker posluchac;
        bool Konec = false;
        public MujServer()
        {
            server = new TcpListener(IPAddress.Loopback, NastaveniInfraViewer.ServerPort);
            server.Start();
            posluchac = new BackgroundWorker();
            posluchac.DoWork += poslouchej; 
            posluchac.RunWorkerAsync();
        }

        private void poslouchej(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (!Konec)
                {
                    ClientWorking cw = new ClientWorking(server.AcceptTcpClient());
                    cw.Init(MW);
                    new Thread(new ThreadStart(cw.DoSomethingWithClient)).Start();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(NastaveniInfraViewer.ServerNepripojen + ex.Message);
            }
        }
        public void Close()
        {
            server.Stop();
            Konec = true;
        }
        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
        }
    }
    public class ClientWorking
    {
        private Stream ClientStream;
        private TcpClient Client;

        public ClientWorking(TcpClient Client)
        {
            this.Client = Client;
            ClientStream = Client.GetStream();
        }

        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
        }

        public void DoSomethingWithClient()
        {
            try
            {
                StreamWriter sw = new StreamWriter(ClientStream) { AutoFlush = true };
                //StreamReader sr = new StreamReader(sw.BaseStream);
                StreamReader sr = new StreamReader(ClientStream);
                sw.WriteLine("Pripojeno");
                //sw.Flush();
                while (true)
                {
                    string inputLine = "";
                    while (inputLine != null)
                    {
                        inputLine = sr.ReadLine();
                        sw.WriteLine(Zpracuj(inputLine));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Server was disconnect from client.");
                Console.WriteLine(e.Message);
            }
        }
        Stopwatch SW = new Stopwatch();
        public string Zpracuj(string text)
        {
            SW.Restart();
            string odpovedOk = "OK;";
            string odpovedErr = "ERR;";

            if (MW.MojeKamera == null || text == null || text == "") return odpovedErr + "Chyba inicializace";
            string[] s = text.Split(' ');
            if (s.Length >= 1)
            {
                try
                {
                    switch (s[0])
                    {
                        case "IMG":
                            MW.Obrazek = null;
                            SpinWait.SpinUntil(() => MW.Obrazek != null, 5000);
                            if(MW.Obrazek==null)return odpovedErr + SW.ElapsedMilliseconds;
                            MW.Save(null, null);
                            return odpovedOk + SW.ElapsedMilliseconds;
                        case "IMGR":
                            MW.MojeKamera.ResetKonvert();
                            MW.Obrazek = null;
                            SpinWait.SpinUntil(() => MW.Obrazek != null, 5000);
                            if (MW.Obrazek == null) return odpovedErr + SW.ElapsedMilliseconds;
                            MW.Save(null, null);
                            return odpovedOk + SW.ElapsedMilliseconds;
                        case "MIR":
                            //MW.motor.SetPozice(float.Parse(s[2]));
                            return odpovedOk + SW.ElapsedMilliseconds;
                        case "SCAN":
                            int i = 2;
                            float p1= float.Parse(s[i++]);
                            float p2 = float.Parse(s[i++]);
                            float k = float.Parse(s[i++]);
                            int t = int.Parse(s[i++]);
                            int l = (int)((p2 - p1) / k);
                            for (i = 0; i < l; i++)
                            {
                                //MW.motor.SetPozice((p1 + k * j));
                                MW.Obrazek = null;
                                SpinWait.SpinUntil(() => false, t);
                                SpinWait.SpinUntil(() => MW.Obrazek != null, 5000);
                                MW.Save(null, null);
                            }
                            return odpovedOk + SW.ElapsedMilliseconds;
                        case "SET":
                            switch (s[1])
                            {
                                case "RAW":
                                    MW.SaveRaw = bool.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "INT":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerINT).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "GSK":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerGSK).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "GFD":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerGFID).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "VBS":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerVBUS).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "VDT":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerVDET).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "AVG":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.NUCcomputationFrames).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "OFF":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.NUCzeroOffset).Hodnota = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "COC":
                                    MW.Korekce?.Send(1);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "COK":
                                    MW.corr?.ControlCorrection_Click(null, null);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "SE":
                                    MW.MojeKamera.MojeHodnoty.Get(CameraValuesBool.ShutterEnable).Hodnota = bool.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                //case "TE":
                                //    MW.mcb1.Hodnota.Hodnota = bool.Parse(s[2]);
                                //    return odpovedOk + SW.ElapsedMilliseconds;
                                case "KC":
                                    MW.MojeKamera.Convert = bool.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "KK":
                                    MW.MojeKamera.Colored = bool.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "KD":
                                    MW.MojeKamera.Dummy = bool.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "CS":
                                    string cesticka = "";
                                    i = 2;
                                    do
                                    {
                                        cesticka += s[i++];
                                    } while (i < s.Length);
                                    NastaveniInfraViewer.CestaSave = cesticka;
                                    MW.cc.VybranaCesta = cesticka;
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "COR":
                                    if (bool.Parse(s[2]))
                                    {
                                        MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt3.ImageDestination).Hodnota = new Tuple<long, long, long>((long)EDestination1.SensorCapture, (long)EDestination2.Accelerator, (long)EDestination3.Move);
                                        MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt3.ImageDestination).Hodnota = new Tuple<long, long, long>((long)EDestination1.Accelerator, (long)EDestination2.Ethernet, (long)EDestination3.Move);
                                        MW.MojeKamera.MojeHodnoty.Get(CameraValuesBit.ImageUpdateSetup).Hodnota = 1;
                                    }
                                    else
                                    {
                                        MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt3.ImageDestination).Hodnota = new Tuple<long, long, long>((long)EDestination1.SensorCapture, (long)EDestination2.Ethernet, (long)EDestination3.Move);
                                        MW.MojeKamera.MojeHodnoty.Get(CameraValuesBit.ImageUpdateSetup).Hodnota = 1;
                                    }
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                case "IA":
                                    MW.MojeKamera.Average = int.Parse(s[2]);
                                    return odpovedOk + SW.ElapsedMilliseconds;
                                default:
                                    return odpovedErr + "Neplatny parametr";
                            }
                        case "GET":
                            switch (s[1])
                            {
                                case "ON":
                                    return odpovedOk + MW.MojeKamera.Pripojeno;
                                case "INT":                                    
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerINT).Hodnota;
                                case "GSK":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerGSK).Hodnota;
                                case "GFD":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerGFID).Hodnota;
                                case "VBS":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerVBUS).Hodnota;
                                case "VDT":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.BolometerVDET).Hodnota;
                                case "AVG":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.NUCcomputationFrames).Hodnota;
                                case "OFF":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.NUCzeroOffset).Hodnota;
                                case "SE":
                                    return odpovedOk + MW.MojeKamera.MojeHodnoty.Get(CameraValuesBool.ShutterEnable).Hodnota;
                                //case "TE":
                                //    return odpovedOk + MW.mcb1.Hodnota.Hodnota;
                                case "KC":
                                    return odpovedOk + MW.MojeKamera.Convert;
                                case "KK":
                                    return odpovedOk + MW.MojeKamera.Colored;
                                case "KD":
                                    return odpovedOk + MW.MojeKamera.Dummy;
                                case "KA":
                                    return odpovedOk + MW.MojeKamera.Average;
                                case "RAW":
                                    return odpovedOk + MW.SaveRaw;
                                //case "COR":
                                //    return odpovedOk + (MW.MojeKamera.MojeHodnoty.Get(CameraValuesInt.Destination).Hodnota == 32);
                                case "CS":
                                    return odpovedOk + MW.cc.VybranaCesta;
                                default:
                                    return odpovedErr + "Neplatny parametr";
                            }
                        default:
                            return odpovedErr + "Neplatny parametr";
                    }
                }
                catch(Exception e)
                {
                    return odpovedErr + e.Message;
                }
            }
            else return odpovedErr + "Neplatna delka prikazu";
        }
    }
}
