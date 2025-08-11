using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace src.Shared.Data
{
    public class CustomGuidV7Generator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues => false; // Indicates that the generated values are not temporary

        public override Guid Next(EntityEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry), "Entity entry cannot be null.");

            try
            {
                return Guid.CreateVersion7();
            }
            catch (PlatformNotSupportedException)
            {
                // Fallback for platforms that do not support Guid.CreateVersion7()
                // This will generate a standard Guid, which may not be suitable for all use cases
                return Guid.NewGuid();
            }
        }
    }
}