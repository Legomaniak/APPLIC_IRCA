import serial
import time
import io

class WheelFW102():
    def __init__(self,port,Pos=6):#'/dev/ttyUSB0'
        self.Connect(port)  # open serial port
        self.Positions=Pos
        
    def Connect(self,port):#'/dev/ttyUSB0'
        self.ser = serial.Serial(port,115200, timeout=1)  # open serial port
        
    def GetID(self):
        assert self.ser.is_open
        cmd = "*idn?\r"
        self.ser.write(cmd.encode())     # write a string
        return self.__Read()
    
    def __GetPositions(self):
        assert self.ser.is_open
        cmd = "pcount?\r"
        self.ser.write(cmd.encode())     # write a string
        self.Positions = int(self.ser.readline().decode("utf-8"))
        
    def GetPosition(self):
        assert self.ser.is_open
        cmd = "pos?\r"
        self.ser.write(cmd.encode())     # write a string
        return self.__Read()
            
    def Close(self):
        self.ser.close()             # close port
        
    def SetPosition(self,value):
        assert self.ser.is_open
        cmd = "pos=" + str(value+1) + "\r"
        self.ser.write(cmd.encode())     # write a string
        self.__Read()
        
    def SetHome(self):
        self.SetPosition(0)     
        
    def __Read(self):
        ret = ""
        while True:
            r = self.ser.read().decode("utf-8")
            if r ==">":
                break
            else:
                ret = ret+r
        return ret
        
class Mirror():
    def __init__(self,port,PosA,PosB,PosH):#'/dev/ttyUSB0'
        self.Connect(port)  # open serial port
        self.PosA=PosA
        self.PosB=PosB
        self.PosH=PosH
        
    def Connect(self,port):#'/dev/ttyUSB0'
        self.ser = serial.Serial(port,115200, timeout=30)  # open serial port
        
    def Close(self):
        self.ser.close()             # close port
        
    def SetSpeed(self,value):
        assert self.ser.is_open
        cmd = "SMV " + "{:.3f}".format(value) + "\r"
        self.ser.write(cmd.encode())     # write a string
        self.ser.readline()
        
    def SetValue(self,value):
        assert self.ser.is_open
        #print("Mirror",value)
        self.Pos=value
        cmd = "SSP " + "{:.3f}".format(value) + "\r"
        self.ser.write(cmd.encode())     # write a string
#         time.sleep(0.1)
        self.ser.readline()
        
    def GetValue(self):
        assert self.ser.is_open
        self.ser.write(b'GPS\r')     # write a string
        return float(self.ser.readline().decode("utf-8"))
    
    def GetSettedValue(self):#nonimportant
        assert self.ser.is_open
        self.ser.write(b'GSP\r')     # write a string
        return float(self.ser.readline().decode("utf-8"))
        
    def SetValueProc(self,value,invert=False):
        if invert:
            self.SetValue(self.PosB-value/100*(self.PosB-self.PosA))     # write a string
        else:
            self.SetValue(self.PosA+value/100*(self.PosB-self.PosA))     # write a string
        
    def SetHome(self):
        self.SetValue(self.PosH)     # write a string
        
    def SetStart(self,invert=False):
        if invert:
            self.SetValue(self.PosB)     # write a string
        else:
            self.SetValue(self.PosA)     # write a string            

class BlackBodyFluke():
    def __init__(self,port):#'/dev/ttyUSB0'
        self.Connect(port)  # open serial port
        
    def Connect(self,port):#'/dev/ttyUSB0'
        self.ser = serial.Serial(port, 9600, timeout=1)  # open serial port
                
    def GetID(self):
        assert self.ser.is_open
        self.ser.write(b'*IDN?\n')     # write a string
        return self.ser.readline().decode("utf-8")
    
    def SetEnable(self,value):
        assert self.ser.is_open
        if value:
            self.ser.write(b'OUTP:STAT 1\n')     # write a string
        else:
            self.ser.write(b'OUTP:STAT 0\n')     # write a string
        
    def GetEnable(self):
        assert self.ser.is_open
        self.ser.write(b'OUTP:STAT?\n')     # write a string
        return self.ser.readline()==b'1\r\n'
    
    def SetTemp(self,value):
        assert self.ser.is_open
        cmd = "SOUR:SPO " + "{:.4f}".format(value) + "\n"
        self.ser.write(cmd.encode())     # write a string
        time.sleep(10)
        
    def GetTemp(self):
        assert self.ser.is_open
        self.ser.write(b'SOUR:SPO?\n')     # write a string
        return float(self.ser.readline().decode("utf-8"))
    
    def GetActualTemp(self):
        assert self.ser.is_open
        self.ser.write(b'SOUR:SENS:DATA?\n')     # write a string
        return float(self.ser.readline().decode("utf-8"))
    
    #0.01-5.0
    def SetStableLim(self,value):
        assert self.ser.is_open
        cmd = "SOUR:STAB:LIM " + "{:.2f}".format(value) + "\n"
        self.ser.write(cmd.encode())     # write a string
    
    def GetStableLim(self):
        assert self.ser.is_open
        self.ser.write(b'SOUR:STAB:LIM?\n')     # write a string
        return float(self.ser.readline().decode("utf-8"))
    
    def IsStable(self):
        assert self.ser.is_open
        self.ser.write(b'SOUR:STAB:TEST?\n')     # write a string
        return self.ser.readline()==b'1\r\n'
    
    #sec
    def WaitForStable(self,Refresh=5,Timeout=3600,Show=False):
        dT=0
        while Timeout>dT:
            if Show:
                print("ActualTemp",self.GetActualTemp(),"°C")
            if self.IsStable():
                return True
            else:
                time.sleep(Refresh)
            dT=dT+Refresh
        return False
        
    def Close(self):
        self.ser.close()