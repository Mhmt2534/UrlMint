using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlMint.Domain.Interfaces;
using UrlMint.Infrastructure.BackgroundTasks;
using UrlMint.Infrastructure.Encoding;
using UrlMint.Infrastructure.Persistence;
using UrlMint.Infrastructure.Repositories;
using UrlMint.Services;
using UrlMint.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//Database
builder.Services.AddDbContext<UrlMintDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        );
});

string redisConnectionString = builder.Configuration.GetConnectionString("Redis");

//Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "UrlMint_";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

//DI Registration
builder.Services.AddSingleton<IUrlEncoder, UrlEncoder>();
builder.Services.AddScoped<IShortUrlRepository,ShortUrlRepository>();
builder.Services.AddScoped<IShortUrlService,ShortUrlService>();

builder.Services.AddHostedService<UrlStatsBackgroundService>();


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
