using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "REST API for the RaceDay event management platform (PROG6212 POE Part 2)."
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("RaceDayCors", policy =>
    policy.WithOrigins("http://localhost:5001", "https://localhost:7001")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("RaceDayCors");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
