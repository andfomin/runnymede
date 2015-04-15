


create FUNCTION [dbo].[sesGetSessionPrice]
(
	@Start smalldatetime,
	@End smalldatetime
)
RETURNS TABLE 	
AS
RETURN 

	select cast(Rate * datediff(minute, @Start, @End) / 60 as decimal(9,2)) as Price
	from dbo.sesRates 
	where [Weekday] = datepart(weekday, @Start)
		and [Hour] = datepart(hour, @Start)
	;