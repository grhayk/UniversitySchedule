-- ============================================================================
-- SEED TEST DATA FOR SCHEDULE GENERATOR TESTING
-- University Schedule System — Realistic data at production scale
-- ============================================================================
-- Run against: UniversitySchedule database
-- Prerequisites: Semesters and TimeTables must already exist
-- ============================================================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

-- ============================================================================
-- 1. STRUCTURES (University → Institutes → Chairs)
-- ============================================================================

-- University
INSERT INTO Structures (Code, [Type], ParentId, FromDate, ClassroomId)
VALUES (N'NPUA', 1, NULL, '2000-01-01', NULL);
DECLARE @uniId INT = SCOPE_IDENTITY();

-- 6 Institutes
DECLARE @instIds TABLE (Idx INT IDENTITY(1,1), Id INT);
DECLARE @instNames TABLE (Idx INT, Code NVARCHAR(50));
INSERT INTO @instNames VALUES
(1, N'INST-CS'),    -- Computer Science & IT
(2, N'INST-MECH'),  -- Mechanical Engineering
(3, N'INST-CIVIL'), -- Civil Engineering
(4, N'INST-ELEC'),  -- Electrical Engineering
(5, N'INST-ECON'),  -- Economics & Management
(6, N'INST-HUMA');  -- Humanities & Social Sciences

DECLARE @i INT = 1;
WHILE @i <= 6
BEGIN
    DECLARE @instCode NVARCHAR(50);
    SELECT @instCode = Code FROM @instNames WHERE Idx = @i;
    INSERT INTO Structures (Code, [Type], ParentId, FromDate, ClassroomId)
    VALUES (@instCode, 2, @uniId, '2000-01-01', NULL);
    INSERT INTO @instIds (Id) VALUES (SCOPE_IDENTITY());
    SET @i = @i + 1;
END;

-- ~25 Chairs distributed across institutes
-- Each institute gets 4-5 chairs
DECLARE @chairIds TABLE (Idx INT IDENTITY(1,1), Id INT, InstIdx INT);
DECLARE @chairDefs TABLE (Idx INT IDENTITY(1,1), InstIdx INT, Code NVARCHAR(50));
INSERT INTO @chairDefs (InstIdx, Code) VALUES
-- Institute 1: CS & IT (5 chairs)
(1, N'CHR-PROG'),   -- Programming
(1, N'CHR-AI'),     -- AI & Data Science
(1, N'CHR-NETS'),   -- Networks & Cybersecurity
(1, N'CHR-SENG'),   -- Software Engineering
(1, N'CHR-MATH'),   -- Applied Mathematics
-- Institute 2: Mechanical (4 chairs)
(2, N'CHR-MDES'),   -- Machine Design
(2, N'CHR-THERMO'), -- Thermodynamics
(2, N'CHR-MATSC'),  -- Materials Science
(2, N'CHR-ROBOT'),  -- Robotics & Automation
-- Institute 3: Civil (4 chairs)
(3, N'CHR-STRUC'),  -- Structural Engineering
(3, N'CHR-HYDRO'),  -- Hydraulics & Water
(3, N'CHR-GEOT'),   -- Geotechnics
(3, N'CHR-ARCH'),   -- Architecture
-- Institute 4: Electrical (4 chairs)
(4, N'CHR-POWER'),  -- Power Systems
(4, N'CHR-MICRO'),  -- Microelectronics
(4, N'CHR-TELECOM'),-- Telecommunications
(4, N'CHR-CONTROL'),-- Control Systems
-- Institute 5: Economics (4 chairs)
(5, N'CHR-MGMT'),   -- Management
(5, N'CHR-FIN'),    -- Finance & Accounting
(5, N'CHR-MKTG'),   -- Marketing
(5, N'CHR-ECON'),   -- Economic Theory
-- Institute 6: Humanities (4 chairs)
(6, N'CHR-LANG'),   -- Foreign Languages
(6, N'CHR-HIST'),   -- History
(6, N'CHR-PHIL'),   -- Philosophy
(6, N'CHR-PHYS');   -- Physics (general)

DECLARE @c INT = 1;
DECLARE @totalChairs INT = 25;
WHILE @c <= @totalChairs
BEGIN
    DECLARE @cInstIdx INT, @cCode NVARCHAR(50);
    SELECT @cInstIdx = InstIdx, @cCode = Code FROM @chairDefs WHERE Idx = @c;

    DECLARE @cInstId INT;
    SELECT @cInstId = Id FROM @instIds WHERE Idx = @cInstIdx;

    INSERT INTO Structures (Code, [Type], ParentId, FromDate, ClassroomId)
    VALUES (@cCode, 3, @cInstId, '2000-01-01', NULL);
    INSERT INTO @chairIds (Id, InstIdx) VALUES (SCOPE_IDENTITY(), @cInstIdx);
    SET @c = @c + 1;
END;

-- ============================================================================
-- 2. CLASSROOMS (~200 across 6 buildings)
-- ============================================================================
-- Building distribution: ~33 per building
-- Most classrooms belong to university (general), some to chairs (specialized)
-- Capacity ranges: small 20-30, medium 40-60, large 80-150

DECLARE @crIds TABLE (Idx INT IDENTITY(1,1), Id INT, Building TINYINT, StructId INT);

-- General university classrooms (~150, spread across 6 buildings)
DECLARE @bldg INT = 1;
WHILE @bldg <= 6
BEGIN
    DECLARE @room INT = 1;
    DECLARE @roomsInBldg INT = CASE
        WHEN @bldg <= 3 THEN 28  -- Larger buildings
        ELSE 22                   -- Smaller buildings
    END;

    WHILE @room <= @roomsInBldg
    BEGIN
        DECLARE @crName NVARCHAR(20) = CAST(@bldg AS NVARCHAR) + RIGHT('00' + CAST(@room AS NVARCHAR), 2);
        DECLARE @capacity INT = CASE
            WHEN @room <= 3 THEN 100 + (@room * 20)  -- Large lecture halls
            WHEN @room <= 8 THEN 50 + (@room * 5)    -- Medium rooms
            ELSE 20 + (ABS(CHECKSUM(NEWID())) % 20)   -- Small rooms 20-39
        END;

        INSERT INTO Classrooms ([Name], StructureId, Building)
        VALUES (@crName, @uniId, @bldg);

        DECLARE @crId INT = SCOPE_IDENTITY();
        INSERT INTO @crIds (Id, Building, StructId) VALUES (@crId, @bldg, @uniId);

        -- Characteristics
        INSERT INTO ClassroomCharacteristics (ClassroomId, [Type], SeatCapacity, HasComputer, ComputerCount, HasProjector, RenovationStatus, BlackboardCondition)
        VALUES (@crId,
            CASE WHEN @room <= 3 THEN 4 ELSE 4 END, -- DirectClassroom
            @capacity,
            CASE WHEN @room <= 5 THEN 1 ELSE 0 END,
            CASE WHEN @room <= 5 THEN @capacity ELSE 0 END,
            CASE WHEN @room <= 10 THEN 1 ELSE 0 END,
            1, -- Good
            3  -- Good
        );

        SET @room = @room + 1;
    END;
    SET @bldg = @bldg + 1;
END;

-- Chair-specific classrooms (~50 total, ~2 per chair, labs/specialized)
-- These belong to chairs (StructureType=3) and go into SubjectClassroom
DECLARE @ch INT = 1;
WHILE @ch <= @totalChairs
BEGIN
    DECLARE @chId INT;
    DECLARE @chInstIdx INT;
    SELECT @chId = Id, @chInstIdx = InstIdx FROM @chairIds WHERE Idx = @ch;

    -- Each chair gets 2 specialized rooms in its institute's building
    DECLARE @chBldg TINYINT = CAST(@chInstIdx AS TINYINT); -- Institute maps to building

    DECLARE @labRoom INT = 1;
    WHILE @labRoom <= 2
    BEGIN
        DECLARE @labName NVARCHAR(20) = CAST(@chBldg AS NVARCHAR) + RIGHT('00' + CAST(50 + (@ch - 1) * 2 + @labRoom AS NVARCHAR), 3);
        DECLARE @labCap INT = CASE WHEN @labRoom = 1 THEN 30 ELSE 20 END;

        INSERT INTO Classrooms ([Name], StructureId, Building)
        VALUES (@labName, @chId, @chBldg);

        SET @crId = SCOPE_IDENTITY();
        INSERT INTO @crIds (Id, Building, StructId) VALUES (@crId, @chBldg, @chId);

        INSERT INTO ClassroomCharacteristics (ClassroomId, [Type], SeatCapacity, HasComputer, ComputerCount, HasProjector, RenovationStatus, BlackboardCondition)
        VALUES (@crId,
            CASE WHEN @labRoom = 1 THEN 8   -- ChairRoom (the chair's office room)
                 ELSE 2                       -- Chair (chair teaching room)
            END,
            @labCap,
            1, @labCap, 1,
            1, 3
        );

        SET @labRoom = @labRoom + 1;
    END;

    SET @ch = @ch + 1;
END;

-- Update Structure.ClassroomId for each chair (first lab room of that chair)
UPDATE s SET s.ClassroomId = cr.Id
FROM Structures s
INNER JOIN (
    SELECT StructureId, MIN(Id) AS Id
    FROM Classrooms
    WHERE StructureId IN (SELECT Id FROM @chairIds)
    GROUP BY StructureId
) cr ON cr.StructureId = s.Id;

-- ============================================================================
-- 3. EDUCATION PROGRAMS (~73, distributed across chairs)
-- ============================================================================

DECLARE @epIds TABLE (Idx INT IDENTITY(1,1), Id INT, ChairIdx INT);

-- ~3 programs per chair = 75 programs (close to 73)
DECLARE @epDegrees TABLE (Idx INT, Degree TINYINT, Suffix NVARCHAR(10));
INSERT INTO @epDegrees VALUES (1, 1, N'-B'), (2, 2, N'-M'), (3, 1, N'-B2');

SET @ch = 1;
WHILE @ch <= @totalChairs
BEGIN
    SELECT @chId = Id FROM @chairIds WHERE Idx = @ch;
    SELECT @cCode = Code FROM @chairDefs WHERE Idx = @ch;

    DECLARE @ep INT = 1;
    DECLARE @epCount INT = CASE WHEN @ch <= 23 THEN 3 ELSE 2 END; -- 23*3 + 2*2 = 73

    WHILE @ep <= @epCount
    BEGIN
        DECLARE @epDeg TINYINT;
        DECLARE @epSuf NVARCHAR(10);
        SELECT @epDeg = Degree, @epSuf = Suffix FROM @epDegrees WHERE Idx = @ep;

        INSERT INTO EducationPrograms (Code, [Name], StructureId, ParentId)
        VALUES (
            @cCode + @epSuf,
            N'Program ' + @cCode + @epSuf,
            @chId,
            NULL
        );
        INSERT INTO @epIds (Id, ChairIdx) VALUES (SCOPE_IDENTITY(), @ch);
        SET @ep = @ep + 1;
    END;
    SET @ch = @ch + 1;
END;

-- ============================================================================
-- 4. SUBJECTS (~1003, distributed across chairs)
-- ============================================================================

DECLARE @subIds TABLE (Idx INT IDENTITY(1,1), Id INT, ChairIdx INT);

-- ~40 subjects per chair = 1000 subjects (close to 1003)
-- Each subject has SemesterIdFrom and SemesterIdTo range

-- Subject name prefixes by chair type
DECLARE @subjPrefixes TABLE (ChairIdx INT, Prefix NVARCHAR(50));
INSERT INTO @subjPrefixes VALUES
(1, N'Programming'),(2, N'AI'),(3, N'Networks'),(4, N'Software'),(5, N'Mathematics'),
(6, N'Mechanics'),(7, N'Thermodynamics'),(8, N'Materials'),(9, N'Robotics'),
(10, N'Structures'),(11, N'Hydraulics'),(12, N'Geotechnics'),(13, N'Architecture'),
(14, N'Power'),(15, N'Electronics'),(16, N'Telecom'),(17, N'Control'),
(18, N'Management'),(19, N'Finance'),(20, N'Marketing'),(21, N'Economics'),
(22, N'Languages'),(23, N'History'),(24, N'Philosophy'),(25, N'Physics');

SET @ch = 1;
WHILE @ch <= @totalChairs
BEGIN
    SELECT @chId = Id FROM @chairIds WHERE Idx = @ch;

    DECLARE @subjPrefix NVARCHAR(50);
    SELECT @subjPrefix = Prefix FROM @subjPrefixes WHERE ChairIdx = @ch;

    DECLARE @s INT = 1;
    DECLARE @subjCount INT = CASE
        WHEN @ch <= 3 THEN 45   -- Larger departments
        WHEN @ch <= 10 THEN 40
        ELSE 38
    END;

    WHILE @s <= @subjCount
    BEGIN
        -- Semester range: subjects span 1-2 semesters
        -- Use Bachelor Stationary semesters (IDs 1-8) as the range
        DECLARE @semFrom INT = (((@s - 1) % 8) + 1); -- 1 to 8
        DECLARE @semTo INT = CASE
            WHEN @semFrom + 1 <= 8 THEN @semFrom + 1
            ELSE @semFrom
        END;

        INSERT INTO Subjects (Code, [Name], SemesterIdFrom, SemesterIdTo, StructureId)
        VALUES (
            @subjPrefix + '-' + RIGHT('000' + CAST(@s AS NVARCHAR), 3),
            @subjPrefix + N' Course ' + CAST(@s AS NVARCHAR),
            @semFrom,
            @semTo,
            @chId
        );
        INSERT INTO @subIds (Id, ChairIdx) VALUES (SCOPE_IDENTITY(), @ch);
        SET @s = @s + 1;
    END;
    SET @ch = @ch + 1;
END;

-- ============================================================================
-- 5. SUBJECT CONFIGS (Lecture/Practical/Lab hours per subject)
-- ============================================================================
-- Most subjects: Lecture(2) + Practical(1)
-- Some subjects: Lecture(2) + Lab(1)
-- Some subjects: Lecture(1) + Practical(2) + Lab(1)
-- Some subjects: Lecture(2) only

DECLARE @si INT = 1;
DECLARE @totalSubjects INT = (SELECT COUNT(*) FROM @subIds);

WHILE @si <= @totalSubjects
BEGIN
    DECLARE @subId INT;
    SELECT @subId = Id FROM @subIds WHERE Idx = @si;

    DECLARE @pattern INT = ((@si - 1) % 4); -- 0,1,2,3

    IF @pattern = 0
    BEGIN
        -- Lecture(2) + Practical(1)
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 1, 2);
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 2, 1);
    END
    ELSE IF @pattern = 1
    BEGIN
        -- Lecture(2) + Lab(1)
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 1, 2);
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 3, 1);
    END
    ELSE IF @pattern = 2
    BEGIN
        -- Lecture(1) + Practical(2) + Lab(1)
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 1, 1);
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 2, 2);
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 3, 1);
    END
    ELSE
    BEGIN
        -- Lecture(2) only
        INSERT INTO SubjectConfigs (SubjectId, LessonType, [Hours]) VALUES (@subId, 1, 2);
    END;

    SET @si = @si + 1;
END;

-- ============================================================================
-- 6. LECTURERS (~698)
-- ============================================================================

-- Armenian first/last names for realism
DECLARE @firstNames TABLE (Idx INT IDENTITY(1,1), [Name] NVARCHAR(50));
INSERT INTO @firstNames ([Name]) VALUES
(N'Armen'),(N'Hayk'),(N'Davit'),(N'Gor'),(N'Tigran'),(N'Sargis'),(N'Levon'),(N'Narek'),
(N'Vahe'),(N'Gagik'),(N'Arman'),(N'Karen'),(N'Hovhannes'),(N'Ashot'),(N'Gevorg'),
(N'Anna'),(N'Maria'),(N'Lilit'),(N'Ani'),(N'Nare'),(N'Sona'),(N'Marine'),(N'Lusine'),
(N'Gayane'),(N'Tatev'),(N'Elen'),(N'Diana'),(N'Nune'),(N'Anahit'),(N'Ruzanna');

DECLARE @lastNames TABLE (Idx INT IDENTITY(1,1), [Name] NVARCHAR(50));
INSERT INTO @lastNames ([Name]) VALUES
(N'Hakobyan'),(N'Grigoryan'),(N'Sargsyan'),(N'Hovhannisyan'),(N'Petrosyan'),
(N'Ghazaryan'),(N'Harutyunyan'),(N'Martirosyan'),(N'Karapetyan'),(N'Poghosyan'),
(N'Manukyan'),(N'Arakelyan'),(N'Davtyan'),(N'Movsisyan'),(N'Avetisyan'),
(N'Stepanyan'),(N'Gabrielyan'),(N'Yeghiazaryan'),(N'Asatryan'),(N'Baghdasaryan'),
(N'Khachatryan'),(N'Danielyan'),(N'Galstyan'),(N'Mkrtchyan'),(N'Simonyan');

DECLARE @fnCount INT = 30;
DECLARE @lnCount INT = 25;

DECLARE @lecIds TABLE (Idx INT IDENTITY(1,1), Id INT, ChairIdx INT);
DECLARE @lec INT = 1;
DECLARE @lecTotal INT = 698;

WHILE @lec <= @lecTotal
BEGIN
    -- Distribute lecturers across chairs (~28 per chair)
    DECLARE @lecChairIdx INT = ((@lec - 1) % @totalChairs) + 1;
    SELECT @chId = Id FROM @chairIds WHERE Idx = @lecChairIdx;

    DECLARE @fn NVARCHAR(50), @ln NVARCHAR(50);
    SELECT @fn = [Name] FROM @firstNames WHERE Idx = ((@lec - 1) % @fnCount) + 1;
    SELECT @ln = [Name] FROM @lastNames WHERE Idx = ((@lec - 1) % @lnCount) + 1;

    -- Add a number suffix to avoid duplicate names
    IF @lec > @fnCount * @lnCount
        SET @fn = @fn + CAST(@lec / (@fnCount * @lnCount) AS NVARCHAR);

    INSERT INTO Person (FirstName, LastName, StructureId, BirthDate, PersonType, GroupId, EducationDegree, EducationType)
    VALUES (
        @fn, @ln, @chId,
        DATEADD(YEAR, -(30 + (@lec % 30)), '2026-01-01'),
        N'Lecturer',
        NULL, NULL, NULL
    );
    INSERT INTO @lecIds (Id, ChairIdx) VALUES (SCOPE_IDENTITY(), @lecChairIdx);
    SET @lec = @lec + 1;
END;

-- ============================================================================
-- 7. LECTURER-SUBJECT ASSIGNMENTS
-- ============================================================================
-- Each lecturer teaches 2-4 subjects from their chair
-- Each subject should have at least 1 lecturer

DECLARE @lsIds TABLE (Idx INT IDENTITY(1,1), Id INT, LecturerId INT, SubjectId INT);

-- First: ensure every subject has at least one lecturer
SET @si = 1;
WHILE @si <= @totalSubjects
BEGIN
    DECLARE @sChairIdx INT;
    SELECT @subId = Id, @sChairIdx = ChairIdx FROM @subIds WHERE Idx = @si;

    -- Pick a lecturer from the same chair
    DECLARE @lecForSubj INT;
    SELECT TOP 1 @lecForSubj = Id FROM @lecIds
    WHERE ChairIdx = @sChairIdx
    ORDER BY (SELECT ABS(CHECKSUM(NEWID())));

    IF @lecForSubj IS NOT NULL
    BEGIN
        INSERT INTO LecturerSubjects (LecturerId, SubjectId)
        VALUES (@lecForSubj, @subId);
        INSERT INTO @lsIds (Id, LecturerId, SubjectId) VALUES (SCOPE_IDENTITY(), @lecForSubj, @subId);
    END;
    SET @si = @si + 1;
END;

-- Second: give some lecturers additional subjects (so they teach 2-3)
SET @lec = 1;
WHILE @lec <= @lecTotal
BEGIN
    DECLARE @lecIdVal INT, @lecChIdx INT;
    SELECT @lecIdVal = Id, @lecChIdx = ChairIdx FROM @lecIds WHERE Idx = @lec;

    -- Add 1 extra subject from same chair (if not already assigned)
    DECLARE @extraSubId INT;
    SELECT TOP 1 @extraSubId = s.Id
    FROM @subIds s
    WHERE s.ChairIdx = @lecChIdx
      AND s.Id NOT IN (SELECT SubjectId FROM @lsIds WHERE LecturerId = @lecIdVal)
    ORDER BY (SELECT ABS(CHECKSUM(NEWID())));

    IF @extraSubId IS NOT NULL
    BEGIN
        INSERT INTO LecturerSubjects (LecturerId, SubjectId)
        VALUES (@lecIdVal, @extraSubId);
        INSERT INTO @lsIds (Id, LecturerId, SubjectId) VALUES (SCOPE_IDENTITY(), @lecIdVal, @extraSubId);
    END;

    SET @lec = @lec + 1;
END;

-- ============================================================================
-- 8. GROUPS (281 parent + child subgroups)
-- ============================================================================
-- Each ed program gets ~4 parent groups distributed across semesters
-- Parent groups: LessonType=1 (Lecture), ParentId=NULL
-- Children: Practical (ceil(students/25)), Lab (ceil(students/16))

DECLARE @grpIds TABLE (Idx INT IDENTITY(1,1), Id INT, EpIdx INT, SemId INT, IsParent BIT, ParentIdx INT, StudentCount INT);
DECLARE @grpCount INT = 0;
DECLARE @totalEps INT = (SELECT COUNT(*) FROM @epIds);
DECLARE @parentGroupCount INT = 0;

DECLARE @epi INT = 1;
WHILE @epi <= @totalEps AND @parentGroupCount < 281
BEGIN
    DECLARE @epId INT, @epChIdx INT;
    SELECT @epId = Id, @epChIdx = ChairIdx FROM @epIds WHERE Idx = @epi;

    -- Each ed program gets 3-4 parent groups in different semesters
    DECLARE @gpPerEp INT = CASE WHEN @epi <= 50 THEN 4 ELSE 3 END;
    IF @parentGroupCount + @gpPerEp > 281
        SET @gpPerEp = 281 - @parentGroupCount;

    DECLARE @g INT = 1;
    WHILE @g <= @gpPerEp
    BEGIN
        -- Assign to a semester (cycle through Bachelor Stationary 1-8)
        DECLARE @semIdx INT = ((@parentGroupCount) % 8) + 1; -- Semester IDs 1-8

        -- Random student count 15-45
        DECLARE @studCount INT = 15 + (ABS(CHECKSUM(NEWID())) % 31);

        INSERT INTO Groups (ParentId, EducationProgramId, SemesterId, LessonType, IsActive, StartDate, IndexNumber, BranchedFromGroupId)
        VALUES (NULL, @epId, @semIdx, 1, 1, '2025-09-01', @g, NULL);

        SET @grpCount = @grpCount + 1;
        SET @parentGroupCount = @parentGroupCount + 1;
        DECLARE @parentGrpId INT = SCOPE_IDENTITY();
        DECLARE @parentIdx INT = @grpCount;
        INSERT INTO @grpIds (Id, EpIdx, SemId, IsParent, ParentIdx, StudentCount)
        VALUES (@parentGrpId, @epi, @semIdx, 1, @parentIdx, @studCount);

        -- Create Practical subgroups: ceil(students/25)
        DECLARE @practCount INT = (@studCount + 24) / 25;
        DECLARE @p INT = 1;
        WHILE @p <= @practCount
        BEGIN
            INSERT INTO Groups (ParentId, EducationProgramId, SemesterId, LessonType, IsActive, StartDate, IndexNumber, BranchedFromGroupId)
            VALUES (@parentGrpId, @epId, @semIdx, 2, 1, '2025-09-01', @p, NULL);

            SET @grpCount = @grpCount + 1;
            INSERT INTO @grpIds (Id, EpIdx, SemId, IsParent, ParentIdx, StudentCount)
            VALUES (SCOPE_IDENTITY(), @epi, @semIdx, 0, @parentIdx, (@studCount + @practCount - 1) / @practCount);

            SET @p = @p + 1;
        END;

        -- Create Lab subgroups: ceil(students/16)
        DECLARE @labCount INT = (@studCount + 15) / 16;
        DECLARE @l INT = 1;
        WHILE @l <= @labCount
        BEGIN
            INSERT INTO Groups (ParentId, EducationProgramId, SemesterId, LessonType, IsActive, StartDate, IndexNumber, BranchedFromGroupId)
            VALUES (@parentGrpId, @epId, @semIdx, 3, 1, '2025-09-01', @l, NULL);

            SET @grpCount = @grpCount + 1;
            INSERT INTO @grpIds (Id, EpIdx, SemId, IsParent, ParentIdx, StudentCount)
            VALUES (SCOPE_IDENTITY(), @epi, @semIdx, 0, @parentIdx, (@studCount + @labCount - 1) / @labCount);

            SET @l = @l + 1;
        END;

        SET @g = @g + 1;
    END;
    SET @epi = @epi + 1;
END;

-- ============================================================================
-- 9. STUDENTS (~5287)
-- ============================================================================
-- Distribute across parent groups based on StudentCount

DECLARE @studTotal INT = 0;
DECLARE @gi INT = 1;
DECLARE @totalGroups INT = @grpCount;

-- Only iterate parent groups
WHILE @gi <= @totalGroups AND @studTotal < 5287
BEGIN
    DECLARE @gIsParent BIT, @gId INT, @gStudCount INT, @gSemId INT;
    SELECT @gIsParent = IsParent, @gId = Id, @gStudCount = StudentCount, @gSemId = SemId
    FROM @grpIds WHERE Idx = @gi;

    IF @gIsParent = 1
    BEGIN
        -- Get chair for this group's ed program
        DECLARE @gEpIdx INT;
        SELECT @gEpIdx = EpIdx FROM @grpIds WHERE Idx = @gi;
        SELECT @epChIdx = ChairIdx FROM @epIds WHERE Idx = @gEpIdx;
        SELECT @chId = Id FROM @chairIds WHERE Idx = @epChIdx;

        DECLARE @st INT = 1;
        WHILE @st <= @gStudCount AND @studTotal < 5287
        BEGIN
            SELECT @fn = [Name] FROM @firstNames WHERE Idx = ((@studTotal) % @fnCount) + 1;
            SELECT @ln = [Name] FROM @lastNames WHERE Idx = ((@studTotal) % @lnCount) + 1;

            INSERT INTO Person (FirstName, LastName, StructureId, BirthDate, PersonType, GroupId, EducationDegree, EducationType)
            VALUES (
                @fn, @ln, @chId,
                DATEADD(YEAR, -(18 + (@studTotal % 7)), '2026-01-01'),
                N'Student',
                @gId,
                1, -- Bachelor
                1  -- Stationary
            );

            -- Also add to StudentGroups
            INSERT INTO StudentGroups (StudentId, GroupId, SemesterId)
            VALUES (SCOPE_IDENTITY(), @gId, @gSemId);

            SET @studTotal = @studTotal + 1;
            SET @st = @st + 1;
        END;
    END;
    SET @gi = @gi + 1;
END;

-- ============================================================================
-- 10. EDUCATION PROGRAM SUBJECTS
-- ============================================================================
-- Each ed program gets 6-8 subjects from its chair
-- This defines which subjects are taught in which program/semester

SET @epi = 1;
WHILE @epi <= @totalEps
BEGIN
    SELECT @epId = Id, @epChIdx = ChairIdx FROM @epIds WHERE Idx = @epi;

    DECLARE @epsCount INT = 6 + (ABS(CHECKSUM(NEWID())) % 3); -- 6-8 subjects
    DECLARE @eps INT = 1;

    -- Get subjects from this chair
    WHILE @eps <= @epsCount
    BEGIN
        -- Pick the eps-th subject from this chair
        DECLARE @epsSubId INT = NULL;
        SELECT @epsSubId = Id FROM (
            SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS RN
            FROM @subIds WHERE ChairIdx = @epChIdx
        ) t WHERE RN = @eps;

        IF @epsSubId IS NOT NULL
        BEGIN
            -- Assign to a semester (cycle through 1-8)
            DECLARE @epsSemId INT = ((@eps - 1) % 8) + 1;

            INSERT INTO EducationProgramSubjects (EducationProgramId, SubjectId, SemesterId, FromDate, ToDate)
            VALUES (@epId, @epsSubId, @epsSemId, '2025-09-01', NULL);
        END;
        SET @eps = @eps + 1;
    END;
    SET @epi = @epi + 1;
END;

-- ============================================================================
-- 11. SUBJECT-CLASSROOM (specialized chair classrooms only)
-- ============================================================================
-- Only subjects that need lab rooms get entries here
-- Maps chair subjects to their chair's lab classrooms

SET @si = 1;
WHILE @si <= @totalSubjects
BEGIN
    SELECT @subId = Id, @sChairIdx = ChairIdx FROM @subIds WHERE Idx = @si;

    -- Check if this subject has Lab config (pattern 1 or 2 from SubjectConfigs)
    IF EXISTS (SELECT 1 FROM SubjectConfigs WHERE SubjectId = @subId AND LessonType = 3)
    BEGIN
        -- Get chair's lab classrooms
        SELECT @chId = Id FROM @chairIds WHERE Idx = @sChairIdx;

        INSERT INTO SubjectClassrooms (SubjectId, LessonType, ClassroomId)
        SELECT @subId, 3, c.Id
        FROM Classrooms c
        WHERE c.StructureId = @chId;
    END;

    SET @si = @si + 1;
END;

-- ============================================================================
-- 12. GROUP-SUBJECT-WITH-LECTURER (the backbone)
-- ============================================================================
-- For each parent group: find subjects from its ed program's EducationProgramSubjects
-- that match the group's semester, then create GSWL entries

-- Process parent groups
SET @gi = 1;
WHILE @gi <= @totalGroups
BEGIN
    SELECT @gIsParent = IsParent, @gId = Id, @gSemId = SemId
    FROM @grpIds WHERE Idx = @gi;

    IF @gIsParent = 1
    BEGIN
        SELECT @gEpIdx = EpIdx FROM @grpIds WHERE Idx = @gi;
        DECLARE @gEpId INT;
        SELECT @gEpId = Id FROM @epIds WHERE Idx = @gEpIdx;

        -- Get subjects for this group's ed program and semester
        DECLARE @subjCursor TABLE (SubjId INT, LessonType TINYINT, [Hours] TINYINT);
        DELETE FROM @subjCursor;

        INSERT INTO @subjCursor (SubjId, LessonType, [Hours])
        SELECT eps.SubjectId, sc.LessonType, sc.[Hours]
        FROM EducationProgramSubjects eps
        INNER JOIN SubjectConfigs sc ON sc.SubjectId = eps.SubjectId
        WHERE eps.EducationProgramId = @gEpId
          AND eps.SemesterId = @gSemId;

        -- For each subject+lessonType, create GSWL
        DECLARE @scSubjId INT, @scLT TINYINT, @scHours TINYINT;
        DECLARE subjCur CURSOR LOCAL FAST_FORWARD FOR
            SELECT SubjId, LessonType, [Hours] FROM @subjCursor;
        OPEN subjCur;
        FETCH NEXT FROM subjCur INTO @scSubjId, @scLT, @scHours;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Find a LecturerSubject for this subject
            DECLARE @lsId INT = NULL;
            SELECT TOP 1 @lsId = Id FROM @lsIds WHERE SubjectId = @scSubjId;

            IF @lsId IS NOT NULL
            BEGIN
                IF @scLT = 1 -- Lecture: assign to parent group
                BEGIN
                    INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
                    VALUES (@lsId, @gId, NULL, @scHours, @scLT);
                END
                ELSE -- Practical/Lab: assign to each matching child group
                BEGIN
                    INSERT INTO GroupSubjectsWithLecturer (LecturerSubjectId, GroupId, FlowId, [Hours], LessonType)
                    SELECT @lsId, g.Id, NULL, @scHours, @scLT
                    FROM @grpIds gd
                    INNER JOIN Groups g ON g.Id = gd.Id
                    WHERE gd.ParentIdx = @gi
                      AND gd.IsParent = 0
                      AND g.LessonType = @scLT;
                END;
            END;

            FETCH NEXT FROM subjCur INTO @scSubjId, @scLT, @scHours;
        END;
        CLOSE subjCur;
        DEALLOCATE subjCur;
    END;
    SET @gi = @gi + 1;
END;

-- ============================================================================
-- SUMMARY
-- ============================================================================
PRINT 'Seed data inserted successfully!';
PRINT '';

DECLARE @msg NVARCHAR(200);

SET @msg = 'Structures: ' + CAST((SELECT COUNT(*) FROM Structures) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Classrooms: ' + CAST((SELECT COUNT(*) FROM Classrooms) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Education Programs: ' + CAST((SELECT COUNT(*) FROM EducationPrograms) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Subjects: ' + CAST((SELECT COUNT(*) FROM Subjects) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Subject Configs: ' + CAST((SELECT COUNT(*) FROM SubjectConfigs) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Lecturers: ' + CAST((SELECT COUNT(*) FROM Person WHERE PersonType = 'Lecturer') AS NVARCHAR);
PRINT @msg;
SET @msg = 'Students: ' + CAST((SELECT COUNT(*) FROM Person WHERE PersonType = 'Student') AS NVARCHAR);
PRINT @msg;
SET @msg = 'Parent Groups: ' + CAST((SELECT COUNT(*) FROM Groups WHERE ParentId IS NULL) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Total Groups (with children): ' + CAST((SELECT COUNT(*) FROM Groups) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Lecturer-Subject: ' + CAST((SELECT COUNT(*) FROM LecturerSubjects) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Ed Program Subjects: ' + CAST((SELECT COUNT(*) FROM EducationProgramSubjects) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Subject Classrooms: ' + CAST((SELECT COUNT(*) FROM SubjectClassrooms) AS NVARCHAR);
PRINT @msg;
SET @msg = 'GroupSubjectWithLecturer: ' + CAST((SELECT COUNT(*) FROM GroupSubjectsWithLecturer) AS NVARCHAR);
PRINT @msg;
SET @msg = 'Student Groups: ' + CAST((SELECT COUNT(*) FROM StudentGroups) AS NVARCHAR);
PRINT @msg;

COMMIT;
