


CREATE FUNCTION [dbo].[appGetPricelistItems]
(
)
RETURNS TABLE 	
AS
RETURN 

	--select q.Title, q.Value.value('Price[1]', 'decimal(9,2)') as Price, q.Position
	--from (
	--	select T.Title, T1.C1.value('.', 'nvarchar(10)') as Position, cast(V.Value as xml) as Value
	--	from dbo.appTypes T
	--		cross apply T.Details.nodes('PriceListPosition[1]') T1(C1)
	--		inner join dbo.appValues V on T.Id = V.[Type],
	--		( 
	--			select sysutcdatetime() as [Now]
	--		) n
	--		where substring(T.Id, 1, 2) = 'SV'
	--			and V.Start <= n.[Now] -- otherwise the function would be called for every row
	--			and (V.[End] > n.[Now] or V.[End] is null)
	--) q

	select T.Title, dbo.appGetServicePrice(T.Id) as Price, T1.C1.value('.', 'nvarchar(10)') as Position
	from dbo.appTypes T
		cross apply T.Details.nodes('PriceListPosition[1]') T1(C1)
	where substring(T.Id, 1, 2) = 'SV'
	;
GO
GRANT SELECT
    ON OBJECT::[dbo].[appGetPricelistItems] TO [websiterole]
    AS [dbo];

