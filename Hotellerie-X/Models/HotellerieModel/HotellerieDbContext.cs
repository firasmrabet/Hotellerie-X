using Microsoft.EntityFrameworkCore;

namespace Hotellerie_X.Models.HotellerieModel
{
    public class HotellerieDbContext : DbContext
    {
        public HotellerieDbContext(DbContextOptions<HotellerieDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Hotel> Hotels { get; set; } = null!;
        public virtual DbSet<Appreciation> Appreciations { get; set; } = null!;
    }
}
