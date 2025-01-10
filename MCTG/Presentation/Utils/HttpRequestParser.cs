using System.Text;

namespace MCTG.Presentation.Utils
{
    public class HttpRequest
    {
        public required string Method { get; set; }
        public required string Path { get; set; }
        public required string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }
    }

    // Parses raw HTTP request strings into structured HttpRequest objects
    public class HttpRequestParser
    {
        public HttpRequest Parse(string rawRequest)
        {
            // Initialize new request object
            HttpRequest request = new HttpRequest()
            {
                Method = string.Empty,
                Path = string.Empty,
                Body = string.Empty
            };

            // Split request into lines
            string[] lines = rawRequest.Split("\r\n");

            // Parse request line (method and path)
            string[] requestLine = lines[0].Split(' ');
            request.Method = requestLine[0];
            request.Path = requestLine[1];

            // Parse headers
            int i = 1;
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                string[] header = lines[i].Split(": ");
                if (header.Length == 2)
                {
                    request.Headers.Add(header[0], header[1]);
                }
                i++;
            }
            i--;

            // Parse body
            var body = new StringBuilder();
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                body.AppendLine(lines[i]);
                i++;
            }
            request.Body = body.ToString();

            return request;
        }
    }
}
