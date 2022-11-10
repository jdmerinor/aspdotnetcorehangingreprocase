using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var stopwatch = new Stopwatch();
stopwatch.Start();
var count = 0;
using var pacho = SHA512.Create();
var buffer = Encoding.ASCII.GetBytes("sadfhasdhfhasdfklsadjhfklsdahfojhdsaf");
while (stopwatch.Elapsed < TimeSpan.FromMinutes(1.5))
{
    count--;
    pacho.ComputeHash(buffer);
    count++;
}

app.Run();
