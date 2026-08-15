using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EnterpriseBase.EntityFrameworkCore;

public static class EnterpriseBaseDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<EnterpriseBaseDbContext> builder, string connectionString)
    {
        // builder.UseSqlServer(connectionString);
        builder.UseNpgsql(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<EnterpriseBaseDbContext> builder, DbConnection connection)
    {
        //builder.UseSqlServer(connection);
        builder.UseNpgsql(connection);
    }
}
