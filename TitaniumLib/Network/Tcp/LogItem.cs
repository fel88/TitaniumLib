using System.IO;

namespace Titanium.Web.Proxy.Network.Tcp
{    
    public class LogItem
    {
        public string Str;
        public MemoryStream Mem = new MemoryStream();
        public bool IsRead;
    }

}

