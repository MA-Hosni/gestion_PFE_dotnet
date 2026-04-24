using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");
            builder.HasIndex(s => s.Cin).IsUnique();

            builder.HasOne(s => s.CompSupervisor)
                .WithMany(cs => cs.Students)
                .HasForeignKey(s => s.CompSupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.UniSupervisor)
                .WithMany(us => us.Students)
                .HasForeignKey(s => s.UniSupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Project)
                .WithMany(p => p.Contributors)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
