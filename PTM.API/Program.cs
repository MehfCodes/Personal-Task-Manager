using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using PTM.API;
using PTM.API.Middlewares;
using PTM.Application;
using PTM.Infrastructure;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    config.Filters.Add(new AuthorizeFilter(policy));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters
    .Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});
builder.Services.AddEndpointsApiExplorer();

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug(); 

// builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Custom DI
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddExceptionHandlers();

// Swagger
builder.Services.AddSwagger();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();             // serve swagger.json
    app.UseSwaggerUI(c =>         // serve swagger ui
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PTM API V1");
        c.RoutePrefix = "swagger"; // /swagger/index.html
    });
}
// Custom middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();
app.MapControllers();

// app.MapGet("/test-api", () => "API is Working...");

app.Run();

public partial class Program { }