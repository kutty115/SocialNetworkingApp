using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Models;

namespace SocialNetworkApi.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
    public DbSet<Friendship> Friendships => Set<Friendship>();




    protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // ✅ FriendRequests (you already saw this)
    builder.Entity<FriendRequest>()
        .HasOne(fr => fr.Sender)
        .WithMany()
        .HasForeignKey(fr => fr.SenderId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<FriendRequest>()
        .HasOne(fr => fr.Receiver)
        .WithMany()
        .HasForeignKey(fr => fr.ReceiverId)
        .OnDelete(DeleteBehavior.NoAction);

        // Friendship: prevent cascade cycles
        builder.Entity<Friendship>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friendship>()
            .HasOne(x => x.Friend)
            .WithMany()
            .HasForeignKey(x => x.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: no duplicate friendships
        builder.Entity<Friendship>()
            .HasIndex(x => new { x.UserId, x.FriendId })
            .IsUnique();

        // ✅ Messages (avoid next crash)
        builder.Entity<Message>()
        .HasOne(m => m.Sender)
        .WithMany()
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<Message>()
        .HasOne(m => m.Receiver)
        .WithMany()
        .HasForeignKey(m => m.ReceiverId)
        .OnDelete(DeleteBehavior.NoAction);

        // Block table
        builder.Entity<UserBlock>()
            .HasOne(x => x.Blocker)
            .WithMany()
            .HasForeignKey(x => x.BlockerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserBlock>()
            .HasOne(x => x.Blocked)
            .WithMany()
            .HasForeignKey(x => x.BlockedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserBlock>()
            .HasIndex(x => new { x.BlockerId, x.BlockedId })
            .IsUnique();

        // ✅ Comments: IMPORTANT (your current error)
        builder.Entity<Comment>()
        .HasOne(c => c.User)
        .WithMany()
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<Comment>()
        .HasOne(c => c.Post)
        .WithMany(p => p.Comments)
        .HasForeignKey(c => c.PostId)
        .OnDelete(DeleteBehavior.NoAction);

    // ✅ Posts -> User (avoid cascade chain)
    builder.Entity<Post>()
        .HasOne(p => p.User)
        .WithMany()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.NoAction);

    // ✅ PostLikes -> User/Post (avoid cascade chain)
    builder.Entity<PostLike>()
        .HasOne(pl => pl.User)
        .WithMany()
        .HasForeignKey(pl => pl.UserId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<PostLike>()
        .HasOne(pl => pl.Post)
        .WithMany(p => p.Likes)
        .HasForeignKey(pl => pl.PostId)
        .OnDelete(DeleteBehavior.NoAction);

    // ✅ Reports -> User/Post (same pattern)
    builder.Entity<Report>()
        .HasOne(r => r.Reporter)
        .WithMany()
        .HasForeignKey(r => r.ReporterId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<Report>()
        .HasOne(r => r.Post)
        .WithMany()
        .HasForeignKey(r => r.PostId)
        .OnDelete(DeleteBehavior.NoAction);

    }
}