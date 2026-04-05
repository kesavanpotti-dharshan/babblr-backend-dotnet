using Babblr.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Babblr.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(100);
            e.HasOne(r => r.CreatedBy)
             .WithMany()
             .HasForeignKey(r => r.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Message>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Content).IsRequired().HasMaxLength(4000);
            e.HasOne(m => m.Sender)
             .WithMany(u => u.Messages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Room)
             .WithMany(r => r.Messages)
             .HasForeignKey(m => m.RoomId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RoomMember>(e =>
        {
            e.HasKey(rm => rm.Id);
            e.HasIndex(rm => new { rm.RoomId, rm.UserId }).IsUnique();
            e.HasOne(rm => rm.Room)
             .WithMany(r => r.Members)
             .HasForeignKey(rm => rm.RoomId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(rm => rm.User)
             .WithMany(u => u.RoomMemberships)
             .HasForeignKey(rm => rm.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}