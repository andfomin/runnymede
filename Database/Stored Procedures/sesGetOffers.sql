
CREATE PROCEDURE [dbo].[sesGetOffers]
	@Start smalldatetime,
	@End smalldatetime,
	@UserId int = null
AS
BEGIN

set nocount on;

declare @t table (
	Id int primary key,
	UserId int,
	Start smalldatetime,
	[End] smalldatetime,
	[Type] char(6)
);

insert @t (Id, UserId, Start, [End], [Type])
	select Id, UserId, Start, [End], [Type]
	from dbo.sesScheduleEvents
	where Start < @End 
		and [End] > @Start
		--and [End] > sysutcdatetime()
		and [Type] = 'SES_VT';

set nocount off;

-- The order of the recordsets does matter.

select U.Id, U.DisplayName, U.Announcement, 
	coalesce(F.SessionRate, U.SessionRate, 0) as SessionRate
	-- F.IsActive as IsActiveToViewer, FU.IsActive as IsActiveFromViewer
from (
	select distinct UserId from @t 
) T
	inner join dbo.appUsers U on T.UserId = U.Id
	left join dbo.friFriends F on F.UserId = T.UserId and F.FriendUserId = @UserId
	left join dbo.friFriends FU on FU.UserId = @UserId and FU.FriendUserId = T.UserId
where coalesce(F.IsActive, 1) = 1
	and coalesce(FU.IsActive, 1) = 1;

select Id, UserId, Start, [End], [Type]
from @t;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetOffers] TO [websiterole]
    AS [dbo];

