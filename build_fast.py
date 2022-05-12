import os
import shutil
from distutils.dir_util import copy_tree
import subprocess
import time

os.chdir("\\".join(__file__.split("\\")[:-1]))

start = time.time()

config_only = False

folders = ["Shared", "Server-Spooler", "Load-Balancer", "Game-Server", "Lobby-Server", "Matchmaker", "NoGui-Client"]

# if not config_only:
#     done = []
#     for f in folders[:-1]:
#         done.append(subprocess.call(["dotnet", "build", f]))
# 
#     subprocess.call(["dotnet", "build", f"{folders[-1]}"])

# cont = False
# while not cont:
#     cont = True
#     for d in done:
#         if d.poll() is None:
#             cont = False
#             break

subprocess.call(["dotnet", "build", "."])

print("Copying files")
for f in folders:
    if not config_only:
        copy_tree(f + "\\bin\\Debug\\net6.0", ".Build\\" + f)

    shutil.copy("base_config.json", ".Build\\" + f + "\\config.json")

    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".exe", ".Build")
    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".dll", ".Build")
    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".runtimeconfig.json", ".Build")
    
print(f"Completed in {round(time.time() - start, 2)}s")