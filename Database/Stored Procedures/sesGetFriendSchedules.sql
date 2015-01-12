
CREATE PROCEDURE [dbo].[sesGetFriendSchedules]
	@Start smalldatetime,
	@End smalldatetime,
	@UserId int
AS
BEGIN

set nocount on;

declare @t table (
	Id int primary key,
	UserId int,
	Start smalldatetime,
	[End] smalldatetime,
	[Type] char(6),
	SessionRate decimal(9,2)
);

insert @t (Id, UserId, Start, [End], [Type], SessionRate)
	select S.Id, S.UserId, S.Start, S.[End], S.[Type], F1.SessionRate
	from dbo.sesScheduleEvents S
		inner join dbo.friFriends F1 on S.UserId = F1.UserId and F1.FriendUserId = @UserId
		inner join dbo.friFriends F2 on F1.UserId = F2.FriendUserId and F1.FriendUserId = F2.UserId
	where S.Start < @End 
		and S.[End] > @Start
		--and [End] > sysutcdatetime()
		and F1.IsActive = 1
		and F2.IsActive = 1;		

set nocount off;

-- The order of the recordsets does matter.

select U.Id, U.DisplayName, coalesce(T.SessionRate, U.SessionRate, 0) as SessionRate
from (
	select distinct UserId, SessionRate from @t 
) T
	inner join dbo.appUsers U on T.UserId = U.Id

select Id, UserId, Start, [End], [Type]
from @t;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetFriendSchedules] TO [websiterole]
    AS [dbo];

