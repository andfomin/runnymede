



CREATE FUNCTION [dbo].[sesGetItalkiTeachers]
(
)
RETURNS TABLE 	
AS
RETURN 

	select Id as UserId, DisplayName,
		T.C.value('ItalkiUserId[1]', 'int') as ItalkiUserId,
		T.C.value('CourseId[1]', 'int') as CourseId,
		T.C.value('ScheduleUrl[1]', 'nvarchar(200)') as ScheduleUrl,
		q.Rate
	from dbo.appUsers U
		cross apply U.Details.nodes('ItalkiTeacher') T(C),
		(select dbo.appGetServicePrice('SVSSSN') as Rate) q
	where IsTeacher = 1;
GO
GRANT SELECT
    ON OBJECT::[dbo].[sesGetItalkiTeachers] TO [websiterole]
    AS [dbo];

