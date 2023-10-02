using Amazon.DynamoDBv2.DocumentModel;

namespace MoviesAPI.DynamoDB
{
    public interface ITableWrapper
    {
        Search Query(Primitive hashKey, QueryFilter filter);
    }
}