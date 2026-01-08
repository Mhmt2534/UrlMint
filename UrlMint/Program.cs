using Microsoft.EntityFrameworkCore;
using UrlMint.Domain.Interfaces;
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
