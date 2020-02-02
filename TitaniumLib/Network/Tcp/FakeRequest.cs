using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class FakeRequest
    {
        public string Host;
        public WebCache Parent;
        public int Port;
        public string Text;
        public StringBuilder Response = new StringBuilder();
        public MemoryStream Mem = new MemoryStream();
        public int FilePos;
        public bool IsOffline;

        public byte[] GetBody()
        {
            
            var mm = Mem;
            if (IsOffline)
            {
                var req = Parent.ReadRecord(this);
                mm = req.Mem;
            }

            var m = mm.ToArray();
            byte[] dat = new byte[4];
            int cuti = -1;
            for (int i = 0; i < m.Length; i++)
            {
                for (int j = 0; j < dat.Length - 1; j++)
                {
                    dat[j] = dat[j + 1];
                }
                dat[dat.Length - 1] = m[i];
                if (dat[0] == 0x0D && dat[1] == 0x0A && dat[2] == 0x0D && dat[3] == 0x0A)
                {
                    cuti = i + 1;
                    break;
                }
            }
            m = m.Skip(cuti).ToArray();


            return m;
        }
        public byte[] GetHeaders()
        {
            var m = Mem.ToArray();
            byte[] dat = new byte[4];
            int cuti = -1;
            for (int i = 0; i < m.Length; i++)
            {
                for (int j = 0; j < dat.Length - 1; j++)
                {
                    dat[j] = dat[j + 1];
                }
                dat[dat.Length - 1] = m[i];
                if (dat[0] == 0x0D && dat[1] == 0x0A && dat[2] == 0x0D && dat[3] == 0x0A)
                {
                    cuti = i + 1;
                    break;
                }
            }
            m = m.Take(cuti - 4).ToArray();


            return m;
        }

        public List<Tuple<string, string>> Headers = new List<Tuple<string, string>>();
        public void ExtractHeaders()
        {
            var rr = Parent.ReadRecord(this);

            var h = rr.GetHeaders();
            var str = Encoding.UTF8.GetString(h);
            var sp = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            Headers.Clear();
            foreach (var s in sp)
            {
                var ind1 = s.IndexOf(':');
                if (ind1 == -1) continue;
                Tuple<string, string> r = new Tuple<string, string>(s.Substring(0, ind1), s.Substring(ind1 - 1));
                Headers.Add(r);
            }
        }

    }
}

