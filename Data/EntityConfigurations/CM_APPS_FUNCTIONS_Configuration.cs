using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBACapi.Models;

namespace RBACapi.Data.EntityConfigurations
{
    public class CM_APPS_FUNCTIONS_Configuration : IEntityTypeConfiguration<CM_APPS_FUNCTIONS>
    {
        public void Configure(EntityTypeBuilder<CM_APPS_FUNCTIONS> builder)
        {
            builder.HasKey(e => e.FUNC_CODE);

            builder.Property(e => e.FUNC_CODE).IsRequired();
            builder.Property(e => e.APP_CODE).IsRequired();

            builder.HasOne(e => e.CM_APPLICATIONS)
                    .WithMany()
                    .HasForeignKey(e => e.APP_CODE)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("CM_APPS_FUNCTIONS");
        }
    }
}