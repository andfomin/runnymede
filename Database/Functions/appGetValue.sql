



CREATE FUNCTION [dbo].[appGetValue]
(
	@Type char(6)
) 
RETURNS xml
AS
BEGIN
/*
20150417 AF
*/

declare @Value nvarchar(max);

declare @Now smalldatetime = sysutcdatetime();

select @Value = Value
from dbo.appValues
where [Type] = @Type
	and Start <= @Now
	and ([End] > @Now or [End] is null)

return @Value;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[appGetValue] TO [websiterole]
    AS [dbo];

