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
selected		| Displays the IP, username, and IP of the currently selected client.


## Control Commands


Command Syntax		| Description
----------------------  | --------------------
```cd [DIR]```			| Changes the client's current working directory to [DIR].
ls (DIR)			| Lists the files and directories inside the current working directory or that of (DIR) (optional).
pwd				| Displays the current working directory.
rm [FILE]			| Deletes the specified [FILE].
rmd [DIR]			| Deletes the specified [DIR].
mv [SOURCE_PATH] [DEST_PATH] 	| Moves the file at [SOURCE_PATH] to [DEST_PATH].
cp [SOURCE_PATH] [DEST_PATH] 	| Copes the file at [SOURCE_PATH] to [DEST_PATH].
mkdir [DIR]			| Creates a directory at [DIR].
cat [FILE]			| Displays the text inside of [FILE].
dl [URL] [DEST_PATH]		| Downloads a file from [URL] to [DEST_PATH] location.
put [LOCAL_PATH] [DEST_PATH] 	| Sends a file from the server's [LOCAL_PATH] to the client's [DEST_PATH].
get [CLIENT_PATH] [DEST_PATH] 	| Transfers a file from the client's [CLIENT_PATH] to the server's [DEST_PATH].
start [CLIENT_PATH] [ARGS]  	| Starts the exe on the client's [CLIENT_PATH] with [ARGS].
getstart [CLIENT_PATH] [ARGS] 	| Starts the exe on the client's [CLIENT_PATH] with [ARGS] and displays the STDOUT.
proclist			| Displays the processes running on the client.
kill [PROC_NAME]        	| Kills the process named [PROC_NAME].
coderun [LOCAL_PATH]		| Starts the Hassium script located on the server's [LOCAL_PATH] on the client's machine.
mp [X] [Y] 			| Sets the mouse pointer on the client's machine to [X], [Y].
leftclick			| Simulates a left mouse click on the client's machine.
rightclick			| Simulates a right mouse click on the client's machine.
msg [MESSAGE]			| Displays a message box with the [MESSAGE] as it's text on the client's machine.
logout				| Starts the windows logout on the client's machine.
restart				| Restarts the client's machine.
shutdown			| Shuts down the client's machine.
