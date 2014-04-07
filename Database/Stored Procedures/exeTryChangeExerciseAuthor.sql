

CREATE PROCEDURE [dbo].[exeTryChangeExerciseAuthor]
	@ExerciseId int,
	@UserId int, -- Original owner
	@Skype nvarchar(100) = null -- Skype name of the new owner
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

	declare @Count int, @NewUserId int;

	declare @t table (
		Id int
	);

	insert @t
		select Id from dbo.appUsers where Skype = @Skype;

	select @Count = count(*) from @t;
	select @NewUserId = Id from @t where @Count = 1;

	if @ExternalTran = 0
		begin transaction;

			update dbo.exeExercises
			set UserId = @NewUserId
			where @NewUserId is not null
				and Id = @ExerciseId
				and UserId = @UserId;

	if @ExternalTran = 0
		commit;

	select @NewUserId;

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

