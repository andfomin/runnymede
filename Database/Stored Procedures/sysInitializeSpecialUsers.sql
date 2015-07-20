CREATE PROCEDURE [dbo].[sysInitializeSpecialUsers]
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

	declare @Comment nvarchar(100) = 'Special account';

	--------------------- $Service

	declare @CompanyUserName nvarchar(100) = N'$Service';
	declare @CompanyUserId int =  dbo.appGetConstantAsInt('UserId.$Company');

	insert dbo.aspnetUsers (Id, UserName) values (@CompanyUserId, @CompanyUserName);

	insert dbo.appUsers (Id, DisplayName) values (@CompanyUserId, @CompanyUserName);

	declare @Types xml = 
		N'<Types>
			<Type>ACSREV</Type>
			<Type>ACPPCA</Type>
			<Type>ACPPIF</Type>
			<Type>ACPPIT</Type>
		</Types>';

	exec dbo.accCreateAccountsIfNotExist @UserId = @CompanyUserId, @Types = @Types;

	insert dbo.appConstants (Name, Value, Comment)
		select 'Account.$Service.ServiceRevenue', Id, @Comment
		from dbo.accAccounts
		where UserId = @CompanyUserId 
			and [Type] = 'ACSREV';

	insert dbo.appConstants (Name, Value, Comment)
		select 'Account.$Service.PayPalCash', Id, @Comment
		from dbo.accAccounts
		where UserId = @CompanyUserId 
			and [Type] = 'ACPPCA';

	insert dbo.appConstants (Name, Value, Comment)
		select 'Account.$Service.IncomingPayPalPaymentFee', Id, @Comment
		from dbo.accAccounts
		where UserId = @CompanyUserId 
			and [Type] = 'ACPPIF';

	insert dbo.appConstants (Name, Value, Comment)
		select 'Account.$Service.IncomingPayPalPaymentSalesTax', Id, @Comment
		from dbo.accAccounts
		where UserId = @CompanyUserId 
			and [Type] = 'ACPPIT';

	-- We do not use constants with these accounts
	--declare @AccountTypes xml = (
	--	select [Type] 
	--	from (
	--		select 'ACCDSE' as [Type]
	--		union
	--		select 'ACCDSF' as [Type]
	--	) q
	--	for xml path(''), root('Types')
	--);

	--------------------- $UnknownPayPalPayer

	declare @PayerUserId int = 2;
	declare @PayerUserName nvarchar(100) = N'$UnknownPayPalPayer';

	insert dbo.aspnetUsers (Id, UserName) values (@PayerUserId, @PayerUserName);

	insert dbo.appUsers (Id, DisplayName) values (@PayerUserId, @PayerUserName);

	set @Types = 
		N'<Types>
			<Type>ACUCSH</Type>
		</Types>';

	exec dbo.accCreateAccountsIfNotExist @UserId = @PayerUserId, @Types = @Types;

	insert dbo.appConstants (Name, Value, Comment)
		select 'Account.$UnknownPayPalPayer.Personal', Id, @Comment
		from dbo.accAccounts
		where UserId = @PayerUserId 
			and [Type] = 'ACUCSH';

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