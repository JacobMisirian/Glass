# Glass Protocol

This goes over the networked protocol of how the server and clients communicate with eachother
and the syntax of what gets written and when. Note: This is all written in the context of C#'s
BinaryReader and BinaryWriter classes, which have their own way of encoding data to network
streams.

## Server Requests and Client Responses

### RequestScreen (0x00) - SendingScreen (0x01)

Server: ```(byte)RequestScreen```

Client: ```(byte)SendingScreen (int)Width (int)Height [(byte)R, (byte)B, (byte)G]...```

### RequestFile (0x03) - SendingFile (0x04)

Server: ```(byte)RequestFile (string)Path```

Client: ```(byte)SendingFile (long)Length (byte[])data```

### SendingFile (0x04)

Server: ```(byte)SendingFile (long)Length (byte[])data```

### RequestCurrentDirectory (0x05) - SendingCurrentDirectory (0x06)

Server: ```(byte)RequestCurrentDirectory```

Client: ```(byte)SendingCurrentDirectory (string)CurrentDirectory```

### SetCurrentDirectory (0x07)

Server: ```(byte)SetCurrentDirectory (string)CurrentDirectory```

### RequestFileDownload (0x08)

Server: ```(byte)RequestFileDownload (string)Url (string)Save_Path```

### RequestFileListing (0x09) - SendingFileListing (0x0A)

Server: ```(byte)RequestFileListing (string)Path```

Client: ```(byte)SendingFileListing (int)Amount [(string)File_Path]```

### RequestDirectoryListing (0x0B) - SendingDirectoryListing (0x0C)

Server: ```(byte)RequestDirectoryListing (string)Path```

Client: ```(byte)SendingFileListing (int)Amount [(string)Dir_Path]```

### RequestDeleteFile (0x0E)

Server: ```(byte)RequestDeleteFile (string)Path```

### RequestDeleteDir (0x0F)

Server: ```(byte)RequestDeleteDir (string)Path```

### RequestMessageDisplay (0x10)

Server: ```(byte)RequestMessageDisplay (string)Msg```

### RequestProgramStart (0x11)

Server: ```(byte)RequestProgramStart (string)Path (string)Args```

### RequestProgramStartStdout (0x12) - SendingProgramStdout (0x13)

Server: ```(byte)RequestProgramStartStdout (string)Path (string)Args```

Client: ```(byte)SendingStdout (string)Stdout```

### RequestFileCopy (0x14)

Server: ```(byte)RequestFileCopy (string)Source_Path (string)Dest_Path```

### RequestFileMove (0x15)

Server: ```(byte)RequestFileMove (string)Source_Path (string)Dest_Path```

### RequestProcList (0x16) - SendingProcList (0x17)

Server: ```(byte)RequestingProcList```

Client: ```(byte)SendingProcList (string)ProcList```

### RequestProcKill (0x18)

Server: ```(byte)RequestProcKill (string)ProcName```

### RequestCodeRun (0x19)

Server: ```(byte)RequestCodeRun (string)Hassium_Code```

### RequestSetMousePosition (0x20)

Server: ```(byte)RequestSetMousePosition (int)X (int)Y```

### RequestLeftMouseClick (0x21)

Server: ```(byte)RequestLeftMouseClick```

### RequestRightMouseClick (0x22)

Server: ```(byte)RequestRightMouseClick```

### RequestFileText (0x23) - SendingFileText (0x24)

Server: ```(byte)RequestFileText (string)Path```

Client: ```(byte)SendingFileText (string)Text```

### RequestCreateDirectory (0x25)

Server: ```(byte)RequestCreateDirectory (string)Path```

### RequestLogout (0x26)

Server: ```(byte)RequestLogout```

### RequestRestart (0x27)

Server: ```(byte)RequestRestart```

### RequestShutdown (0x28)

Server: ```(byte)RequestShutdown```
