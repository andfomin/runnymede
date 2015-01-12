


CREATE FUNCTION [dbo].[sesGetEarliestPossibleStartTime]
(	
)
RETURNS smalldatetime
AS
BEGIN
/*
The time which is at least 15 minutes ahead from now, rounded up to the next nearest quarter hour (i.e. 0, 15, 30, 45 minute)
Corresponds to App.Conversations_Utils.minStart()
*/
	declare @NowPlus30Min smalldatetime = dateadd(minute, 30, sysutcdatetime());
	declare @QuarterHour smalldatetime = dateadd(minute, datediff(minute, 0, @NowPlus30Min) / 15 * 15, 0);
	return @QuarterHour;

END