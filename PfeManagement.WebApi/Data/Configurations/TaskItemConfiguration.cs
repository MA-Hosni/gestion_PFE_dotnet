using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Data.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");

            builder.HasOne(t => t.UserStory)
                .WithMany(us => us.Tasks)
                .HasForeignKey(t => t.UserStoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
