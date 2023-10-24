using Amazon.DynamoDBv2.DataModel;
using MoviesAPI.Controllers.Models;
using MoviesAPI.DynamoDB.PropertyConversion;
using System.Globalization;

namespace MoviesAPI.DynamoDB.Models
{
    public class Movie
    {
        [DynamoDBHashKey(AttributeNames.PK)]
        public static string PK { get; set; } = "Movie";

        [DynamoDBRangeKey(AttributeNames.SK, typeof(MovieIdConverter))]
        public string MovieId { get; init; }

        [DynamoDBProperty(AttributeNames.Created)]
        public string Created { get; init; } // use ISO 8601

        [DynamoDBProperty(AttributeNames.Title)]
        public string Title { get; init; }

        [DynamoDBProperty(AttributeNames.ImdbLink)]
        public string? ImdbLink { get; init; }

        [DynamoDBProperty(AttributeNames.PickedBy)]
        public string PickedBy { get; init; }

        public static Movie Create(AddMovieRequest request)
        {
            return new Movie
            {
                MovieId = Guid.NewGuid().ToString(),
                Created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                Title = request.Title,
                ImdbLink = request.ImdbLink,
                PickedBy = request.PickedBy,
            };
        }

        public Movie CopyWithNewValues(UpdateMovieRequest request)
        {
            return new Movie
            {
                MovieId = MovieId,
                Created = Created,
                Title = request.Title ?? Title,
                ImdbLink = request.ImdbLink ?? ImdbLink,
                PickedBy = request.PickedBy ?? PickedBy
            };
        }
    }
}
