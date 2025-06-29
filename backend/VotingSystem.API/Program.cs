using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using VotingSystem.API.Data;
using VotingSystem.API.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, // ������������ ���������� ��������� �������
            maxRetryDelay: TimeSpan.FromSeconds(30), // ������������ ����� �������� ����� ���������
            errorCodesToAdd: null // null ��� ������ ������ - ���������� ����������� ������ ��� �������
        );
    });
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Configuration"]));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    db.Database.Migrate();

    // ������� ��������� ����� �����������, ���� ��� �� �������
    if (!db.Votes.Any())
    {
        db.Votes.AddRange(new[]
        {
            new Vote { Option = "C#" },
            new Vote { Option = "JavaScript" },
            new Vote { Option = "Python" },
            new Vote { Option = "TypeScript" },
            new Vote { Option = "Go" },
            new Vote { Option = "Rust" }
        });
        db.SaveChanges();
    }
}

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection(); // ��������� ��� Docker (HTTP only)

app.MapControllers();
app.Run();