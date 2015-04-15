


create PROCEDURE [dbo].[sesGetMessages]
	@UserId int,
	@SessionId int
AS
BEGIN
SET NOCOUNT ON;

declare @Attribute nvarchar(100) = cast(@SessionId as nvarchar(100)); 

select M.Id, M.[Type], M.PostTime, M.ReceiveTime, M.ExtId,
	 M.SenderUserId, M.SenderDisplayName, 
	 M.RecipientUserId, M.RecipientDisplayName
from dbo.appMessages M
where M.Attribute = @Attribute
	and substring([Type], 1, 4) = 'MSSS'
	and (M.SenderUserId = @UserId or M.RecipientUserId = @UserId)
order by M.PostTime desc;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetMessages] TO [websiterole]
    AS [dbo];

