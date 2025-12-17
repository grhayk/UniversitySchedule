using Domain.Configurations;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Domain.Context
{
    public class UniversityScheduleDbContext : DbContext
    {
        public UniversityScheduleDbContext(DbContextOptions<UniversityScheduleDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Structure> Structures { get; set; } = null!;
        public DbSet<Semester> Semesters { get; set; } = null!;
        public DbSet<EducationProgram> EducationPrograms { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<StudentGroup> StudentGroups { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<TimeTable> TimeTables { get; set; } = null!;
        public DbSet<Classroom> Classrooms { get; set; } = null!;
        public DbSet<ClassroomCharacteristics> ClassroomCharacteristics { get; set; } = null!;
        public DbSet<Lecturer> Lecturers { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;
        public DbSet<ScheduleGroup> ScheduleGroups { get; set; } = null!;
        public DbSet<LecturerSubject> StaffSubjects { get; set; } = null!;
        public DbSet<GroupSubjectWithLecturer> GroupSubjectsWithStaff { get; set; } = null!;
        public DbSet<EducationProgramSubject> EducationProgramSubjects { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the Configurations namespace
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetAssembly(typeof(StructureConfiguration)) ??
                throw new InvalidOperationException("Could not find configuration assembly"));
        }
    }
}
