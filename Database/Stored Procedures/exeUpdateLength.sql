

CREATE PROCEDURE [dbo].[exeUpdateLength]
	@ExerciseId int,
	@UserId int,
	@Length int
AS
BEGIN
SET NOCOUNT ON;
/* Only word count may be changed manually. Recording length is red from the mp3 file. */

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeExercises 
		set [Length] = @Length 
		where Id = @ExerciseId 
			and UserId = @UserId
			and [Type] = 'EXWRPH'
			and not exists (
				select *
				from dbo.exeReviews
				where ExerciseId = @ExerciseId
					and StartTime is not null
			)

		if @@rowcount = 0
			raiserror('%s,%d,%d:: Length cannot be changed.', 16, 1, @ProcName, @UserId, @ExerciseId);	

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
    ON OBJECT::[dbo].[exeUpdateLength] TO [websiterole]
    AS [dbo];

