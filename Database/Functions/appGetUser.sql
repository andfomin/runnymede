


CREATE FUNCTION [dbo].[appGetUser]
(
	@Id int
)
RETURNS TABLE 	
AS
RETURN 

	select Id, DisplayName, IsTeacher, SkypeName, RecordingRate, WritingRate, SessionRate, Announcement
	from dbo.appUsers
	where Id = @Id
	;
GO
GRANT SELECT
    ON OBJECT::[dbo].[appGetUser] TO [websiterole]
    AS [dbo];

