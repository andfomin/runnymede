

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

-- AC accAccounts
insert into dbo.appTypes (Id, Name, Title, Details) values ('ACUCSH', 'Account.User.Cash', 'Cash', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('ACUESC', 'Account.User.Escrow', 'Escrow', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('ACSREV', 'Account.Company.Revenue', 'Company Revenue', '<Kind>REVENUE</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, Details) values ('ACPPCA', 'PayPal Cash', '<Kind>ASSET</Kind><IsDebit>1</IsDebit>');
insert into dbo.appTypes (Id, Name, Details) values ('ACPPIF', 'Incoming PayPal Payment Fee', '<Kind>EXPENSE</Kind><IsDebit>1</IsDebit>');
insert into dbo.appTypes (Id, Name, Details) values ('ACPPIT', 'Incoming PayPal Payment Sales Tax', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, Details) values ('ACREVW', 'DEPRECATED Personal Review Account', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
insert into dbo.appTypes (Id, Name, Details) values ('ACRQRV', 'DEPRECATED Requested Reviews Account', '<Kind>REVENUE</Kind><IsDebit>0</IsDebit>');
-- Added 20150429
--insert into dbo.appTypes (Id, Name, Title, [Description]) values ('ACURIW', 'Account.User.Reviews.IELTS.Writing', 'Reviews - IELTS Writing', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit><IsMonetary>0</IsMonetary>');
--insert into dbo.appTypes (Id, Name, Title, [Description]) values ('ACURIS', 'Account.User.Reviews.IELTS.Speaking', 'Reviews - IELTS Speaking', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit><IsMonetary>0</IsMonetary>');
--insert into dbo.appTypes (Id, Name, Title, [Description]) values ('ACCRIW', 'Account.Company.RequestedReviews.IELTS.Writing', 'Requested reviews - IELTS Writing', '<Kind>REVENUE</Kind><IsDebit>0</IsDebit><IsMonetary>0</IsMonetary>');
--insert into dbo.appTypes (Id, Name, Title, [Description]) values ('ACCRIS', 'Account.Company.RequestedReviews.IELTS.Speaking', 'Requested reviews - IELTS Speaking', '<Kind>REVENUE</Kind><IsDebit>0</IsDebit><IsMonetary>0</IsMonetary>');
-- Added 20150528
--insert into dbo.appTypes (Id, Name, Title, [Description]) values ('ACCFIW', 'Account.Company.FreeReviews.IELTS.Writing', 'Free reviews - IELTS Writing. Free of charge.', '<Kind>EXPENSE</Kind><IsDebit>1</IsDebit><IsMonetary>0</IsMonetary>');
--insert into dbo.appTypes (Id, Name, Title, [Description]) values ('ACCFIS', 'Account.Company.FreeReviews.IELTS.Speaking', 'Free reviews - IELTS Speaking. Free of charge.', '<Kind>EXPENSE</Kind><IsDebit>1</IsDebit><IsMonetary>0</IsMonetary>');
-- Added 20150608
--insert into dbo.appTypes (Id, Name, Title, [Description], Details) values ('ACUDSC', 'Account.User.Discount', 'Discount', 'Discounts promised to the user. Below-the-line account', '<Kind>LIABILITY</Kind><IsDebit>0</IsDebit>');
--insert into dbo.appTypes (Id, Name, [Description], Details) values ('ACCDSE', 'Account.Company.Discounts.Emitted', 'Discounts emitted by Company. Below-the-line account ', '<Kind>EXPENSE</Kind><IsDebit>1</IsDebit>');
--insert into dbo.appTypes (Id, Name, [Description], Details) values ('ACCDSF', 'Account.Company.Discounts.Fulfilled', 'Discounts fulfilled by Company. Below-the-line account ', '<Kind>REVENUE</Kind><IsDebit>0</IsDebit>');

-- AT Attributes
insert dbo.appTypes (Id, Name) values ('ATSSSN', 'dbo.sesSessions');
insert dbo.appTypes (Id, Name) values ('ATREVW', 'dbo.exeReviews');
insert dbo.appTypes (Id, Name) values ('ATPPTI', 'PayPal transaction Id');

-- EX exeExercises ArtifactType
insert dbo.appTypes (Id, Name, [Description]) values ('ARMP3_', 'Artifact.MP3', 'Audio recording, MP3');
insert dbo.appTypes (Id, Name, [Description]) values ('ARJPEG', 'Artifact.JPEG', 'Photo of writing, JPEG');

-- FR libResources
insert into dbo.appTypes (Id, Name, Title) values ('FRCIEX', 'Resource.Format.CoreInventory', 'Core Inventory exponents');
insert into dbo.appTypes (Id, Name, Title) values ('FRYTVD', 'Resource.Format.YoutubeVideo', 'YouTube video');
insert into dbo.appTypes (Id, Name, Title) values ('FRHTML', 'Resource.Format.Webpage', 'Web page');
insert into dbo.appTypes (Id, Name, Title, [Description]) values ('FRBCEG', 'Resource.Format.BCEG', 'British Council English Grammar', '{"urlBase":"http://learnenglish.britishcouncil.org/en/english-grammar/"}');

-- SE sesScheduleEvents
--insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SES_VT', 'Session._.VacantTime', 'Announced period of availability for sessions', null);
--insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSRQ', 'Session.Skype.Requested', 'Skype session request', 'ATSSSN');
--insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSCF', 'Session.Skype.Confirmed', 'Confirmed session', 'ATSSSN');
--insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSCS', 'Session.Skype.Canceled.Self', 'Session canceled by the user themself', 'ATSSSN');
--insert dbo.appTypes (Id, Name, [Description], AttributeType) values ('SESSCO', 'Session.Skype.Canceled.Other', 'Session canceled by the other user', 'ATSSSN');

-- MS appMessages
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSRQ', 'Skype session request', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSCF', 'Session confirmed', 'ATSSSN');
--insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSCH', 'Session canceled by the host user', 'ATSSSN');
--insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSCG', 'Session canceled by the guest user', 'ATSSSN');
--insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSDH', 'Session disputed by the host user', 'ATSSSN');
--insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSDG', 'Session disputed by the guest user', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('MSSSMS', 'A custom message related to the session', 'ATSSSN');

-- PL Pricelist in dbo.appValues
insert into dbo.appTypes (Id, Name, [Description]) values ('PLDSCT', 'Pricelist.Discounts', 'Price list - Discounts for total amount');

-- SV Corresponds to Runnymede.Common.Models.ServiceType in ExerciseModels.cs
insert into dbo.appTypes (Id, Name, Title, Details) values ('SVRIS_', 'Service.Review.IELTS.Speaking', 'Review of an exercise, IELTS Speaking', '<PriceListPosition>01.03</PriceListPosition>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('SVRIW1', 'Service.Review.IELTS.WritingTask1', 'Review of an exercise, IELTS Writing Task 1', '<PriceListPosition>01.01</PriceListPosition>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('SVRIW2', 'Service.Review.IELTS.WritingTask2', 'Review of an exercise, IELTS Writing Task 2', '<PriceListPosition>01.02</PriceListPosition>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('SVRIR_', 'Service.Review.IELTS.Reading', 'Review of an exercise, IELTS Reading', '<PriceListPosition>01.04</PriceListPosition>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('SVRIL_', 'Service.Review.IELTS.Listening', 'Review of an exercise, IELTS Listening', '<PriceListPosition>01.05</PriceListPosition>');
insert into dbo.appTypes (Id, Name, Title, Details) values ('SVSSSN', 'Service.Session', 'Session with a teacher', '<PriceListPosition>01.06</PriceListPosition>');

-- TR dbo.accTransactions
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
insert dbo.appTypes (Id, Name, AttributeType) values ('TRSSCN', 'Session cancelled', 'ATSSSN');
insert dbo.appTypes (Id, Name, AttributeType) values ('TRSSCL', 'Session closed', 'ATSSSN');

-- CD dbo.exeCards  Corresponds to Runnymede.Common.Models.CardType in ExerciseModels.cs
insert into dbo.appTypes (Id, Name, Title) values ('CDIW1A', 'Card.IELTS.WritingTask1.Academic', 'Task 1 Academic');
insert into dbo.appTypes (Id, Name, Title) values ('CDIW1G', 'Card.IELTS.WritingTask1.General', 'Task 1 General Training');
insert into dbo.appTypes (Id, Name, Title) values ('CDIW2_', 'Card.IELTS.WritingTask2', 'Task 2');
insert into dbo.appTypes (Id, Name, Title) values ('CDIS__', 'Card.IELTS.Speaking', 'Speaking');

--

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