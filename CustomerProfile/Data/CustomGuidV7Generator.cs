using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace CustomerProfile.Data
{
    public class CustomGuidV7Generator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues => false; // Indicates that the generated values are not temporary

        public override Guid Next(EntityEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry), "Entity entry cannot be null.");
            }

            return Guid.CreateVersion7();
        }
    }
}