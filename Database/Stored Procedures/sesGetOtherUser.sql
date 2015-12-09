



CREATE PROCEDURE [dbo].[sesGetOtherUser]
	@UserId int,
	@SessionId int
AS
BEGIN
SET NOCOUNT ON;

declare @Start smalldatetime, @End smalldatetime, @OtherUserId int;

select @Start = Start, @End = [End],
	@OtherUserId = case 
		when TeacherUserId = @UserId then LearnerUserId
		when LearnerUserId = @UserId then TeacherUserId
		else null
	end
from dbo.sesSessions
where Id = @SessionId;

--if @Start is null begin
--	declare @ProcName sysname = object_name(@@procid);
--	raiserror('%s,%d,%d:: The user is not related to the session.', 16, 1, @ProcName, @UserId, @SessionId);
--end

select Id, DisplayName, iif(sysutcdatetime() between @Start and @End, SkypeName, null) as SkypeName
from dbo.appUsers
where Id = @OtherUserId;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetOtherUser] TO [websiterole]
    AS [dbo];

