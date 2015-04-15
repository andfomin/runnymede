

CREATE PROCEDURE [dbo].[libPostResourceView]
	@UserId int,
	@ResourceId int
AS
BEGIN
/*
 This SP is called on every resource view. Make it as lightweight as possible.
*/
SET NOCOUNT ON;

	--merge dbo.libUserResources as Trg
	--using (values(@UserId, @ResourceId)) as Src (UserId, ResourceId)
	--	on Trg.UserId = Src.UserId and Trg.ResourceId = Src.ResourceId
	--when not matched then
	--	insert (UserId, ResourceId, IsPersonal, LanguageLevelRating, DescriptionId, Comment, ReindexSearch)
	--		values (@UserId, @ResourceId, 0, null, null, null, 0);

	insert dbo.libUserResources (UserId, ResourceId, IsPersonal, LanguageLevelRating, DescriptionId, Comment, ReindexSearch)
		select @UserId, @ResourceId, 0, null, null, null, 0
		where not exists (
			select * 
			from dbo.libUserResources
			where UserId = @UserId
				and ResourceId = @ResourceId
		);

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[libPostResourceView] TO [websiterole]
    AS [dbo];

