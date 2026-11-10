using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain;

namespace UserManagement.Infrastructure.Configurations
{
    internal class TokenInfoConfiguration : IEntityTypeConfiguration<TokenInfoExpired>
    {
        public void Configure(EntityTypeBuilder<TokenInfoExpired> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Jti).IsUnique();
        }
    }
}
