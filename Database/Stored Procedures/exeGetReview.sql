



CREATE PROCEDURE [dbo].[exeGetReview]
	@UserId int,
	@Id int -- This parameter name is shared with dbo.exeGetExerciseWithReviews
AS
BEGIN

set nocount on;

-- R.ExerciseId is used as a divider to split rows on exercise and review entities in Runnymede.Website.Controllers.Mvc.ReviewsController.QueryExerciseReviews
select E.Id, E.[Length], E.ServiceType, E.CardId, E.ArtifactType, E.Artifact, E.Details,
	R.ExerciseId, R.Id, R.StartTime, R.FinishTime
from dbo.exeExercises E 	
	inner join dbo.exeReviews as R on E.Id = R.ExerciseId
where R.Id = @Id 
    and R.UserId = @UserId;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetReview] TO [websiterole]
    AS [dbo];

