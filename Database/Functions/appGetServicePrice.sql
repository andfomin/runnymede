




CREATE FUNCTION [dbo].[appGetServicePrice]
(
	@ServiceType char(6)
) 
RETURNS decimal(9,2)
AS
BEGIN

	return dbo.appGetValue(@ServiceType).value('Price[1]', 'decimal(9,2)')

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[appGetServicePrice] TO [websiterole]
    AS [dbo];

