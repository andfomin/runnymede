


CREATE FUNCTION [dbo].[libGetResources]
(
	@UserId int,
	@Resources xml
)
RETURNS TABLE 	
AS
RETURN 

	select R.Id, R.[Format], R.NaturalKey, R.Segment,
		TT.Title, D.CategoryIds, D.Tags, S.Name as [Source],
		D.HasExplanation, D.HasExample, D.HasExercise, D.HasText, D.HasPicture, D.HasAudio, D.HasVideo,
		UR.IsPersonal, UR.Comment,
		convert(bit, iif(UR.IsPersonal is null, 0, 1)) as Viewed
	from @Resources.nodes('/Resources/R') T(C) 
		inner join dbo.libResources R on T.C.value('@Id[1]', 'int') = R.Id
		left join dbo.libUserResources UR on @UserId = UR.UserId and R.Id = UR.ResourceId
		left join dbo.libDescriptions D on coalesce(UR.DescriptionId, R.DescriptionId) = D.Id
		left join dbo.libTitles TT on D.TitleId = TT.Id
		left join dbo.libSources S on D.SourceId = S.Id
	;
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetResources] TO [websiterole]
    AS [dbo];

