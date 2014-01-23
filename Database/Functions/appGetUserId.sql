﻿



CREATE FUNCTION [dbo].[appGetUserId]
(
	@UserName nvarchar(200) = null
)
RETURNS int
AS
BEGIN

declare @UserId int;

select @UserId = Id 
from dbo.aspnetUsers
where UserName = @UserName

return @UserId;

END