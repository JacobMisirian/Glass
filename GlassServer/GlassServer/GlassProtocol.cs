using System;

namespace GlassServer
{
    public enum GlassProtocol
    {
        RequestScreen =                 0x00,
        SendingScreen =                 0x01,
        Identify =                      0x02,
        RequestFile =                   0x03,
        SendingFile =                   0x04,
        RequestCurrentDirectory =       0x05,
        SendingCurrentDirectory =       0x06,
        SetCurrentDirectory =           0x07,
        RequestFileDownload =           0x08,
        RequestFileListing =            0x09,
        SendingFileListing =            0x0A,
        RequestDirectoryListing =       0x0B,
        SendingDirectoryListing =       0x0C,
        SendingError =                  0x0D,
        RequestDeleteFile =             0x0E,
        RequestDeleteDir =              0x0F,
        RequestMessageDisplay =         0x10,
        RequestProgramStart =           0x11,
        RequestProgramStartStdout =     0x12,
        SendingProgramStdout =          0x13,
        RequestFileCopy =               0x14,
        RequestFileMove =               0x15,
        RequestProcList =               0x16,
        SendingProcList =               0x17,
        RequestProcKill =               0x18,
        RequestCodeRun =                0x19,
        RequestSetMousePosition =       0x20,
        RequestLeftMouseClick =         0x21,
        RequestRightMouseClick =        0x22,
        RequestFileText =               0x23,
        SendingFileText =               0x24,
        RequestCreateDirectory =        0x25,
        RequestLogout =                 0x26,
        RequestRestart =                0x27,
        RequestShutdown =               0x28,
        RequestDllLoad =                0x29
    }
}