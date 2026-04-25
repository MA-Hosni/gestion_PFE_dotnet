using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // TPT base table for shared user fields.
            builder.ToTable("Users");

            // Indexes for fast lookup
            builder.HasIndex(u => u.Email).IsUnique();

            // Ignore logical properties
            builder.Ignore(u => u.IsDeleted);
        }
    }
}
