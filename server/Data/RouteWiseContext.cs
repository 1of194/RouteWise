using Microsoft.EntityFrameworkCore;

namespace server.Models {
    public class RouteWiseContext : DbContext 
    {
        public RouteWiseContext (DbContextOptions<RouteWiseContext> options) : base(options)
        {
            

        }


        // DATABASE TABLES (DbSets)
        // Each DbSet represents a table in your SQL database.
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; } 
        public DbSet<GeoLocation> GeoLocations {get; set;}
        public DbSet<Bookmark> Bookmarks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens {get; set;}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DeliveryAddress -- GeoLocation (one-to-one)
            modelBuilder.Entity<DeliveryAddress>()
            .HasOne(d => d.Geolocation)
            .WithOne()
            .HasForeignKey<DeliveryAddress>(d => d.GeolocationId)
            .OnDelete(DeleteBehavior.Cascade); // deletes GeoLocation when DeliveryAddress is deleted

            // Bookmark -- DeliveryAddresses (One-to-Many with Cascade Delete)
            // One-to-Many: One saved route (Bookmark) contains multiple stops (Addresses).
            modelBuilder.Entity<Bookmark>()
            .HasMany(b => b.DeliveryAddresses)
            .WithOne(d => d.Bookmark)
            .HasForeignKey(d => d.BookmarkId)
            .OnDelete(DeleteBehavior.Cascade);


            //  RELATIONSHIP: Bookmark -- GeoLocations
            // One-to-Many: Direct link between the route and all its map markers.
            modelBuilder.Entity<Bookmark>()
                .HasMany(b => b.GeoLocations)
                .WithOne(g => g.Bookmark)
                .HasForeignKey(g => g.BookmarkId)
                .OnDelete(DeleteBehavior.Cascade);

            // RELATIONSHIP: User and Bookmark 
            // One-to-Many: A user can have many saved routes.
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)            
                .WithMany(u => u.Bookmarks)        
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // RELATIONSHIP: User and RefreshToken
            // One-to-Many: A user can be logged in on multiple devices (multiple tokens).
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
                
        }

        




    }
}