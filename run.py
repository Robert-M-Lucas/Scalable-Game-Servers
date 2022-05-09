import os

os.chdir("\\".join(__file__.split("\\")[:-1]) + "\\.Build\\Server-Spooler")
os.system('Server-Spooler.exe')
# C:\Users\rober\Documents\Mixed Code Projects\Scalable-Game-Servers\config.json

# os.system('.Build\\Server-Spooler\\Server-Spooler.exe "C:\\Users\\rober\\Documents\\Projects\\Scalable-Game-Servers\\config.json"')