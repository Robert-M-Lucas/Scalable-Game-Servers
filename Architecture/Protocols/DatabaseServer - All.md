# Database Server - All

All strings will be ASCII

Name length 16 characters (16 bytes)
Password length 16 character (16 bytes)

## Name and password requirements

- No spaces in names
- At least 2 chars for name
- At least 8 chars for password

## Verify player

Client sends

- [2 bytes] 0, request ID
- [16 bytes] username
- [16 bytes] password

If players is created or already exists,

- [2 bytes] 0, response ID
- [4 bytes] player ID
- [4 bytes] currency amount

Else

- [2 bytes] 1, response ID
