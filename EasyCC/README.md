# Application commands:
  Camera commands from cheatsheet SET/GET
  
    Data value of command can be in hexa code with prefix „0x“
  Camera commands for byte arrays SETB/GETB
  
    Command parameter is one word name of file
  Command for data save from camera SAVE/SAVEB/SAVEU PATH COUNT TIMEOUT
  
    Automaticly save 0 to COUNT images with image Header ".h" and image data „.bmp/.bin“
  Application specific commands starts with "APP"
  
    „DMY 0/1“  - with „1“ only recieve data from camera and dropping all frames
    „CNV 0/1“ – with „1“ enable image postprocesing (image coloring, ...)
    „AD 0/1“ – with „1“ process all frames without dropping (maximal buffer size is 100)
    „AVG uint“ – number of recieved images for preprocess average
    „CLR 0/1“ – with „1“ enable refresh of showen image
    „FC 0/1“ – switch 64/16bit filtr mode
	„COLOR 0-6“ – switch collor pallete
	„CQ 0/1“ – set image quality control
	„MUX N“ – drop each N frames
	„ACG“ – Auctomatic GSK correction

    
  Loading window can be disabled with „TajnyKodNaVypnutiHlasky 0/1“ – with 1 is disabled
