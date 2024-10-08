using MCTG.Dal;
using MCTG.PresentationLayer.Controller;
using MCTG.PresentationLayer.Services;
using MCTG.PresentationLayer.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MCTG.PresentationLayer
{
    internal class httpServer
    {
        private readonly TcpListener _listener;
        private readonly HttpRequestParser _requestParser;
        private readonly HttpResponseParser _responseParser;

        private readonly AuthController _authController;

        public httpServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 1001);
            _requestParser = new HttpRequestParser();
            _responseParser = new HttpResponseParser();

            // Initialize repositories
            IUserRepository playerRepository = new InMemoryUserRepository();

            // Initialize services
            AuthService authService = new AuthService(playerRepository);
            UserService userService = new UserService(playerRepository);

            // Initialize controllers
            _authController = new AuthController(authService);

        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine($"Server started on {_listener.LocalEndpoint}");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                using var reader = new StreamReader(client.GetStream());


                //// ----- 1. Read the HTTP-Request -----
                //string? line;

                //// 1.1 first line in HTTP contains the method, path and HTTP version
                //line = reader.ReadLine();
                //if (line != null)
                //    Console.WriteLine(line);

                //// 1.2 read the HTTP-headers (in HTTP after the first line, until the empy line)
                //int content_length = 0; // we need the content_length later, to be able to read the HTTP-content
                //while ((line = reader.ReadLine()) != null)
                //{
                //    Console.WriteLine(line);
                //    if (line == "")
                //    {
                //        break;  // empty line indicates the end of the HTTP-headers
                //    }

                //    // Parse the header
                //    var parts = line.Split(':');

                //    if (parts.Length == 2 && parts[0] == "Content-Length")
                //    {
                //        content_length = int.Parse(parts[1].Trim());
                //    }
                //}

                //// 1.3 read the body if existing
                //if (content_length > 0)
                //{
                //    var data = new StringBuilder(200);
                //    char[] chars = new char[1024];
                //    int bytesReadTotal = 0;

                //    while (bytesReadTotal < content_length)
                //    {
                //        var bytesRead = reader.Read(chars, 0, chars.Length);
                //        bytesReadTotal += bytesRead;
                //        if (bytesRead == 0)
                //            break;
                //        data.Append(chars, 0, bytesRead);
                //    }
                //    Console.WriteLine(data.ToString());
                //}

                //// ----- 2. Do the processing -----
                //// .... 

                //Console.WriteLine("----------------------------------------");

                //// ----- 3. Write the HTTP-Response -----
                //var writerAlsoToConsole = new StreamTracer(writer);  // we use a simple helper-class StreamTracer to write the HTTP-Response to the client and to the console

                //writerAlsoToConsole.WriteLine("HTTP/1.0 200 OK");    // first line in HTTP-Response contains the HTTP-Version and the status code
                //writerAlsoToConsole.WriteLine("Content-Type: text/html; charset=utf-8");     // the HTTP-headers (in HTTP after the first line, until the empy line)
                //writerAlsoToConsole.WriteLine();
                //writerAlsoToConsole.WriteLine("<html><body><h1>Hello World!</h1></body></html>");    // the HTTP-content (here we just return a minimalistic HTML Hello-World)


                //// Read the request
                string rawRequest = ReadRequest(reader);
                Console.WriteLine($"\nRaw Request:\n{rawRequest}");

                //// Parse the request
                HttpRequest request = _requestParser.Parse(rawRequest);

                //// Route the request
                HttpResponse response = RouteRequest(request);

                //// Create raw response
                string rawResponse = _responseParser.CreateResponse(response);
                Console.WriteLine($"\nRaw Response:\n{rawResponse}");

                //// Send response
                writer.Write(rawResponse);

                client.Close();
                Console.WriteLine("Client disconnected.\n");
            }
        }

        private string ReadRequest(StreamReader reader)
        {
            StringBuilder requestBuilder = new StringBuilder();
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                requestBuilder.AppendLine(line);
            }

            // Read body if present
            if (requestBuilder.ToString().Contains("Content-Length:"))
            {
                int contentLength = GetContentLength(requestBuilder.ToString());
                if (contentLength > 0)
                {
                    char[] buffer = new char[contentLength];
                    reader.ReadBlock(buffer, 0, contentLength);
                    requestBuilder.Append(buffer);
                }
            }

            return requestBuilder.ToString();
        }

        private int GetContentLength(string request)
        {
            string[] lines = request.Split("\r\n");
            foreach (var line in lines)
            {
                if (line.StartsWith("Content-Length:"))
                {
                    string[] parts = line.Split(": ");
                    if (parts.Length == 2 && int.TryParse(parts[1], out int length))
                        return length;
                }
            }
            return 0;
        }

        private HttpResponse RouteRequest(HttpRequest request)
        {
            if (request.Path == "/register" && request.Method == "POST")
            {
                string result = _authController.Register(request.Body);
                return CreateHttpResponse(result);
            }
            else if (request.Path == "/login" && request.Method == "POST")
            {
                string result = _authController.Login(request.Body);
                return CreateHttpResponse(result);
            }
            else
            {
                return new HttpResponse
                {
                    StatusCode = 404,
                    StatusDescription = "Not Found",
                    Body = "Resource not found"
                };
            }
        }

        private string ExtractTokenFromHeaders(Dictionary<string, string> headers)
        {
            if (headers.ContainsKey("Authorization"))
            {
                string authHeader = headers["Authorization"];
                return TokenUtils.ExtractToken(authHeader);
            }
            return null;
        }

        private string ExtractOpponentUsername(string body)
        {
            var parameters = body.Split('&');
            foreach (var param in parameters)
            {
                var keyValue = param.Split('=');
                if (keyValue.Length == 2 && keyValue[0] == "opponent")
                    return keyValue[1];
            }
            return null;
        }

        private HttpResponse CreateHttpResponse(string responseString)
        {
            string[] parts = responseString.Split('\n', 2);
            int statusCode = int.Parse(parts[0].Split(' ')[0]);
            string statusDescription = parts[0].Split(' ')[1];
            string body = parts.Length > 1 ? parts[1] : "";

            return new HttpResponse
            {
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                Body = body
            };
        }
    }
}
