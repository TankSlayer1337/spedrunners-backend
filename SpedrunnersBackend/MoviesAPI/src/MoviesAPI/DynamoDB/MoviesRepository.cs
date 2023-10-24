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

        public async Task UpdateMovieAsync(UpdateMovieRequest request)
        {
            var movie = await GetMovieAsync(request.MovieId);
            var updatedMovie = movie.CopyWithNewValues(request);
            await _dynamoDbContext.SaveAsync(updatedMovie);
        }

        public async Task DeleteMovieAsync(string movieId)
        {
            var movie = await GetMovieAsync(movieId);
            await _dynamoDbContext.DeleteAsync(movie);
        }

        private async Task<Movie> GetMovieAsync(string movieId)
        {
            var movies = await _dynamoDbContext.QueryAsync<Movie>(Movie.PK, Amazon.DynamoDBv2.DocumentModel.QueryOperator.Equal, new string[] { movieId });
            if (movies == null || !movies.Any())
            {
                throw new BadHttpRequestException($"Movie with ID {movieId} was not found in the database.");
            }
            return movies.Single();
        }

        public async Task<List<Movie>> GetMoviesAsync()
        {
            var movies = await _dynamoDbContext.QueryWithEmptyBeginsWith<Movie>(Movie.PK);
            return movies;
        }
    }
}
