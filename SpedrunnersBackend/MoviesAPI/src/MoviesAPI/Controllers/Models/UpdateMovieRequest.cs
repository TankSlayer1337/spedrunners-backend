namespace MoviesAPI.Controllers.Models
{
    public class UpdateMovieRequest
    {
        public string MovieId { get; init; }
        public string? Title { get; init; }
        public string? ImdbLink { get; init; }
        public string? PickedBy { get; init; }

        public UpdateMovieRequest(string movieId, string? title, string? imdbLink, string? pickedBy)
        {
            MovieId = movieId;
            Title = title;
            ImdbLink = imdbLink;
            PickedBy = pickedBy;
        }
    }
}
