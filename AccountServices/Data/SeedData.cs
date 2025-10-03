using AccountServices.Domain.Entities;

namespace AccountServices.Data;

// seed initial data if necessary
public static class SeedData
{
    public static void Initialize(AccountDbContext context)
    {
        context.Database.EnsureCreated();

        // Look for any accounts.
        if (context.Accounts.Any())
        {
            return;   // DB has been seeded
        }

        List<Account> accounts = [
            Account.Create(Guid.NewGuid(), "1234567890"),
            Account.Create(Guid.NewGuid(), "0987654321"),
            Account.Create(Guid.NewGuid(), "1122334455")
        ];


        context.Accounts.AddRange(accounts);
        context.SaveChanges();
    }
}
