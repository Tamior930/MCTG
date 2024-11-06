using System.Text;

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
        //Parse HTTP Requests
        public HttpRequest Parse(string rawRequest)
        {
            HttpRequest request = new HttpRequest();
            string[] lines = rawRequest.Split("\r\n");

            // Parse the first line for the HTTP method and path
            string[] requestLine = lines[0].Split(' ');
            request.Method = requestLine[0];
            request.Path = requestLine[1];

            // Parse headers
            int i = 1;
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                string[] header = lines[i].Split(": ");
                if (header.Length == 2) // Check if we have a valid header (name and value)
                {
                    string headerName = header[0];  // Name of the header
                    string headerValue = header[1]; // Value of the header

                    request.Headers.Add(headerName, headerValue);
                }
                i++;
            }
            i--;

            // Body
            var body = new StringBuilder();
            while (i < lines.Length)
            {
                body.AppendLine(lines[i]);
                i++;
            }
            request.Body = body.ToString().TrimEnd(); // Remove trailing newline

            return request;
        }
    }
}
