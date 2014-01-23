



CREATE FUNCTION [dbo].[appGetConstantAsInt](@Name nvarchar(100)) 
RETURNS int
AS
BEGIN
/*
20121117 AF
*/

declare @Value int;

select @Value = cast(Value as int) from dbo.[appConstants] where Name = @Name;

return @Value;

END