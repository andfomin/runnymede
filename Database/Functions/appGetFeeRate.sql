





CREATE FUNCTION [dbo].[appGetFeeRate]
(
	@Type char(6),
	@Date smalldatetime,
	@PriceRate decimal(9,2)
) 
RETURNS float
AS
BEGIN

declare @Value float;

select @Value = min(C.value('(.)[1]', 'float'))
from dbo.appFeeRates 
	cross apply FeeRates.nodes('/FeeRates/FeeRate') T(C)
where [Type] = @Type
	and @Date > Start 
	and (@Date < [End] or [End] is null)
	and @PriceRate between C.value('@priceRateFrom[1]', 'decimal(9,2)') and C.value('@priceRateTo[1]', 'decimal(9,2)')

return coalesce(@Value, 0.0);

END