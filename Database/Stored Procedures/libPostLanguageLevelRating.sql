

CREATE PROCEDURE [dbo].[libPostLanguageLevelRating]
	@UserId int,
	@ResourceId int,
	@LanguageLevelRating tinyint -- Understood: 1 - almost nothing, 2 - almost everything, 3 - everything
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

declare @CombinedShift int = 32;
declare @MinLevel int = 1;
declare @MaxLevel int = 255; -- LanguageLevel is stored as tinyint
declare @MaxUserMaturity int = 32;
declare @MaxResourceMaturity int = 255; -- LanguageLevelMaturity is stored as int
declare @InitialUserLevel tinyint = 128 - 16;
declare @InitialResourceLevel tinyint = 128 + 16;

declare @ResourceViewId int, @UserMaturity int, @ResourceMaturity int, @ShiftQuantum float, @UserShift int, @ResourceShift int;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	-- Mutually shift LanguageLevel of the user and the resource in opposite directions. Shifts are inversely proportional to maturity.
	-- We limit maturities of both sides for calculations.				
	select @UserMaturity = iif(LanguageLevelMaturity > @MaxUserMaturity, @MaxUserMaturity, coalesce(LanguageLevelMaturity, 1))				
	from dbo.appUsers
	where Id = @UserId;

	select @ResourceMaturity = iif(LanguageLevelMaturity > @MaxResourceMaturity, @MaxResourceMaturity, coalesce(LanguageLevelMaturity, 1))
	from dbo.libResources
	where Id = @ResourceId;

	-- Understood: 1 - almost nothing, 2 - almost everything, 3 - everything
	set @ShiftQuantum = 1.0 * (@LanguageLevelRating - 2) * @CombinedShift / (@UserMaturity + @ResourceMaturity);
	set @UserShift = convert(int, round(@ShiftQuantum * @ResourceMaturity, 0));
	set @ResourceShift = -1 * convert(int, round(@ShiftQuantum * @UserMaturity, 0));

	if @ExternalTran = 0
		begin transaction;

			-- We expect a dbo.libPostResourceView call has happend before this call and we assume the record does exist. 
			-- We allow the user to set LanguageLevelRating only once.
			update dbo.libUserResources 
				set LanguageLevelRating = @LanguageLevelRating
			where UserId = @UserId
				and ResourceId = @ResourceId
				and LanguageLevelRating is null;

			if (@@rowcount != 0) begin

				update dbo.appUsers
				set 
					LanguageLevel = case
						when LanguageLevel + @UserShift < @MinLevel then @MinLevel
						when LanguageLevel + @UserShift > @MaxLevel then @MaxLevel
						else coalesce(LanguageLevel, @InitialUserLevel) + @UserShift
					end,
					LanguageLevelMaturity = coalesce(LanguageLevelMaturity, 1) + 1										
				where Id = @UserId;

				update dbo.libResources
				set 
					LanguageLevel = case
						when LanguageLevel + @ResourceShift < @MinLevel then @MinLevel
						when LanguageLevel + @ResourceShift > @MaxLevel then @MaxLevel
						else coalesce(LanguageLevel, @InitialResourceLevel) + @ResourceShift
					end,
					LanguageLevelMaturity = coalesce(LanguageLevelMaturity, 1) + 1										
				where Id = @ResourceId;

			end

	if @ExternalTran = 0
		commit;

end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback;
	if @XState = -1
		rollback;
	throw;
end catch

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[libPostLanguageLevelRating] TO [websiterole]
    AS [dbo];

