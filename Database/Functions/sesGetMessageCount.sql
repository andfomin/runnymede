﻿


CREATE FUNCTION [dbo].[sesGetMessageCount]
(
	@UserId int,
	@SessionId int
)
RETURNS int
AS
BEGIN

declare @MessageCount int;

declare @Attribute nvarchar(100) = cast(@SessionId as nvarchar(100)); 

select @MessageCount = count(*)
from dbo.appMessages 
where Attribute = @Attribute
	and substring([Type], 1, 4) = 'MSSS'
	and (SenderUserId = @UserId or RecipientUserId = @UserId);

return @MessageCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetMessageCount] TO [websiterole]
    AS [dbo];

