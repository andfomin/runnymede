


CREATE FUNCTION [dbo].[appGetConstant](@Name nvarchar(100)) 
RETURNS nvarchar(max)
AS
BEGIN
/*
20121117 AF
*/

declare @Value nvarchar(max);

select @Value = Value from dbo.appConstants where Name = @Name;

return @Value;

END