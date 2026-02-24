-- ============================================================================
-- FIX 2: Redistribute lecturers so no lecturer has > 50 events
-- Problem: All groups for same subject got same lecturer (MIN), causing 170+ events
-- Solution: Round-robin lecturers for same subject across groups
-- ============================================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

-- Step 1: Delete all GSWL and recreate with proper lecturer distribution
DELETE FROM GroupSubjectsWithLecturer;
PRINT 'Cleared GroupSubjectsWithLecturer';

-- Step 2: For each (SubjectId, LessonType) combination, we need to distribute
-- across multiple lecturers. Get all available lecturers per subject.
-- Then assign groups round-robin across those lecturers.

-- Lectures for parent groups
;WITH GroupSubjects AS (
    SELECT DISTINCT g.Id AS GroupId, eps.SubjectId, g.SemesterId
    FROM Groups g
    INNER JOIN EducationProgramSubjects eps
        ON eps.EducationProgramId = g.EducationProgramId
        AND eps.SemesterId = g.SemesterId
    WHERE g.ParentId IS NULL
),
SubjectLecturers AS (
    SELECT ls.Id AS LsId, ls.SubjectId,
           ROW_NUMBER() OVER (PARTITION BY ls.SubjectId ORDER BY ls.Id) AS LecIdx,
           COUNT(*) OVER (PARTITION BY ls.SubjectId) AS LecCount
    FROM LecturerSubjects ls
),
RankedGroups AS (
    SELECT gs.GroupId, gs.SubjectId, gs.SemesterId,
           ROW_NUMBER() OVER (PARTITION BY gs.SubjectId ORDER BY gs.GroupId) AS GrpIdx
    FROM GroupSubjects gs
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT sl.LsId, rg.GroupId, NULL, sc.[Hours], 1
FROM RankedGroups rg
INNER JOIN SubjectConfigs sc ON sc.SubjectId = rg.SubjectId AND sc.LessonType = 1
INNER JOIN SubjectLecturers sl ON sl.SubjectId = rg.SubjectId
    AND sl.LecIdx = ((rg.GrpIdx - 1) % sl.LecCount) + 1;

PRINT 'Inserted Lecture GSWL: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Practicals for child groups
;WITH GroupSubjects AS (
    SELECT DISTINCT g.Id AS ParentId, eps.SubjectId, g.SemesterId
    FROM Groups g
    INNER JOIN EducationProgramSubjects eps
        ON eps.EducationProgramId = g.EducationProgramId
        AND eps.SemesterId = g.SemesterId
    WHERE g.ParentId IS NULL
),
ChildGroups AS (
    SELECT child.Id AS ChildId, gs.SubjectId, gs.SemesterId
    FROM GroupSubjects gs
    INNER JOIN Groups child ON child.ParentId = gs.ParentId AND child.LessonType = 2
),
SubjectLecturers AS (
    SELECT ls.Id AS LsId, ls.SubjectId,
           ROW_NUMBER() OVER (PARTITION BY ls.SubjectId ORDER BY ls.Id) AS LecIdx,
           COUNT(*) OVER (PARTITION BY ls.SubjectId) AS LecCount
    FROM LecturerSubjects ls
),
RankedChildren AS (
    SELECT cg.ChildId, cg.SubjectId,
           ROW_NUMBER() OVER (PARTITION BY cg.SubjectId ORDER BY cg.ChildId) AS GrpIdx
    FROM ChildGroups cg
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT sl.LsId, rc.ChildId, NULL, sc.[Hours], 2
FROM RankedChildren rc
INNER JOIN SubjectConfigs sc ON sc.SubjectId = rc.SubjectId AND sc.LessonType = 2
INNER JOIN SubjectLecturers sl ON sl.SubjectId = rc.SubjectId
    AND sl.LecIdx = ((rc.GrpIdx - 1) % sl.LecCount) + 1;

PRINT 'Inserted Practical GSWL: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Labs for child groups
;WITH GroupSubjects AS (
    SELECT DISTINCT g.Id AS ParentId, eps.SubjectId, g.SemesterId
    FROM Groups g
    INNER JOIN EducationProgramSubjects eps
        ON eps.EducationProgramId = g.EducationProgramId
        AND eps.SemesterId = g.SemesterId
    WHERE g.ParentId IS NULL
),
ChildGroups AS (
    SELECT child.Id AS ChildId, gs.SubjectId, gs.SemesterId
    FROM GroupSubjects gs
    INNER JOIN Groups child ON child.ParentId = gs.ParentId AND child.LessonType = 3
),
SubjectLecturers AS (
    SELECT ls.Id AS LsId, ls.SubjectId,
           ROW_NUMBER() OVER (PARTITION BY ls.SubjectId ORDER BY ls.Id) AS LecIdx,
           COUNT(*) OVER (PARTITION BY ls.SubjectId) AS LecCount
    FROM LecturerSubjects ls
),
RankedChildren AS (
    SELECT cg.ChildId, cg.SubjectId,
           ROW_NUMBER() OVER (PARTITION BY cg.SubjectId ORDER BY cg.ChildId) AS GrpIdx
    FROM ChildGroups cg
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT sl.LsId, rc.ChildId, NULL, sc.[Hours], 3
FROM RankedChildren rc
INNER JOIN SubjectConfigs sc ON sc.SubjectId = rc.SubjectId AND sc.LessonType = 3
INNER JOIN SubjectLecturers sl ON sl.SubjectId = rc.SubjectId
    AND sl.LecIdx = ((rc.GrpIdx - 1) % sl.LecCount) + 1;

PRINT 'Inserted Lab GSWL: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Verify: check max events per lecturer
DECLARE @maxEvents INT;
SELECT @maxEvents = MAX(TotalEvents) FROM (
    SELECT ls.LecturerId, SUM(gsl.[Hours]) AS TotalEvents
    FROM GroupSubjectsWithLecturer gsl
    INNER JOIN LecturerSubjects ls ON ls.Id = gsl.LecturerSubjectId
    GROUP BY ls.LecturerId
) t;
PRINT '';
PRINT 'Max events per lecturer: ' + CAST(@maxEvents AS NVARCHAR) + ' (must be <= 60)';

DECLARE @cnt INT;
SELECT @cnt = COUNT(*) FROM GroupSubjectsWithLecturer;
PRINT 'Total GSWL: ' + CAST(@cnt AS NVARCHAR);

-- Show top 10 busiest lecturers
PRINT '';
PRINT 'Top 10 busiest lecturers:';

DECLARE @info NVARCHAR(200);
DECLARE info_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT TOP 10
        CAST(ls.LecturerId AS NVARCHAR) + ': ' +
        p.FirstName + ' ' + p.LastName + ' = ' +
        CAST(SUM(gsl.[Hours]) AS NVARCHAR) + ' events'
    FROM GroupSubjectsWithLecturer gsl
    INNER JOIN LecturerSubjects ls ON ls.Id = gsl.LecturerSubjectId
    INNER JOIN Person p ON p.Id = ls.LecturerId
    GROUP BY ls.LecturerId, p.FirstName, p.LastName
    ORDER BY SUM(gsl.[Hours]) DESC;

OPEN info_cursor;
FETCH NEXT FROM info_cursor INTO @info;
WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT '  ' + @info;
    FETCH NEXT FROM info_cursor INTO @info;
END;
CLOSE info_cursor;
DEALLOCATE info_cursor;

COMMIT;
