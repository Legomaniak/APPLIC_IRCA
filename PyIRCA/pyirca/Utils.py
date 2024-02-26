import numpy as np
import matplotlib.pyplot as plt

IRCA_Data_Max=16384
IRCA_VREF=3.3
IRCA_TeplotaSenzoru_SVGA=np.poly1d([ -165.2*IRCA_VREF/IRCA_Data_Max, 332.8])
IRCA_TeplotaSenzoru_VGA=np.poly1d([ -207.9*IRCA_VREF/IRCA_Data_Max, 478.17])

StefanBoltzmannKonst=5.670373*pow(10,-8)
TempK0=-273.15
c1=3.7418*10**8 #W-um**4/m**2
c2=1.4388*10**4 #um-K
kb = 1.38064852*10**-23 # J/K
h = 6.626070040*10**-34 # J.s
c0 = 299792458 # m/s

def TempK(T):
    return T-TempK0

def StefanBoltzmannLaw(Tk):
    return StefanBoltzmannKonst*pow(Tk,4)

def iStefanBoltzmannLaw(E):
    return pow(E/StefanBoltzmannKonst,1/4)

def PlancLaw(y,T):
    return c1/y**5/(np.exp(c2/y/T)-1) #W/(m**2-um)

def PlancLaw(y1,y2,T):
    return integrate.quad(lambda y: special.PlancLaw(y,T), y1, y2)

def Norm(prubeh):
    prubeh[np.isnan(prubeh)]=0
    pmin=prubeh.min()
    p=prubeh-pmin
    pmax=p.max()
    p=p/pmax
    #print(pmin,pmax)
    return p

def Decime1(DataC,dx=5):
    s=DataC.shape
    X=int(s[0]/dx)
    DataD=np.zeros((X))
    for x in range(0,X):
            DataD[x]=DataC[x*dx:(x+1)*dx].mean()
    return DataD

def Decime2(DataC,dx=5,dy=5):
    s=DataC.shape
    X=int(np.floor(s[0]/dx))
    Y=int(np.floor(s[1]/dy))
    DataD=np.zeros((X,Y))
    for x in range(0,X):
        for y in range(0,Y):
            DataD[x,y]=DataC[x*dx:(x+1)*dx,y*dy:(y+1)*dy].mean()
    return DataD

def Mean(data):
    return data-data.mean()

def Linearize(d):
    return d-np.linspace(d[0],d[-1],len(d))

def LoadBin2short(cesta):
    with open(cesta+".bin","rb") as f:  
        velikost = np.fromfile(f, dtype=np.int, count=2) 
        #print(velikost)
        data = np.fromfile(f, dtype=np.short).reshape(velikost)        
    return data
def LoadBin3short(cesta):
    with open(cesta+".bin","rb") as f:  
        velikost = np.fromfile(f, dtype=np.int, count=3)  
        #print(velikost) 
        data = np.fromfile(f, dtype=np.short).reshape(velikost)        
    return data
def LoadBin2double(cesta):
    with open(cesta+".bin","rb") as f:  
        velikost = np.fromfile(f, dtype=np.int, count=2) 
        #print(velikost)
        data = np.fromfile(f, dtype=np.double).reshape(velikost)        
    return data
def LoadBin3double(cesta):
    with open(cesta+".bin","rb") as f:  
        velikost = np.fromfile(f, dtype=np.int, count=3)  
        #print(velikost) 
        data = np.fromfile(f, dtype=np.double).reshape(velikost)        
    return data

def Convolve(data,K=np.array([[1, 2, 1],[2, 4, 2], [1, 2, 1]])):
    K=K/K.sum()
    return signal.convolve2d(data,K,mode='valid')

def ShowStats(data,show=False):
    print("Min",data.min())
    print("Max",data.max())
    print("PP",data.max()-data.min())
    print("Mean",data.mean())
    print("Var",data.var())
    print("Std",data.std())
    if show:
        plt.figure()
        plt.imshow(data)
        
def ShowGraphs(data,x,z,label="Measured data",labelX="Snímek",labelY="Hodnota",legend=["10 °C","20 °C","25 °C","30 °C","40 °C","70 °C","100 °C"]):
    plt.figure()
    plt.title(label)
    for i in range(data.shape[0]):
        d=data[i,x,:,z]
        plt.plot(d)
        print(legend[i]," Std:",d.std())
    plt.xlabel(labelX)
    plt.ylabel(labelY)
    plt.legend(legend)
    plt.show()
    

#vykreslení histogramu (x=ADU, y=četnost)
def MyHist(data,N=16385):
    h=np.zeros(N+1)
    for x in range(data.shape[0]):
        for y in range(data.shape[1]):
            d=int(data[x,y])
            if d>N:
                d=N
            h[d]=h[d]+1
    return h
    
#výpis hlavičky z xml
def recursive_print(element, indent):
    element_tag = element.tag
    element_attributes = element.attrib if len(element.attrib) else ""
    element_text = element.text if element.text is not None and len(element.text.strip()) > 0 else ""
    element_tail = element.tail.strip() if element.tail is not None and len(element.tail.strip()) > 0 else ""
    print(" "*indent,element_tag.title(),element_attributes,element_text,element_tail)
    element_children = list(element)
    for child in element_children:
        recursive_print(child, indent + 4)
        
# CONV_KERNEL matrix
def ConvMatrix(typ,flat = False):
    '''
    Return ConvMatrix and divisor for given type
        Parameters:
            typ (string): Gauss24,Gauss8,Gauss16,Sharpen,Sharpen2,3,4,Robinson,Sobel,Prewitt,Kirsch,Shape,Edge,Nothing
            flat (boolean): type of return
        Returns:
            flat=False (predefined) -> return matrix,divisor
            flat=True -> return kernFloatArr flatten with divisor
    '''
    matrix = np.zeros((5,5),dtype = np.float32)
    if typ == "Gauss24":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0, 1, 2, 1, 0],
                        [0, 2, 4 ,2, 0],
                        [0, 1, 2, 1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Gauss8":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0, 1, 1, 1, 0],
                        [0, 1, 8, 1, 0],
                        [0, 1, 1, 1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Gauss16":
        matrix = np.array([
                        [0, 1, 2, 1, 0],
                        [1, 4, 8, 4, 1],
                        [2, 8, 16, 8, 2],
                        [1, 4, 8, 4, 1],
                        [0, 1, 2, 1, 0]],dtype=np.float32)
    if typ == "Sharpen":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0, 0,-1, 0, 0],
                        [0,-1, 5,-1, 0],
                        [0, 0,-1, 0, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Sharpen2":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0,-1,-2,-1, 0],
                        [0,-2,16,-2, 0],
                        [0,-1,-2,-1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Sharpen3":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0,-1,-1,-1, 0],
                        [0,-1, 9,-1, 0],
                        [0,-1,-1,-1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Sharpen4":
        matrix = np.array([
                        [-1,-1,-1,-1,-1],
                        [-1, 2, 2, 2,-1],
                        [-1, 2, 8, 2,-1],
                        [-1, 2, 2, 2,-1],
                        [-1,-1,-1,-1,-1]],dtype=np.float32)
    if typ == "Edge":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0,-1,-1,-1, 0],
                        [0,-1, 8,-1, 0],
                        [0,-1,-1,-1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Shape":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0, 0, 1, 0, 0],
                        [0, 1,-4, 1, 0],
                        [0, 0, 1, 0, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Robinson":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0,-1,-1,-1, 0],
                        [0, 1,-2, 1, 0],
                        [0,-1,-1,-1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Sobel":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0,-1,-2,-1, 0],
                        [0, 0, 0, 0, 0],
                        [0,-1,-2,-1, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Kirsch":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0,-5,-5,-5, 0],
                        [0, 3, 0, 3, 0],
                        [0, 3, 3, 3, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if typ == "Prewitt":
        matrix = np.array([
                        [-2,-2,-2,-2,-2],
                        [-1,-1,-1,-1,-1],
                        [0, 0, 0, 0, 0],
                        [1, 1, 1, 1, 1],
                        [2, 2, 2, 2, 2]],dtype=np.float32)
    if typ == "Nothing":
        matrix = np.array([
                        [0, 0, 0, 0, 0],
                        [0, 0, 0, 0, 0],
                        [0, 0, 1, 0, 0],
                        [0, 0, 0, 0, 0],
                        [0, 0, 0, 0, 0]],dtype=np.float32)
    if np.sum(matrix) != 0:
        delitel = np.sum(matrix)
    else:
        delitel = 1
    d = np.float32(1/delitel)
    if flat:
        kernFloatArr = np.array(matrix.flatten())
        kernFloatArr = np.append(kernFloatArr,d)
        return kernFloatArr
    return matrix,d
        
        
        
