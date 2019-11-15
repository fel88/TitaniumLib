using System.IO;
using System.Text;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class FakeRequest
    {
        public string Host;
        public int Port;
        public string Text;
        public StringBuilder Response = new StringBuilder();
        public MemoryStream Mem = new MemoryStream();
    }
}

