# Lobby Server - Client
Note: This would be heavily modified for a full game

## Initial connection packet
No packet length!
1. [10 bytes] Name (10 chars)

Client request lobby response system

## Request format
1. [2 bytes] Packet length
2. [1 byte ] Request type
3. [x bytes] Payload

## Reply format
1. [2 bytes] Packet length
2. [1 byte ] Reply type
3. [x bytes] Payload

## Example interface - Type 1
1. [2 bytes] Packet length
2. [1 byte ] Request type - 00000001
3. [0 bytes] No payload

Server Response

1. [2 bytes] Packet length (13)
2. [1 byte] Response type - 0000001
3. [10 bytes] Player name