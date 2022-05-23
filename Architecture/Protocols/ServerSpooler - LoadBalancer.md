# Server Spooler - Load Balancer

## Spooler to Load Balancer

1. [2 bytes] Packet length
2. [4 bytes] IP
3. [2 bytes] Port
4. [1 byte ] Server Fill level
5. 2 -> 4 repeated for each lobby sever

## Load Balancer to Spooler

- [2 bytes] Load Balancer queue length
