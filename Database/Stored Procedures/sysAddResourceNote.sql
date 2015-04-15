


create PROCEDURE [dbo].[sysAddResourceNote]
	@UserId int,
	@ResourceId int,
	@Note nvarchar(1000)
AS
BEGIN
SET NOCOUNT ON;

declare @Node xml =
	'<Note date="' + convert(nvarchar(20), cast(sysutcdatetime() as smalldatetime), 127) + 'Z" userId="' + cast(@UserId as nvarchar(20)) + '">' + @Note + '</Note>';

update dbo.libResources
set Notes = N'<Notes />'
where Id = @ResourceId
	and Notes is null;

update dbo.libResources
set Notes.modify('insert sql:variable("@Node") as first into (/Notes)[1]')
where Id = @ResourceId
;

END