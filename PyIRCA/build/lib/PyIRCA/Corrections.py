import numpy as np
import matplotlib.pyplot as plt
from scipy.signal import savgol_filter
from scipy.ndimage import gaussian_filter

def NETD_MAP(A,B,C,DT,axe=2):
    meanA=A.mean(axe)
    print("MeanA",meanA.mean())
    meanC=C.mean(axe)
    print("MeanC",meanC.mean())
    stdB=B.std(axe)
    print("StdB",stdB.mean())    
    TD=(meanC-meanA)/DT
    meanTD=TD.mean()
    print("Mean",meanTD,"ADU/K")    
    TD[np.where(TD==0)]=meanTD
    NETD_MAP=np.divide(stdB, TD)
    #NETD_MAP[NETD_MAP>0.2]=0
    plt.imshow(NETD_MAP.transpose())
    plt.show()
    return NETD_MAP*1000

def DataIP(data1,data2):
    s=data1.shape
    data=np.zeros(s)
    dif=data2-data1
    difa=dif.mean()
    d=np.abs(dif-difa)
    dd=difa*0.2
    for x in range(0,s[0]):
        for y in range(0,s[1]):
            if d[x,y]>dd:
                data[x,y]=1
    return data

def CorrIP(data,dataIP):
    s=data.shape
    d=np.zeros(s)
    v=data.item(0)
    for x in range(0,s[0]):
        for y in range(0,s[1]):
            if dataIP[x,y]==0:
                v=data[x,y]
            d[x,y]=v
    return d

def Noise1D(data,SiTF=1):
    S=data.mean()/SiTF
    print("\nS",S)
        
    SA=data.var()/SiTF
    print("\nSA",SA)
    ShowNoise1D(data)
    
def Noise2D(data,SiTF=1):
    S=data.mean()/SiTF
    print("\nS",S)
        
    d=data.mean(0)
    S0=d.var()/SiTF
    print("\nS0",S0)
    print("Size",d.shape)
    ShowNoise1D(d)

    d=data.mean(1)
    S1=d.var()/SiTF
    print("\nS1",S1)
    print("Size",d.shape)
    ShowNoise1D(d)
    
    SA=data.var()/SiTF
    print("\nSA",SA)
    ShowNoise2D(data)
    
    print("S0/SA",S0/SA)
    print("S1/SA",S1/SA)
    
    print("Ssys",abs(SA) * np.sqrt(1+(S0/SA)**2+(S1/SA)**2))
    
def Noise3D(data,SiTF=1):
    S=data.mean()/SiTF
    print("\nS",S)
        
    d=data.mean(0).mean(0)
    S00=d.var()/SiTF
    S00s=data.std(2).mean()/SiTF
    print("\nS00",S00,S00s)
    print("Size",d.shape)
    ShowNoise1D(d)

    d=data.mean(0).mean(1)
    S01=d.var()/SiTF
    S01s=data.std(1).mean()/SiTF
    print("\nS01",S01,S01s)
    print("Size",d.shape)
    ShowNoise1D(d)

    d=data.mean(1).mean(1)
    S11=d.var()/SiTF
    S11s=data.std(0).mean()/SiTF
    print("\nS11",S11,S11s)
    print("Size",d.shape)
    ShowNoise1D(d)
            
    d=data.mean(0)
    S0=d.var()/SiTF
    print("\nS0",S0)
    print("Size",d.shape)
    ShowNoise2D(d)

    d=data.mean(1)
    S1=d.var()/SiTF
    print("\nS1",S1)
    print("Size",d.shape)
    ShowNoise2D(d)
    
    d=data.mean(2)
    S2=d.var()/SiTF
    print("\nS2",S2)
    print("Size",d.shape)
    ShowNoise2D(d)
    
    SA=data.var()/SiTF
    print("\nSA",SA)
    #ShowNoise3D(data)

    print("S00",S00,S00s)
    print("S01",S01,S01s)
    print("S11",S11,S11s)
    print("S0",S0)
    print("S1",S1)
    print("S2",S2)
    print("SA",SA)
    print("S",S)
    print("\nS00/SA",S00/SA)
    print("S01/SA",S01/SA)
    print("S11/SA",S11/SA)
    print("S0/SA",S0/SA)
    print("S1/SA",S1/SA)
    print("S2/SA",S2/SA)
    
    print("Ssys",abs(SA) * np.sqrt(1+(S00/SA)**2+(S01/SA)**2+(S11/SA)**2+(S0/SA)**2+(S1/SA)**2+(S2/SA)**2))
    
def Noise4D(data,SiTF=1):
    S=data.mean()#/SiTF
    print("\nS",S)
        
    d=data.mean(0).mean(0).mean(0)
    S000=d.var()/SiTF
    print("\nS000",S000)
    print("Size",d.shape)
    ShowNoise1D(d)
    
    d=data.mean(0).mean(0).mean(1)
    S001=d.var()/SiTF
    print("\nS001",S001)
    print("Size",d.shape)
    ShowNoise1D(d)

    d=data.mean(0).mean(1).mean(1)
    S011=d.var()/SiTF
    print("\nS011",S011)
    print("Size",d.shape)
    ShowNoise1D(d)

    d=data.mean(1).mean(1).mean(1)
    S111=d.var()/SiTF
    print("\nS111",S111)
    print("Size",d.shape)
    ShowNoise1D(d)
            
    d=data.mean(0).mean(0)
    S00=d.var()/SiTF
    print("\nS00",S00)
    print("Size",d.shape)
    ShowNoise2D(d)

    d=data.mean(0).mean(1)
    S01=d.var()/SiTF
    print("\nS01",S01)
    print("Size",d.shape)
    ShowNoise2D(d)
    
    d=data.mean(0).mean(2)
    S02=d.var()/SiTF
    print("\nS02",S02)
    print("Size",d.shape)
    ShowNoise2D(d)

    d=data.mean(1).mean(1)
    S11=d.var()/SiTF
    print("\nS11",S11)
    print("Size",d.shape)
    ShowNoise2D(d)
    
    d=data.mean(1).mean(2)
    S12=d.var()/SiTF
    print("\nS12",S12)
    print("Size",d.shape)
    ShowNoise2D(d)
    
    d=data.mean(2).mean(2)
    S22=d.var()/SiTF
    print("\nS22",S22)
    print("Size",d.shape)
    ShowNoise2D(d)
    
    d=data.mean(0)
    S0=d.var()/SiTF
    print("\nS0",S0)
    print("Size",d.shape)
    ShowNoise3D(d)

    d=data.mean(1)
    S1=d.var()/SiTF
    print("\nS1",S1)
    print("Size",d.shape)
    ShowNoise3D(d)
    
    d=data.mean(2)
    S2=d.var()/SiTF
    print("\nS2",S2)
    print("Size",d.shape)
    ShowNoise3D(d)
    
    d=data.mean(3)
    S3=d.var()/SiTF
    print("\nS3",S3)
    print("Size",d.shape)
    ShowNoise3D(d)
    
    SA=data.var()/SiTF
    print("\nSA",SA)
    ShowNoise4D(data)
    
    print("S000/SA",S000/SA)
    print("S001/SA",S001/SA)
    print("S011/SA",S011/SA)
    print("S111/SA",S111/SA)
    print("S00/SA",S00/SA)
    print("S01/SA",S01/SA)
    print("S02/SA",S02/SA)
    print("S11/SA",S11/SA)
    print("S12/SA",S12/SA)
    print("S22/SA",S22/SA)
    print("S0/SA",S0/SA)
    print("S1/SA",S1/SA)
    print("S2/SA",S2/SA)
    print("S3/SA",S3/SA)
    
    print("Ssys",abs(SA) * np.sqrt(1+(S000/SA)**2+(S001/SA)**2+(S011/SA)**2+(S111/SA)**2+(S00/SA)**2+(S01/SA)**2+(S02/SA)**2+(S11/SA)**2+(S12/SA)**2+(S22/SA)**2+(S0/SA)**2+(S1/SA)**2+(S2/SA)**2+(S3/SA)**2))
    
#4D noise
#t,v,h,w
def D(data,i):
    return data.sum(i)/data.shape[i]
def Nw_4(data):
    DD=D(D(D(data,2),1),0)
    return DD-D(DD,0)
def Nh_4(data):
    DD=D(D(D(data,3),1),0)
    return DD-D(DD,0)
def Nv_4(data):
    DD=D(D(D(data,3),2),0)
    return DD-D(DD,0)
def Nt_4(data):
    DD=D(D(D(data,3),2),1)
    return DD-D(DD,0)
def Ntv_4(data):
    DD=D(D(data,3),2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Nth_4(data):
    DD=D(D(data,3),1)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Ntw_4(data):
    DD=D(D(data,2),1)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Nvh_4(data):
    DD=D(D(data,3),0)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Nvw_4(data):
    DD=D(D(data,2),0)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Nhw_4(data):
    DD=D(D(data,1),0)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Ntvh_4(data):
    DD=D(data,3)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Ntvw_4(data):
    DD=D(data,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Nthw_4(data):
    DD=D(data,1)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Nvhw_4(data):
    DD=D(data,0)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)
    return DD-D(DD,0)
def Ntvhw_4(data):
    DD=data-D(data,0)
    DD=np.swapaxes(DD,0,1)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,3)
    return DD-D(DD,0)    

def Nh_3(data):
    DD=D(D(data,0),0)
    return DD-D(DD,0)
def Nv_3(data):
    DD=D(D(data,2),0)
    return DD-D(DD,0)
def Nt_3(data):
    DD=D(D(data,2),1)
    return DD-D(DD,0)
def Nth_3(data):
    Dv=D(data,1)
    Dt=D(data,0)
    DD=Dv-D(Dv,0)
    return DD-D(Dt,0)
def Nth_3b(data):
    DD=D(data,1)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)-D(DD,1)
    return np.swapaxes(DD,0,1)
def Ntv_3(data):
    Dh=D(data,2)
    Dt=D(data,0)
    #DD=np.swapaxes(Dh,0,1)-D(Dh,1)
    DD=Dh-D(Dh,0)
    return DD-D(Dt,1)
def Ntv_3b(data):
    DD=D(data,2)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)-D(DD,1)
    return np.swapaxes(DD,0,1)
def Nvh_3(data):  
    Dv=D(data,1)
    Dt=D(data,0)
    DD=Dt-D(Dt,0)
    return DD-D(Dv,0)
def Nvh_3b(data):  
    DD=D(data,0)
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)-D(DD,1)
    return np.swapaxes(DD,0,1)
def Ntvh_3(data):
    #DD=data-D(data,0)
    #DD=np.swapaxes(DD,0,1)-D(DD,1)
    #DD=np.swapaxes(DD,0,2)-D(DD,2)
    #return DD
    DD=data-D(data,0)
    DD=np.swapaxes(DD,0,1)
    DD=DD-D(data,1)
    DD=np.swapaxes(DD,0,2)
    DD=DD-D(data,2)
    DD=np.swapaxes(DD,0,1)
    DD=np.swapaxes(DD,1,2)
    return DD

def Ntvh_3b(data):
    DD=data
    DD=DD-D(DD,0)
    DD=np.swapaxes(DD,0,1)-D(DD,1)
    #DD=np.swapaxes(DD,0,1)
    DD=np.swapaxes(np.swapaxes(DD,0,2),1,2)-D(DD,2)
    DD=np.swapaxes(DD,0,2)
    return DD

def ShowNoise1D(data,tx="",order=3):
    L=range(len(data))
    #dataL=np.polyval(np.polyfit(L,data,order),L)
    dataL=savgol_filter(data, 11, 3)
    dataH=data-dataL
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.plot(data)
    plt.title("Original")
    plt.xlabel(tx)
    plt.show()
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.hist(data, bins='auto')
    plt.title("Histogram Original")
    plt.show()
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.plot(dataL)
    plt.title("Low-pass")
    plt.xlabel(tx)
    plt.show()
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.hist(dataL, bins='auto')
    plt.title("Histogram Low-pass")
    plt.show()
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.plot(dataH)
    plt.title("High-pass")
    plt.xlabel(tx)
    plt.show()
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.hist(dataH, bins='auto')
    plt.title("Histogram High-pass")
    plt.show()
    
def ShowNoise2D(data,tx="",ty="",order=3):
    #dataL=np.zeros(data.shape)
    #for x in range(data.shape[0]):
    #    L=range(data.shape[1])
    #    dataL[x]=np.polyval(np.polyfit(L,data[x],order),L)
    #for x in range(data.shape[1]):
    #    L=range(data.shape[0])
    #    dataL[:,x]=np.polyval(np.polyfit(L,dataL[:,x],order),L)  
    dataL=gaussian_filter(data, sigma=1)
    dataH=data-dataL
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.imshow(data)
    plt.title("Original")
    plt.xlabel(tx)
    plt.ylabel(ty)
    plt.colorbar()
    plt.show()    
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.hist(data.flatten(), bins='auto')
    plt.title("Histogram Original")
    plt.show()    
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.imshow(dataL)
    plt.title("Low-pass")
    plt.xlabel(tx)
    plt.ylabel(ty)
    plt.colorbar()
    plt.show()
   
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.hist(dataL.flatten(), bins='auto')
    plt.title("Histogram Low-pass")
    plt.show()    
    
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.imshow(dataH)
    plt.title("High-pass")
    plt.xlabel(tx)
    plt.ylabel(ty)
    plt.colorbar()
    plt.show()
 
    #plt.figure(figsize=[20,10])
    plt.figure()
    plt.hist(dataH.flatten(), bins='auto')
    plt.title("Histogram High-pass")
    plt.show()    
    
def ShowNoise3D(data,tx="",ty="",order=3):  
    dataL=gaussian_filter(data, sigma=1)
    dataH=data-dataL
    
    plt.figure(figsize=[20,10])
    plt.hist(data.flatten(), bins='auto')
    plt.title("Histogram Original")
    plt.show()   
    
    plt.figure(figsize=[20,10])
    plt.hist(dataL.flatten(), bins='auto')
    plt.title("Histogram Low-pass")
    plt.show()    
    
    plt.figure(figsize=[20,10])
    plt.hist(dataH.flatten(), bins='auto')
    plt.title("Histogram High-pass")
    plt.show()    
    
def ShowNoise4D(data,tx="",ty="",order=3):  
    dataL=gaussian_filter(data, sigma=1)
    dataH=data-dataL
    
    plt.figure(figsize=[20,10])
    plt.hist(data.flatten(), bins='auto')
    plt.title("Histogram Original")
    plt.show()    
    plt.figure(figsize=[20,10])
    plt.hist(dataL.flatten(), bins='auto')
    plt.title("Histogram Low-pass")
    plt.show()    
    
    plt.figure(figsize=[20,10])
    plt.hist(dataH.flatten(), bins='auto')
    plt.title("Histogram High-pass")
    plt.show()    
    
def show3DQ(data,SiTF):
    print("Data [T,V,H] -> Size [N,m,n]")
    print("Size",data.shape)
    S=data.mean()
    print("S",S)#global avg
    
    Nt=Nt_3(data)#-S
    Qt=Nt.var()/SiTF
    print("\nQt",Qt)
    print("Size",Nt.shape)
    plt.figure()
    plt.plot(Nt)
    plt.show()
    
    Nv=Nv_3(data)#-S
    Qv=Nv.var()/SiTF
    print("\nQv",Qv)
    print("Size",Nv.shape)
    ShowNoise1D(Nv)
    
    Nh=Nh_3(data)#-S
    Qh=Nh.var()/SiTF
    print("\nQh",Qh)
    print("Size",Nh.shape)
    ShowNoise1D(Nh)
    
    Ntv=Ntv_3(data)#-S
    Qtv=Ntv.var()/SiTF
    print("\nQtv",Qtv)
    print("Size",Ntv.shape)
    #NtvM=abs(Ntv.mean())
    #print("Ntv mean",NtvM)
    #plt.figure()
    #plt.imshow(abs(Ntv),vmin=NtvM-Qtv, vmax=NtvM+Qtv)
    #plt.imshow(Ntv)
    #plt.show()
    ShowNoise2D(Ntv)
    
    Nth=Nth_3(data)#-S
    Qth=Nth.var()/SiTF
    print("\nQth",Qth)
    print("Size",Nth.shape)
    #NthM=abs(Nth.mean())
    #print("Nth mean",NthM)
    #plt.figure()
    #plt.imshow(abs(Nth),vmin=NthM-Qth, vmax=NthM+Qth)
    #plt.imshow(Nth)
    #plt.show()
    ShowNoise2D(Nth)
    
    Nvh=Nvh_3(data)#-S
    Qvh=Nvh.var()/SiTF
    print("\nQvh",Qvh)
    print("Size",Nvh.shape)
    NvhM=abs(Nvh.mean())
    #print("Nvh mean",NvhM)
    #plt.figure()
    #plt.imshow(abs(Nvh),vmin=NvhM-Qvh/2, vmax=NvhM+Qvh/2)
    #plt.imshow(Nvh)
    #plt.show()
    ShowNoise2D(Nvh)
    
    Ntvh=Ntvh_3(data)#-S
    Qtvh=Ntvh.var()/SiTF
    print("\nQtvh",Qtvh)
    print("Size",Ntvh.shape)

    print("\nQvh/Qtvh",Qvh/Qtvh)
    print("Qtv/Qtvh",Qtv/Qtvh)
    print("Qth/Qtvh",Qth/Qtvh)
    print("Qh/Qtvh",Qh/Qtvh)
    print("Qv/Qtvh",Qv/Qtvh)
    print("Qt/Qtvh",Qt/Qtvh)
    
    print("\nQsys",Qtvh * np.sqrt(1+(Qvh/Qtvh)**2+(Qtv/Qtvh)**2+(Qth/Qtvh)**2+(Qh/Qtvh)**2+(Qv/Qtvh)**2+(Qt/Qtvh)**2))
    
def show3DQb(data,SiTF):
    print("Data [T,V,H] -> Size [N,m,n]")
    print("Size",data.shape)
    S=data.mean()
    print("S",S)#global avg
    
    Nt=Nt_3(data-S)
    Qt=Nt.var()/SiTF
    print("\nQt",Qt)
    print("Size",Nt.shape)
    plt.figure()
    plt.plot(Nt)
    plt.show()
    
    Nv=Nv_3(data-S)
    Qv=Nv.var()/SiTF
    print("\nQv",Qv)
    print("Size",Nv.shape)
    ShowNoise1D(Nv)
    
    Nh=Nh_3(data-S)
    Qh=Nh.var()/SiTF
    print("\nQh",Qh)
    print("Size",Nh.shape)
    ShowNoise1D(Nh)
    
    Ntv=Ntv_3b(data-S)
    Qtv=Ntv.var()/SiTF
    print("\nQtv",Qtv)
    print("Size",Ntv.shape)
    #NtvM=abs(Ntv.mean())
    #print("Ntv mean",NtvM)
    #plt.figure()
    #plt.imshow(abs(Ntv),vmin=NtvM-Qtv, vmax=NtvM+Qtv)
    #plt.imshow(Ntv)
    #plt.show()
    ShowNoise2D(Ntv)
    
    Nth=Nth_3b(data-S)
    Qth=Nth.var()/SiTF
    print("\nQth",Qth)
    print("Size",Nth.shape)
    #NthM=abs(Nth.mean())
    #print("Nth mean",NthM)
    #plt.figure()
    #plt.imshow(abs(Nth),vmin=NthM-Qth, vmax=NthM+Qth)
    #plt.imshow(Nth)
    #plt.show()
    ShowNoise2D(Nth)
    
    Nvh=Nvh_3b(data-S)
    Qvh=Nvh.var()/SiTF
    print("\nQvh",Qvh)
    print("Size",Nvh.shape)
    #NvhM=abs(Nvh.mean())
    #print("Nvh mean",NvhM)
    #plt.figure()
    #plt.imshow(abs(Nvh),vmin=NvhM-Qvh, vmax=NvhM+Qvh)
    #plt.imshow(Nvh)
    #plt.show()
    ShowNoise2D(Nvh)
    
    Ntvh=Ntvh_3b(data-S)
    Qtvh=Ntvh.var()/SiTF
    print("\nQtvh",Qtvh)
    print("Size",Ntvh.shape)

    print("\nQvh/Qtvh",Qvh/Qtvh)
    print("Qtv/Qtvh",Qtv/Qtvh)
    print("Qth/Qtvh",Qth/Qtvh)
    print("Qh/Qtvh",Qh/Qtvh)
    print("Qv/Qtvh",Qv/Qtvh)
    print("Qt/Qtvh",Qt/Qtvh)
    
    print("\nQsys",Qtvh * np.sqrt(1+(Qvh/Qtvh)**2+(Qtv/Qtvh)**2+(Qth/Qtvh)**2+(Qh/Qtvh)**2+(Qv/Qtvh)**2+(Qt/Qtvh)**2))
    
def show4DQ(data,SiTF,t1="t",t2="v",t3="h",t4="w",tQ="Q_"):
    #T,t,v,h
    tQt=tQ+t1
    tQv=tQ+t2
    tQh=tQ+t3
    tQw=tQ+t4
    tQtv=tQt+t2
    tQth=tQt+t3
    tQtw=tQt+t4
    tQvh=tQv+t3
    tQvw=tQv+t4
    tQhw=tQh+t4
    tQtvh=tQtv+t3
    tQtvw=tQtv+t4
    tQthw=tQth+t4
    tQvhw=tQvh+t4
    tQtvhw=tQtvh+t4
    
    print("Size",data.shape)
    S=data.mean()
    print("S",S)#global avg
    #data=data-S
    
    N=Nt_4(data)
    Qt=N.var()/SiTF
    print(str(tQt),Qt)
    print("Size",N.shape)
    ShowNoise1D(N,t1)
    
    N=Nv_4(data)
    Qv=N.var()/SiTF
    print(tQv,Qv)
    print("Size",N.shape)
    ShowNoise1D(N,t2)
    
    N=Nh_4(data)
    Qh=N.var()/SiTF
    print(tQh,Qh)
    print("Size",N.shape)
    ShowNoise1D(N,t3)
    
    N=Nw_4(data)
    Qw=N.var()/SiTF
    print(tQw,Qw)
    print("Size",N.shape)
    ShowNoise1D(N,t4)
    
    N=Ntv_4(data)
    Qtv=N.var()/SiTF
    print(tQtv,Qtv)
    print("Size",N.shape)
    ShowNoise2D(N,t1,t2)
    
    N=Nth_4(data)
    Qth=N.var()/SiTF
    print(tQth,Qth)
    print("Size",N.shape)
    ShowNoise2D(N,t1,t3)
    
    N=Ntw_4(data)
    Qtw=N.var()/SiTF
    print(tQtw,Qtw)
    print("Size",N.shape)
    ShowNoise2D(N,t1,t4)
    
    N=Nvh_4(data)
    Qvh=N.var()/SiTF
    print(tQvh,Qvh)
    print("Size",N.shape)
    ShowNoise2D(N,t2,t3)
    
    N=Nvw_4(data)
    Qvw=N.var()/SiTF
    print(tQvw,Qvw)
    print("Size",N.shape)
    ShowNoise2D(N,t2,t4)
    
    N=Nhw_4(data)
    Qhw=N.var()/SiTF
    print(tQhw,Qhw)
    print("Size",N.shape)
    ShowNoise2D(N,t3,t4)
    
    N=Ntvh_4(data)
    Qtvh=N.var()/SiTF
    print(tQtvh,Qtvh)
    print("Size",N.shape)
    ShowNoise3D(N)
    
    N=Ntvw_4(data)
    Qtvw=N.var()/SiTF
    print(tQtvw,Qtvw)
    print("Size",N.shape)
    ShowNoise3D(N)
    
    N=Nthw_4(data)
    Qthw=N.var()/SiTF
    print(tQthw,Qthw)
    print("Size",N.shape)
    ShowNoise3D(N)
    
    N=Nvhw_4(data)
    Qvhw=N.var()/SiTF
    print(tQvhw,Qvhw)
    print("Size",N.shape)
    ShowNoise3D(N)
    
    N=Ntvhw_4(data)
    Qtvhw=N.var()/SiTF
    print(tQtvhw,Qtvhw)
    print("Size",N.shape)
    ShowNoise4D(N)

    print(tQvhw+"/"+tQtvhw,Qvhw/Qtvhw)
    print(tQtvw+"/"+tQtvhw,Qtvw/Qtvhw)
    print(tQthw+"/"+tQtvhw,Qthw/Qtvhw)
    print(tQtvh+"/"+tQtvhw,Qtvh/Qtvhw)
    print(tQhw+"/"+tQtvhw,Qhw/Qtvhw)
    print(tQvw+"/"+tQtvhw,Qvw/Qtvhw)
    print(tQvh+"/"+tQtvhw,Qvh/Qtvhw)
    print(tQtv+"/"+tQtvhw,Qtv/Qtvhw)
    print(tQth+"/"+tQtvhw,Qth/Qtvhw)
    print(tQtw+"/"+tQtvhw,Qtw/Qtvhw)
    print(tQt+"/"+tQtvhw,Qt/Qtvhw)
    print(tQh+"/"+tQtvhw,Qh/Qtvhw)
    print(tQv+"/"+tQtvhw,Qv/Qtvhw)
    print(tQw+"/"+tQtvhw,Qw/Qtvhw)
    
    print("Qsys",abs(Qtvhw) * np.sqrt(1+(Qvhw/Qtvhw)**2+(Qtvw/Qtvhw)**2+(Qthw/Qtvhw)**2+(Qtvh/Qtvhw)**2+(Qhw/Qtvhw)**2+(Qvw/Qtvhw)**2+(Qvh/Qtvhw)**2+(Qtv/Qtvhw)**2+(Qth/Qtvhw)**2+(Qtw/Qtvhw)**2+(Qv/Qtvhw)**2+(Qh/Qtvhw)**2+(Qv/Qtvhw)**2+(Qw/Qtvhw)**2))