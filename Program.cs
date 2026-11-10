using GreenDonut.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Keyset paging sorted on a free-text column, as `[UsePaging]` + `[UseSorting]` does it.
// Only the last row of the page matters: its sort key goes into the end cursor.
using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

string[] comments =
[
    new string('a', 200),   // 200 UTF-8 bytes: works
    new string('a', 300),   // 300 UTF-8 bytes: fails
    new string('é', 150),   // 150 characters, 300 UTF-8 bytes: fails
];

foreach (var comment in comments)
{
    await using var db = new AppDbContext(options);
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    db.Persons.Add(new Person { Comment = comment });
    await db.SaveChangesAsync();

    var page = await db.Persons
        .OrderBy(p => p.Comment)
        .ThenBy(p => p.Id)
        .ToPageAsync(new PagingArguments { First = 1 });

    var label = $"{comment.Length} chars, {System.Text.Encoding.UTF8.GetByteCount(comment)} UTF-8 bytes";

    try
    {
        var endCursor = page.CreateEndCursor();
        Console.WriteLine($"OK    {label}: end cursor has {endCursor!.Length} chars");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAIL  {label}:");
        Console.WriteLine(ex);
    }
}

public sealed class Person
{
    public int Id { get; set; }

    public string Comment { get; set; } = "";
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
}
