namespace MCTG.Presentation.Utils
{
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public required string StatusDescription { get; set; }
        public required string Body { get; set; }
    }

    // Creates formatted HTTP response strings from HttpResponse objects
    public class HttpResponseParser
    {
        public string CreateResponse(HttpResponse response)
        {
            // Format status line
            string statusLine = $"HTTP/1.1 {response.StatusCode} {response.StatusDescription}\r\n";

            // Add headers
            string headers = $"Content-Type: text/plain\r\nContent-Length: {response.Body.Length}\r\n\r\n";

            // Combine all parts
            return statusLine + headers + response.Body;
        }
    }
}
