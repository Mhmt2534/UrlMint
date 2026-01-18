using Microsoft.EntityFrameworkCore;
using UrlMint.Domain.Interfaces;
using UrlMint.Infrastructure.BackgroundTasks;
using UrlMint.Infrastructure.Encoding;
using UrlMint.Infrastructure.Persistence;
using UrlMint.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

//Database
builder.Services.AddDbContext<UrlMintDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        );
});

//DI Registration
builder.Services.AddSingleton<IUrlEncoder, Base62Encoder>();
builder.Services.AddScoped<IShortUrlRepository,ShortUrlRepository>();

//Queue
// Add the queue as a singleton (Only one queue)
builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
{
    return new BackgroundTaskQueue(100); // Queue capacity 100  
});

// Add worker services (will run in the background)
builder.Services.AddHostedService<QueuedHostedService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();





// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
