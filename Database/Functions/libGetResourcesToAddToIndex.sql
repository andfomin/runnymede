
CREATE FUNCTION [dbo].[libGetResourcesToAddToIndex]
(
	@MaxCount int
)
RETURNS TABLE 	
AS
RETURN 

	select top(@MaxCount) R.Id, R.[Format], R.NaturalKey, R.Segment,
		T.Title, D.CategoryIds, D.Tags, S.Name as [Source],
		D.HasExplanation, D.HasExample, D.HasExercise, D.HasText, D.HasPicture, D.HasAudio, D.HasVideo,
		R.LanguageLevel, R.Rating
	from dbo.libResources R
		inner join dbo.libDescriptions D on R.DescriptionId = D.Id
		inner join dbo.libTitles T on D.TitleId = T.Id
		left join dbo.libSources S on D.SourceId = S.Id
	where R.ReindexSearch = 1
		and R.IsCommon = 1;
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetResourcesToAddToIndex] TO [websiterole]
    AS [dbo];

