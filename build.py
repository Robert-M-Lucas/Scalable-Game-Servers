import os
import shutil
from distutils.dir_util import copy_tree

folders = ["Game-Server", "Load-Balancer", "Lobby-Server", "Matchmaker", "Server-Spooler", "NoGui-Client"]

for f in folders:
    os.system(f"dotnet build {f}")

for f in folders:
    copy_tree(f + "\\bin\\Debug\\net6.0", ".Build\\" + f)

    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".exe", ".Build")
    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".dll", ".Build")
    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".runtimeconfig.json", ".Build")