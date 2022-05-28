# Lobby Server - Client

Note: This would be heavily modified for a full game

## Initial connection packet

No packet length!

1. [16 bytes] Name (10 chars)
2. [16 bytes] Password (10 chars)

Reply if incorrect password

1. [1 byte] 0

Reply if correct password

1. [1 byte] 1
2. [4 bytes] currency amount

Reply if account created

1. [1 byte] 2
2. [4 bytes] currency amount

## Client request lobby response system

### Request format

1. [2 bytes] Packet length
2. [1 byte ] Request type
3. [x bytes] Payload

### Reply format

1. [2 bytes] Packet length
2. [1 byte ] Reply type
3. [x bytes] Payload

### Example interface - Type 1

1. [2 bytes] Packet length
2. [1 byte ] Request type - 00000001
3. [0 bytes] No payload

Server Response

1. [2 bytes] Packet length (17)
2. [1 byte] Response type - 0000001
3. [16 bytes] Player name

### Example interface - Type 2

1. [2 bytes] Packet length
2. [1 byte ] Request type - 00000010
3. [0 bytes] No payload

Server Response

1. [2 bytes] Packet length (4)
2. [1 byte] Response type - 0000010
3. [1 bytes] player counter
