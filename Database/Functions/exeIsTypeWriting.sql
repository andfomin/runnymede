

CREATE FUNCTION [dbo].[exeIsTypeWriting]
(
	@Type char(6)
)
RETURNS bit
AS
BEGIN

return iif(@Type = 'EXWRPH', 1, 0);

END