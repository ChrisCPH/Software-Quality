using Microsoft.EntityFrameworkCore;
using Backend.Data;

public static class TestHelper
{
    public static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();
        
        return context;
    }
}