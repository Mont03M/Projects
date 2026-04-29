using server.apps.contexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("Backend Data");

// get required cors path
var allowedOrigins = builder.Configuration
    .GetRequiredSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

// enable cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// add controllers
builder.Services.AddControllers();

// Mongo
builder.Services.AddSingleton<MongoContext>();

// add services
builder.Services.AddAppServices();

// build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();
