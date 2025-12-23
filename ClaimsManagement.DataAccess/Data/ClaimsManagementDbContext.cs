using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Data
{
    public class ClaimsManagementDbContext : DbContext
    {
        public ClaimsManagementDbContext(DbContextOptions<ClaimsManagementDbContext> options) : base(options)
        {
        }

        // DbSets for all models
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }
        public DbSet<ClaimStatusHistory> ClaimStatusHistories { get; set; }
        public DbSet<ClaimApproval> ClaimApprovals { get; set; }
        public DbSet<ClaimComment> ClaimComments { get; set; }
        public DbSet<ClaimPayment> ClaimPayments { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<UserClaimStats> UserClaimStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Claim relationships
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Category)
                .WithMany(ec => ec.Claims)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClaimDocument relationships
            modelBuilder.Entity<ClaimDocument>()
                .HasOne(cd => cd.Claim)
                .WithMany(c => c.Documents)
                .HasForeignKey(cd => cd.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimDocument>()
                .HasOne(cd => cd.UploadedByUser)
                .WithMany()
                .HasForeignKey(cd => cd.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ClaimStatusHistory relationships
            modelBuilder.Entity<ClaimStatusHistory>()
                .HasOne(csh => csh.Claim)
                .WithMany(c => c.StatusHistory)
                .HasForeignKey(csh => csh.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimStatusHistory>()
                .HasOne(csh => csh.ChangedByUser)
                .WithMany()
                .HasForeignKey(csh => csh.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ClaimApproval relationships
            modelBuilder.Entity<ClaimApproval>()
                .HasOne(ca => ca.Claim)
                .WithMany(c => c.Approvals)
                .HasForeignKey(ca => ca.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimApproval>()
                .HasOne(ca => ca.Approver)
                .WithMany(u => u.ClaimApprovals)
                .HasForeignKey(ca => ca.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClaimComment relationships
            modelBuilder.Entity<ClaimComment>()
                .HasOne(cc => cc.Claim)
                .WithMany(c => c.Comments)
                .HasForeignKey(cc => cc.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimComment>()
                .HasOne(cc => cc.User)
                .WithMany()
                .HasForeignKey(cc => cc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClaimPayment relationships
            modelBuilder.Entity<ClaimPayment>()
                .HasOne(cp => cp.Claim)
                .WithOne(c => c.Payment)
                .HasForeignKey<ClaimPayment>(cp => cp.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimPayment>()
                .HasOne(cp => cp.ProcessedByUser)
                .WithMany()
                .HasForeignKey(cp => cp.ProcessedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // UserNotification relationships
            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(un => un.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.RelatedClaim)
                .WithMany()
                .HasForeignKey(un => un.RelatedClaimId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserClaimStats relationships
            modelBuilder.Entity<UserClaimStats>()
                .HasOne(ucs => ucs.User)
                .WithMany()
                .HasForeignKey(ucs => ucs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.ClaimNumber)
                .IsUnique();

            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}