using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Models.IRequest;
using App.Models;

namespace App.Models
{
    // razorweb.models.AppDbContextModelSnapshot
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //..
            // this.Roles
            // IdentityRole<string>
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            modelBuilder.Entity<Priority>(entity =>
            {
                entity.ToTable("Priority");
                entity.HasKey(p => p.PriorityID);
            });

            // Định nghĩa ánh xạ cho Status
            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");
                entity.HasKey(s => s.StatusID);
            });

            // Định nghĩa ánh xạ cho Workflow
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.ToTable("Workflow");
                entity.HasKey(w => w.WorkflowID);
            });
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.Id); // Ánh xạ cột UserId
            });
            modelBuilder.Entity<App.Models.IRequest.Request>(entity =>
            {
                entity.HasOne(r => r.User)               // Một Request có một User
                      .WithMany(u => u.Requests)         // Một User có nhiều Requests
                      .HasForeignKey(r => r.UsersId)      // Khóa ngoại UserId trong Request
                      .OnDelete(DeleteBehavior.Restrict); // Không xóa Cascade, không xóa khi xóa User
            });
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Department)  // Một AppUser thuộc về một Department
                .WithMany(d => d.Users)     // Một Department có nhiều AppUser
                .HasForeignKey(u => u.DepartmentID)  // Khóa ngoại từ AppUser đến Department
                .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Request)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RequestId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình cho RequestHistory
            modelBuilder.Entity<RequestHistory>(entity =>
            {
                entity.ToTable("RequestHistories");
                entity.HasKey(rh => rh.HistoryID);

                entity.HasOne(rh => rh.Request)
                    .WithMany(r => r.Histories)
                    .HasForeignKey(rh => rh.RequestID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rh => rh.WorkflowStep)
                    .WithMany()
                    .HasForeignKey(rh => rh.StepID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rh => rh.User)
                    .WithMany()
                    .HasForeignKey(rh => rh.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
        // public DbSet<Contact> Contacts { get; set; } 
        public DbSet<App.Models.IRequest.Request> Requests { get; set; }
        public DbSet<Status> Status { get; set; } = default!;
        // public DbSet<Category> Categories {set; get;}
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<Department> Departments { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<RequestHistory> RequestHistories { get; set; }
    }
}