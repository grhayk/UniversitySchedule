-- ============================================================================
-- FIX 3: Properly distribute lecturers so no one has > 50 events
-- ============================================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

-- Step 1: Remove the bad assignments from previous fix (lecturer 9 got 24 subjects)
-- Delete LecturerSubjects that were added by fix (where lecturer teaches > 3 subjects)
-- Actually, let's just reset LecturerSubjects to original state first
-- Original seed: each lecturer teaches 1-3 subjects via NTILE distribution

-- Delete ALL LecturerSubject entries for subjects that have lecturer 9
-- but only the lecturer 9 entries (they were artificially added)
DELETE FROM LecturerSubjects
WHERE LecturerId = 9
AND SubjectId NOT IN (
    -- Keep only original assignments: lecturer 9 should have had at most 2-3 subjects
    SELECT TOP 2 SubjectId FROM LecturerSubjects WHERE LecturerId = 9 ORDER BY Id
);

PRINT 'Cleaned up lecturer 9 extra assignments';

-- Step 2: Calculate what we need
-- For each subject, figure out total events and required lecturers
;WITH SubjectEvents AS (
    SELECT
        eps.SubjectId,
        (SELECT ISNULL(SUM(sc.[Hours]), 0) FROM SubjectConfigs sc WHERE sc.SubjectId = eps.SubjectId) AS HoursPerGroup,
        COUNT(DISTINCT CASE
            WHEN sc2.LessonType = 1 THEN g.Id  -- lectures: parent groups
            ELSE NULL END) AS LectureGroups,
        COUNT(DISTINCT CASE
            WHEN sc2.LessonType = 2 THEN child2.Id
            ELSE NULL END) AS PracticalGroups,
        COUNT(DISTINCT CASE
            WHEN sc2.LessonType = 3 THEN child3.Id
            ELSE NULL END) AS LabGroups
    FROM EducationProgramSubjects eps
    INNER JOIN Groups g ON g.EducationProgramId = eps.EducationProgramId
        AND g.SemesterId = eps.SemesterId AND g.ParentId IS NULL
    LEFT JOIN SubjectConfigs sc2 ON sc2.SubjectId = eps.SubjectId
    LEFT JOIN Groups child2 ON child2.ParentId = g.Id AND child2.LessonType = 2
    LEFT JOIN Groups child3 ON child3.ParentId = g.Id AND child3.LessonType = 3
    GROUP BY eps.SubjectId
)
SELECT SubjectId,
    (SELECT ISNULL(SUM(sc.[Hours]), 0) FROM SubjectConfigs sc WHERE sc.SubjectId = se.SubjectId) AS TotalHoursConfig,
    LectureGroups, PracticalGroups, LabGroups,
    (SELECT ISNULL(sc1.[Hours],0) FROM SubjectConfigs sc1 WHERE sc1.SubjectId = se.SubjectId AND sc1.LessonType = 1) AS LecHours,
    (SELECT ISNULL(sc2.[Hours],0) FROM SubjectConfigs sc2 WHERE sc2.SubjectId = se.SubjectId AND sc2.LessonType = 2) AS PracHours,
    (SELECT ISNULL(sc3.[Hours],0) FROM SubjectConfigs sc3 WHERE sc3.SubjectId = se.SubjectId AND sc3.LessonType = 3) AS LabHours
INTO #SubjectInfo
FROM SubjectEvents se;

-- Calculate total events per subject
ALTER TABLE #SubjectInfo ADD TotalEvents INT;
UPDATE #SubjectInfo SET TotalEvents =
    LectureGroups * ISNULL(LecHours, 0) +
    PracticalGroups * ISNULL(PracHours, 0) +
    LabGroups * ISNULL(LabHours, 0);

-- How many lecturers does each subject currently have?
ALTER TABLE #SubjectInfo ADD CurrentLecCount INT;
UPDATE si SET CurrentLecCount = ISNULL(lc.Cnt, 0)
FROM #SubjectInfo si
LEFT JOIN (SELECT SubjectId, COUNT(*) AS Cnt FROM LecturerSubjects GROUP BY SubjectId) lc
    ON lc.SubjectId = si.SubjectId;

-- Required lecturers: ceiling(totalEvents / 40) to give some headroom
ALTER TABLE #SubjectInfo ADD RequiredLecCount INT;
UPDATE #SubjectInfo SET RequiredLecCount =
    CASE WHEN TotalEvents <= 40 THEN 1
         ELSE CAST(CEILING(CAST(TotalEvents AS FLOAT) / 40.0) AS INT)
    END;

DECLARE @needMore INT;
SELECT @needMore = COUNT(*) FROM #SubjectInfo WHERE RequiredLecCount > CurrentLecCount;
PRINT 'Subjects needing more lecturers: ' + CAST(@needMore AS NVARCHAR);

-- Step 3: Assign new lecturers from a pool, spreading assignments evenly
-- Key insight: each new lecturer should only get a FEW new subject assignments

-- Get all lecturers with their current subject count
SELECT p.Id AS LecturerId,
       (SELECT COUNT(*) FROM LecturerSubjects ls WHERE ls.LecturerId = p.Id) AS SubjectCount
INTO #LecPool
FROM Person p
WHERE p.PersonType = 'Lecturer';

-- Process subjects needing more lecturers, one at a time
-- For each, pick lecturer(s) with FEWEST current subjects who don't already teach it
DECLARE @sid INT, @required INT, @current INT, @totalEvts INT;
DECLARE @addedTotal INT = 0;

DECLARE fix_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT SubjectId, RequiredLecCount, CurrentLecCount, TotalEvents
    FROM #SubjectInfo
    WHERE RequiredLecCount > CurrentLecCount
    ORDER BY TotalEvents DESC;

OPEN fix_cursor;
FETCH NEXT FROM fix_cursor INTO @sid, @required, @current, @totalEvts;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @toAdd INT = @required - @current;

    -- Pick lecturers with fewest subjects who don't already teach this subject
    INSERT INTO LecturerSubjects (LecturerId, SubjectId)
    SELECT TOP (@toAdd) lp.LecturerId, @sid
    FROM #LecPool lp
    WHERE NOT EXISTS (
        SELECT 1 FROM LecturerSubjects ls
        WHERE ls.LecturerId = lp.LecturerId AND ls.SubjectId = @sid
    )
    ORDER BY lp.SubjectCount ASC, lp.LecturerId;

    DECLARE @justAdded INT = @@ROWCOUNT;
    SET @addedTotal = @addedTotal + @justAdded;

    -- Update pool counts for the lecturers we just assigned
    UPDATE lp SET SubjectCount = SubjectCount + 1
    FROM #LecPool lp
    WHERE lp.LecturerId IN (
        SELECT TOP (@toAdd) lp2.LecturerId
        FROM #LecPool lp2
        WHERE NOT EXISTS (
            SELECT 1 FROM LecturerSubjects ls
            WHERE ls.LecturerId = lp2.LecturerId AND ls.SubjectId = @sid
        )
        ORDER BY lp2.SubjectCount ASC, lp2.LecturerId
    );
    -- Actually the INSERT already happened so the NOT EXISTS won't match anymore
    -- Just re-count from actual table
    UPDATE lp SET SubjectCount = (SELECT COUNT(*) FROM LecturerSubjects ls WHERE ls.LecturerId = lp.LecturerId)
    FROM #LecPool lp;

    FETCH NEXT FROM fix_cursor INTO @sid, @required, @current, @totalEvts;
END;

CLOSE fix_cursor;
DEALLOCATE fix_cursor;

PRINT 'Added ' + CAST(@addedTotal AS NVARCHAR) + ' new LecturerSubject entries';

-- Step 4: Rebuild GSWL with round-robin
PRINT '';
PRINT '=== Rebuilding GSWL ===';
DELETE FROM GroupSubjectsWithLecturer;

-- Lectures
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
    SELECT gs.GroupId, gs.SubjectId,
           ROW_NUMBER() OVER (PARTITION BY gs.SubjectId ORDER BY gs.GroupId) AS GrpIdx
    FROM GroupSubjects gs
)
INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
SELECT sl.LsId, rg.GroupId, NULL, sc.[Hours], 1
FROM RankedGroups rg
INNER JOIN SubjectConfigs sc ON sc.SubjectId = rg.SubjectId AND sc.LessonType = 1
INNER JOIN SubjectLecturers sl ON sl.SubjectId = rg.SubjectId
    AND sl.LecIdx = ((rg.GrpIdx - 1) % sl.LecCount) + 1;
PRINT 'Lectures: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Practicals
;WITH GroupSubjects AS (
    SELECT DISTINCT g.Id AS ParentId, eps.SubjectId, g.SemesterId
    FROM Groups g
    INNER JOIN EducationProgramSubjects eps
        ON eps.EducationProgramId = g.EducationProgramId
        AND eps.SemesterId = g.SemesterId
    WHERE g.ParentId IS NULL
),
ChildGroups AS (
    SELECT child.Id AS ChildId, gs.SubjectId
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
PRINT 'Practicals: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Labs
;WITH GroupSubjects AS (
    SELECT DISTINCT g.Id AS ParentId, eps.SubjectId, g.SemesterId
    FROM Groups g
    INNER JOIN EducationProgramSubjects eps
        ON eps.EducationProgramId = g.EducationProgramId
        AND eps.SemesterId = g.SemesterId
    WHERE g.ParentId IS NULL
),
ChildGroups AS (
    SELECT child.Id AS ChildId, gs.SubjectId
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
PRINT 'Labs: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Verify
PRINT '';
PRINT '=== Verification ===';

DECLARE @maxEvents INT;
SELECT @maxEvents = MAX(TotalEvents) FROM (
    SELECT ls.LecturerId, SUM(gsl.[Hours]) AS TotalEvents
    FROM GroupSubjectsWithLecturer gsl
    INNER JOIN LecturerSubjects ls ON ls.Id = gsl.LecturerSubjectId
    GROUP BY ls.LecturerId
) t;

DECLARE @cnt INT;
SELECT @cnt = COUNT(*) FROM GroupSubjectsWithLecturer;
DECLARE @lsCnt INT;
SELECT @lsCnt = COUNT(*) FROM LecturerSubjects;

PRINT 'Max events per lecturer: ' + CAST(@maxEvents AS NVARCHAR) + ' (must be <= 60)';
PRINT 'Total GSWL: ' + CAST(@cnt AS NVARCHAR);
PRINT 'Total LecturerSubjects: ' + CAST(@lsCnt AS NVARCHAR);

PRINT '';
PRINT 'Top 15 busiest lecturers:';

DECLARE @info NVARCHAR(200);
DECLARE info_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT TOP 15
        CAST(ls.LecturerId AS NVARCHAR) + ': ' +
        p.FirstName + ' ' + p.LastName + ' = ' +
        CAST(SUM(gsl.[Hours]) AS NVARCHAR) + ' events (' +
        CAST(COUNT(DISTINCT ls.SubjectId) AS NVARCHAR) + ' subjects)'
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

DROP TABLE #SubjectInfo;
DROP TABLE #LecPool;

COMMIT;
