using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBACapi.Models;

namespace RBACapi.Data.EntityConfigurations
{
    public class CM_RBAC_Configuration : IEntityTypeConfiguration<CM_RBAC>
    {
        public void Configure(EntityTypeBuilder<CM_RBAC> builder)
        {
            builder.ToTable("CM_RBAC");
            builder.HasKey(r => r.RBAC_CODE);

            builder.Property(r => r.RBAC_CODE).IsRequired();

            builder.HasOne(r => r.CM_APPS_ROLES)
                    .WithMany()
                    .HasForeignKey(r => r.ROLE_CODE)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.CM_APPS_FUNCTIONS)
                    .WithMany()
                    .HasForeignKey(r => r.FUNC_CODE)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}