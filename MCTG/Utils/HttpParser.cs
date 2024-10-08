using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.Utils
{
    internal class HttpParser
    {
        public (string method, string endpoint, string body) ParseRequest(NetworkStream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                string method = tokens[0];
                string endpoint = tokens[1];
                string body = null;

                // Parse body if POST request
                if (method == "POST")
                {
                    while (!string.IsNullOrEmpty(line))
                    {
                        line = reader.ReadLine();
                    }
                    body = reader.ReadToEnd();
                }

                return (method, endpoint, body);
            }
        }

        public void SendResponse(NetworkStream stream, string response, int statusCode = 200)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine($"HTTP/1.1 {statusCode} OK");
                writer.WriteLine("Content-Type: text/plain");
                writer.WriteLine($"Content-Length: {response.Length}");
                writer.WriteLine();
                writer.WriteLine(response);
            }
        }
    }
}
