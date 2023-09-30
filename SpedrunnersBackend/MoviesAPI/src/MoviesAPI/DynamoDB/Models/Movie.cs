using Amazon.DynamoDBv2.DataModel;

namespace MoviesAPI.DynamoDB.Models
{
    public class Movie
    {
        [DynamoDBHashKey(AttributeNames.PK)]
        public string PK { get; } = "Movie";

        [DynamoDBRangeKey(AttributeNames.SK)]
        public string MovieId { get; init; }

        public Movie(string movieId)
        {
            MovieId = movieId;
        }
    }
}
