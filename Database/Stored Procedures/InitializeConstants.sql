CREATE PROCEDURE [dbo].[InitializeConstants] 
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

insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('PERS', 'Personal', 'LIABILITY', 0);
insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('ESCR', 'Escrow', 'LIABILITY', 0);
insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('SREV', 'Service Revenue', 'REVENUE', 0);
insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('PPCA', 'PayPal Cash', 'ASSET', 1);
insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('PPIF', 'Incoming PayPal Payment Fee', 'EXPENSE', 1);

insert dbo.accTransactionAttributeTypes (Id, [Description]) values ('EXER', 'ExerciseId');
insert dbo.accTransactionAttributeTypes (Id, [Description]) values ('REVW', 'ReviewId');
insert dbo.accTransactionAttributeTypes (Id, [Description]) values ('PPRI', 'PayPal Receipt Id');
-- New
insert dbo.accTransactionAttributeTypes (Id, [Description]) values ('LITI', 'LearnerUserId to TutorUserId');

insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('PPIP', 'Incoming PayPal payment', 'PPRI');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('PPIF', 'Incoming PayPal payment fee compensation', 'PPRI');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('PPOP', 'Outgoing PayPal payment', null);
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('NACC', 'New account', null);
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXRR', 'Request for review of exercise', 'REVW');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXRC', 'Request for review canceled', 'REVW');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXRF', 'Review finished', 'REVW');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXFD', 'Service fee deducted from reward', 'REVW');
-- New
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('IPLT', 'Internal payment from learner to tutor', 'LITI');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('IPFD', 'Service fee deducted from internal payment', 'LITI');

insert into dbo.appConstants (Name, Value, Comment) values ('Relationships.Tutors.BuketCount', '1', 'Used for fast paging');
--Update
insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.Reviews.ServiceFeeRate', '0.29', 'Service fee rate. 29% of earnings.');

-- New
insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.Reviews.WorkDurationRatio', '4.0', 'Average ratio of work duration to exercise length.');
insert into dbo.appConstants (Name, Value, Comment) values ('Accounting.Transfer.MinimalAmount', '1.0', 'Minimal amount of internal transfer from learner to tutor.');

/* Inserted into dbo.appConstants whithin dbo.newInitializeSpecialUsers:
'Account.$Service.ServiceRevenue',
'Account.$Service.PayPalCash', 
'Account.$Service.PayPalIncomingPaymentFee', 
'Account.$UnknownPayPalPayer.Personal'
*/

insert into dbo.exeExerciseTypes (Id, [Description]) values ('AREC', 'Audio Recording');

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