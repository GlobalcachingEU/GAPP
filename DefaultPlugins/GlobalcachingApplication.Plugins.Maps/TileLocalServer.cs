using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media.Imaging;

namespace GlobalcachingApplication.Plugins.Maps
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer srv;

        private Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();


        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }


        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        public void process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
            try
            {
                parseRequest();
                readHeaders();
                if (http_method.Equals("GET"))
                {
                    handleGETRequest();
                }
                else if (http_method.Equals("POST"))
                {
                    handlePOSTRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                writeFailure();
            }
            try
            {
                outputStream.Flush();
                // bs.Flush(); // flush any remaining output
                inputStream = null; outputStream = null; // bs = null;            
                socket.Close();
            }
            catch
            {
            }
        }

        public void parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];

            Console.WriteLine("starting: " + request);
        }

        public void readHeaders()
        {
            Console.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                string value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }

        private const int BUF_SIZE = 4096;
        public void handlePOSTRequest()
        {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    Console.WriteLine("starting Read, to_read={0}", to_read);

                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    Console.WriteLine("read finished, numread={0}", numread);
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }

        public void writeSuccess(string content_type = "text/html")
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeFailure()
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }

    public abstract class HttpServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();
            while (is_active)
            {
                TcpClient s = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.IsBackground = true;
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }

    public class OSMTileHttpServer : HttpServer
    {
        private Framework.Interfaces.ICore _core = null;
        private MapProviders.OSMOffline.MapControlFactoryOSMOffline _mapControlFactory = null;

        public OSMTileHttpServer(int port, Framework.Interfaces.ICore core)
            : base(port)
        {
            _core = core;
            _mapControlFactory = new MapProviders.OSMOffline.MapControlFactoryOSMOffline(_core);
            _mapControlFactory.Init();
        }

        public void SettingsChanged()
        {
            if (_mapControlFactory != null)
            {
                _mapControlFactory.SettingsChanged();
            }
        }

        public override void handleGETRequest(HttpProcessor p)
        {
            //   /zoom/x/y.png
            if (p.http_url.ToLower().EndsWith(".png"))
            {
                string[] parts = p.http_url.Split(new char[] { '/', '\\', '.' }, StringSplitOptions.RemoveEmptyEntries);
                _mapControlFactory.LastRequestedZoomLevel = int.Parse(parts[0]);
                BitmapImage bmpImg = _mapControlFactory.TileGenerator.GetTileImage(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                using (MemoryStream outStream = new MemoryStream())
                {
                    PngBitmapEncoder enc = new PngBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bmpImg));
                    p.writeSuccess("image/png");
                    enc.Save(outStream);
                    byte[] data = outStream.ToArray();
                    p.outputStream.BaseStream.Write(data, 0, data.Length);
                    p.outputStream.BaseStream.Flush();
                }
            }
            else
            {
                p.writeFailure();
            }
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            //NA
        }
    }

    public class TileLocalServer
    {
        private OSMTileHttpServer httpServer = null;

        public bool Active { get; private set; }

        public TileLocalServer()
        {
            Active = false;
        }

        public void SettingsChanged()
        {
            if (httpServer != null)
            {
                httpServer.SettingsChanged();
            }
        }

        public void Start(Framework.Interfaces.ICore core)
        {
            if (httpServer == null)
            {
                try
                {
                    httpServer = new OSMTileHttpServer(Properties.Settings.Default.TileServerPort, core);
                    Thread thread = new Thread(new ThreadStart(httpServer.listen));
                    thread.IsBackground = true;
                    thread.Start();
                    Active = true;
                }
                catch
                {
                    httpServer = null;
                }
            }
        }

    }
}
