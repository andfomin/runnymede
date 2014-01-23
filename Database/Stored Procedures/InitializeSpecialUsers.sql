CREATE PROCEDURE [dbo].[InitializeSpecialUsers]
AS
BEGIN
	SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);
--raiserror('%s: ', 16, 1, @ProcName);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

declare @Comment nvarchar(100) = 'Special account.';

--------------------- $Service

declare @ServiceUserId int;
declare @ServiceUserName nvarchar(100) = N'$Service';

insert dbo.aspnetUsers (UserName) values (@ServiceUserName);

select @ServiceUserId = scope_identity() where @@rowcount != 0;

insert dbo.appUsers (Id, DisplayName) values (@ServiceUserId, @ServiceUserName);

execute dbo.accCreateAccount @ServiceUserId, 'SREV';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.ServiceRevenue', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.AccountTypeId = 'SREV';

execute dbo.accCreateAccount @ServiceUserId, 'PPCA';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.PayPalCash', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.AccountTypeId = 'PPCA';

execute dbo.accCreateAccount @ServiceUserId, 'PPIF';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.IncomingPayPalPaymentFee', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.AccountTypeId = 'PPIF';

--------------------- $UnknownPayPalPayer

declare @PayerUserId int;
declare @PayerUserName nvarchar(100) = N'$UnknownPayPalPayer';

insert dbo.aspnetUsers (UserName) values (@PayerUserName);

select @PayerUserId = scope_identity() where @@rowcount != 0;

insert dbo.appUsers (Id, DisplayName) values (@PayerUserId, @PayerUserName);

execute dbo.accCreateAccount @PayerUserId, 'PERS';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$UnknownPayPalPayer.Personal', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @PayerUserId
		and AA.AccountTypeId = 'PERS';

---------------------

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