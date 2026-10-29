-- Turns the HTML that the old site stored (rich-text editor output) into plain text.
-- The new forms edit these fields as plain text, and the public pages show them as text, so tags would appear literally.
--
-- Run after MigrateFromWebcomcs.sql (safe to run again: only values that still contain tags or entities are touched):
--   sqlcmd -S .\SQLEXPRESS -E -C -I -f 65001 -d ComSciDb -i CleanHtmlFields.sql
--
-- Block tags (p, div, li, h1-h6, tr, br) become line breaks, other tags are removed, entities are decoded.
-- Images and links inside the text are dropped. The original HTML stays in the old Webcomcs database.

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.StripHtmlTemp') IS NOT NULL DROP FUNCTION dbo.StripHtmlTemp;
GO

CREATE FUNCTION dbo.StripHtmlTemp(@html nvarchar(max))
RETURNS nvarchar(max)
AS
BEGIN
    IF @html IS NULL RETURN NULL;

    DECLARE @lineBreak nvarchar(1) = NCHAR(10);
    DECLARE @text nvarchar(max) = REPLACE(REPLACE(@html, NCHAR(13), N''), NCHAR(10), N' ');

    SET @text = REPLACE(@text, N'<br>', @lineBreak);
    SET @text = REPLACE(@text, N'<br/>', @lineBreak);
    SET @text = REPLACE(@text, N'<br />', @lineBreak);
    SET @text = REPLACE(@text, N'</p>', @lineBreak);
    SET @text = REPLACE(@text, N'</div>', @lineBreak);
    SET @text = REPLACE(@text, N'</li>', @lineBreak);
    SET @text = REPLACE(@text, N'</ul>', @lineBreak);
    SET @text = REPLACE(@text, N'</ol>', @lineBreak);
    SET @text = REPLACE(@text, N'</tr>', @lineBreak);
    SET @text = REPLACE(@text, N'</h1>', @lineBreak);
    SET @text = REPLACE(@text, N'</h2>', @lineBreak);
    SET @text = REPLACE(@text, N'</h3>', @lineBreak);
    SET @text = REPLACE(@text, N'</h4>', @lineBreak);
    SET @text = REPLACE(@text, N'</h5>', @lineBreak);
    SET @text = REPLACE(@text, N'</h6>', @lineBreak);

    DECLARE @start int = PATINDEX(N'%<[a-zA-Z/!]%>%', @text);
    DECLARE @end int;
    WHILE @start > 0
    BEGIN
        SET @end = CHARINDEX(N'>', @text, @start);
        IF @end = 0 BREAK;
        SET @text = STUFF(@text, @start, @end - @start + 1, N'');
        SET @start = PATINDEX(N'%<[a-zA-Z/!]%>%', @text);
    END;

    SET @text = REPLACE(@text, NCHAR(160), N' ');
    SET @text = REPLACE(@text, N'&nbsp;', N' ');
    SET @text = REPLACE(@text, N'&lt;', N'<');
    SET @text = REPLACE(@text, N'&gt;', N'>');
    SET @text = REPLACE(@text, N'&quot;', N'"');
    SET @text = REPLACE(@text, N'&#39;', N'''');
    SET @text = REPLACE(@text, N'&apos;', N'''');
    SET @text = REPLACE(@text, N'&amp;', N'&');

    WHILE CHARINDEX(N'  ', @text) > 0 SET @text = REPLACE(@text, N'  ', N' ');
    SET @text = REPLACE(@text, N' ' + @lineBreak, @lineBreak);
    SET @text = REPLACE(@text, @lineBreak + N' ', @lineBreak);
    WHILE CHARINDEX(@lineBreak + @lineBreak + @lineBreak, @text) > 0
        SET @text = REPLACE(@text, @lineBreak + @lineBreak + @lineBreak, @lineBreak + @lineBreak);

    SET @text = LTRIM(RTRIM(@text));
    WHILE LEFT(@text, 1) = @lineBreak SET @text = SUBSTRING(@text, 2, LEN(@text));
    WHILE RIGHT(@text, 1) = @lineBreak SET @text = LEFT(@text, LEN(@text) - 1);

    IF @text = N'' RETURN NULL;
    RETURN @text;
END;
GO

BEGIN TRANSACTION;

UPDATE Lecturers SET Expertise = dbo.StripHtmlTemp(Expertise)
WHERE Expertise LIKE N'%<[a-zA-Z/!]%>%' OR Expertise LIKE N'%&[a-z#]%;%';
PRINT 'Lecturers.Expertise: ' + CAST(@@ROWCOUNT AS varchar(10));

UPDATE Projects SET Description = dbo.StripHtmlTemp(Description)
WHERE Description LIKE N'%<[a-zA-Z/!]%>%' OR Description LIKE N'%&[a-z#]%;%';
PRINT 'Projects.Description: ' + CAST(@@ROWCOUNT AS varchar(10));

UPDATE News SET Body = ISNULL(dbo.StripHtmlTemp(Body), N'')
WHERE Body LIKE N'%<[a-zA-Z/!]%>%' OR Body LIKE N'%&[a-z#]%;%';
PRINT 'News.Body: ' + CAST(@@ROWCOUNT AS varchar(10));

UPDATE Generations SET Description = dbo.StripHtmlTemp(Description)
WHERE Description LIKE N'%<[a-zA-Z/!]%>%' OR Description LIKE N'%&[a-z#]%;%';
PRINT 'Generations.Description: ' + CAST(@@ROWCOUNT AS varchar(10));

UPDATE TechSubjects SET Description = dbo.StripHtmlTemp(Description)
WHERE Description LIKE N'%<[a-zA-Z/!]%>%' OR Description LIKE N'%&[a-z#]%;%';
PRINT 'TechSubjects.Description: ' + CAST(@@ROWCOUNT AS varchar(10));

UPDATE Courses SET Description = dbo.StripHtmlTemp(Description)
WHERE Description LIKE N'%<[a-zA-Z/!]%>%' OR Description LIKE N'%&[a-z#]%;%';
PRINT 'Courses.Description: ' + CAST(@@ROWCOUNT AS varchar(10));

COMMIT TRANSACTION;
GO

DROP FUNCTION dbo.StripHtmlTemp;
GO
