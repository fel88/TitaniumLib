using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class StreamWrapper : IStream
    {
        Stream Base;

        public static int NewId = 0;
        public int Id = 0;
        public static List<StreamWrapper> Wrappers = new List<StreamWrapper>();
        public StreamWrapper(Stream b)
        {
            lock (Wrappers)
            {
                Id = NewId++;
                Wrappers.Add(this);
            }
            Base = b;
        }
        public long Position { get => Base.Position; set => Base.Position = value; }

        public long Length => Base.Length;

        public bool CanWrite => Base.CanWrite;

        public bool CanTimeout => Base.CanTimeout;

        public bool CanSeek => Base.CanSeek;

        public bool CanRead => Base.CanRead;

        public int ReadTimeout { get => Base.ReadTimeout; set => Base.ReadTimeout = value; }
        public int WriteTimeout { get => WriteTimeout; set => WriteTimeout = value; }






        public void Dispose()
        {
            Base.Dispose();
        }




        public bool CacheWrapper = false;

        public void Flush()
        {
            Base.Flush();
        }


        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return Base.FlushAsync(cancellationToken);
        }


        public int Read(byte[] buffer, int offset, int count)
        {
            return Base.Read(buffer, offset, count);
        }
        public List<LogItem> Logs = new List<LogItem>();
        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var ret = Base.ReadAsync(buffer, offset, count, cancellationToken);
            ret = ret.ContinueWith((z) =>
            {
                var h = this.GetHashCode();
                var res = z.Result;
                var str = Encoding.UTF8.GetString(buffer, offset, res);

                if (!CacheWrapper) return res;
                Logs.Add(new LogItem() { Str = str, IsRead = true });
                sb.Append(str);
                if (str.ToLower().Contains("chunked".ToLower()))
                {

                }
                if (Requests.Any())
                {
                    var last = Requests.Last();
                    last.Response.Append(str);
                    last.Mem.Write(buffer, offset, res);
                }

                return res;
            });
            return ret;
        }


        public long Seek(long offset, SeekOrigin origin)
        {
            return Base.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            Base.SetLength(value);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Base.Write(buffer, offset, count);
        }
        public StringBuilder sb = new StringBuilder();
        public StringBuilder sbw = new StringBuilder();


        List<FakeRequest> Requests = new List<FakeRequest>();
        public static FakeRequest[] StaticRequests
        {
            get
            {
                return Caches.SelectMany(z => z.Table.ToArray()).ToArray();
            }
        }


        public static WebCache ActiveCache;
        public static List<WebCache> Caches=new  List<WebCache>();
        //public static List<FakeRequest> RestoredRequests = new List<FakeRequest>();

        public string ExcludeAcceptEncoding(string str)
        {
            var a = str;
            if (a.Contains("Accept-Encoding"))
            {
                var ind = a.IndexOf("Accept-Encoding");
                var ind2 = a.IndexOf("\r\n", ind);
                var a1 = a.Substring(0, ind);
                var a2 = a.Substring(ind2 + 2);
                a = a1 + a2;
            }
            return a;
        }


        public string Host;
        public int Port;

        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {

            var str2 = Encoding.UTF8.GetString(buffer, offset, count);
            /*if (str2.Contains("Accept-Encoding"))
            {
                str2 = ExcludeAcceptEncoding(str2);
                var bb = Encoding.UTF8.GetBytes(str2);
                buffer = bb;
                offset = 0;
                count = bb.Length;
                //return Task.Run(() => { });
            }*/
            return Base.WriteAsync(buffer, offset, count, cancellationToken).ContinueWith((z) =>
            {
                var str = Encoding.UTF8.GetString(buffer, offset, count);

                if (!CacheWrapper) return;
                Logs.Add(new LogItem() { Str = str, IsRead = false });
                sbw.Append(str);
                if (str.Contains("Content-Length"))
                {

                }
                if (str.ToLower().Contains("Transfer-Encoding: chunked".ToLower()))
                {

                }
                if (str.ToLower().Contains("chunked".ToLower()))
                {

                }
                if (str.Contains("\r\n\r\n"))
                {
                    var h = this.GetHashCode();
                    //if (sb.ToString().Contains("GET"))
                    {
                        lock (Requests)
                        {
                            var a = sbw.ToString();
                            /*if (a.Contains("Accept-Encoding"))
                            {
                                var ind = a.IndexOf("Accept-Encoding");
                                var ind2 = a.IndexOf("\r\n", ind);
                                var a1=a.Substring(0, ind);
                                var a2 = a.Substring(ind2 + 2);
                                a = a1 + a2;
                            }*/
                            Requests.Add(new FakeRequest() { Text = a, Host = Host, Port = Port });
                            ActiveCache.Table.Add(Requests.Last());
                            //StaticRequests.Add(Requests.Last());
                        }
                    }
                    sbw.Clear();
                }
            });
        }


    }
}

