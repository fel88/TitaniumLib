using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Titanium.Web.Proxy.Network.Tcp
{
    public class WebCache
    {
        public string Path;
        public List<FakeRequest> Table = new List<FakeRequest>();

        public static void SaveCacheBinary(string str, FakeRequest[] reqs)
        {
            using (var fr = new FileStream(str, FileMode.Create))
            {
                //write index first                               
                //var roots = BuildIndex(reqs.ToArray());                
                int sectorSize = 2048;

                WriteInt(fr, sectorSize);
                WriteInt(fr, reqs.Length);
                var tableLocation = fr.Position;

                long[] poss = new long[reqs.Length];
                var offset = poss.Length * sectorSize;
                fr.Seek(offset, SeekOrigin.Current);

                if (fr.Position % sectorSize != 0)
                {
                    var last = sectorSize - fr.Position % sectorSize;
                    WriteZeroArray(fr, last);
                }


                for (int i = 0; i < reqs.Length; i++)
                {
                    var item = StreamWrapper.StaticRequests[i];
                    poss[i] = fr.Position / sectorSize;
                    WriteUtf8String(fr, item.Text.ToString());
                    WriteUtf8String(fr, item.Response.ToString());
                    WriteArray(fr, item.Mem.ToArray());
                    if (fr.Position % sectorSize != 0)
                    {
                        var last = sectorSize - fr.Position % sectorSize;
                        WriteZeroArray(fr, last);
                    }
                }

                //write index table
                fr.Seek(tableLocation, SeekOrigin.Begin);
                for (int i = 0; i < reqs.Length; i++)
                {
                    var item = StreamWrapper.StaticRequests[i];
                    var txt = item.Text;
                    WriteUtf8String(fr, item.Host);
                    WriteInt(fr, item.Port);
                    WriteLong(fr, poss[i]);
                    var l = sectorSize - fr.Position % sectorSize - sizeof(int);
                    if (txt.Length > l)
                    {
                        txt = txt.Substring(0, (int)l);
                    }
                    WriteUtf8String(fr, txt);
                    if (fr.Position % sectorSize != 0)
                    {
                        var last = sectorSize - fr.Position % sectorSize;
                        WriteZeroArray(fr, last);
                    }
                }
            }
        }
        public static void WriteArray(FileStream fs, byte[] data)
        {
            var b1 = data;
            var b1l = BitConverter.GetBytes(b1.Length);
            fs.Write(b1l, 0, b1l.Length);
            fs.Write(b1, 0, b1.Length);
        }
        public static void WriteInt(FileStream fs, int d)
        {
            var b1 = BitConverter.GetBytes(d);
            WriteArray(fs, b1);
        }


        public static int ReadLen(FileStream fs)
        {
            byte[] r = new byte[4];
            fs.Read(r, 0, r.Length);
            return BitConverter.ToInt32(r, 0);
        }

        public static int ReadInt(FileStream fs)
        {
            var l = ReadLen(fs);
            l = ReadLen(fs);

            return l;
        }
        public static long ReadLong(FileStream fs)
        {
            var ar = ReadArray(fs);
            return BitConverter.ToInt64(ar, 0);
        }

        public static void WriteLong(FileStream fs, long d)
        {
            var b1 = BitConverter.GetBytes(d);
            WriteArray(fs, b1);
        }
        public static void WriteUtf8String(FileStream fs, string str)
        {
            var b1 = Encoding.UTF8.GetBytes(str);
            WriteArray(fs, b1);

        }
        public static byte[] ReadArray(FileStream fs)
        {
            var len = ReadLen(fs);
            byte[] d = new byte[len];
            fs.Read(d, 0, len);
            return d;
        }
        public static string ReadUtf8String(FileStream fs)
        {
            var ar = ReadArray(fs);
            return Encoding.UTF8.GetString(ar);

        }
        public static void WriteZeroArray(FileStream fs, long len)
        {
            byte[] ar = new byte[len];
            fs.Write(ar, 0, ar.Length);
        }
        public void RestoreTable(string str)
        {
            Table.Clear();
            Table.AddRange(RestoreTableStatic(str));
            foreach (var item in Table)
            {
                item.Parent = this;
                item.IsOffline = true;
            }
        }

        public static FakeRequest[] RestoreTableStatic(string str)
        {
            List<FakeRequest> ret = new List<FakeRequest>();

            using (var fr = new FileStream(str, FileMode.Open, FileAccess.Read))
            {

                long posadd = 0;
                while (true)
                {
                    int sectorSize = ReadInt(fr);
                    if (sectorSize < 2048 || sectorSize > 32 * 1024 || sectorSize % 128 != 0) throw new Exception("wrong file");
                    int records = ReadInt(fr);
                    for (int i = 0; i < records; i++)
                    {
                        var host = ReadUtf8String(fr);
                        var port = ReadInt(fr);
                        var pos = ReadLong(fr);
                        var txt = ReadUtf8String(fr);
                        ret.Add(new FakeRequest() { FilePos = (int)(pos + posadd), Host = host, Port = port, Text = txt });
                        if (fr.Position % sectorSize != 0)
                        {
                            var last = sectorSize - fr.Position % sectorSize;
                            fr.Seek(last, SeekOrigin.Current);
                        }
                    }
                    var ss = ret.Last().FilePos * sectorSize;
                    fr.Seek(ss, SeekOrigin.Begin);
                    var txt1 = ReadUtf8String(fr);
                    var resp = ReadUtf8String(fr);
                    var mem = ReadArray(fr);
                    if (fr.Position % sectorSize != 0)
                    {
                        var last = sectorSize - fr.Position % sectorSize;
                        fr.Seek(last, SeekOrigin.Current);
                    }
                    if (fr.Position == fr.Length) break;
                    posadd += (fr.Position / sectorSize);
                }
            }

            return ret.ToArray();
        }

        public FakeRequest ReadRecord(FakeRequest freq)
        {
            var filePos = freq.FilePos;
            var f = new FakeRequest() { };
            using (var fr = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                var sectorSize = ReadInt(fr);
                fr.Seek(filePos * sectorSize, SeekOrigin.Begin);
                var txt = ReadUtf8String(fr);
                var resp = ReadUtf8String(fr);
                var mem = ReadArray(fr);
                f.Text = txt;
                f.Host = freq.Host;
                f.Port = freq.Port;
                f.Mem = new MemoryStream(mem);
                f.Response = new StringBuilder(resp);
            }
            return f;
        }
    }
}

