using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassServer.Events
{
    public class FileTextRecievedEventArgs : EventArgs
    {
        public string FileText { get; set; }
    }
}
