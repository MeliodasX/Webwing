using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace myOwnWebServer
{
    class WebServer
    {
        private TcpListener listen;
        private String directory = @"";
        private IPAddress addr = IPAddress.Parse("127.0.0.1");
        private int port = 5000;   
        public WebServer(String webAddr, int webPort, String webRoot)
        {
            try
            {
                //Mount values from console arguments
                addr = IPAddress.Parse(webAddr);
                port = webPort;
                directory += webRoot;

                //Start listening on the given port  
                listen = new TcpListener(addr, port);
                listen.Start();
                Logger.WriteLog("[Server Start] @Address: " + addr + " @Port: " + port + " @Root: " + directory);  
                Thread thread = new Thread(new ThreadStart(StartListen));
                thread.Start();
            }
            catch (Exception e)
            {
                Logger.WriteLog("[Server Start] Exception: " + e.ToString());
            }
        }


        public void SendHeader(string httpVersion, string MIMEHeader, int totalBytes, string statusCode, ref Socket socket)
        {
            String buffer = "";
            
            // If Mime type not found, default it to text/html
            if (MIMEHeader.Length == 0)
            {
                MIMEHeader = "text/html"; 
            }
            buffer = buffer + httpVersion + statusCode + "\r\n";
            buffer = buffer + "Server: cx1193719-b\r\n";
            buffer = buffer + "Content-Type: " + MIMEHeader + "\r\n";
            buffer = buffer + "Accept-Ranges: bytes\r\n";
            buffer = buffer + "Content-Length: " + totalBytes + "\r\n\r\n";
            Byte[] sendData = Encoding.ASCII.GetBytes(buffer);
            SendToBrowser(sendData, ref socket);
        }

        public string GetMimeType(string requestedFile)
        {
            StreamReader sr;
            String line = "";
            String MimeType = "";
            String fileExtension = "";
            String MimeExtension = "";
            
            // Convert to lowercase  
            requestedFile = requestedFile.ToLower();
            int startPosition = requestedFile.IndexOf(".");
            fileExtension = requestedFile.Substring(startPosition);
            
            try
            {
                //Search Mime.dat for the appropriate Mime type  
                sr = new StreamReader("Mime.dat");
                while ((line = sr.ReadLine()) != null)
                {
                    line.Trim();
                    if (line.Length > 0)
                    {
                        startPosition = line.IndexOf(";");
                        line = line.ToLower();
                        MimeExtension = line.Substring(0, startPosition);
                        MimeType = line.Substring(startPosition + 1);
                        if (MimeExtension == fileExtension)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog("[Exception] An Exception Occurred : " + e.ToString());
            }
            if (MimeExtension == fileExtension)
                return MimeType;
            else
                return "";
        }

        public void SendToBrowser(String data, ref Socket socket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(data), ref socket);
        }
        public void SendToBrowser(Byte[] sendData, ref Socket socket)
        {
            int numBytes = 0;
            try
            {
                if (socket.Connected)
                {
                    if ((numBytes = socket.Send(sendData, sendData.Length, 0)) == -1)
                        Logger.WriteLog("[Exception] Socket Error");
                }
                else Logger.WriteLog("[Exception] Connection Dropped");
            }
            catch (Exception e)
            {
                Logger.WriteLog("[Exception] An Exception Occurred : " + e.ToString());
            }
        }

        public void StartListen()
        {
            int startPosition = 0;
            String request;
            String directoryName;
            String requestedFile;
            String errorMessage;
            String localDirectory;
            String physicalFilePath = "";
            String response = "";
            while (true)
            {
                //Accept a new connection  
                Socket socket = listen.AcceptSocket();
                if (socket.Connected)
                {
                    //Receive data from client side (browser)   
                    Byte[] receive = new Byte[1024];
                    socket.Receive(receive, receive.Length, 0);

                    //Convert Byte to String  
                    string buffer = Encoding.ASCII.GetString(receive);

                    string requestType = buffer.Substring(0, 3);

                    //Check to see if the request is anything else other than GET  
                    if (requestType != "GET")
                    {
                        Logger.WriteLog("[Exception] Not a GET Request. Closing Connection.");
                        socket.Close();
                        return;
                    }

                    // Monitor HTTP request
                    startPosition = buffer.IndexOf("HTTP", 1);  

                    //Cleanup Request
                    string httpVersion = buffer.Substring(startPosition, 8); 
                    request = buffer.Substring(0, startPosition - 1);
                    request.Replace("\\", "/");

                    //Extract the name of the requested file along with its directory
                    if ((request.IndexOf(".") < 1) && (!request.EndsWith("/")))
                    {
                        request = request + "/";
                    }
                    startPosition = request.LastIndexOf("/") + 1;
                    requestedFile = request.Substring(startPosition); 
                    directoryName = request.Substring(request.IndexOf("/"), request.LastIndexOf("/") - 3);

                    Logger.WriteLog("[Request] " + requestType + " " + directoryName + requestedFile);

                    //Check if the directory given is empty
                    localDirectory = directory + directoryName;
                    if (localDirectory.Length == 0)
                    {
                        errorMessage = "<H2>Request Directory is Incorrect</H2><Br>";
                        Logger.WriteLog("[Response] 404");
                        SendHeader(httpVersion, "", errorMessage.Length, " 404 Not Found", ref socket);
                        SendToBrowser(errorMessage, ref socket);
                        socket.Close();
                        continue;
                    }

                    //Check if the filename given is empty
                    if (requestedFile.Length == 0)
                    {
                        errorMessage = "<H2>No File Name Specified</H2>";
                        Logger.WriteLog("[Response] 404");
                        SendHeader(httpVersion, "", errorMessage.Length, " 404 Not Found", ref socket);
                        SendToBrowser(errorMessage, ref socket);
                        socket.Close();
                    }
                    
                    //Get the proper MIME type from the dat file for the requested file extension
                    String MimeType = GetMimeType(requestedFile);
                    physicalFilePath = localDirectory + requestedFile;

                    //Check if the file exists physically in the requested directory
                    if (File.Exists(physicalFilePath) == false)
                    {
                        errorMessage = "<H2>File Not Found</H2>";
                        Logger.WriteLog("[Response] 404");
                        SendHeader(httpVersion, "", errorMessage.Length, " 404 Not Found", ref socket);
                        SendToBrowser(errorMessage, ref socket);
                    }
                    else
                    {
                        int totalBytes = 0;
                        response = "";

                        //Read the file
                        FileStream fs = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);  
                        BinaryReader reader = new BinaryReader(fs);
                        byte[] bytes = new byte[fs.Length];
                        int read;
                        while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                        {  
                            response = response + Encoding.ASCII.GetString(bytes, 0, read);
                            totalBytes = totalBytes + read;
                        }
                        reader.Close();
                        fs.Close();

                        Logger.WriteLog("[Response] " + MimeType + " " + totalBytes + " Bytes " + "Server: cx1193719-b");

                        //Send the data to client (Browser)
                        SendHeader(httpVersion, MimeType, totalBytes, " 200 OK", ref socket);
                        SendToBrowser(bytes, ref socket);  
                    }
                    socket.Close();
                }
            }
        }
    }
}
