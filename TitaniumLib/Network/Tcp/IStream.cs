using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public interface IStream
    {
        
          long Position { get; set; }
       
          long Length { get; }
       
          bool CanWrite { get; }
      
        
          bool CanTimeout { get; }
    
          bool CanSeek { get; }
   
          bool CanRead { get; }
       
        
          int ReadTimeout { get; set; }
       
        
          int WriteTimeout { get; set; }

          
    
         void Dispose();
       
          void Flush();
     
        
          Task FlushAsync(CancellationToken cancellationToken);
      
          int Read(byte[] buffer, int offset, int count);
      
        
          Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
       
          long Seek(long offset, SeekOrigin origin);
        
      
          void SetLength(long value);
       
          void Write(byte[] buffer, int offset, int count);
      
        
          Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
      
     
     
    }
        
}

