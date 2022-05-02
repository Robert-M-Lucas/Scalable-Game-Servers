import sys

exclude = ["example.cs"]

path = " ".join(sys.argv[1:])

print(f"Path: {path}")

for e in exclude:
    if e in path:
        print(f"Excluded '{e}' found in path '{path}'")
        exit(0)

split_path = path.split("\\")
print(split_path)

namespace = split_path[split_path.index("Scalable Game Servers")+1].replace("-", "")

namespace_text = f"namespace"

with open(path, "r") as f:
    data = f.read()
    if namespace_text in data:
        print("Namespace found")
        exit(0)

with open(path, "w") as f:
    f.write(namespace_text + " " + namespace + ";\r\n" + data)