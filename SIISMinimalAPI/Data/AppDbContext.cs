using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
    {

        public DbSet<StudentModel> Students { get; set; }
        public DbSet<SchoolModel> School { get; set; }
        public DbSet<InternshipModel> Internship { get; set; }
        public DbSet<RequirementModel> Requirements { get; set; }
        public DbSet<ApplicationModel> Applications { get; set; }
        public DbSet<OfficeModel> Offices { get; set; }
        public DbSet<OfficeAccountModel> OfficeAccounts { get; set; }
        public DbSet<RegistrationTokenModel> RegistrationTokens { get; set; }
        // DbSet<LogsModel> Logs {get; set;}



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentModel>(student =>
            {

                //student has one school details
                student.HasOne(u => u.School)
                    .WithOne(s => s.Student)
                    .HasForeignKey<SchoolModel>(u => u.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                //student has one internship details
                student.HasOne(u => u.Internship)
                    .WithOne(i => i.Student)
                    .HasForeignKey<InternshipModel>(i => i.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                //student has one application
                student.HasOne(u => u.Application)
                    .WithOne(a => a.Student)
                    .HasForeignKey<ApplicationModel>(a => a.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                //student has many requirements
                student.HasMany(u => u.Requirements)
                    .WithOne(a => a.Student)
                    .HasForeignKey(a => a.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                student.HasOne(u => u.Office)
                    .WithMany(o => o.Students)
                    .HasForeignKey(s => s.OfficeId)
                    .OnDelete(DeleteBehavior.Restrict);


                student.HasIndex(u => u.StudentUUID);
                student.HasQueryFilter(u => !u.IsDeleted);
            });

            // Office
            builder.Entity<OfficeModel>(office =>
            {
                office.HasQueryFilter(o => !o.IsDeleted);
            });

            // Office Account
            builder.Entity<OfficeAccountModel>(acc =>
            {
                acc.HasIndex(a => a.Email).IsUnique();
                acc.HasIndex(a => a.Username).IsUnique();
                acc.HasQueryFilter(a => !a.IsDeleted);
            });

            // School
            builder.Entity<SchoolModel>(school =>
            {
                school.HasQueryFilter(s => !s.IsDeleted);
            });

            //internship
            builder.Entity<InternshipModel>(req =>
            {
                req.HasQueryFilter(r => !r.IsDeleted);
            });

            // Requirement
            builder.Entity<RequirementModel>(req =>
            {
                req.HasQueryFilter(r => !r.IsDeleted);
            });

            // Requirement
            builder.Entity<ApplicationModel>(req =>
            {
                req.HasQueryFilter(r => !r.IsDeleted);
            });

            //registrationtoken
            builder.Entity<RegistrationTokenModel>(req =>
            {
                req.HasIndex(r => r.Token); 
            });




        }
    }
}
