namespace MoviesAPI.Controllers.Models
{
    public class AddMovieRequest
    {
        public string Title { get; init; }
        public string? ImdbLink { get; init; }
        public string PickedBy { get; init; }

        public AddMovieRequest(string title, string? imdbLink, string pickedBy)
        {
            Title = title;
            ImdbLink = imdbLink;
            PickedBy = pickedBy;
        }
    }
}
