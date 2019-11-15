using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class TcpClientMy : ITcpClient, IDisposable
    {

        IStream bstr;
        public TcpClientMy(IStream strm)
        {
            bstr = strm;
            

        }
        
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }


        public bool Connected => true;

        //public int Available => true;

        public Socket Client { get; set; }
        public LingerOption LingerState { get; set; }
        public bool NoDelay { get; set; }




        public void Close()
        {

        }


        public Task ConnectAsync(IPAddress address, int port)
        {
            return Task.Delay(0);
        }


        public void Dispose()
        {

        }

                
        public IStream GetStream()
        {
            return bstr;            
        }
    }



}

