import os

os.chdir("\\".join(__file__.split("\\")[:-1]))
os.system('.Build\\NoGui-Client\\NoGui-Client.exe')