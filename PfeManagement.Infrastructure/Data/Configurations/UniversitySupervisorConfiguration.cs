using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Data.Configurations
{
    public class UniversitySupervisorConfiguration : IEntityTypeConfiguration<UniversitySupervisor>
    {
        public void Configure(EntityTypeBuilder<UniversitySupervisor> builder)
        {
            builder.ToTable("UniversitySupervisors");
        }
    }
}
