# Game Server - Client

Largely same as Lobby Server

Two categories of packets. Server packets and Game Packets. If client tries to send game packet when game hasn't started 'game not started packet' returned

## Client to server

### 0 - Place piece

- [2 byte ] 3
- [1 byte ] uint place

## Server to client

### 0 - Board

- [2 byte ] 14
- [1 byte ] turn
- [1 byte ] player (0 or 1)
- [9 bytes] board (NOT UINTS!)
- [1 byte ] game over? (-1 no, 0 or 1 for winner)
