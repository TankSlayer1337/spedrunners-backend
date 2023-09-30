namespace MoviesAPI.Utilities
{
    public interface IEnvironmentVariableGetter
    {
        string Get(string name);
    }
}