# Commands for Glass Server

These commands are used by the operator of the server to manage, view, and control the connecting clients
from a TUI interface. There are two types of commands, manager commands which are used for managing the 
clients as a whole, and control commands that are used for manipulating the clients themselves.

## Manager Commands


Command Syntax          | Description
---------------------   | -------------------
```list```    		| Displays the ID, username, and IP of each connected client.
info [ID]		| Displays the ID, username, and IP of the client with the specified [ID].
select [ID]		| Sets the selected client (for control) to that of the [ID].
selected		| Displays teh IP, username, and IP of the currently selected client.


## Control Commands


Command Syntax		| Description
----------------------  | --------------------
cd [DIR]		| Changes the client's current working directory to [DIR].
ls (DIR)		| Lists the files and directories inside the current working directory or that of (DIR) (optional).
pwd			| Displays the current working directory.
rm [FILE]		| Deletes the specified [FILE].
rmd [DIR]		| Deletes the specified [DIR].
mv [SOURCE_FILE] [DEST_FILE]
