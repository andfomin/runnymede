

CREATE FUNCTION [dbo].[GetTenDigitBase36Number]
(	
)
RETURNS nchar(10)
AS
BEGIN
/*
Generates ten Base36 characters. The first seven ones are sequential over time, resolution is 1/36 of second. The three last ones are random.

We declare result type as nchar(), not char() because the values are used outside the database.
The client code always sends parameters as nchar. 
Type mismatch causes implicit convertion of column values and adds overhead within queries, so we better avoid it.
*/

	declare @Characters char(36) = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
	declare @Result varchar(10) = ''; 

	declare @Now datetime2 = sysutcdatetime();

	-- The six first chars are seconds since the origin.
	declare @Origin datetime2 = datetime2fromparts(2013, 12, 01, 16, 20, 0, 0, 0); -- Arbitrary origin. The same as in the C# code.
	declare @Seconds int = datediff(second, @Origin, @Now);

	while @Seconds > 0  
		select 
			@Result = substring(@Characters, @Seconds % 36 + 1, 1) + @Result, 
			@Seconds = @Seconds / 36;

	-- Left padded with zeros.
	set @Result = right('000000' + @Result, 6);

	--- The seventh digit is the projection of milliseconds.
	declare @Digit int = datepart(millisecond, @Now) * 35 / 999;
	set @Result = @Result + substring(@Characters, @Digit + 1, 1);

	-- The three last chars are random.
	-- dbo.appRandomHelper is a work around the error "Invalid use of a side-effecting operator 'rand' within a function."
	declare @Random float;
	select @Random = R from dbo.appRandomHelper;

	set @Result = @Result +  
	substring(@Characters, convert(int, @Random * 36) + 1, 1) +
	substring(@Characters, abs(checksum(@Random)) % 36 + 1, 1) +
	substring(@Characters, checksum(@Random * @Random) % 36 + 1, 1);

	-- Return the result of the function
	return cast(@Result as nchar(10));

END