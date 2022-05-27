import os

os.chdir("\\".join(__file__.split("\\")[:-1]) + "\\.Build\\Database-Server")
os.system('Database-Server.exe')
