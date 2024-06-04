using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.Entity<AppRole>()
             .HasMany(ur => ur.UserRoles)
             .WithOne(u => u.Roles)
             .HasForeignKey(ur => ur.RoleId)
             .IsRequired();


        builder.Entity<UserLike>()
            .HasKey(e => new { e.SourceUserId, e.TargetUserId });

        builder.Entity<UserLike>()
            .HasOne(s => s.SourceUser)
            .WithMany(s => s.LikedUser)
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLike>()
            .HasOne(s => s.TargetUser)
            .WithMany(s => s.LikedByUser)
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<Message>()
        .HasKey(e => new { e.Id });

        builder.Entity<Message>()
            .HasOne(s => s.Recipient)
            .WithMany(s => s.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
           .HasOne(s => s.Sender)
           .WithMany(s => s.MessagesSent)
           .OnDelete(DeleteBehavior.Restrict);


    }
}
