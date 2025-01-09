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

    public class HttpRequestParser
    {
        public HttpRequest Parse(string rawRequest)
        {
            HttpRequest request = new HttpRequest()
            {
                Method = string.Empty,
                Path = string.Empty,
                Body = string.Empty
            };
            string[] lines = rawRequest.Split("\r\n");

            string[] requestLine = lines[0].Split(' ');
            request.Method = requestLine[0];
            request.Path = requestLine[1];

            int i = 1;
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                string[] header = lines[i].Split(": ");
                if (header.Length == 2)
                {
                    string headerName = header[0];
                    string headerValue = header[1];

                    request.Headers.Add(headerName, headerValue);
                }
                i++;
            }
            i--;

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
