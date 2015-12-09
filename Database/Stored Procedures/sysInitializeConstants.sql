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

/* Inserted into dbo.appConstants whithin dbo.newInitializeSpecialUsers:
'Account.$Service.PayPalIncomingPaymentFee', 
'Account.$Service.PayPalIncomingPaymentTax', 
'Account.$Service.PayPalCash', 
'Account.$Service.ServiceRevenue',
'Account.$UnknownPayPalPayer.Personal'
*/

-- 20150304
insert into dbo.appConstants (Name, Value, Comment) values ('Sessions.Length.Minutes', '25', 'The length of the session');
insert into dbo.appConstants (Name, Value, Comment) values ('Sessions.SlotLength.Minutes', '30', 'The length of the session slot');
insert into dbo.appConstants (Name, Value, Comment) values ('Sessions.BookingAdvance.Minutes', '5', 'Session must booked at least X minutes in advance');
-- end 20150304
-- 20150313
--insert into dbo.appConstants (Name, Value, Comment) values ('Accounting.Transfer.MinimalAmount', '1.0', 'Minimal amount of internal transfer from learner to teacher.');
--insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.Reviews.ServiceFeeRate', '0.29', 'Service fee rate. 29% of earnings.');
--insert into dbo.appConstants (Name, Value, Comment) values ('Sessions.ServiceFeeRate', '0.29', 'Service fee rate. 29% of session price.');
--insert into dbo.appConstants (Name, Value, Comment) values ('Relationships.Teachers.BuketCount', '1', 'Used for fast paging');
--insert into dbo.appConstants (Name, Value, Comment) values ('AnyTeacher.ReviewRate.EXAREC', '2.5', 'Anonymous teacher recordings review rate');
--insert into dbo.appConstants (Name, Value, Comment) values ('AnyTeacher.ReviewRate.EXWRPH', '3.0', 'Anonymous teacher writings review rate');
insert into dbo.appConstants (Name, Value, Comment) values ('Sessions.ClosingDelay.Minutes', '60', 'The learner has an opportunity to dispute a session until it is closed programmatically.');
--insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.ReviewRate.EXAREC', '1.0', 'Review rate for recordings, $/1minute');
--insert into dbo.appConstants (Name, Value, Comment) values ('Exercises.ReviewRate.EXWRPH', '1.0', 'Review rate for writings, $/100words');
-- end 20150313
-- 20150528
insert into dbo.appConstants (Name, Value, Comment) values ('UserId.$Company', '1', 'The special user associated with the Company.');

--insert dbo.appAttributeTypes (Id, Name, [Description]) values ('SCEV', 'dbo.sesScheduleEvents', null);
--insert dbo.appAttributeTypes (Id, Name, [Description]) values ('SSSN', 'dbo.sesSessions', null);--T
--insert dbo.appAttributeTypes (Id, Name, [Description]) values ('REVW', 'dbo.exeReviews', 'ReviewId');--T
--insert dbo.appAttributeTypes (Id, Name, [Description]) values ('PPRI', null, 'PayPal Receipt Id');--T
--insert dbo.appAttributeTypes (Id, Name, [Description]) values ('LITI', null, 'LearnerUserId to TeacherUserId');--T

--insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('PERS', 'Personal', 'LIABILITY', 0);--T
--insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('ESCR', 'Escrow', 'LIABILITY', 0);--T
--insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('SREV', 'Service Revenue', 'REVENUE', 0);--T
--insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('PPCA', 'PayPal Cash', 'ASSET', 1);--T
--insert into dbo.accAccountTypes (Id, [Description], Kind, IsDebit) values ('PPIF', 'Incoming PayPal Payment Fee', 'EXPENSE', 1);--T

--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('IPFD', 'Service fee deducted from internal payment', 'LITI');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('IPLT', 'Internal payment from learner to teacher', 'LITI');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('NACC', 'New account', null);--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('PPIF', 'Incoming PayPal payment fee compensation', 'PPRI');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('PPIP', 'Incoming PayPal payment', 'PPRI');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('PPOP', 'Outgoing PayPal payment', null);--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('RVRQ', 'Request for review of exercise', 'REVW');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('RVRC', 'Request for review canceled', 'REVW');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('RVFN', 'Review finished', 'REVW');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('RVFD', 'Service fee deducted from review price', 'REVW');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('SSRQ', 'Request for session', 'SSSN');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('SSFN', 'Session finished', 'SSSN');--T
--insert into dbo.accTransactionTypes (Id, [Description], AttributeType) values ('SSFD', 'Service fee deducted from session payment', 'SSSN');--T

--insert dbo.sesScheduleEventTypes (Id, Name, [Description], AttributeType) values ('S_VT', 'Session._.VacantTime', 'Announced period of availability for sessions', null);--T
--insert dbo.sesScheduleEventTypes (Id, Name, [Description], AttributeType) values ('SSRQ', 'Session.Skype.Requested', 'Skype session request', 'SSSN');--T
--insert dbo.sesScheduleEventTypes (Id, Name, [Description], AttributeType) values ('SSCF', 'Session.Skype.Confirmed', 'Confirmed session', 'SSSN');--T
--insert dbo.sesScheduleEventTypes (Id, Name, [Description], AttributeType) values ('SSCS', 'Session.Skype.Canceled.Self', 'Session canceled by the user themself', 'SSSN');--T
--insert dbo.sesScheduleEventTypes (Id, Name, [Description], AttributeType) values ('SSCO', 'Session.Skype.Canceled.Other', 'Session canceled by the other user', 'SSSN');--T

--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSRQ', 'Skype session request', 'SSSN');--T
--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSCF', 'Session confirmed', 'SSSN');--T
--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSCH', 'Session canceled by the host user', 'SSSN');--T
--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSCG', 'Session canceled by the guest user', 'SSSN');--T
--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSDH', 'Session disputed by the host user', 'SSSN');--T
--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSDG', 'Session disputed by the guest user', 'SSSN');--T
--insert dbo.appMessageTypes (Id, [Description], AttributeType) values ('SSMS', 'A custom message related to the session', 'SSSN');--T

insert into dbo.libSources(Id, Name, HomePage, IconUrl) values ('BCEQ', 'British Council & EAQUALS', 'http://eaquals.org/cefr/', 'http://eaquals.org/favicon.ico');
insert into dbo.libSources (Id, Name, HomePage, IconUrl) values ('BCEG', 'British Council, English Grammar', 'http://learnenglish.britishcouncil.org/en/english-grammar', '//s2.googleusercontent.com/s2/favicons?domain=learnenglish.britishcouncil.org');

--insert into dbo.libFormats (Id, Name) values ('CIEX', 'Core Inventory exponents');--T
--insert into dbo.libFormats (Id, Name) values ('YTVD', 'YouTube Video');--T

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