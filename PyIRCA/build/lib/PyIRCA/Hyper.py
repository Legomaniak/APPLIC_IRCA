import os.path
import numpy as np
import matplotlib.pyplot as plt

def LoadCube(cesta):
    #with open(cesta+"\\Nastaveni.txt","r") as f: 
    #    INT = (int)(f.readline().replace('\n','').split(' ')[1])
    #    GSK = (int)(f.readline().replace('\n','').split(' ')[1])
    #    GFID = (int)(f.readline().replace('\n','').split(' ')[1])
    #    VBUS = (int)(f.readline().replace('\n','').split(' ')[1])
    #    VDET = (int)(f.readline().replace('\n','').split(' ')[1])
    #    GMS = (int)(f.readline().replace('\n','').split(' ')[1])
    with open(cesta+"\\Kostka.bip.hdr","r") as f: 
        Sirka = (int)(f.readline().replace('\n','').split(' ')[2])
        Vyska = (int)(f.readline().replace('\n','').split(' ')[2])
        Pocet = (int)(f.readline().replace('\n','').split(' ')[2])
    with open(cesta+"\\Kostka.bip","rb") as f:  
        data = np.fromfile(f, dtype=np.short).reshape(Sirka,Pocet,Vyska)        
    return data

def LoadCube1(cesta,DelkaBB):
    with open(cesta+"\\Nastaveni.txt","r") as f: 
        INT = (int)(f.readline().replace('\n','').split(' ')[1])
        GSK = (int)(f.readline().replace('\n','').split(' ')[1])
        GFID = (int)(f.readline().replace('\n','').split(' ')[1])
        VBUS = (int)(f.readline().replace('\n','').split(' ')[1])
        VDET = (int)(f.readline().replace('\n','').split(' ')[1])
        GMS = (int)(f.readline().replace('\n','').split(' ')[1])
    with open(cesta+"\\Kostka.bip.hdr","r") as f: 
        Sirka = (int)(f.readline().replace('\n','').split(' ')[2])
        Vyska = (int)(f.readline().replace('\n','').split(' ')[2])
        Pocet = (int)(f.readline().replace('\n','').split(' ')[2])
    with open(cesta+"\\Kostka.bip","rb") as f:  
        data = np.fromfile(f, dtype=np.short).reshape(Sirka,Pocet,Vyska)
    
    TempA=3
    DataIRCA=data[TempA:-TempA,DelkaBB:,:].mean(1)
    DataIRCA0=data[TempA:-TempA,:DelkaBB,:].mean(1)
    
    #DataC=(DataIRCA-DataIRCA[0].mean())/(DataIRCA0-DataIRCA0[0].mean())
    DataC=DataIRCA-DataIRCA0
    
    return DataC[:,:]

def LoadCube1s(cesta,DelkaBB):
    with open(cesta+"\\Nastaveni.txt","r") as f: 
        INT = (int)(f.readline().replace('\n','').split(' ')[1])
        GSK = (int)(f.readline().replace('\n','').split(' ')[1])
        GFID = (int)(f.readline().replace('\n','').split(' ')[1])
        VBUS = (int)(f.readline().replace('\n','').split(' ')[1])
        VDET = (int)(f.readline().replace('\n','').split(' ')[1])
        GMS = (int)(f.readline().replace('\n','').split(' ')[1])
    with open(cesta+"\\Kostka.bip.hdr","r") as f: 
        Sirka = (int)(f.readline().replace('\n','').split(' ')[2])
        Vyska = (int)(f.readline().replace('\n','').split(' ')[2])
        Pocet = (int)(f.readline().replace('\n','').split(' ')[2])
    with open(cesta+"\\Kostka.bip","rb") as f:  
        data = np.fromfile(f, dtype=np.short).reshape(Sirka,Pocet,Vyska)
    
    TempA=3
    DataIRCA=data[TempA:-TempA,DelkaBB:,:].std(1)
    DataIRCA0=data[TempA:-TempA,:DelkaBB,:].mean(1)
    
    #DataC=(DataIRCA-DataIRCA[0].mean())/(DataIRCA0-DataIRCA0[0].mean())
    DataC=DataIRCA-DataIRCA0
    
    return DataC[:,:]

def LoadCube2(cesta,DelkaBB):
    with open(cesta+"\\Kostka.bip.hdr","r") as f: 
        Sirka = (int)(f.readline().replace('\n','').split(' ')[2])
        Vyska = (int)(f.readline().replace('\n','').split(' ')[2])
        Pocet = (int)(f.readline().replace('\n','').split(' ')[2])
    with open(cesta+"\\Kostka.bip","rb") as f:  
        data = np.fromfile(f, dtype=np.short).reshape(Sirka,Pocet,Vyska)
    
    TempA=3
    DataIRCA0=data[TempA:-TempA,:DelkaBB,:].mean(1)
    N=int(np.floor(Pocet/DelkaBB))
    DataIRCA=np.zeros((Sirka-2*TempA,N,Vyska))
    for i in range(0,N):
        DataIRCA[:,i,:]=data[TempA:-TempA,DelkaBB+i*DelkaBB:DelkaBB+(i+1)*DelkaBB,:].mean(1)-DataIRCA0    
    return DataIRCA

def LoadCubeD(cesta):
    with open(cesta+"\\Kostka.bip.hdr","r") as f: 
        Sirka = (int)(f.readline().replace('\n','').split(' ')[2])
        Vyska = (int)(f.readline().replace('\n','').split(' ')[2])
        Pocet = (int)(f.readline().replace('\n','').split(' ')[2])
    with open(cesta+"\\Kostka.bip","rb") as f:  
        data = np.fromfile(f, dtype=np.double).reshape(Sirka,Pocet,Vyska)
    return data

def LoadCubeDJ(cesta,jmeno):
    with open(cesta+"\\"+jmeno+".hdr","r") as f: 
        Sirka = (int)(f.readline().replace('\n','').split(' ')[2])
        Vyska = (int)(f.readline().replace('\n','').split(' ')[2])
        Pocet = (int)(f.readline().replace('\n','').split(' ')[2])
    with open(cesta+"\\"+jmeno,"rb") as f:  
        data = np.fromfile(f, dtype=np.double).reshape(Sirka,Pocet,Vyska)
    return data

def GetLine(data,Delta=200,X1=55,X2=600,Y1=100,Y2=230,YC1=20,YC2=300):
    #d=data[RODES_X1+200:RODES_X2-200,RODES_YC1:RODES_YC2].mean(0)
    #return (d-np.linspace(d[0],d[-1],len(d)))[RODES_Y1-RODES_YC1:RODES_Y2-RODES_YC1]
    d=data[X1+Delta:X2-Delta,Y1:Y2].mean(0)
    return d-np.linspace(d[0],d[-1],len(d))
    
def GetData(data,X1=55,X2=600,Y1=100,Y2=230):   
    d=data[X1:X2,Y1:Y2]
    DataL=np.zeros(d.shape)
    for x in range(0,d.shape[0]): 
        DataL[x]=d[x]-np.linspace(d[x,0],d[x,-1],len(d[1]))
    return DataL
