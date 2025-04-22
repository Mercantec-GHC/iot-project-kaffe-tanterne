using Microsoft.EntityFrameworkCore;

namespace KaffeMaskineProject.Repository
{
    public class KaffeDBContext : DbContext
    {
        public KaffeDBContext(DbContextOptions<KaffeDBContext> options) : base(options)
        {
        }
    }
}
