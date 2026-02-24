-- ============================================================================
-- FIX: Add more EducationProgramSubjects and GroupSubjectWithLecturer
-- ============================================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

-- Step 1: Build a ranked subject list per chair
-- Then join to ed programs to assign top 6 subjects per chair to each group's semester
;WITH RankedSubjects AS (
    SELECT s.Id AS SubjectId, s.StructureId AS ChairId,
           ROW_NUMBER() OVER (PARTITION BY s.StructureId ORDER BY s.Id) AS RN
    FROM Subjects s
    INNER JOIN Structures ch ON ch.Id = s.StructureId AND ch.[Type] = 3
),
NewEps AS (
    SELECT DISTINCT ep.Id AS EpId, rs.SubjectId, g.SemesterId
    FROM EducationPrograms ep
    INNER JOIN RankedSubjects rs ON rs.ChairId = ep.StructureId AND rs.RN <= 6
    INNER JOIN Groups g ON g.EducationProgramId = ep.Id AND g.ParentId IS NULL
    WHERE NOT EXISTS (
        SELECT 1 FROM EducationProgramSubjects eps2
        WHERE eps2.EducationProgramId = ep.Id
          AND eps2.SubjectId = rs.SubjectId
          AND eps2.SemesterId = g.SemesterId
    )
)
INSERT INTO EducationProgramSubjects (EducationProgramId, SubjectId, SemesterId, FromDate, ToDate)
SELECT EpId, SubjectId, SemesterId, '2025-09-01', NULL
FROM NewEps;

PRINT 'Added EducationProgramSubjects: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Step 2: Add Lecture GSWL for parent groups
;WITH MinLS AS (
    SELECT SubjectId, MIN(Id) AS LsId FROM LecturerSubjects GROUP BY SubjectId
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT mls.LsId, g.Id, NULL, sc.[Hours], 1
FROM Groups g
INNER JOIN EducationProgramSubjects eps ON eps.EducationProgramId = g.EducationProgramId AND eps.SemesterId = g.SemesterId
INNER JOIN SubjectConfigs sc ON sc.SubjectId = eps.SubjectId AND sc.LessonType = 1
INNER JOIN MinLS mls ON mls.SubjectId = eps.SubjectId
WHERE g.ParentId IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM GroupSubjectsWithLecturer gsl
    INNER JOIN LecturerSubjects ls2 ON ls2.Id = gsl.LecturerSubjectId
    WHERE gsl.GroupId = g.Id AND ls2.SubjectId = eps.SubjectId AND gsl.LessonType = 1
  );

PRINT 'Added Lecture GSWL: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Step 3: Add Practical GSWL for child groups
;WITH MinLS AS (
    SELECT SubjectId, MIN(Id) AS LsId FROM LecturerSubjects GROUP BY SubjectId
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT mls.LsId, child.Id, NULL, sc.[Hours], 2
FROM Groups g
INNER JOIN Groups child ON child.ParentId = g.Id AND child.LessonType = 2
INNER JOIN EducationProgramSubjects eps ON eps.EducationProgramId = g.EducationProgramId AND eps.SemesterId = g.SemesterId
INNER JOIN SubjectConfigs sc ON sc.SubjectId = eps.SubjectId AND sc.LessonType = 2
INNER JOIN MinLS mls ON mls.SubjectId = eps.SubjectId
WHERE g.ParentId IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM GroupSubjectsWithLecturer gsl
    INNER JOIN LecturerSubjects ls2 ON ls2.Id = gsl.LecturerSubjectId
    WHERE gsl.GroupId = child.Id AND ls2.SubjectId = eps.SubjectId AND gsl.LessonType = 2
  );

PRINT 'Added Practical GSWL: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Step 4: Add Lab GSWL for child groups
;WITH MinLS AS (
    SELECT SubjectId, MIN(Id) AS LsId FROM LecturerSubjects GROUP BY SubjectId
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT mls.LsId, child.Id, NULL, sc.[Hours], 3
FROM Groups g
INNER JOIN Groups child ON child.ParentId = g.Id AND child.LessonType = 3
INNER JOIN EducationProgramSubjects eps ON eps.EducationProgramId = g.EducationProgramId AND eps.SemesterId = g.SemesterId
INNER JOIN SubjectConfigs sc ON sc.SubjectId = eps.SubjectId AND sc.LessonType = 3
INNER JOIN MinLS mls ON mls.SubjectId = eps.SubjectId
WHERE g.ParentId IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM GroupSubjectsWithLecturer gsl
    INNER JOIN LecturerSubjects ls2 ON ls2.Id = gsl.LecturerSubjectId
    WHERE gsl.GroupId = child.Id AND ls2.SubjectId = eps.SubjectId AND gsl.LessonType = 3
  );

PRINT 'Added Lab GSWL: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Final counts
PRINT '';
DECLARE @cnt1 INT, @cnt2 INT;
SELECT @cnt1 = COUNT(*) FROM EducationProgramSubjects;
SELECT @cnt2 = COUNT(*) FROM GroupSubjectsWithLecturer;
PRINT 'Total EducationProgramSubjects: ' + CAST(@cnt1 AS NVARCHAR);
PRINT 'Total GroupSubjectWithLecturer: ' + CAST(@cnt2 AS NVARCHAR);

COMMIT;
