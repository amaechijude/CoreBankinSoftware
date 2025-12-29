using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace TransactionService.Data;

public sealed class CustomGuidV7Generator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false; // Indicates that the generated values are not temporary

    public override Guid Next(EntityEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry), "Entity entry cannot be null.");
        }
        try
        {

            return Guid.CreateVersion7();
        }
        catch (PlatformNotSupportedException)
        {
            return Guid.NewGuid();
        }
    }
}
