namespace Solution.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Solution.Domain.Entities;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using IServiceScope scope = serviceProvider.CreateScope();
        var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

        await appDb.Database.EnsureCreatedAsync().ConfigureAwait(false);
        await identityDb.Database.EnsureCreatedAsync().ConfigureAwait(false);

        // Ensure default event exists if not seeded by EnsureCreated
        if (!await appDb.ReunionEvents.AnyAsync().ConfigureAwait(false))
        {
            appDb.ReunionEvents.Add(new IafReunionEvent
            {
                EventId = 1,
                Title = "Indian Air Force Batch 1986 Grand Reunion",
                Batch = "1986",
                EventDate = new DateTime(2027, 2, 20),
                EventEndDate = new DateTime(2027, 2, 22),
                Venue = "Air Force Officers Mess & Convention Centre, Chennai",
                RegistrationDeadline = new DateTime(2027, 1, 31),
                BaseFeeAmount = 5000.00m,
                IsActive = true,
                Description = "Grand 40-Year Reunion celebrating the Indian Air Force Batch of 1986 veterans, spouses, and families.",
                CreatedAtUtc = DateTime.UtcNow
            });
            await appDb.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
