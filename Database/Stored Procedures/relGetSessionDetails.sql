

CREATE PROCEDURE [dbo].[relGetSessionDetails]
	@EventId int,
	@UserId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname = object_name(@@procid);

declare @Attribute nvarchar(100); 

select @Attribute = cast(Id as nvarchar(100)) 
from dbo.relScheduleEvents
where Id = @EventId
	and (UserId = @UserId or SecondUserId = @UserId);

if @Attribute is null
	raiserror('%s,%d,%d:: The user is not related to the session.', 16, 1, @ProcName, @UserId, @EventId);

select S.Id, S.Start, S.[End], S.[Type], S.UserId, S.SecondUserId, S.Price, 
	S.CreationTime, S.ConfirmationTime, S.CancellationTime, S.ClosingTime,
	U.DisplayName as UserDisplayName, case when S.[Type] = 'CFSN' then U.Skype else null end as UserSkype,
	SU.DisplayName as SecondUserDisplayName, case when S.[Type] = 'CFSN' then SU.Skype else null end as SecondUserSkype
from dbo.relScheduleEvents S
	left join dbo.appUsers U on U.Id = case when @UserId = S.SecondUserId then S.UserId else null end
	left join dbo.appUsers SU on SU.Id = case when @UserId = S.UserId then S.SecondUserId else null end
where S.Id = @EventId;

select Id, [Type], PostTime, SenderUserId, RecipientUserId, SenderDisplayName, RecepientDisplayName, [Text]
from dbo.relMessages
where Attribute = @Attribute
	and [Type] = 'SSSN'
order by PostTime desc;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[relGetSessionDetails] TO [websiterole]
    AS [dbo];

