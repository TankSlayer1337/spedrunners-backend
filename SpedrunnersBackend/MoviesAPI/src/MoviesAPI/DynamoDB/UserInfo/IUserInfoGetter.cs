namespace MoviesAPI.DynamoDB.UserInfo
{
    public interface IUserInfoGetter
    {
        Task<string> GetUserIdAsync(string authorizationHeader);
    }
}