




CREATE FUNCTION [dbo].[lckGetDigits]
(
)
RETURNS TABLE 	
AS
RETURN 

	select top(7) R1.[Date],
		stuff((
				select N', ' + right(R2.Rate, 1)
				from dbo.lckRates R2
				where R2.[Date] = R1.[Date] 
					and R2.Active = 1
				order by R2.Position
				for xml path('')
			), 1, 2, '') as Digits
	from dbo.lckRates R1
	where R1.Active = 1
	group by R1.[Date]
	order by R1.[Date] desc
	;
GO
GRANT SELECT
    ON OBJECT::[dbo].[lckGetDigits] TO [websiterole]
    AS [dbo];

