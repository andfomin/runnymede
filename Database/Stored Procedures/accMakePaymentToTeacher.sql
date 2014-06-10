

CREATE PROCEDURE [dbo].[accMakePaymentToTeacher]
	@UserId int,
	@TeacherUserId int,
	@Amount decimal(18,2)                    
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @AmountText nvarchar(100), @Attribute nvarchar(100),
		@SenderAccountId int, @RecepientAccountId int, @RevenueAccountId int, 
		@ServiceFeeRate float, @Fee decimal(18,2), @InitialRecepientBalance decimal(18,2),
		@TransferTransactionId int, @FeeTransactionId int;

	set @AmountText = cast(@Amount as nvarchar(100));

	if (@Amount < dbo.appGetConstantAsFloat('Accounting.Transfer.MinimalAmount')) 
	begin
		raiserror('%s,%d,%s:: Amount is less then allowed minimum.', 16, 1, @ProcName, @UserId, @AmountText);
	end;

	if (@Amount > dbo.accGetBalance(@UserId))
	begin
		raiserror('%s,%d,%s:: Non-sufficient funds.', 16, 1, @ProcName, @UserId, @AmountText);
	end;

	if not exists (select * from dbo.appUsers where Id = @UserId and [IsTeacher] = 0) 
		raiserror('%s,%d: The payer is supposed to be not a teacher.', 16, 1, @ProcName, @UserId);

	if not exists (select * from dbo.appUsers where Id = @TeacherUserId and [IsTeacher] = 1) 
		raiserror('%s,%d: The recepient is not a teacher.', 16, 1, @ProcName, @TeacherUserId);

	if not exists (select * from dbo.relLearnersTeachers where LearnerUserId = @UserId and TeacherUserId = @TeacherUserId)
		raiserror('%s,%d,%d: A learner can pay to a favorite teacher only.', 16, 1, @ProcName, @UserId, @TeacherUserId);

	set @SenderAccountId = dbo.accGetPersonalAccount(@UserId);

	set @RecepientAccountId = dbo.accGetPersonalAccount(@TeacherUserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@SenderAccountId is null) or (@RecepientAccountId is null) or (@RevenueAccountId is null)
	begin
		raiserror('%s,%d,%d:: Account not found.', 16, 1, @ProcName, @UserId, @TeacherUserId);  
	end;

	set @ServiceFeeRate = cast(dbo.appGetConstant('Exercises.Reviews.ServiceFeeRate') as float);
	set @Fee = cast((@Amount * @ServiceFeeRate) as decimal(18,2));

	set @Attribute = cast(@UserId as nvarchar(100)) + ' ' + cast(@TeacherUserId as nvarchar(100));

	declare @Now datetime2(0) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		-- Make transfer.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute)
			values ('IPLT', @Now, @Attribute);

		select @TransferTransactionId = scope_identity() where @@rowcount != 0;

		-- Deduct service fee.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute)
			values ('IPFD', @Now, @Attribute);

		select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the sender account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransferTransactionId, @SenderAccountId, @Amount, null, Balance - @Amount
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @SenderAccountId);

		-- We are going to post two entries. Reuse the balance.
		select @InitialRecepientBalance = Balance
		from dbo.accEntries
		where Id = (select max(Id) from dbo.accEntries where AccountId = @RecepientAccountId);

		-- Temporary credit the recepient account with the full transfer amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@TransferTransactionId, @RecepientAccountId, null, @Amount, @InitialRecepientBalance + @Amount);

		-- Deduct service fee from the recepient account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @RecepientAccountId, @Fee, null, @InitialRecepientBalance + @Amount - @Fee);

		-- Credit the service.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @FeeTransactionId, @RevenueAccountId, null, @Fee, Balance + @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

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
    ON OBJECT::[dbo].[accMakePaymentToTeacher] TO [websiterole]
    AS [dbo];

