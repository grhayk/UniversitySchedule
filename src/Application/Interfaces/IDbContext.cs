using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IDbContext
    {
        DbSet<Structure> Structures { get; }
        DbSet<Semester> Semesters { get; }
        DbSet<EducationProgram> EducationPrograms { get; }
        DbSet<Group> Groups { get; }
        DbSet<Student> Students { get; }
        DbSet<StudentGroup> StudentGroups { get; }
        DbSet<Subject> Subjects { get; }
        DbSet<TimeTable> TimeTables { get; }
        DbSet<Classroom> Classrooms { get; }
        DbSet<ClassroomCharacteristics> ClassroomCharacteristics { get; }
        DbSet<Lecturer> Lecturers { get; }
        DbSet<Schedule> Schedules { get; }
        DbSet<ScheduleGroup> ScheduleGroups { get; }
        DbSet<LecturerSubject> LecturerSubjects { get; }
        DbSet<GroupSubjectWithLecturer> GroupSubjectsWithLecturer { get; }
        DbSet<EducationProgramSubject> EducationProgramSubjects { get; }
        DbSet<SubjectConfig> SubjectConfigs { get; }
        DbSet<SubjectClassroom> SubjectClassrooms { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
