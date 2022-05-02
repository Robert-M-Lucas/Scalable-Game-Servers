import os

folders = ["Game-Server", "Load-Balancer", "Lobby-Server", "Matchmaker", "Server-Spooler"]

for f in folders:
    os.system(f"dotnet build {f}")
