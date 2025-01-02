namespace MCTG.PresentationLayer.Utils
{
    public static class TokenUtils
    {
        public static string ExtractAuthToken(HttpRequest request)
        {
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                // Remove "Bearer " prefix if present
                return authHeader.Replace("Bearer ", "").Trim();
            }
            return string.Empty;
        }
    }
}
