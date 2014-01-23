

CREATE PROCEDURE [dbo].[relEnsureLearnerTutorRelation]
	@LearnerUserId int,
	@TutorUserId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;
--raiserror('%s,%d: ', 16, 1, @ProcName, @);

begin try

	if not exists (select * from dbo.relLearnersTutors where LearnerUserId = @LearnerUserId and TutorUserId = @TutorUserId) begin

		if @ExternalTran > 0
			save transaction ProcedureSave;

		declare @LearnerDisplayName nvarchar(100), @TutorDisplayName nvarchar(100);

		select @LearnerDisplayName = DisplayName from dbo.appUsers where Id = @LearnerUserId and coalesce(IsTutor, 0) = 0;
	
		if @LearnerDisplayName is null 
			raiserror('%s,%d: The user is supposed to be a learner.', 16, 1, @ProcName, @LearnerUserId);

		select @TutorDisplayName = DisplayName from dbo.appUsers where Id = @TutorUserId and IsTutor = 1;
	
		if @TutorDisplayName is null 
			raiserror('%s,%d: The user is not a tutor.', 16, 1, @ProcName, @TutorUserId);

		if @ExternalTran = 0
			begin transaction;

			insert dbo.relLearnersTutors (LearnerUserId, TutorUserId, LearnerDisplayName, TutorDisplayName)
				select @LearnerUserId, @TutorUserId, @LearnerDisplayName, @TutorDisplayName
				where not exists (select * from dbo.relLearnersTutors where LearnerUserId = @LearnerUserId and TutorUserId = @TutorUserId)

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
    ON OBJECT::[dbo].[relEnsureLearnerTutorRelation] TO [websiterole]
    AS [dbo];

