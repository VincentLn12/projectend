/*
  Copies the data of the old website database into the new ComSci database.
  See README.md in this folder for how to run it and for the rules it applies.

  sqlcmd -S .\SQLEXPRESS -E -C -I -f 65001 -v SourceDb=Webcomcs TargetDb=ComSciDb ShowProfile=0 -i MigrateFromWebcomcs.sql

  Safe to run again: rows that already exist are skipped (children such as subjects or pictures are only
  added to a parent that has none yet), and everything runs in one transaction.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

USE [$(TargetDb)];
GO

BEGIN TRANSACTION;

DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @showProfile bit = $(ShowProfile);
DECLARE @seededWebName nvarchar(200) = N'สาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยราชภัฏกาญจนบุรี';

-- The API creates this admin and the four roles on its first start.
DECLARE @adminId uniqueidentifier = (SELECT TOP (1) Id FROM dbo.Users WHERE Email = N'admin@comsci.local');
IF @adminId IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
    THROW 50000, 'Start the API once first so it creates the roles and the first admin.', 1;

CREATE TABLE #Report (Item nvarchar(60) NOT NULL, RowsAdded int NOT NULL);

-- ---------------------------------------------------------------------------
-- 1. Which old users move.  IsUsed = 0 are test or switched-off accounts: skipped, together with
--    the job histories and course registrations that belong to them.
--    The type follows the old role rows: Admin > Lecturer > Student > everything else.
-- ---------------------------------------------------------------------------
CREATE TABLE #Users (OldId nvarchar(900) NOT NULL, NewId uniqueidentifier NOT NULL PRIMARY KEY, UserType tinyint NOT NULL);

INSERT INTO #Users (OldId, NewId, UserType)
SELECT u.Id,
       CAST(u.Id AS uniqueidentifier),
       CASE
           WHEN EXISTS (SELECT 1 FROM [$(SourceDb)].dbo.AspNetUserRoles ur JOIN [$(SourceDb)].dbo.AspNetRoles r ON r.Id = ur.RoleId WHERE ur.UserId = u.Id AND r.Name = N'Admin') THEN 4
           WHEN EXISTS (SELECT 1 FROM [$(SourceDb)].dbo.AspNetUserRoles ur JOIN [$(SourceDb)].dbo.AspNetRoles r ON r.Id = ur.RoleId WHERE ur.UserId = u.Id AND r.Name = N'Lecturer') THEN 3
           WHEN EXISTS (SELECT 1 FROM [$(SourceDb)].dbo.AspNetUserRoles ur JOIN [$(SourceDb)].dbo.AspNetRoles r ON r.Id = ur.RoleId WHERE ur.UserId = u.Id AND r.Name = N'Student') THEN 2
           ELSE 1
       END
FROM [$(SourceDb)].dbo.AspNetUsers u
WHERE u.IsUsed = 1;

-- ---------------------------------------------------------------------------
-- 2. Ids.  The old content ids are text like 'PROJ261D1D6D260729', so each row gets a GUID computed
--    from its old id (same input, same GUID): that is what makes the script re-runnable.
-- ---------------------------------------------------------------------------
CREATE TABLE #ProjectMap  (OldId nvarchar(100) NOT NULL PRIMARY KEY, NewId uniqueidentifier NOT NULL);
CREATE TABLE #NewsMap     (OldId nvarchar(100) NOT NULL PRIMARY KEY, NewId uniqueidentifier NOT NULL);
CREATE TABLE #CourseMap   (OldId nvarchar(100) NOT NULL PRIMARY KEY, NewId uniqueidentifier NOT NULL);
CREATE TABLE #GenMap      (OldId nvarchar(100) NOT NULL PRIMARY KEY, NewId uniqueidentifier NOT NULL);
CREATE TABLE #SyllabusMap (OldId nvarchar(100) NOT NULL PRIMARY KEY, NewId uniqueidentifier NOT NULL);

INSERT INTO #ProjectMap
SELECT p.Id, CAST(HASHBYTES('MD5', N'proj:' + p.Id) AS uniqueidentifier)
FROM [$(SourceDb)].dbo.Projects p
WHERE EXISTS (SELECT 1 FROM #Users m WHERE m.OldId = p.StudentId);

INSERT INTO #NewsMap
SELECT n.Id, CAST(HASHBYTES('MD5', N'news:' + n.Id) AS uniqueidentifier) FROM [$(SourceDb)].dbo.Newses n;

INSERT INTO #CourseMap
SELECT c.Id, CAST(HASHBYTES('MD5', N'course:' + c.Id) AS uniqueidentifier) FROM [$(SourceDb)].dbo.Courses c;

INSERT INTO #GenMap
SELECT g.Id, CAST(HASHBYTES('MD5', N'gen:' + g.Id) AS uniqueidentifier) FROM [$(SourceDb)].dbo.Generations g;

INSERT INTO #SyllabusMap
SELECT s.Id, CAST(HASHBYTES('MD5', N'syllabus:' + s.Id) AS uniqueidentifier) FROM [$(SourceDb)].dbo.Syllabuses s;

-- ---------------------------------------------------------------------------
-- 3. Files.  The old database only keeps file NAMES (the files lived on the old server's disk).
--    Each name becomes a row with StoragePath = legacy/<name>; copy the old files into
--    <uploads>/legacy/ and they start working (see README.md).
-- ---------------------------------------------------------------------------
CREATE TABLE #FileMap (Name nvarchar(450) NOT NULL PRIMARY KEY, Id uniqueidentifier NOT NULL);

INSERT INTO #FileMap (Name, Id)
SELECT x.Name, CAST(HASHBYTES('MD5', N'file:' + x.Name) AS uniqueidentifier)
FROM (
    SELECT u.Image AS Name FROM [$(SourceDb)].dbo.AspNetUsers u JOIN #Users m ON m.OldId = u.Id
    UNION SELECT Image FROM [$(SourceDb)].dbo.Lecturers
    UNION SELECT Url FROM [$(SourceDb)].dbo.NewsPhotos
    UNION SELECT Url FROM [$(SourceDb)].dbo.HomePhotos
    UNION SELECT Url FROM [$(SourceDb)].dbo.CoursePhoto
    UNION SELECT URL FROM [$(SourceDb)].dbo.ComputerScienceImages
    UNION SELECT p.Image FROM [$(SourceDb)].dbo.Projects p JOIN #ProjectMap pm ON pm.OldId = p.Id
    UNION SELECT p.PDF FROM [$(SourceDb)].dbo.Projects p JOIN #ProjectMap pm ON pm.OldId = p.Id
    UNION SELECT PDF FROM [$(SourceDb)].dbo.Syllabuses
    UNION SELECT GenPhoto FROM [$(SourceDb)].dbo.Generations
    UNION SELECT Logo FROM [$(SourceDb)].dbo.SystemSettings
) x
WHERE x.Name IS NOT NULL AND LTRIM(RTRIM(x.Name)) <> N'';

INSERT INTO dbo.Files (Id, OriginalName, StoragePath, ContentType, SizeBytes, UploadedBy, UploadedAt)
SELECT f.Id,
       f.Name,
       N'legacy/' + f.Name,
       CASE
           WHEN LOWER(f.Name) LIKE N'%.pdf'  THEN N'application/pdf'
           WHEN LOWER(f.Name) LIKE N'%.png'  THEN N'image/png'
           WHEN LOWER(f.Name) LIKE N'%.gif'  THEN N'image/gif'
           WHEN LOWER(f.Name) LIKE N'%.webp' THEN N'image/webp'
           ELSE N'image/jpeg'
       END,
       0,
       @adminId,
       @now
FROM #FileMap f
WHERE NOT EXISTS (SELECT 1 FROM dbo.Files x WHERE x.Id = f.Id);
INSERT INTO #Report VALUES (N'Files (names only, see README)', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 4. Users, roles and student profiles.
--    Passwords are copied as they are: both systems use the same ASP.NET Identity v3 hash.
--    ShowProfile starts as the value given on the command line (default 0 = hidden until the
--    person agrees); nobody gets a consent date because none was ever recorded.
-- ---------------------------------------------------------------------------
INSERT INTO dbo.Users (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp,
                       ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount,
                       FullName, Bio, ImageFileId, UserType, ShowEmail, ShowProfile, ConsentAt, IsPublished,
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT m.NewId, u.UserName, u.NormalizedUserName, u.Email, u.NormalizedEmail, u.EmailConfirmed, u.PasswordHash, u.SecurityStamp,
       u.ConcurrencyStamp, u.PhoneNumber, u.PhoneNumberConfirmed, u.TwoFactorEnabled, u.LockoutEnd, u.LockoutEnabled, 0,
       ISNULL(NULLIF(LTRIM(RTRIM(u.FullName)), N''), u.UserName), u.Bio, fm.Id, m.UserType, 0, @showProfile, NULL, 1,
       u.CreatedAt, NULL, NULL, NULL, 0
FROM #Users m
JOIN [$(SourceDb)].dbo.AspNetUsers u ON u.Id = m.OldId
LEFT JOIN #FileMap fm ON fm.Name = u.Image
WHERE NOT EXISTS (SELECT 1 FROM dbo.Users x WHERE x.Id = m.NewId);
INSERT INTO #Report VALUES (N'Users', @@ROWCOUNT);

INSERT INTO dbo.UserRoles (UserId, RoleId)
SELECT m.NewId, r.Id
FROM #Users m
JOIN dbo.Roles r ON r.Name = CASE m.UserType WHEN 4 THEN N'Admin' WHEN 3 THEN N'Lecturer' WHEN 2 THEN N'Student' ELSE N'User' END
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserRoles x WHERE x.UserId = m.NewId AND x.RoleId = r.Id);
INSERT INTO #Report VALUES (N'UserRoles', @@ROWCOUNT);

-- Student code = old user name (11 digits). Entry year = first two digits of the code + 2500:
-- the old UserStudent.YearEdu does not agree with the codes, so it is not used.
INSERT INTO dbo.StudentProfiles (UserId, StudentCode, EntryYear, PreviousSchool, Address)
SELECT m.NewId, u.UserName, CAST(2500 + CAST(LEFT(u.UserName, 2) AS int) AS smallint), s.OldEdu, s.Address
FROM #Users m
JOIN [$(SourceDb)].dbo.AspNetUsers u ON u.Id = m.OldId
LEFT JOIN [$(SourceDb)].dbo.UserStudent s ON s.Id = u.Id
WHERE m.UserType = 2
  AND u.UserName NOT LIKE N'%[^0-9]%' AND LEN(u.UserName) >= 8
  AND NOT EXISTS (SELECT 1 FROM dbo.StudentProfiles x WHERE x.UserId = m.NewId);
INSERT INTO #Report VALUES (N'StudentProfiles', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 5. Lecturers.  The 7 on the old public list keep their ids and visibility.
--    Accounts with the Lecturer role, and accounts used as a course's lecturer, get a lecturer row
--    (same id as the user, not published) so courses can point at it.
-- ---------------------------------------------------------------------------
INSERT INTO dbo.Lecturers (Id, UserId, Prefix, FullName, Position, Expertise, EducationLevel, Program, Contact, ImageFileId,
                           SortOrder, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT l.Id, NULL, l.Prefix, ISNULL(l.FullName, N''), l.Position, l.Expert, NULL, NULL, NULL, fm.Id,
       CAST(ROW_NUMBER() OVER (ORDER BY l.CreatedAt, l.Id) - 1 AS int),
       CASE WHEN l.Hidden = 1 THEN 0 ELSE 1 END,
       l.CreatedAt, NULL, NULL, NULL, 0
FROM [$(SourceDb)].dbo.Lecturers l
LEFT JOIN #FileMap fm ON fm.Name = l.Image
WHERE NOT EXISTS (SELECT 1 FROM dbo.Lecturers x WHERE x.Id = l.Id);
INSERT INTO #Report VALUES (N'Lecturers (public list)', @@ROWCOUNT);

CREATE TABLE #LecturerUsers (UserId uniqueidentifier NOT NULL PRIMARY KEY);
INSERT INTO #LecturerUsers (UserId)
SELECT m.NewId
FROM #Users m
WHERE EXISTS (SELECT 1 FROM [$(SourceDb)].dbo.AspNetUserRoles ur JOIN [$(SourceDb)].dbo.AspNetRoles r ON r.Id = ur.RoleId
              WHERE ur.UserId = m.OldId AND r.Name = N'Lecturer')
   OR EXISTS (SELECT 1 FROM [$(SourceDb)].dbo.Courses c WHERE c.LecturerId = m.OldId);

INSERT INTO dbo.Lecturers (Id, UserId, Prefix, FullName, Position, Expertise, EducationLevel, Program, Contact, ImageFileId,
                           SortOrder, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT lu.UserId, lu.UserId, NULL, usr.FullName, ul.Position, ul.Expert, ul.LvEdu, ul.Program, ul.Contact, usr.ImageFileId,
       1000, 0, usr.CreatedAt, NULL, NULL, NULL, 0
FROM #LecturerUsers lu
JOIN dbo.Users usr ON usr.Id = lu.UserId
JOIN #Users m ON m.NewId = lu.UserId
LEFT JOIN [$(SourceDb)].dbo.UserLecturer ul ON ul.Id = m.OldId
WHERE NOT EXISTS (SELECT 1 FROM dbo.Lecturers x WHERE x.Id = lu.UserId);
INSERT INTO #Report VALUES (N'Lecturers (from accounts)', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 6. Projects.  The old table has no academic year, so it is taken from the created date
--    (Thai academic year starts in June: Jan-May belong to the previous year).
-- ---------------------------------------------------------------------------
INSERT INTO dbo.Projects (Id, NameTH, NameEN, Description, AcademicYear, KeyWords, VideoUrl, WebUrl, GithubUrl, ImageFileId, PdfFileId,
                          StudentId, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT pm.NewId, ISNULL(p.NameTH, N''), p.NameEN, p.Description,
       CAST(YEAR(p.CreatedAt) + 543 - CASE WHEN MONTH(p.CreatedAt) < 6 THEN 1 ELSE 0 END AS smallint),
       p.KeyWords, NULLIF(NULLIF(LTRIM(RTRIM(p.VideoUrl)), N''), N'-'), NULLIF(NULLIF(LTRIM(RTRIM(p.WebUrl)), N''), N'-'), NULLIF(NULLIF(LTRIM(RTRIM(p.GithubUrl)), N''), N'-'), imageFile.Id, pdfFile.Id,
       CAST(p.StudentId AS uniqueidentifier), p.IsUsed, p.CreatedAt, NULL, NULL, NULL, 0
FROM #ProjectMap pm
JOIN [$(SourceDb)].dbo.Projects p ON p.Id = pm.OldId
LEFT JOIN #FileMap imageFile ON imageFile.Name = p.Image
LEFT JOIN #FileMap pdfFile ON pdfFile.Name = p.PDF
WHERE NOT EXISTS (SELECT 1 FROM dbo.Projects x WHERE x.Id = pm.NewId);
INSERT INTO #Report VALUES (N'Projects', @@ROWCOUNT);

-- The old links contain blanks, a lone '-' for "none" and stray spaces; the new validation rejects those.
UPDATE p
SET VideoUrl  = NULLIF(NULLIF(LTRIM(RTRIM(p.VideoUrl)), N''), N'-'),
    WebUrl    = NULLIF(NULLIF(LTRIM(RTRIM(p.WebUrl)), N''), N'-'),
    GithubUrl = NULLIF(NULLIF(LTRIM(RTRIM(p.GithubUrl)), N''), N'-')
FROM dbo.Projects p
JOIN #ProjectMap pm ON pm.NewId = p.Id
WHERE ISNULL(p.VideoUrl, N'<null>') <> ISNULL(NULLIF(NULLIF(LTRIM(RTRIM(p.VideoUrl)), N''), N'-'), N'<null>')
   OR ISNULL(p.WebUrl, N'<null>') <> ISNULL(NULLIF(NULLIF(LTRIM(RTRIM(p.WebUrl)), N''), N'-'), N'<null>')
   OR ISNULL(p.GithubUrl, N'<null>') <> ISNULL(NULLIF(NULLIF(LTRIM(RTRIM(p.GithubUrl)), N''), N'-'), N'<null>');
INSERT INTO #Report VALUES (N'Projects (links tidied)', @@ROWCOUNT);

-- Advisors were free text.  Match the name to a lecturer (exact, then "name contains the lecturer's name"
-- because the old text carries a title); names that match nothing become lecturers that are NOT published.
-- The old text never said who the main advisor was, so everyone is stored as main (Role = 1).
CREATE TABLE #ConsultantNames (Name nvarchar(400) NOT NULL PRIMARY KEY, LecturerId uniqueidentifier NULL);

INSERT INTO #ConsultantNames (Name)
SELECT DISTINCT LTRIM(RTRIM(c.LecturerName))
FROM [$(SourceDb)].dbo.Consultants c
JOIN #ProjectMap pm ON pm.OldId = c.ProjectId
WHERE c.LecturerName IS NOT NULL AND LTRIM(RTRIM(c.LecturerName)) <> N'';

UPDATE n
SET LecturerId = (SELECT TOP (1) l.Id FROM dbo.Lecturers l WHERE l.UserId IS NULL AND l.FullName = n.Name ORDER BY l.CreatedAt)
FROM #ConsultantNames n;

UPDATE n
SET LecturerId = (SELECT TOP (1) l.Id FROM dbo.Lecturers l
                  WHERE l.UserId IS NULL AND LEN(l.FullName) > 3 AND n.Name LIKE N'%' + l.FullName + N'%'
                  ORDER BY LEN(l.FullName) DESC, l.CreatedAt)
FROM #ConsultantNames n
WHERE n.LecturerId IS NULL;

INSERT INTO dbo.Lecturers (Id, UserId, Prefix, FullName, Position, Expertise, EducationLevel, Program, Contact, ImageFileId,
                           SortOrder, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT CAST(HASHBYTES('MD5', N'consultant:' + n.Name) AS uniqueidentifier), NULL, NULL, n.Name, NULL, NULL, NULL, NULL, NULL, NULL,
       2000, 0, @now, NULL, NULL, NULL, 0
FROM #ConsultantNames n
WHERE n.LecturerId IS NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Lecturers x WHERE x.Id = CAST(HASHBYTES('MD5', N'consultant:' + n.Name) AS uniqueidentifier));
INSERT INTO #Report VALUES (N'Lecturers (advisor names, not published)', @@ROWCOUNT);

UPDATE #ConsultantNames
SET LecturerId = CAST(HASHBYTES('MD5', N'consultant:' + Name) AS uniqueidentifier)
WHERE LecturerId IS NULL;

INSERT INTO dbo.ProjectAdvisors (Id, ProjectId, LecturerId, Role)
SELECT NEWID(), x.ProjectId, x.LecturerId, 1
FROM (
    SELECT DISTINCT pm.NewId AS ProjectId, n.LecturerId
    FROM [$(SourceDb)].dbo.Consultants c
    JOIN #ProjectMap pm ON pm.OldId = c.ProjectId
    JOIN #ConsultantNames n ON n.Name = LTRIM(RTRIM(c.LecturerName))
) x
WHERE NOT EXISTS (SELECT 1 FROM dbo.ProjectAdvisors a WHERE a.ProjectId = x.ProjectId AND a.LecturerId = x.LecturerId);
INSERT INTO #Report VALUES (N'ProjectAdvisors', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 7. News and their pictures.  A news item whose author was not migrated is given to the first admin.
-- ---------------------------------------------------------------------------
INSERT INTO dbo.News (Id, Title, SubTitle, Body, AuthorId, PublishedAt, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT nm.NewId, ISNULL(n.Title, N''), n.SubTitle, ISNULL(n.Body, N''), ISNULL(um.NewId, @adminId),
       CASE WHEN n.IsHidden = 0 THEN n.CreatedAt END, CASE WHEN n.IsHidden = 0 THEN 1 ELSE 0 END,
       n.CreatedAt, NULL, NULL, NULL, 0
FROM #NewsMap nm
JOIN [$(SourceDb)].dbo.Newses n ON n.Id = nm.OldId
LEFT JOIN #Users um ON um.OldId = n.AuthorId
WHERE NOT EXISTS (SELECT 1 FROM dbo.News x WHERE x.Id = nm.NewId);
INSERT INTO #Report VALUES (N'News', @@ROWCOUNT);

INSERT INTO dbo.NewsFiles (Id, NewsId, FileId, IsMain, SortOrder)
SELECT np.Id, nm.NewId, fm.Id, np.IsMain,
       CAST(ROW_NUMBER() OVER (PARTITION BY np.NewsId ORDER BY np.IsMain DESC, np.CreatedAt, np.Id) - 1 AS int)
FROM [$(SourceDb)].dbo.NewsPhotos np
JOIN #NewsMap nm ON nm.OldId = np.NewsId
JOIN #FileMap fm ON fm.Name = np.Url
WHERE NOT EXISTS (SELECT 1 FROM dbo.NewsFiles x WHERE x.NewsId = nm.NewId);
INSERT INTO #Report VALUES (N'NewsFiles', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 8. Home banners (shown only if both old flags were on), tech subject cards, settings.
-- ---------------------------------------------------------------------------
INSERT INTO dbo.HomeBanners (Id, Title, FileId, LinkUrl, SortOrder, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT h.Id, h.Title, fm.Id, NULL, CAST(ROW_NUMBER() OVER (ORDER BY h.CreatedAt, h.Id) - 1 AS int),
       CASE WHEN h.IsEnable = 1 AND h.IsUsed = 1 THEN 1 ELSE 0 END, h.CreatedAt, NULL, NULL, NULL, 0
FROM [$(SourceDb)].dbo.HomePhotos h
JOIN #FileMap fm ON fm.Name = h.Url
WHERE NOT EXISTS (SELECT 1 FROM dbo.HomeBanners x WHERE x.Id = h.Id);
INSERT INTO #Report VALUES (N'HomeBanners', @@ROWCOUNT);

INSERT INTO dbo.TechSubjects (Id, Icon, SubjectName, SubTitle, Description, SortOrder, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT s.Id, s.Icon, ISNULL(s.SubjectName, N''), s.SubTitle, s.Description,
       CAST(ROW_NUMBER() OVER (ORDER BY s.CreatedAt, s.Id) - 1 AS int), 1, s.CreatedAt, NULL, NULL, NULL, 0
FROM [$(SourceDb)].dbo.ComputerScienceSubjects s
WHERE NOT EXISTS (SELECT 1 FROM dbo.TechSubjects x WHERE x.Id = s.Id);
INSERT INTO #Report VALUES (N'TechSubjects', @@ROWCOUNT);

INSERT INTO dbo.TechSubjectFiles (Id, TechSubjectId, FileId, SortOrder)
SELECT i.Id, i.ComputerScienceSubjectId, fm.Id,
       CAST(ROW_NUMBER() OVER (PARTITION BY i.ComputerScienceSubjectId ORDER BY i.Id) - 1 AS int)
FROM [$(SourceDb)].dbo.ComputerScienceImages i
JOIN #FileMap fm ON fm.Name = i.URL
WHERE i.ComputerScienceSubjectId IS NOT NULL
  AND EXISTS (SELECT 1 FROM dbo.TechSubjects t WHERE t.Id = i.ComputerScienceSubjectId)
  AND NOT EXISTS (SELECT 1 FROM dbo.TechSubjectFiles x WHERE x.TechSubjectId = i.ComputerScienceSubjectId);
INSERT INTO #Report VALUES (N'TechSubjectFiles', @@ROWCOUNT);

-- The single settings row already exists (created by the API).  It is only overwritten while it still
-- has the placeholder name, so running this again never undoes edits made in the new system.
UPDATE t
SET WebName = ISNULL(s.WebName, t.WebName), RegisterUrl = s.RegisterUrl, KruUrl = s.KruUrl, PageFacebook = s.PageFacebook,
    LogoFileId = fm.Id, VideoUrl = s.VideoUrl, MapLocation = s.Location
FROM dbo.SystemSettings t
CROSS JOIN (SELECT TOP (1) * FROM [$(SourceDb)].dbo.SystemSettings) s
LEFT JOIN #FileMap fm ON fm.Name = s.Logo
WHERE t.WebName = @seededWebName;
INSERT INTO #Report VALUES (N'SystemSettings (updated)', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 9. Job histories (the old id of a job is the id of its owner).
-- ---------------------------------------------------------------------------
INSERT INTO dbo.JobHistories (Id, UserId, JobName, Company, Position, StartDate, Description, IsPublished,
                              CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT CAST(HASHBYTES('MD5', N'job:' + j.Id) AS uniqueidentifier), m.NewId, ISNULL(j.JobName, N''), j.Company, j.Position,
       CAST(j.Date AS date), j.Description, j.IsUsed, j.CreatedAt, NULL, NULL, NULL, 0
FROM [$(SourceDb)].dbo.JobHistories j
JOIN #Users m ON m.OldId = j.Id
WHERE NOT EXISTS (SELECT 1 FROM dbo.JobHistories x WHERE x.Id = CAST(HASHBYTES('MD5', N'job:' + j.Id) AS uniqueidentifier));
INSERT INTO #Report VALUES (N'JobHistories', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 10. Training courses.  The old registrations carried no status: all become Confirmed (2).
-- ---------------------------------------------------------------------------
INSERT INTO dbo.Courses (Id, Title, Description, LecturerId, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT cm.NewId, ISNULL(c.Title, N''), c.Description,
       (SELECT TOP (1) l.Id FROM dbo.Lecturers l WHERE l.Id = TRY_CAST(c.LecturerId AS uniqueidentifier)),
       c.IsUsed, c.CreatedAt, NULL, NULL, NULL, 0
FROM #CourseMap cm
JOIN [$(SourceDb)].dbo.Courses c ON c.Id = cm.OldId
WHERE NOT EXISTS (SELECT 1 FROM dbo.Courses x WHERE x.Id = cm.NewId);
INSERT INTO #Report VALUES (N'Courses', @@ROWCOUNT);

INSERT INTO dbo.CoursePhotos (Id, CourseId, FileId, IsMain)
SELECT cp.Id, cm.NewId, fm.Id, cp.IsMain
FROM [$(SourceDb)].dbo.CoursePhoto cp
JOIN #CourseMap cm ON cm.OldId = cp.CourseId
JOIN #FileMap fm ON fm.Name = cp.Url
WHERE NOT EXISTS (SELECT 1 FROM dbo.CoursePhotos x WHERE x.CourseId = cm.NewId);
INSERT INTO #Report VALUES (N'CoursePhotos', @@ROWCOUNT);

INSERT INTO dbo.Generations (Id, CourseId, SubTitle, Description, StartDate, EndDate, Quantity, Fee, PhotoFileId, IsCancelled,
                             CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT gm.NewId, cm.NewId, g.SubTitle, g.Description, g.StartDate, g.EndDate, g.Quantity, NULL, fm.Id, g.IsCancelled,
       g.CreatedAt, NULL, NULL, NULL, 0
FROM #GenMap gm
JOIN [$(SourceDb)].dbo.Generations g ON g.Id = gm.OldId
JOIN #CourseMap cm ON cm.OldId = g.CourseId
LEFT JOIN #FileMap fm ON fm.Name = g.GenPhoto
WHERE NOT EXISTS (SELECT 1 FROM dbo.Generations x WHERE x.Id = gm.NewId);
INSERT INTO #Report VALUES (N'Generations', @@ROWCOUNT);

INSERT INTO dbo.CourseRegistrations (UserId, GenerationId, Status, RegisteredAt, CancelledAt)
SELECT m.NewId, gm.NewId, 2, a.CreatedAt, NULL
FROM [$(SourceDb)].dbo.CourseAttendees a
JOIN #Users m ON m.OldId = a.AppUserId
JOIN #GenMap gm ON gm.OldId = a.GenerationId
WHERE EXISTS (SELECT 1 FROM dbo.Generations g WHERE g.Id = gm.NewId)
  AND NOT EXISTS (SELECT 1 FROM dbo.CourseRegistrations x WHERE x.UserId = m.NewId AND x.GenerationId = gm.NewId);
INSERT INTO #Report VALUES (N'CourseRegistrations', @@ROWCOUNT);

INSERT INTO dbo.CourseComments (Id, Body, AuthorId, GenerationId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT c.Id, ISNULL(c.Body, N''), m.NewId, gm.NewId, c.CreatedAt, NULL, NULL, NULL, 0
FROM [$(SourceDb)].dbo.CourseComments c
JOIN #Users m ON m.OldId = c.AuthorId
JOIN #GenMap gm ON gm.OldId = c.GenerationId
WHERE EXISTS (SELECT 1 FROM dbo.Generations g WHERE g.Id = gm.NewId)
  AND NOT EXISTS (SELECT 1 FROM dbo.CourseComments x WHERE x.Id = c.Id);
INSERT INTO #Report VALUES (N'CourseComments', @@ROWCOUNT);

-- ---------------------------------------------------------------------------
-- 11. Syllabuses.  The old year is a Gregorian year (2026); the new system uses the Thai year (+543).
--     Shown = IsUsed and not Hidden.  Current = the newest shown one.  Total credits = sum of the
--     groups (left empty if it is over 400, which would not pass the new validation).
--     Objectives / occupations had no order column, so they keep an arbitrary but stable order.
-- ---------------------------------------------------------------------------
DECLARE @currentYear int = (SELECT MAX(CAST(Year AS int)) FROM [$(SourceDb)].dbo.Syllabuses WHERE IsUsed = 1 AND Hidden = 0);

INSERT INTO dbo.Syllabuses (Id, NameTH, NameEN, Year, DegreeTH, DegreeEN, TotalCredits, PdfFileId, IsCurrent, IsPublished,
                            CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
SELECT sm.NewId, ISNULL(s.NameTH, N''), s.NameEN, CAST(CAST(s.Year AS int) + 543 AS smallint), s.DegreeTH, s.DegreeEN,
       CASE WHEN t.Credits BETWEEN 0 AND 400 THEN CAST(ROUND(t.Credits, 0) AS smallint) END,
       fm.Id,
       CASE WHEN s.IsUsed = 1 AND s.Hidden = 0 AND CAST(s.Year AS int) = @currentYear THEN 1 ELSE 0 END,
       CASE WHEN s.IsUsed = 1 AND s.Hidden = 0 THEN 1 ELSE 0 END,
       s.CreatedAt, NULL, NULL, NULL, 0
FROM #SyllabusMap sm
JOIN [$(SourceDb)].dbo.Syllabuses s ON s.Id = sm.OldId
LEFT JOIN #FileMap fm ON fm.Name = s.PDF
OUTER APPLY (SELECT SUM(CAST(sub.Credit AS decimal(9, 1))) AS Credits FROM [$(SourceDb)].dbo.Subjects sub WHERE sub.SyllabusId = s.Id) t
WHERE NOT EXISTS (SELECT 1 FROM dbo.Syllabuses x WHERE x.Id = sm.NewId);
INSERT INTO #Report VALUES (N'Syllabuses', @@ROWCOUNT);

INSERT INTO dbo.Subjects (Id, SyllabusId, Code, Name, Credit, SubjectCategory)
SELECT sub.Id, sm.NewId, NULL, ISNULL(sub.Name, N''), CAST(sub.Credit AS decimal(6, 1)), CAST(sub.SubjectCategory AS tinyint)
FROM [$(SourceDb)].dbo.Subjects sub
JOIN #SyllabusMap sm ON sm.OldId = sub.SyllabusId
WHERE NOT EXISTS (SELECT 1 FROM dbo.Subjects x WHERE x.SyllabusId = sm.NewId);
INSERT INTO #Report VALUES (N'Subjects', @@ROWCOUNT);

INSERT INTO dbo.SyllabusObjectives (Id, SyllabusId, Sentence, SortOrder)
SELECT o.Id, sm.NewId, ISNULL(o.Sentence, N''), CAST(ROW_NUMBER() OVER (PARTITION BY o.SyllabusId ORDER BY o.Id) - 1 AS int)
FROM [$(SourceDb)].dbo.Objectives o
JOIN #SyllabusMap sm ON sm.OldId = o.SyllabusId
WHERE NOT EXISTS (SELECT 1 FROM dbo.SyllabusObjectives x WHERE x.SyllabusId = sm.NewId);
INSERT INTO #Report VALUES (N'SyllabusObjectives', @@ROWCOUNT);

INSERT INTO dbo.SyllabusOccupations (Id, SyllabusId, Sentence, SortOrder)
SELECT o.Id, sm.NewId, ISNULL(o.Sentence, N''), CAST(ROW_NUMBER() OVER (PARTITION BY o.SyllabusId ORDER BY o.Id) - 1 AS int)
FROM [$(SourceDb)].dbo.Occupations o
JOIN #SyllabusMap sm ON sm.OldId = o.SyllabusId
WHERE NOT EXISTS (SELECT 1 FROM dbo.SyllabusOccupations x WHERE x.SyllabusId = sm.NewId);
INSERT INTO #Report VALUES (N'SyllabusOccupations', @@ROWCOUNT);

COMMIT TRANSACTION;

-- ---------------------------------------------------------------------------
-- Report: rows added by this run, and what was left behind on purpose
-- ---------------------------------------------------------------------------
SELECT Item, RowsAdded FROM #Report;

SELECT N'Old users not migrated (IsUsed = 0: test or switched off)' AS Skipped, COUNT(*) AS N
FROM [$(SourceDb)].dbo.AspNetUsers WHERE IsUsed = 0
UNION ALL SELECT N'Old job histories not migrated (owner skipped)', COUNT(*)
FROM [$(SourceDb)].dbo.JobHistories j WHERE NOT EXISTS (SELECT 1 FROM #Users m WHERE m.OldId = j.Id)
UNION ALL SELECT N'Old registrations not migrated (user skipped)', COUNT(*)
FROM [$(SourceDb)].dbo.CourseAttendees a WHERE NOT EXISTS (SELECT 1 FROM #Users m WHERE m.OldId = a.AppUserId)
UNION ALL SELECT N'Old comments not migrated (author or generation missing)', COUNT(*)
FROM [$(SourceDb)].dbo.CourseComments c
WHERE NOT EXISTS (SELECT 1 FROM #Users m WHERE m.OldId = c.AuthorId) OR NOT EXISTS (SELECT 1 FROM #GenMap g WHERE g.OldId = c.GenerationId)
UNION ALL SELECT N'Old advisor rows not migrated (project missing)', COUNT(*)
FROM [$(SourceDb)].dbo.Consultants c WHERE NOT EXISTS (SELECT 1 FROM #ProjectMap pm WHERE pm.OldId = c.ProjectId);
GO
