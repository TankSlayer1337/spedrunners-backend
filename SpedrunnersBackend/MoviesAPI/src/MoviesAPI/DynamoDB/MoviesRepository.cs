using MoviesAPI.Controllers.Models;
using MoviesAPI.DynamoDB.Models;
using MoviesAPI.DynamoDB.Wrappers;

namespace MoviesAPI.DynamoDB
{
    public class MoviesRepository
    {
        private readonly IDynamoDbContextWrapper _dynamoDbContext;

        public MoviesRepository(IDynamoDbContextWrapper dynamoDbContext)
        {
            _dynamoDbContext = dynamoDbContext;
        }

        public async Task AddMovieAsync(AddMovieRequest request)
        {
            var movie = Movie.Create(request);
            await _dynamoDbContext.SaveAsync(movie);
        }

        public async Task<List<Movie>> GetMoviesAsync()
        {
            var movies = await _dynamoDbContext.QueryWithEmptyBeginsWith<Movie>(Movie.PK);
            return movies;
        }
    }
}
