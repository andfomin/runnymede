CREATE PROCEDURE [dbo].[sysInitializeSpecialUsers]
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

declare @Comment nvarchar(100) = 'Special account';

--------------------- $Service

declare @ServiceUserId int = 1;
declare @ServiceUserName nvarchar(100) = N'$Service';

insert dbo.aspnetUsers (Id, UserName) values (@ServiceUserId, @ServiceUserName);

insert dbo.appUsers (Id, DisplayName) values (@ServiceUserId, @ServiceUserName);

execute dbo.accCreateAccount @ServiceUserId, 'ACSREV';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.ServiceRevenue', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.[Type] = 'ACSREV';

execute dbo.accCreateAccount @ServiceUserId, 'ACRQRV';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.RequestedReviews', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.[Type] = 'ACRQRV';

execute dbo.accCreateAccount @ServiceUserId, 'ACPPCA';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.PayPalCash', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.[Type] = 'ACPPCA';

execute dbo.accCreateAccount @ServiceUserId, 'ACPPIF';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.IncomingPayPalPaymentFee', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.[Type] = 'ACPPIF';

execute dbo.accCreateAccount @ServiceUserId, 'ACPPIT';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$Service.IncomingPayPalPaymentTax', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @ServiceUserId
		and AA.[Type] = 'ACPPIT';

--------------------- $UnknownPayPalPayer

declare @PayerUserId int = 2;
declare @PayerUserName nvarchar(100) = N'$UnknownPayPalPayer';

insert dbo.aspnetUsers (Id, UserName) values (@PayerUserId, @PayerUserName);

insert dbo.appUsers (Id, DisplayName) values (@PayerUserId, @PayerUserName);

execute dbo.accCreateAccount @PayerUserId, 'ACPERS';

insert dbo.appConstants (Name, Value, Comment)
	select 'Account.$UnknownPayPalPayer.Personal', AA.Id, @Comment
	from dbo.accAccounts AA
	where AA.UserId = @PayerUserId
		and AA.[Type] = 'ACPERS';

--------------------- $AnyTeacher

declare @AnyTeacherUserId int = 3; -- Hardcoded in app.AnyTeacherId
declare @AnyTeacherUserName nvarchar(100) = N'$AnyTeacher';

insert dbo.aspnetUsers (Id, UserName) values (@AnyTeacherUserId, @AnyTeacherUserName);

insert dbo.appUsers (Id, DisplayName) values (@AnyTeacherUserId, @AnyTeacherUserName);

execute dbo.accCreateAccount @AnyTeacherUserId, 'ACPERS';

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