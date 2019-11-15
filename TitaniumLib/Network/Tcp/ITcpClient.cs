using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public interface ITcpClient
    {

        
      
         int SendTimeout { get; set; }
     
         int ReceiveTimeout { get; set; }
        
    
         bool Connected { get; }
       
         Socket Client { get; set; }
        //
        // Summary:
        //     Gets or sets information about the linger state of the associated socket.
        //
        // Returns:
        //     A System.Net.Sockets.LingerOption. By default, lingering is disabled.
         LingerOption LingerState { get; set; }
        //
        // Summary:
        //     Gets or sets a value that disables a delay when send or receive buffers are not
        //     full.
        //
        // Returns:
        //     true if the delay is disabled, otherwise false. The default value is false.
         bool NoDelay { get; set; }
      
        //
        // Summary:
        //     Disposes this System.Net.Sockets.TcpClient instance and requests that the underlying
        //     TCP connection be closed.
         void Close();
      
      
         Task ConnectAsync(IPAddress address, int port);
        
    
         IStream GetStream();
        
    }

    public interface ISocket
    {

    }
}


