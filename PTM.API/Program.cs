using System.Text.Json;
using System.Text.Json.Serialization;
using PTM.API.Middlewares;
using PTM.Application;
using PTM.Infrastructure;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters
    .Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Custom DI
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Custom middlewares
app.UseMiddleware<ProtectedRoute>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/test-api", () => "API is Working...");

app.Run();

