

CREATE FUNCTION [dbo].[exeIsTypeWriting]
(
	@Type char(6)
)
RETURNS bit
AS
BEGIN

return iif(@Type = 'ARJPEG', 1, 0);

END