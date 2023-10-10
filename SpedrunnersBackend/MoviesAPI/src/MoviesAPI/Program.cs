using Amazon.DynamoDBv2;
using MoviesAPI.DynamoDB;
using MoviesAPI.DynamoDB.Wrappers;
using MoviesAPI.Utilities;

const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://spedrunners.cloudchaotic.com");
        });
});

// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

builder.Services.AddTransient<MoviesRepository>();
builder.Services.AddScoped<IDynamoDbContextWrapper, DynamoDbContextWrapper>();
builder.Services.AddScoped<AmazonDynamoDBClient>();
builder.Services.AddTransient<IEnvironmentVariableGetter, EnvironmentVariableGetter>();

var app = builder.Build();


app.UseHttpsRedirection();
app.UseCors(myAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();
