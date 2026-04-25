using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");
            builder.HasIndex(s => s.Cin).IsUnique();

            // Navigation to Supervisors (No cascade delete, just set null if supervisor is deleted or fail)
            builder.HasOne(s => s.CompSupervisor)
                .WithMany(cs => cs.Students)
                .HasForeignKey(s => s.CompSupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.UniSupervisor)
                .WithMany(us => us.Students)
                .HasForeignKey(s => s.UniSupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project reference
            builder.HasOne(s => s.Project)
                .WithMany(p => p.Contributors)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.SetNull); // Aggregation relation (If project is deleted, student loses reference)
        }
    }
}
