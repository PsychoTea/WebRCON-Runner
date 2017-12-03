# WebRCON-Runner
Allows you to run WebRCON commands from the command line

Usage:
```
WebRCON-Runner.exe {ip} {port} {password} "{command}" "[listen for]"
```

IP: the IP address of the server to connect to
Port: the RCON port of the server to connect to
Password: the RCON password of the server
Command: the command to be ran - enclosed in quotes
Listen For: the server response to wait for before exiting the program

Example usage:
```
WebRCON-Runner.exe 127.0.0.1 28016 password123 "restart 10" "Restarting in"
```

Any suggestions or improvements, feel free to create a fork and PR :)

Thanks to Maxaki for the idea.
