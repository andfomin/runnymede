

CREATE PROCEDURE [dbo].[sysInitializeTypes]
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

--insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('', null, null, null);

		--insert dbo.appTypes (Id, TableName, Name, [Description], AttributeType) 
		--	values ('FRAR', 'appFees', 'Fees.AudioRecord', 'Fee for reviewing audio record', 'Fee for reviewing audio record', null);
		--insert dbo.appTypes (Id, TableName, Name, [Description], AttributeType) 
		--	values ('FRWP', 'appFees', 'Fees.WritingPhoto', 'Fee for reviewing photographed writing', 'Fee for reviewing photographed writing', null);

-- AT Attributes
insert dbo.appTypes (Id, Name) values ('ATSSSN', 'dbo.sesSessions');
insert dbo.appTypes (Id, Name) values ('ATREVW', 'dbo.exeReviews');
insert dbo.appTypes (Id, Name) values ('ATPPTI', 'PayPal transaction Id');

-- AC accAccounts
insert into dbo.appTypes (Id, Name, [Description]) values ('ACPERS', 'Personal', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, [Description]) values ('ACESCR', 'Escrow', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, [Description]) values ('ACSREV', 'Service Revenue', '<Kind>REVENUE</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, [Description]) values ('ACPPCA', 'PayPal Cash', '<Kind>ASSET</Kind><IsDebit>1</IsDebit>');
insert into dbo.appTypes (Id, Name, [Description]) values ('ACPPIF', 'Incoming PayPal Payment Fee', '<Kind>EXPENSE</Kind><IsDebit>1</IsDebit>');
insert into dbo.appTypes (Id, Name, [Description]) values ('ACPPIT', 'Incoming PayPal Payment Sales Tax', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');

-- TR accTransactions
insert dbo.appTypes (Id, Name, AttributeType) values ('TRNACC', 'New account', null);
insert dbo.appTypes (Id, Name, AttributeType) values ('TRPPIF', 'Incoming PayPal payment - Transfer fee deducted', 'ATPPTI');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRPPIP', 'Incoming PayPal payment', 'ATPPTI');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRPPIT', 'Incoming PayPal payment - Sales tax deducted', 'ATPPTI');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRPPOP', 'Outgoing PayPal payment', null);
insert dbo.appTypes (Id, Name, AttributeType) values ('TRRVRQ', 'Request for review of exercise', 'ATREVW');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRRVRC', 'Request for review canceled', 'ATREVW');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRRVST', 'Review started - Refund of extra escrow', 'ATREVW');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRRVFN', 'Review finished', 'ATREVW');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRRVFD', 'Service fee deducted from review price', 'ATREVW');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRSSRQ', 'Request for session', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRSSCL', 'Session closed', 'ATSSSN');

-- EX exeExercises
insert dbo.appTypes (Id, Name) values ('EXAREC', 'Audio Recording MP3');
insert dbo.appTypes (Id, Name) values ('EXWRPH', 'Writing Photo JPEG');

-- SE sesScheduleEvents
insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SES_VT', 'Session._.VacantTime', 'Announced period of availability for sessions', null);
insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSRQ', 'Session.Skype.Requested', 'Skype session request', 'ATSSSN');
insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSCF', 'Session.Skype.Confirmed', 'Confirmed session', 'ATSSSN');
insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSCS', 'Session.Skype.Canceled.Self', 'Session canceled by the user themself', 'ATSSSN');
insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSCO', 'Session.Skype.Canceled.Other', 'Session canceled by the other user', 'ATSSSN');

-- MS appMessages
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSRQ', 'Skype session request', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSCF', 'Session confirmed', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSCH', 'Session canceled by the host user', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSCG', 'Session canceled by the guest user', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSDH', 'Session disputed by the host user', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSDG', 'Session disputed by the guest user', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSMS', 'A custom message related to the session', 'ATSSSN');

-- CN friFriends
insert into dbo.appTypes (Id, Name) values ('CN__AE', 'Added with email');
insert into dbo.appTypes (Id, Name) values ('CN__RR', 'Exercise review requested');
insert into dbo.appTypes (Id, Name) values ('CN__RS', 'Exercise review started');
insert into dbo.appTypes (Id, Name) values ('CN__SU', 'Skype session requested by the user/guest is confirmed by the friend/host');
insert into dbo.appTypes (Id, Name) values ('CN__SF', 'Skype session requested by the friend/guest is confirmed by the user/host');

-- FR libResources
insert into dbo.appTypes (Id, Name) values ('FRCIEX', 'Core Inventory exponents');
insert into dbo.appTypes (Id, Name) values ('FRYTVD', 'YouTube video');
insert into dbo.appTypes (Id, Name) values ('FRHTML', 'Web page');
insert into dbo.appTypes (Id, Name, [Description]) values ('FRBCEG', 'British Council English Grammar', '{"urlBase":"http://learnenglish.britishcouncil.org/en/english-grammar/"}');

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