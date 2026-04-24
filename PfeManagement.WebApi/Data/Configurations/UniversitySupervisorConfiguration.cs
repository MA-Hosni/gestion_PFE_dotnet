using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Data.Configurations
{
    public class UniversitySupervisorConfiguration : IEntityTypeConfiguration<UniversitySupervisor>
    {
        public void Configure(EntityTypeBuilder<UniversitySupervisor> builder)
        {
            builder.ToTable("UniversitySupervisors");
        }
    }
}
