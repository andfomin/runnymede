

CREATE PROCEDURE [dbo].[appCreateUserAndAccounts]
	@UserId int,
	@DisplayName nvarchar(100) 
AS
BEGIN
	SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		insert dbo.appUsers (Id, DisplayName) values (@UserId, @DisplayName);

		exec dbo.accCreateUserAccounts @UserId;

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
    ON OBJECT::[dbo].[appCreateUserAndAccounts] TO [websiterole]
    AS [dbo];

