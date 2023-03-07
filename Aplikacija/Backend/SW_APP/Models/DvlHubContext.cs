using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class DvlHubContext : DbContext
    {
        public DbSet<Admin> Admini { get; set; }
        public DbSet<CV> CV { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Oglasi> Oglasi { get; set; }
        public DbSet<Poslodavac> Poslodavci { get; set; }

        public DbSet<Private_Message> Private_Message { get; set;}

        public DbSet<Radnik> Radnici { get; set; }
       
        public DbSet<Zahtevi> Zahtevi { get; set; }
        public DvlHubContext(DbContextOptions options):base(options)
        {
            
        }
    }
}