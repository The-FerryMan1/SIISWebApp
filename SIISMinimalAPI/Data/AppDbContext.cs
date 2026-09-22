using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.SystemSettings;

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
        public DbSet<OfficeNotification> OfficeNotifications { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<WeeklyReport> WeeklyReports { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }

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

                placement.HasOne(p => p.Progress)
                    .WithOne(p => p.Placement)
                    .HasForeignKey<Progress>(p => p.PlacementId);
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
                progress.HasMany(p => p.WeeklyReports)
                    .WithOne(p => p.Progress)
                    .HasForeignKey(p => p.ProgressId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<WeeklyReport>(weekly =>
            {
                weekly.HasMany(w => w.DailyReport)
                    .WithOne(d => d.WeeklyReport)
                    .HasForeignKey(w => w.WeeklyReportId)
                    .OnDelete(DeleteBehavior.Cascade);
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

            builder.Entity<OfficeNotification>(notification =>
            {
                notification.HasIndex(n => new { n.OfficeId, n.CreatedAt });
                notification.HasOne(n => n.Office)
                    .WithMany()
                    .HasForeignKey(n => n.OfficeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Otp>(otp =>
            {
                otp.HasIndex(t => new { t.Identifier, t.HashToken });
                otp.Property(t => t.Identifier).HasMaxLength(320);
                otp.Property(t => t.HashToken).HasMaxLength(64);
            });

        }
    }
}