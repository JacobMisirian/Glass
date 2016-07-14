﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassClient
{
    public enum GlassProtocol
    {
        RequestScreen =             0x00,
        SendingScreen =             0x01,
        Identify =                  0x02,
        RequestFile =               0x03,
        SendingFile =               0x04,
        RequestCurrentDirectory =   0x05,
        SendingCurrentDirectory =   0x06,
        SetCurrentDirectory =       0x07,
        RequestFileDownload =       0x08,
        RequestFileListing =        0x09,
        SendingFileListing =        0x0A,
        RequestDirectoryListing =   0x0B,
        SendingDirectoryListing =   0x0C,
        SendingError =              0x0D,
        RequestDeleteFile =         0x0E,
        RequestDeleteDir =          0x0F,
        RequestMessageDisplay =     0x10
    }
}
