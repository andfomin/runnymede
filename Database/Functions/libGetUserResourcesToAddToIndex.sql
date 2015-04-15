


CREATE FUNCTION [dbo].[libGetUserResourcesToAddToIndex]
(
)
RETURNS TABLE 	
AS
RETURN 

	select UR.UserId, R.Id, R.[Format], R.NaturalKey, R.Segment,
		T.Title, D.CategoryIds, D.Tags, S.Name as [Source],
		D.HasExplanation, D.HasExample, D.HasExercise, D.HasText, D.HasPicture, D.HasAudio, D.HasVideo,
		UR.Comment
	from dbo.libUserResources UR
		inner join dbo.libResources R on UR.ResourceId = R.Id
		inner join dbo.libDescriptions D on UR.DescriptionId = D.Id 
		inner join dbo.libTitles T on D.TitleId = T.Id
		left join dbo.libSources S on D.SourceId = S.Id
	where UR.ReindexSearch = 1
		and UR.IsPersonal = 1
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetUserResourcesToAddToIndex] TO [websiterole]
    AS [dbo];

