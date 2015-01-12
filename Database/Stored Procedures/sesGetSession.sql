


CREATE PROCEDURE [dbo].[sesGetSession]
	@EventId int,
	@UserId int
AS
BEGIN
SET NOCOUNT ON;

declare	@Id int, @Attribute nvarchar(100);

select @Id = S.Id, @Attribute = SE.Attribute
from dbo.sesSessions S
	inner join dbo.sesScheduleEvents SE on cast(S.Id as nvarchar(100)) = SE.Attribute
where SE.Id = @EventId
	and (dbo.sesIsSession(SE.[Type]) = 1)
	and SE.UserId = @UserId 
	and (S.HostUserId = @UserId or S.GuestUserId = @UserId);

if @Attribute is null begin
	declare @ProcName sysname = object_name(@@procid);
	raiserror('%s,%d,%d:: The user is not related to the event.', 16, 1, @ProcName, @UserId, @EventId);
end

-- The order of the recordsets does matter.

select M.Id, M.[Type], M.PostTime, M.ReceiveTime, M.ExtId,
	 M.SenderUserId, M.SenderDisplayName, 
	 M.RecipientUserId, M.RecipientDisplayName
from dbo.appMessages M
	inner join dbo.appTypes T on M.[Type] = T.Id
where M.Attribute = @Attribute
	and T.AttributeType = 'ATSSSN'
	and (M.SenderUserId = @UserId or M.RecipientUserId = @UserId)
order by M.PostTime desc;

select S.Id, S.HostUserId, S.GuestUserId, S.Start, S.[End], S.Price, S.CancellationUserId,
	S.RequestTime, S.ConfirmationTime, S.CancellationTime, S.DisputeTimeByHost, S.DisputeTimeByGuest, S.FinishTime,
	UH.DisplayName as HostDisplayName, iif(S.ConfirmationTime is not null and S.CancellationTime is null, UH.SkypeName, null) as HostSkypeName,
	UG.DisplayName as GuestDisplayName, iif(S.ConfirmationTime is not null and S.CancellationTime is null, UG.SkypeName, null) as GuestSkypeName
from dbo.sesSessions S
	inner join dbo.appUsers UH on S.HostUserId = UH.Id
	inner join dbo.appUsers UG on S.GuestUserId = UG.Id
where S.Id = @Id;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetSession] TO [websiterole]
    AS [dbo];

