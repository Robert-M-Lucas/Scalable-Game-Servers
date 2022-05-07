# Load Balancer - Client

## Load Balancer to Client
There are three packet types
1. 00000000 - Position in queue
2. 00000001 - Transfer to lobby server
3. 00000011 - Connection rejected - queue full

### 00 - Position in queue
1. [1 byte ] 00000000 - Packet code
2. [2 bytes] Position in queue

### 01 - Transfer to lobby server
1. [1 byte ] 00000001 - Packet code
2. [4 bytes] IP
3. [2 bytes] Port

### 11 - Connection rejected - queue full
1. [1 byte] 00000011 - Packet code