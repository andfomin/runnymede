CREATE PROCEDURE [dbo].[sysInitializeConstants] 
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
-- Update
insert dbo.accTransactionAttributeTypes (Id, [Description]) values ('LITI', 'LearnerUserId to TeacherUserId');
--Insert
insert dbo.accTransactionAttributeTypes (Id, [Description]) values ('SCEV', 'Id in dbo.relScheduleEvents. Session');

insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('PPIP', 'Incoming PayPal payment', 'PPRI');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('PPIF', 'Incoming PayPal payment fee compensation', 'PPRI');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('PPOP', 'Outgoing PayPal payment', null);
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('NACC', 'New account', null);
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXRR', 'Request for review of exercise', 'REVW');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXRC', 'Request for review canceled', 'REVW');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXRF', 'Review finished', 'REVW');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('EXFD', 'Service fee deducted from reward', 'REVW');
-- Update
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('IPLT', 'Internal payment from learner to teacher', 'LITI');
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('IPFD', 'Service fee deducted from internal payment', 'LITI');
--Insert
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('SNRQ', 'Session request', 'SCEV');
--Insert
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('SNCN', 'Session cancellation', 'SCEV');
--Insert
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('SNPY', 'Payment for session', 'SCEV');
--Insert
insert into dbo.accTransactionTypes (Id, [Description], AttributeId) values ('SNFD', 'Service fee deducted from session payment', 'SCEV');

-- Update
insert into dbo.appConstants (Name, Value, Comment) values ('Relationships.Teachers.BuketCount', '1', 'Used for fast paging');
insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.Reviews.ServiceFeeRate', '0.29', 'Service fee rate. 29% of earnings.');
insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.Reviews.WorkDurationRatio', '4.0', 'Average ratio of work duration to exercise length.');
-- Update
insert into dbo.appConstants (Name, Value, Comment) values ('Accounting.Transfer.MinimalAmount', '1.0', 'Minimal amount of internal transfer from learner to teacher.');

--Insert
insert into dbo.appConstants (Name, Value, Comment) values ('Relationships.Sessions.ServiceFeeRate', '0.29', 'Service fee rate. 29% of session price.');

--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('OFFR', 'Offer', 'Offered period of availability for sessions');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('ROFR', 'Revoked Offer', 'Revoked offer of availability for sessions');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('RQSN', 'Requested Session', 'Session request');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('CFSN', 'Confirmed Session', 'Confirmed session');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('CSUS', 'Cancelled Session, by User', 'Session cancelled by User');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('CSSU', 'Cancelled Session, by SecondUser', 'Session cancelled by SecondUser');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('CLSN', 'Closed Session', 'Closed session');
--Insert
insert dbo.relScheduleEventTypes (Id, Name, [Description]) values ('DSSN', 'Disputed Session', 'Disputed session');
--Insert
insert dbo.relMessageAttributeTypes (Id, [Description]) values ('SCEV', 'Id in dbo.relScheduleEvents');
--Insert
insert dbo.relMessageTypes (Id, [Description], AttributeType) values ('SSSN', 'Session', 'SCEV');


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