




CREATE FUNCTION [dbo].appExtIdToUrl
(
	@BaseUrl nvarchar(2000),
	@ExtId uniqueidentifier
)
RETURNS nvarchar(2000)
AS
BEGIN

return @BaseUrl + upper(convert(nchar(36), @ExtId));

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[appExtIdToUrl] TO [websiterole]
    AS [dbo];

