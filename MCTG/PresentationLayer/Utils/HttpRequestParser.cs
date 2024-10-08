using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.PresentationLayer.Utils
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }
    }

    public class HttpRequestParser
    {
        public HttpRequest Parse(string rawRequest)
        {
            HttpRequest request = new HttpRequest();
            string[] lines = rawRequest.Split("\r\n");

            // Parse request line
            string[] requestLine = lines[0].Split(' ');
            request.Method = requestLine[0];
            request.Path = requestLine[1];

            // Parse headers
            int i = 1;
            while (!string.IsNullOrWhiteSpace(lines[i]))
            {
                string[] header = lines[i].Split(": ");
                if (header.Length == 2)
                    request.Headers[header[0]] = header[1];
                i++;
            }

            // Parse body
            if (request.Method == "POST" || request.Method == "PUT")
            {
                StringBuilder bodyBuilder = new StringBuilder();
                for (int j = i + 1; j < lines.Length; j++)
                {
                    bodyBuilder.AppendLine(lines[j]);
                }
                request.Body = bodyBuilder.ToString().Trim();
            }

            return request;
        }
    }
}
