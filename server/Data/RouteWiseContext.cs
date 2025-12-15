using Microsoft.EntityFrameworkCore;
//using server.Models;

namespace server.Models {
    public class RouteWiseContext : DbContext 
    {
        public RouteWiseContext (DbContextOptions<RouteWiseContext> options) : base(options)
        {
            

        }  

       
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; } 
        public DbSet<GeoLocation> GeoLocations {get; set;}
        public DbSet<Bookmark> Bookmarks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens {get; set;}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DeliveryAddress → GeoLocation (one-to-one)
            modelBuilder.Entity<DeliveryAddress>()
            .HasOne(d => d.Geolocation)
            .WithOne()
            .HasForeignKey<DeliveryAddress>(d => d.GeolocationId)
            .OnDelete(DeleteBehavior.Cascade); // deletes GeoLocation when DeliveryAddress is deleted

            // Bookmark → DeliveryAddresses (One-to-Many with Cascade Delete)
            modelBuilder.Entity<Bookmark>()
            .HasMany(b => b.DeliveryAddresses)
            .WithOne(d => d.Bookmark)
            .HasForeignKey(d => d.BookmarkId)
            .OnDelete(DeleteBehavior.Cascade);

            // 📌 Bookmark → GeoLocations (One-to-Many with Cascade Delete)
            modelBuilder.Entity<Bookmark>()
                .HasMany(b => b.GeoLocations)
                .WithOne(g => g.Bookmark)
                .HasForeignKey(g => g.BookmarkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)            
                .WithMany(u => u.Bookmarks)        
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
                
        }



    }
}