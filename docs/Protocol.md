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
client: ```(byte)SendingFileListing (int)Amount [(string)Dir_Path]```
