using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain;

namespace UserManagement.Infrastructure.Configurations
{
    public class UserInfoConfiguration : IEntityTypeConfiguration<UserInformation>
    {
        public void Configure(EntityTypeBuilder<UserInformation> builder)
        {
            builder.HasKey(u => u.Id);

            builder.HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<UserInformation>(u => u.UserId);

            builder.HasIndex(u => u.NationalId)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}