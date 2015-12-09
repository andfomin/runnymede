

CREATE PROCEDURE [dbo].[accCreateAccountsIfNotExist]
	@UserId int,
	@Types xml
AS
BEGIN
/*
Creates new money accounts for a user. Initializes zero balance on the accounts.

20150429 AF. Initial release.

<Types>
  <Type>ACUCSH</Type>
  <Type>ACRVIW</Type>
  <Type>ACRVIS</Type>
</Types>

*/
SET NOCOUNT ON;
declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Accounts table (
		[Type] char(6) primary key,
		Id int,
		IsDebit bit
	);

	-- Find the account types for which there is no user account.
	insert @Accounts ([Type])
		select T.C.value('.', 'char(6)')
		from @Types.nodes('Types/Type') T(C)
			left join dbo.accAccounts A on @UserId = A.UserId and T.C.value('.', 'char(6)') = A.[Type]
		where A.Id is null;

	if exists (select * from @Accounts) begin

		update A 
		set IsDebit = q2.IsDebit
			from @Accounts A 
				inner join (
					select q.[Type], T.C.value('.', 'bit') as IsDebit
					from (
						select T1.Id as [Type], T1.Details
						from dbo.appTypes T1
							inner join @Types.nodes('Types/Type') T2(C2) on T1.Id = T2.C2.value('.', 'char(6)')
					) q
						cross apply q.Details.nodes('/IsDebit') T(C)
				) q2 on A.[Type] = q2.[Type];

		if @ExternalTran = 0
			begin transaction;

		-- Create accounts
		insert dbo.accAccounts ([Type], UserId)
			select [Type], @UserId
			from @Accounts;

		update TA
		set Id = A.Id
		from @Accounts TA
			inner join dbo.accAccounts A on @UserId = A.UserId and TA.[Type] = A.[Type]

		if exists (select * from @Accounts where Id is null) begin
			declare @TypesText nvarchar(1000) = cast(@Types as nvarchar(1000));
			raiserror('%s,%s:: Account was not created.', 16, 1, @ProcName, @TypesText);  
		end

		-- Insert initial enties for every new account to start with a zero balance.
		insert into dbo.accTransactions ([Type], ObservedTime)
			values ('TRNACC', sysutcdatetime());

		declare @TransactionId int;

		select @TransactionId = scope_identity() where @@rowcount != 0;

		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, Id,
				--case when IsDebit = 1 then 0 else null end, case when IsDebit = 0 then 0 else null end,
				nullif(0, IsDebit), nullif(IsDebit, 1),
				0.0
			from @Accounts;

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