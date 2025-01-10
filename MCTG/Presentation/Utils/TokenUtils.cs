namespace MCTG.Presentation.Utils
{
    public static class TokenUtils
    {
        // Extracts authentication token from request headers
        public static string ExtractAuthToken(HttpRequest request)
        {
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return authHeader.Trim();
            }
            return string.Empty;
        }
    }
}
