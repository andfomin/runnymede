

CREATE PROCEDURE [dbo].[exeTryChangeExerciseAuthor]
	@ExerciseId int,
	@UserId int, -- Original owner
	@SkypeName nvarchar(100) -- Skype name of the new owner
AS
BEGIN
/*
Do not raise error. We continue the upload procedure with whatever owner.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @NewUserId int = dbo.appGetUserIdBySkypeName(@SkypeName);

	if @ExternalTran = 0
		begin transaction;

			update dbo.exeExercises
			set UserId = @NewUserId
			output inserted.UserId
			where @NewUserId is not null
				and Id = @ExerciseId
				and UserId = @UserId;

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
    ON OBJECT::[dbo].[exeTryChangeExerciseAuthor] TO [websiterole]
    AS [dbo];

