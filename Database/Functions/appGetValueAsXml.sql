


create FUNCTION [dbo].[appGetValueAsXml](@Type char(6)) 
RETURNS xml
AS
BEGIN
/*
20150417 AF
*/

declare @Value xml;

select @Value = cast(Value as xml)
from dbo.appValues 
where [Type] = @Type
	and Start < sysutcdatetime()
	and [End] is null;

return @Value;

END