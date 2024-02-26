import numpy as np
import socket # for socket 
import sys 
import threading
import xml.etree.ElementTree as ET
import asyncio
import time

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
        root = ET.fromstring(source)
        self.DecodeXML(root) 
        
    def DecodeXML(self,root):
        #print(root[0][0].text)
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
        self.TemperatureADCBol=int(root[1][6].text)
        
        self.BitsPerPixel=int(root[2][0].text)
        self.PixelByteStride=int(root[2][1].text)
        self.StartX=int(root[2][2].text)
        self.StartY=int(root[2][3].text)
        self.Width=int(root[2][4].text)
        self.Height=int(root[2][5].text)
        self.ByteSize=int(root[2][6].text)
        self.ImageFlags=root[2][7].text
        
        self.TriggerA=root[3].text
        self.TriggerB=root[4].text
        self.ResolvedTrigger=root[5].text

class CameraSettings:
    GSK = 590
    GFID = 4095
    INT = 306
    VBUS = 2613
    VDET = 3071
    AVG = 4
    OFF = 4096
    GMS = 3
    
class Camera:
    NewImage = asyncio.Event()
    #def __init__(self):
        
    def Connect(self,IP):  
        self.CC = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
        self.SC = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
        self.CC.settimeout(1)
        self.SC.settimeout(1)
        self.CC.connect((IP, 33000)) 
        self.SC.connect((IP, 34000)) 
        self.Connected = True
        
        #self.thread = threading.Thread(target=self.__ImgLoop, args=())
        #self.thread.daemon = True   
    
    def Disconnect(self):
        if self.Connected:
            #self.SimpleCommand("SET IMS DES 0 \n")
            #self.SimpleCommand("SET IMS UPD 1 \n")
            #time.sleep(2)
            self.CC.close()
            self.SC.close()
            self.Connected=False
        
    def Write(self,command):
        self.CC.send(command.encode())
        
    def Read(self,size=BUFFER_SIZE):
        return self.CC.recv(size)
    
    def SimpleCommand(self,command):
        self.Write(command)
        data=self.Read().decode("utf-8") 
        s=data.split(";")
        if s[0][:2]=="OK":
            if len(s)>1:
                return int(s[1])
        else:
            print("SimpleCommand",command,data)
        
    def ComplexCommand(self,command):
        self.Write(command)
        data=self.Read().decode("utf-8") 
        s=data.split(";")
        if s[0]=="OK":
            return self.Read(int(s[1])).decode("utf-8") 
        else:
            print("ComplexCommand",command,data)
    
    def Calibration(self):
        self.SimpleCommand("SET ACC COC 1 \n")
        time.sleep(2)
    
    def SetUp(self,config):
        self.SimpleCommand("SET BOL GSK " + str(config.GSK) + " \n")
        self.SimpleCommand("SET BOL GFD " + str(config.GFID) + " \n")
        self.SimpleCommand("SET BOL INT " + str(config.INT) + " \n")
        self.SimpleCommand("SET BOL VBU " + str(config.VBUS) + " \n")
        self.SimpleCommand("SET BOL VDT " + str(config.VDET) + " \n")
        self.SimpleCommand("SET ACC COF " + str(config.AVG) + " \n")
        self.SimpleCommand("SET ACC ZOF " + str(config.OFF) + " \n")
        self.SimpleCommand("SET BOL GMS " + str(config.GMS) + " \n")
                    
    def StartStream(self,raw=False):
        if raw:
            self.SimpleCommand("SET IMS DES 16\n")
        else:
            self.SimpleCommand("SET IMS DES 32\n")
        self.SimpleCommand("SET IMS UPD 1\n")
        
    def StopStream(self):
        self.SimpleCommand("SET IMS DES 0\n")
        self.SimpleCommand("SET IMS UPD 1\n")
        try:
            while self.SC.recv(BUFFER_SIZE): pass
        except:
            pass
        
    def GetImg(self,N,raw=False):
        assert self.Connected == True
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
                if sizes[0][2:-1]=="IRCA":
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
                    Images.append(np.frombuffer(data,dtype=np.uint16).reshape([IH.Height,IH.Width]))
                else:
                    print("Not IRCA",data)
            except:
                pass
        print("FPS",N/(time.time()-timeS))
        self.StopStream()
        return Images,Headers
            
    def GetImgSubArea(self,N,X1,X2,Y1,Y2,raw=False):
        assert self.Connected == True
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
                if sizes[0][2:-1]=="IRCA":
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
            except:
                pass
        print("FPS",N/(time.time()-timeS))
        self.StopStream()
        return Images,Headers
    
    def GetImgSubPoint(self,N,X,Y,raw=False):
        assert self.Connected == True
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
                if sizes[0][2:-1]=="IRCA":
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
            except:
                pass
        #print("N:",N,"T:",time.time()-timeS,"FPS:",n/(time.time()-timeS))
        print("FPS",N/(time.time()-timeS))
        self.StopStream()
        return Images,Headers