import os

os.chdir("\\".join(__file__.split("\\")[:-1]) + "\\.Build\\NoGui-Client")
os.system('NoGui-Client.exe')