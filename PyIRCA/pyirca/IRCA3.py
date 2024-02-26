import numpy as np
import socket # for socket 
import sys 
import threading
import xml.etree.ElementTree as ET
import asyncio
import time
import scipy.optimize as optimize
import matplotlib.pyplot as plt
from enum import Enum

Default_IP = "192.168.0.10"
BUFFER_SIZE = 1024

#class Command:
#    def __init__(self,Value,Note,IsSet,IsGet,Min,Max):
#        self.Value=Value
#        self.Note=Note
#        self.IsSet=IsSet
#        self.IsGet=IsGet
#        self.Min=Min
#        self.Max=Max
        
class Header:
    def __init__(self,source):
        self.Source=source
        try:
            root = ET.fromstring(source)
        except:
            print("Wrong xml header")
            print(source)
            raise Exception("xml header has wrong format")
        self.DecodeXML(root) 
        
    def DecodeXML(self,root):
        self.CameraID=root[0][0].text
        self.DNA=root[0][1].text
        self.Firmware=root[0][2].text
        self.HWResolutionX=root[0][3].text
        self.HWResolutionY=root[0][4].text
        
        self.Minimum=int(root[1][0].text)
        self.Maximum=int(root[1][1].text)
        self.Sum=int(root[1][2].text)
        self.ColoringMinimum=int(root[1][3].text)
        self.ColoringMaximum=int(root[1][4].text)
        self.FrameNumber=int(root[1][5].text)
        self.TimeStamp=root[1][6].text
        self.TemperatureADCBol=int(root[1][7].text)
        
        self.BitsPerPixel=int(root[2][0].text)
        self.PixelByteStride=int(root[2][1].text)
        self.PixelBitStride=int(root[2][2].text)
        self.StartX=int(root[2][3].text)
        self.StartY=int(root[2][4].text)
        self.Width=int(root[2][5].text)
        self.Height=int(root[2][6].text)
        self.ByteSize=int(root[2][7].text)
        self.ImageFlags=root[2][8].text
        
        self.TriggerA=root[3].text
        self.TriggerB=root[4].text
        self.ResolvedTrigger=root[5].text

class CameraSettings:
    GSK = 424
    GFID = 4095
    INT = 306
    VBUS = 2613
    VDET = 3071
    AVG = 4
    OFF = 4096
    GMS = 3
    
    def ToString(self):
        return "GSK = " + str(self.GSK)+"\nGFID = " + str(self.GFID) + "\nINT = " + str(self.INT) + "\nVBUS = " + str(self.VBUS) + "\nVDET = " + str(self.VDET) + "\nAVG = " + str(self.AVG) + "\nOFF = " + str(self.OFF) + "\nGMS = " + str(self.GMS)
    
    def DecodeXML(self,root):
        self.GSK=int(root[0].text)
        self.GFID=int(root[1].text)
        self.VDET=int(root[2].text)
        self.VBUS=int(root[3].text)
        self.GMS=int(root[4].text)
        
    def EncodeXML(self,root):        
        subel = ET.SubElement(root,"GSK")
        subel.text = str(self.GSK)
        subel = ET.SubElement(root,"GFID")
        subel.text = str(self.GFID)
        subel = ET.SubElement(root,"VDET")
        subel.text = str(self.VDET)
        subel = ET.SubElement(root,"VBUS")
        subel.text = str(self.VBUS)
#         subel = ET.SubElement(config,"AVG")
#         subel.text = str(self.AVG)
#         subel = ET.SubElement(config,"OFF")
#         subel.text = str(self.OFF)
        subel = ET.SubElement(root,"GMS")
        subel.text = str(self.GMS)
        
        
def MyHist(data):
    N=16385
    h=np.zeros(N+1)
    for x in range(data.shape[0]):
        for y in range(data.shape[1]):
            d=data[x,y]
            if d>N:
                d=N
            h[d]=h[d]+1
    return h
    
class StreamMode(Enum):
    RAW = 1
    CORR = 2
    RAW_F = 3
    CORR_F = 4

class Camera:
    NewImage = asyncio.Event()
    Error = asyncio.Event()
    Lock = threading.Lock()
    
    def __init__(self):
        self.Connected = False
        
    def Connect(self,IP):  
        self.CC = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
        self.SC = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
        #self.CC.setblocking(True)
        self.CC.settimeout(1)
        #self.SC.setblocking(True)
        self.SC.settimeout(2)
        self.IP = IP
        self.CC.connect((IP, 33000)) 
        self.SC.connect((IP, 34000)) 
        self.Connected = True
        
        #self.thread = threading.Thread(target=self.__ImgLoop, args=())
        #self.thread.daemon = True   
    
    def Reconnect(self):
        #if self.Connected:
        self.CC.close()
        self.SC.close()
        self.Connected=False
        try:
            while self.SC.recv(BUFFER_SIZE): pass
        except:
            pass
        try:
            while self.CC.recv(BUFFER_SIZE): pass
        except:
            pass
        time.sleep(2)
        self.Connect(self.IP)
            
    def Disconnect(self):
        assert(self.Connected)
        if self.Connected:
            #self.SimpleCommand("SET IMS DES 0 \n")
            #self.SimpleCommand("SET IMS UPD 1 \n")
            #time.sleep(2)
            self.SimpleCommand("SET LEN ENA 0\n")
            self.StopStream()
            self.CC.close()
            self.SC.close()
            self.Connected=False
        
    def Write(self,command):
        assert(self.Connected)
        self.CC.send(command.encode())
        
    def Read(self,size=BUFFER_SIZE):
        assert(self.Connected)
        return self.CC.recv(size)
    
    def ClearCommand(self):
        assert(self.Connected)
        try:
            while self.CC.recv(BUFFER_SIZE): pass
        except:
            pass
        
    def SimpleCommand(self,command):
        assert(self.Connected)
        self.Write(command)
        data=self.Read().decode("utf-8") 
        #print("SimpleCommand",command,data)
        s=data.split(";")
        if s[0][:2]=="OK":
            if len(s)>1:
                return int(s[1])
        else:
            print("SimpleCommand",command,data)
        
    def ComplexCommand(self,command):
        assert(self.Connected)        
        self.Write(command)
        data=self.Read().decode("utf-8") 
        s=data.split(";")
        if s[0]=="OK":
            return self.Read(int(s[1])).decode("utf-8") 
        else:
            print("ComplexCommand",command,data)
            
    def ComplexCommandDataSet(self,command,data):
        assert(self.Connected)        
        # try:
        self.Write(command+" "+str(len(data)*4)+"\n")
        resp=self.Read().decode("utf-8") 
        # print(len(data),resp)
        self.CC.send(bytearray(data))
        resp=self.Read().decode("utf-8") 
        # print(resp)
        # except:
        #     print("Data send error")
            
    def ComplexCommandDataGet(self,command):
        assert(self.Connected)        
        self.Write(command)
        data=self.Read().decode("utf-8") 
        # print(data)
        s=data.split(";")
        if s[0]=="OK":  
            n=int(s[1])
            datarcv = bytearray()
            while len(datarcv) < n:
                # print(len(datarcv))
                packet = self.Read(n - len(datarcv))
                if not packet:
                    break
                datarcv.extend(packet)
            return datarcv
        else:
            print("ComplexCommandDataGet",command,data)
    
    def Calibration(self):
        assert(self.Connected)
        self.SimpleCommand("SET ACC COC 1\n")
        time.sleep(2)
        self.SimpleCommand("SET IMS UPD 1\n")
        
    def minFuncE(self,x):        
        if not self.Connected:
            print("Error")
        self.SimpleCommand("SET BOL GSK " + str(int(x[0])) + "\n")
        time.sleep(1)
        i,h = self.GetImg(1)
        #hi,b=np.histogram(i[0].flatten(),16385,density=False) 
        hi=MyHist(i[0]) 
        d=hi[hi>0].mean()
        #d=hi[0]+hi[-1]
        #d=(hi[0]+hi[-1])*16385 + abs(i[0].mean()-8192)
        #print("minFuncE",int(x[0]),"res",d)
        return d
    
    def SetUpCalibrationTest(self):
        aGSK=self.Config.GSK
        h2=round(self.minFuncE([aGSK+5]),1)
        h3=round(self.minFuncE([aGSK-5]),1)
        h1=round(self.minFuncE([aGSK]),1)
        #print(h1,h2,h3)
        return h1<=h2 and h1<=h3
    
    def SetUpCalibration(self,maxiter=5,Bmin=100,Bmax=1000,disp=False):
        assert(self.Config)
        #minimalizace GSK podle hist obrazku
        res = optimize.minimize(self.minFuncE, [int(self.Config.GSK)], method='Powell', bounds=[(Bmin, Bmax)], options={'maxiter': maxiter, 'maxfev': maxiter, 'xtol': 1, 'ftol': 5, 'disp': disp})
        #res = optimize.minimize(self.minFuncE, [int(self.Config.GSK)], bounds=[(100, 1000)], options={'maxiter': 20, 'disp': True})
        #print(res)
        self.Config.GSK = int(res.x[0])
        self.SetUp(self.Config)
        i,h = self.GetImg(1)
        hi=MyHist(i[0]) 
        plt.figure()
        plt.plot(hi)
        plt.title("GSK " + str(self.Config.GSK))
        plt.show()
    
    def SetUp(self,config):
        self.Config=config
        self.SimpleCommand("SET BOL GSK " + str(config.GSK) + "\n")
        self.SimpleCommand("SET BOL GFD " + str(config.GFID) + "\n")
        self.SimpleCommand("SET BOL INT " + str(config.INT) + "\n")
        self.SimpleCommand("SET BOL VBU " + str(config.VBUS) + "\n")
        self.SimpleCommand("SET BOL VDT " + str(config.VDET) + "\n")
        self.SimpleCommand("SET ACC COF " + str(config.AVG) + "\n")
        self.SimpleCommand("SET ACC ZOF " + str(config.OFF) + "\n")
        self.SimpleCommand("SET BOL GMS " + str(config.GMS) + "\n")
        self.SimpleCommand("SET IMS UPD 1\n")
                    
    def StartStream(self,Mode=2,lens=True):
        if lens:    
            self.SimpleCommand("SET LEN ENA 1\n")
        else:            
            self.SimpleCommand("SET LEN ENA 0\n")
        self.SimpleCommand("SET IMS DEA 0\n")
        self.SimpleCommand("SET IMS CNA -1\n")
        if Mode==2:#Corr
            #self.SimpleCommand("SET IMS GOR 1\n")
            #self.SimpleCommand("SET IMS DCE 1\n")
            self.SimpleCommand("SET IMS OPA 0\n")
            self.SimpleCommand("SET IMS DES 0 4 1\n")
            self.SimpleCommand("SET IMS DES 2 0 1\n")
        elif Mode==3:#raw Filter
            self.SimpleCommand("SET IMS DES 0 3 1\n")
            self.SimpleCommand("SET IMS DES 1 0 1\n")
        elif Mode==4:#Corr Filter
            # self.SimpleCommand("SET IMS DES 0 3 1\n")
            # self.SimpleCommand("SET IMS DES 1 4 1\n")
            # self.SimpleCommand("SET IMS DES 2 0 1\n")
            self.SimpleCommand("SET IMS DES 0 4 1\n")
            self.SimpleCommand("SET IMS DES 2 5 1\n")
            self.SimpleCommand("SET IMS DES 3 0 1\n")
        else:#raw
            self.SimpleCommand("SET IMS DES 0 0 1\n")
#         time.sleep(2)
        time.sleep(0.2)
        self.SimpleCommand("SET IMS UPD 1\n")
        
    def StopStream(self):
        self.SimpleCommand("SET IMS DEA 0\n")
        self.SimpleCommand("SET IMS UPD 1\n")
        try:
            while self.SC.recv(BUFFER_SIZE): pass
        except:
            pass
                
    def __DecodeImg(self,IH,data):
        #print(str(IH.PixelByteStride))
        if IH.PixelByteStride == 2:
            return np.frombuffer(data,dtype=np.uint16).reshape([IH.Height,IH.Width])
        if IH.PixelByteStride == 4:
            return np.frombuffer(data,dtype=np.uint32).reshape([IH.Height,IH.Width])
        
    def ReadImg(self,raw=1,reset=True):
        if not self.Connected:
            if reset:
                print("Camera init reset")
                self.Reconnect()
            else:
                assert(self.Connected)
        timeS=time.time()
        print("Start stream ",raw)
        self.StartStream(raw)
        while True:
            try:
                data = str(self.SC.recv(28))
                #print(data)#b'IRCA3;0000001059;0000614400\n'
                sizes = data.split("\\n")[0].split(";")
                #print(sizes)
                if sizes[0][2:]=="IRCA3":
                    header = self.SC.recv(int(sizes[1])).decode("utf-8") 
                    #print(header)
                    n=int(sizes[2])
                    data = bytearray()
                    while len(data) < n:
                        packet = self.SC.recv(n - len(data))
                        if not packet:
                            break
                        data.extend(packet)
                    IH = Header(header)
                    self.Lock.acquire(timeout=1)
                    self.Image=self.__DecodeImg(IH,data)
                    self.Lock.release()
                    #print(str(IH.FrameNumber),self.Image.shape)
                    self.NewImage.set()
                    #print("Imge Set")
                    #print(".")
                else:
                    print("Not IRCA",data)
                    raise Exception("Data corrupted")
            except:
                self.Error.set()
                if reset:
                    print("Camera reset", sys.exc_info()[0])
                    self.Reconnect()
                    self.StartStream(raw)
                else:
                    break
        print("Elapsed",time.time()-timeS)
        self.StopStream()
        pass
        
    def GetImg(self,N,raw=1,show=False,reset=True,lens=True):
        if not self.Connected:
            if reset:
                print("Camera init reset")
                self.Reconnect()
            else:
                assert(self.Connected)
        self.StartStream(raw,lens=lens)
        Headers=[]
        Images=[]
        timeS=time.time()
        for i in range(0,N):
            try:
                data = str(self.SC.recv(28))
                #print(data)#b'IRCA3;0000001059;0000614400\n'
                sizes = data.split("\\n")[0].split(";")
                #print(sizes)
                if sizes[0][2:]=="IRCA3":                    
                    n=int(sizes[1])
                    headerb = bytearray()
                    while len(headerb) < n:
                        packet = self.SC.recv(n - len(headerb))
                        if not packet:
                            break
                        headerb.extend(packet)
                    header = headerb.decode("utf-8") 
                    #print(header)
                    n=int(sizes[2])
                    data = bytearray()
                    while len(data) < n:
                        packet = self.SC.recv(n - len(data))
                        if not packet:
                            break
                        data.extend(packet)
                    #print(len(data))      
                    #print(type(data))      
                    #root = ET.fromstring(header)
                    IH = Header(header)
                    #IH.DecodeXML(root)  
                    Headers.append(IH)
                    Images.append(self.__DecodeImg(IH,data))
                else:
                    print("Not IRCA",data)
                    raise Exception("Data corrupted")
            except:
                if reset:
                    print("Camera reset")
                    self.Reconnect()
                    self.StartStream(raw)
                pass
        if show:
            print("FPS",N/(time.time()-timeS))
        self.StopStream()
        return Images,Headers
            
    def GetImgSubArea(self,N,X1,X2,Y1,Y2,raw=1,show=False,reset=True):
        if not self.Connected:
            if reset:
                print("Camera init reset")
                self.Reconnect()
            else:
                assert(self.Connected)
        self.StartStream(raw)
        Headers=[]
        Images=[]
        timeS=time.time()
        for i in range(0,N):
            try:
                data = str(self.SC.recv(28))
                #print(data)#b'IRCA3;0000001059;0000614400\n'
                sizes = data.split("\\n")[0].split(";")
                #print(sizes)
                if sizes[0][2:]=="IRCA3":
                    header = self.SC.recv(int(sizes[1])).decode("utf-8") 
                    #print(header)
                    n=int(sizes[2])
                    data = bytearray()
                    while len(data) < n:
                        packet = self.SC.recv(n - len(data))
                        if not packet:
                            break
                        data.extend(packet)
                    #print(len(data))      
                    #print(type(data))      
                    #root = ET.fromstring(header)
                    IH = Header(header)
                    #IH.DecodeXML(root)  
                    Headers.append(IH)
                    Images.append(np.frombuffer(data,dtype=np.uint16).reshape([IH.Height,IH.Width])[X1:X2,Y1:Y2])
                else:
                    print("Not IRCA",data)
                    raise Exception("Data corrupted")
            except:
                if reset:
                    print("Camera reset")
                    self.Reconnect()
                    self.StartStream(raw)
                pass
        if show:
            print("FPS",N/(time.time()-timeS))
        self.StopStream()
        return Images,Headers
    
    def GetImgSubPoint(self,N,X,Y,raw=1,show=False,reset=True):
        if not self.Connected:
            if reset:
                print("Camera init reset")
                self.Reconnect()
            else:
                assert(self.Connected)
        self.StartStream(raw)
        Headers=[]
        Images=[]
        timeS=time.time()
        for i in range(0,N):
            try:
                data = str(self.SC.recv(28))
                #print(data)#b'IRCA3;0000001059;0000614400\n'
                sizes = data.split("\\n")[0].split(";")
                #print(sizes)
                if sizes[0][2:]=="IRCA3":
                    header = self.SC.recv(int(sizes[1])).decode("utf-8") 
                    #print(header)
                    n=int(sizes[2])
                    data = bytearray()
                    while len(data) < n:
                        packet = self.SC.recv(n - len(data))
                        if not packet:
                            break
                        data.extend(packet)
                    #print(len(data))      
                    #print(type(data))      
                    #root = ET.fromstring(header)
                    IH = Header(header)
                    #IH.DecodeXML(root)  
                    Headers.append(IH)
                    Images.append(np.frombuffer(data,dtype=np.uint16).reshape([IH.Height,IH.Width])[X,Y])
                else:
                    print("Not IRCA",data)
                    raise Exception("Data corrupted")
            except:
                if reset:
                    print("Camera reset")
                    self.Reconnect()
                    self.StartStream(raw)
                pass
        #print("N:",N,"T:",time.time()-timeS,"FPS:",N/(time.time()-timeS))
        if show:
            print("FPS",N/(time.time()-timeS))
        self.StopStream()
        return Images,Headers