namespace MoviesAPI.Controllers
{
    public static class RequestExtensions
    {
        public static string GetAuthorizationHeader(this HttpRequest request)
        {
            return request.Headers["Authorization"].First();
        }
    }
}
