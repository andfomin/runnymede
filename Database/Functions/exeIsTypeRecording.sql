﻿

CREATE FUNCTION [dbo].[exeIsTypeRecording]
(
	@Type char(6)
)
RETURNS bit
AS
BEGIN

return iif(@Type = 'ARMP3_', 1, 0);

END