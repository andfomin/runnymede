

CREATE PROCEDURE [dbo].[sesDeleteVacantTime]
	@UserId int,
	@Start smalldatetime,
	@End smalldatetime,
	@VacantTimeFound bit = null output
AS
BEGIN
/*
AF. 20140827
Periods of a user should be continuous and non-overlaping.

This operation is idempotent.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;	

	-- Original periods of vacant time
	declare @t table (
		Id int primary key,
		Start smalldatetime not null,
		[End] smalldatetime not null
	);

	-- The bottom part of a splitted big original period will be kept here
	declare @remains table (
		UserId	int not null,
		[End] smalldatetime not null,
		[Type] char(6) not null,
		Attribute int
	);

	-- There is no point in changing the past.
	if  @End < sysutcdatetime()
		raiserror('%s:: The end of the deleted period is in the past.', 16, 1, @ProcName);

	-- Find all original periods of vacant time which overlap with the parameter period in any way. 
	-- We assume that vacant time periods are sequential and not mutually ovarlapping.
	insert @t (Id, Start, [End])
		select Id, Start, [End]
		from dbo.sesScheduleEvents
		where UserId = @UserId
			and Start < @End
			and [End] > @Start
			and [Type] = 'SES_VT';

	select @VacantTimeFound = cast(count(*) as bit)
	from @t;

	-- Then update the found original periods separately depending on the relation to the parameter period.

	if (@VacantTimeFound = 1) begin

		if @ExternalTran = 0
			begin transaction;

			-- There may be four cases of original and parameter period relation. Those cases are mutually exclusive.
		
			/* CASE 1: An original period is contained within the parameter period.
			 * Original  ----|----|----
			 * Parameter --|--------|--
			 * An original period which is exactly equal to the parameter period goes here as well.
			 */
			delete S
				from dbo.sesScheduleEvents S
					inner join @t T on S.Id = T.Id
				where T.Start >= @Start
					and T.[End] <= @End;

			/* CASE 2: An original period precedes the parameter period.
			 * Original  --|------|----
			 * Parameter ----|------|--
			 */
			update S
				set [End] = @Start
				from dbo.sesScheduleEvents S
					inner join @t T on S.Id = T.Id
				where T.Start < @Start
					and T.[End] <= @End;

			/* CASE 3: An original period follows the parameter period.		
			 * Original  ----|------|--
			 * Parameter --|------|----
			 */
			update S
				set Start = @End
				from dbo.sesScheduleEvents S
					inner join @t T on S.Id = T.Id
				where T.Start >= @Start
					and T.[End] > @End;
			
			/* CASE 4: An original period contains the parameter period.
			 * Original  --|--------|--
			 * Parameter ----|----|----
			 * Update the start part of the big original period inplace.
			 * Keep the end part for inserting as a new row.
			 */
			update S
				set [End] = @Start
				output deleted.UserId, deleted.[End], deleted.[Type], deleted.Attribute into @remains
				from dbo.sesScheduleEvents S
					inner join @t T on S.Id = T.Id
				where T.Start < @Start
					and T.[End] > @End;

			-- Insert a replacement of the end part of a splitted big original period.
			insert dbo.sesScheduleEvents (UserId, Start, [End], [Type], Attribute)
				select UserId, @End, [End], [Type], Attribute
				from @remains;

			-- End of CASE 4.

			/* The following case is a combimnation of the CASE 2 and CASE 3 above and will be handled appropriately.
			 * Originals --|--|--|--|--
			 * Parameter ----|----|----
			 */

		if @ExternalTran = 0
			commit;

	end

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
    ON OBJECT::[dbo].[sesDeleteVacantTime] TO [websiterole]
    AS [dbo];

