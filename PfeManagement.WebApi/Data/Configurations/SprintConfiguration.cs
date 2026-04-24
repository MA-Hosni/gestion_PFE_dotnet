using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Data.Configurations
{
    public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
    {
        public void Configure(EntityTypeBuilder<Sprint> builder)
        {
            builder.ToTable("Sprints");

            builder.HasMany(s => s.UserStories)
                .WithOne(us => us.Sprint)
                .HasForeignKey(us => us.SprintId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
