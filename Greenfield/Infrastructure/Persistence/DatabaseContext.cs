using Microsoft.EntityFrameworkCore;

namespace Greenfield.Infrastructure.Persistence;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
}