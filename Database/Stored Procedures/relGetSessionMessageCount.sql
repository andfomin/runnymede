
CREATE PROCEDURE [dbo].[relGetSessionMessageCount]
	@EventId int,
	@UserId int
AS
BEGIN
SET NOCOUNT ON;

declare @Attribute nvarchar(100); 

select @Attribute = cast(Id as nvarchar(100))
from dbo.relScheduleEvents
where Id = @EventId
	and (UserId = @UserId or SecondUserId = @UserId);

if @Attribute is null begin
	declare @ProcName sysname = object_name(@@procid);
	raiserror('%s,%d,%d:: The user is not related to the session.', 16, 1, @ProcName, @UserId, @EventId);
end

select count(*) as MessageCount
from dbo.relMessages
where Attribute = @Attribute
	and [Type] = 'SSSN';

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[relGetSessionMessageCount] TO [websiterole]
    AS [dbo];

