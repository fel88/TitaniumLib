using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class TcpClientWrapper : ITcpClient, IDisposable
    {
        public TcpClient Base;

        public TcpClientWrapper(TcpClient w)
        {
            Base = w;
        }

        public bool ClientToProxyClient;
        public TcpClientWrapper(IPEndPoint upStreamEndPoint)
        {
            Base = new TcpClient(upStreamEndPoint);
        }

        public string Host;
        public int Port;
        public int SendTimeout { get => Base.SendTimeout; set => Base.SendTimeout = value; }
        public int ReceiveTimeout { get => Base.ReceiveTimeout; set => Base.ReceiveTimeout = value; }




        public bool Connected => Base.Connected;

        public Socket Client { get => Base.Client; set => Base.Client = value; }
        public LingerOption LingerState { get => Base.LingerState; set => Base.LingerState = value; }
        public bool NoDelay { get => Base.NoDelay; set => Base.NoDelay = value; }


        public void Close()
        {
            Base.Close();
        }


        public Task ConnectAsync(IPAddress address, int port)
        {
            return Base.ConnectAsync(address, port);
        }

        public void Dispose()
        {
            Base.Dispose();
        }

        public IStream GetStream()
        {
            return new StreamWrapper(Base.GetStream()) { Host=Host,Port=Port, CacheWrapper = !ClientToProxyClient };
        }
    }


}

