namespace MCTG.PresentationLayer.Utils
{
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string Body { get; set; }
    }

    public class HttpResponseParser
    {
        public string CreateResponse(HttpResponse response)
        {
            string statusLine = $"HTTP/1.1 {response.StatusCode} {response.StatusDescription}\r\n";
            string headers = $"Content-Type: text/plain\r\nContent-Length: {response.Body.Length}\r\n\r\n";
            string body = response.Body;

            return statusLine + headers + body;
        }
    }
}
