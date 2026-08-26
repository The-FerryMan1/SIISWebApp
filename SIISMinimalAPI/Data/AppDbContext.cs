using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<LogsModel> Logs { get; set; }
        public DbSet<Progress> Progresses {get; set;    }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(user =>
            {
                user.HasIndex(u => u.Email).IsUnique();
                user.HasIndex(u => u.NormalizedUserName).IsUnique();
            });

            builder.Entity<Student>(student =>
            {
                student.HasIndex(s => s.Email);
                student.HasQueryFilter(s => !s.IsDeleted);

                student.HasOne(s => s.Application)
                    .WithOne(a => a.Student)
                    .HasForeignKey<Application>(a => a.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                student.HasOne(s => s.Placement)
                    .WithOne(p => p.Student)
                    .HasForeignKey<Placement>(p => p.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                student.HasMany(s => s.Requirements)
                    .WithOne(r => r.Student)
                    .HasForeignKey(r => r.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Application>(app =>
            {
                app.HasIndex(a => a.Uuid);
                app.HasQueryFilter(a => !a.IsDeleted);
            });

            builder.Entity<Placement>(placement =>
            {
                placement.HasQueryFilter(p => !p.IsDeleted);
                placement.HasOne(p => p.Office)
                    .WithMany(o => o.Placements)
                    .HasForeignKey(p => p.OfficeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Office>(office =>
            {
                office.HasIndex(o => o.UserId);
                office.HasQueryFilter(o => !o.IsDeleted);
                office.HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Progress>(progress =>
            {
                progress.HasOne(p => p.Placement)
                    .WithMany(p => p.Progresses)
                    .HasForeignKey(p => p.PlacementId)
                    .OnDelete(DeleteBehavior.SetNull);   
            });

            builder.Entity<Requirement>(req =>
            {
                req.HasQueryFilter(r => !r.IsDeleted);
            });

            builder.Entity<Registration>(reg =>
            {
                reg.HasIndex(r => r.Token).IsUnique();
            });

            builder.Entity<LogsModel>(log =>
            {
                log.HasQueryFilter(l => !l.IsDeleted);
            });

        }
    }
}