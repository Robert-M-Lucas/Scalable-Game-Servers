import os
import shutil
from distutils.dir_util import copy_tree
import subprocess
import time

start = time.time()
folders = ["Shared", "Game-Server", "Load-Balancer", "Lobby-Server", "Matchmaker", "Server-Spooler", "NoGui-Client"]

done = []
for f in folders[:-1]:
    done.append(subprocess.Popen(["dotnet", "build", f"{f}"]))

subprocess.call(["dotnet", "build", f"{folders[-1]}"])

cont = False
while not cont:
    cont = True
    for d in done:
        if d.poll() is None:
            cont = False
            break

for f in folders:
    copy_tree(f + "\\bin\\Debug\\net6.0", ".Build\\" + f)

    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".exe", ".Build")
    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".dll", ".Build")
    # shutil.copy(f + "\\bin\\Debug\\net6.0\\" + f + ".runtimeconfig.json", ".Build")
print(f"Completed in {round(time.time() - start, 2)}s")