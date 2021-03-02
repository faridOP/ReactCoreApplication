using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityAttendee> ActivityAttendees { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserFollowing> UserFollowings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ActivityAttendee>(x => x.HasKey(aa => new { aa.AppUserId, aa.ActivityId }));

            builder.Entity<ActivityAttendee>()
                .HasOne(u => u.AppUser)
                .WithMany(a => a.Activities)
                .HasForeignKey(aa => aa.AppUserId);

            builder.Entity<ActivityAttendee>()
                .HasOne(u => u.Activity)
                .WithMany(a => a.Attendees)
                .HasForeignKey(aa => aa.ActivityId);

            builder.Entity<Comment>()
            .HasOne(x => x.Activity)
            .WithMany(x => x.Comments)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserFollowing>(x =>
                x.HasKey(k => new { k.ObserverId, k.TargetId })

            );
            builder.Entity<UserFollowing>()
             .HasOne(u => u.Observer)
             .WithMany(a => a.Followings)
             .HasForeignKey(x => x.ObserverId)
              .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<UserFollowing>()
              .HasOne(u => u.Target)
              .WithMany(a => a.Followers)
              .HasForeignKey(x => x.TargetId)
             .OnDelete(DeleteBehavior.Cascade);


        }
    }

}

// protected override void OnModelCreating(ModelBuilder modelBuilder)
// {
//     modelBuilder.Entity<Book>()
//     .HasData(
//           new Book { Id = 1, Price = 12, Title = "The assassination of Jesse James by the coward Robert Ford" },
//             new Book { Id = 2, Price = 12, Title = "The secret of Blue Train" },
//             new Book { Id = 3, Price = 12, Title = "The invisible man" }
//     );

// }
