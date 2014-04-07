

CREATE PROCEDURE [dbo].[relAddLearnerToTeacher]
	@LearnerUserId int,
	@TeacherUserId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try

	if not exists (select * from dbo.relLearnersTeachers where LearnerUserId = @LearnerUserId and TeacherUserId = @TeacherUserId) begin

		if @ExternalTran > 0
			save transaction ProcedureSave;

		declare @LearnerDisplayName nvarchar(100), @TeacherDisplayName nvarchar(100);

		select @LearnerDisplayName = DisplayName from dbo.appUsers where Id = @LearnerUserId and IsTeacher = 0;
	
		if @LearnerDisplayName is null 
			raiserror('%s,%d: The user is supposed to be not a teacher.', 16, 1, @ProcName, @LearnerUserId);

		select @TeacherDisplayName = DisplayName from dbo.appUsers where Id = @TeacherUserId and IsTeacher = 1;
	
		if @TeacherDisplayName is null 
			raiserror('%s,%d: The user is not a teacher.', 16, 1, @ProcName, @TeacherUserId);

		if @ExternalTran = 0
			begin transaction;

			insert dbo.relLearnersTeachers (LearnerUserId, TeacherUserId, LearnerDisplayName, TeacherDisplayName)
				select @LearnerUserId, @TeacherUserId, @LearnerDisplayName, @TeacherDisplayName;

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
    ON OBJECT::[dbo].[relAddLearnerToTeacher] TO [websiterole]
    AS [dbo];

