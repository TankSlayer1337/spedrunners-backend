using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Controllers.Models;
using MoviesAPI.DynamoDB;
using MoviesAPI.DynamoDB.Models;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MoviesRepository _moviesRepository;

        public MoviesController(MoviesRepository moviesRepository)
        {
            _moviesRepository = moviesRepository;
        }

        [HttpPost]
        public async Task AddMovie([FromBody] AddMovieRequest request)
        {
            await _moviesRepository.AddMovieAsync(request);
        }

        [HttpDelete("{movieId}")]
        public async Task DeleteMovie(string movieId)
        {
            await _moviesRepository.DeleteMovieAsync(movieId);
        }

        [HttpGet]
        public async Task<List<Movie>> GetMovies()
        {
            return await _moviesRepository.GetMoviesAsync();
        }
    }
}
