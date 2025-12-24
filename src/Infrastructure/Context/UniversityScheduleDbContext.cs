using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Context
{
    public class UniversityScheduleDbContext : DbContext, IDbContext
    {
        public UniversityScheduleDbContext(DbContextOptions<UniversityScheduleDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Structure> Structures => Set<Structure>();
        public DbSet<Semester> Semesters => Set<Semester>();
        public DbSet<EducationProgram> EducationPrograms => Set<EducationProgram>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<TimeTable> TimeTables => Set<TimeTable>();
        public DbSet<Classroom> Classrooms => Set<Classroom>();
        public DbSet<ClassroomCharacteristics> ClassroomCharacteristics => Set<ClassroomCharacteristics>();
        public DbSet<Lecturer> Lecturers => Set<Lecturer>();
        public DbSet<Schedule> Schedules => Set<Schedule>();
        public DbSet<ScheduleGroup> ScheduleGroups => Set<ScheduleGroup>();
        public DbSet<LecturerSubject> LecturerSubjects => Set<LecturerSubject>();
        public DbSet<GroupSubjectWithLecturer> GroupSubjectsWithLecturer => Set<GroupSubjectWithLecturer>();
        public DbSet<EducationProgramSubject> EducationProgramSubjects => Set<EducationProgramSubject>();
        public DbSet<SubjectConfig> SubjectConfigs => Set<SubjectConfig>();
        public DbSet<SubjectClassroom> SubjectClassrooms => Set<SubjectClassroom>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the Configurations namespace
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetAssembly(typeof(UniversityScheduleDbContext)) ??
                throw new InvalidOperationException("Could not find configuration assembly"));
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                // Always set the UpdatedAt on every change
                entityEntry.Entity.UpdatedAt = DateTime.UtcNow;

                // If it's a brand new record, set the CreatedAt too 
                // (Though your GETUTCDATE() default in SQL also handles this)
                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
