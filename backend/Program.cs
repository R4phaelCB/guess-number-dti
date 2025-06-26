using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Services.AddDbContext<GameContext>(opt => opt.UseSqlite("Data Source=game.db"));
var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameContext>();
    db.Database.EnsureCreated();
}

var rnd = new Random();
int secretNumber = rnd.Next(1, 101);
int attemptCount = 0;

app.MapPost("/guess", async (GuessRequest request, GameContext db) =>
{
    attemptCount++;
    if (request.Number == secretNumber)
    {
        db.Rankings.Add(new Ranking { User = request.User, Tries = attemptCount });
        await db.SaveChangesAsync();

        var history = db.Rankings.OrderBy(r => r.Tries).Take(10).ToList();
        var response = new {
            message = $"Acertou! Novo número gerado.",
            tries = attemptCount,
            history = history.Select(r => new { r.User, r.Tries })
        };
        secretNumber = rnd.Next(1, 101);
        attemptCount = 0;
        return Results.Ok(response);
    }

    string feedback = request.Number < secretNumber ? "O número é maior." : "O número é menor.";
    return Results.Ok(new { message = feedback });
});

app.Run();

record GuessRequest(int Number, string User);

class Ranking
{
    public int Id { get; set; }
    public string User { get; set; } = "";
    public int Tries { get; set; }
}

class GameContext : DbContext
{
    public GameContext(DbContextOptions<GameContext> options) : base(options) { }
    public DbSet<Ranking> Rankings => Set<Ranking>();
}
