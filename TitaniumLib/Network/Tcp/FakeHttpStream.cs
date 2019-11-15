using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class FakeHttpStream : IStream
    {
        public FakeHttpStream(bool isHttps)
        {
            IsHttps = isHttps;
        }

        public string Host;
        public int Port;
        public bool IsHttps;
        public long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public long Length => throw new NotImplementedException();

        public bool CanWrite => true;

        public bool CanTimeout => throw new NotImplementedException();

        public bool CanSeek => throw new NotImplementedException();

        public bool CanRead => true;

        public int ReadTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int WriteTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public static bool UseFake { get; set; }

        public void Dispose()
        {

        }

        public void Flush()
        {

        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        Semaphore sem = new Semaphore(0, 1);


        public MemoryStream ResponseMem;

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {

            if (ResponseMem != null)
            {
                if (ResponseMem.CanRead)
                {
                    return ResponseMem.ReadAsync(buffer, offset, count);
                }
                ResponseMem = null;
            }
            
            sem.WaitOne();
            FakeRequest fr = null;
            lock (Requests)
            {
                fr = Requests.First();
                Requests.RemoveAt(0);
            }
            var ar = fr.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var spl = ar[0].Split(new string[] { "GET" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var url = spl[0];

            var str = Encoding.UTF8.GetString(buffer, offset, count);

            var resp1 = "HTTP/1.1 200 OK\r\n";
            var frr = StreamWrapper.StaticRequests.FirstOrDefault(z => z.Text.Contains(url));


            var resp2 = "<html><head></head><body><h1> Page not found </h1> there is no page in cache </body></html>\r\n";
           


            var b1 = Encoding.UTF8.GetBytes(resp2);
            var resp11 = "Content-Length: " + b1.Length + "\r\n";
            var ret = resp1 + resp11 + "\r\n" + resp2;
            if (frr != null)
            {
                ret = frr.Response.ToString();
                ResponseMem = new MemoryStream(frr.Mem.ToArray());
                var check = Encoding.UTF8.GetString(ResponseMem.ToArray());
                return ResponseMem.ReadAsync(buffer, offset, count);
            }



            var b = Encoding.UTF8.GetBytes(ret);
            Array.Copy(b, 0, buffer, offset, b.Length);
            return Task.Run(() => b.Length);

        }

        public long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public StringBuilder sb = new StringBuilder();
        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var str = Encoding.UTF8.GetString(buffer, offset, count);
            sb.Append(str);

            if (str.Contains("\r\n\r\n"))
            {
                lock (Requests)
                {
                    Requests.Add(new FakeRequest() { Text = sb.ToString(), Host = Host, Port = Port });
                }
                sb.Clear();
                sem.Release();
            }
            return Task.Run(() => { });
        }

        public List<FakeRequest> Requests = new List<FakeRequest>();
    }
}

