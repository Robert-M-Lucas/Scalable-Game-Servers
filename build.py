import os
import shutil

folders = ["Game-Server", "Load-Balancer", "Lobby-Server", "Matchmaker", "Server-Spooler"]

for f in folders:
    os.system(f"dotnet build {f}")

for f in folders:
    shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".exe", "build")