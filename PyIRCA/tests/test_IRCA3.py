from PyIRCA import IRCA3
import time
import matplotlib.pyplot as plt

Cam_IP="10.1.15.101"


def test_IRCA():
    cam = IRCA3.Camera()
    cam.Connect(Cam_IP)
    assert cam.Connected==True
    assert cam.SimpleCommand("GET BOL INT \n")>0
    assert cam.ComplexCommand("GET INF FWI \n")[:4]=="IRCA"
    Image=cam.GetImg(1)[0]
    plt.imshow(Image)
    raise