




CREATE FUNCTION [dbo].[appGetConstantAsFloat](@Name nvarchar(100)) 
RETURNS float
AS
BEGIN

declare @Value float;

select @Value = cast(Value as float) 
from dbo.[appConstants] 
where Name = @Name;

return @Value;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[appGetConstantAsFloat] TO [websiterole]
    AS [dbo];

