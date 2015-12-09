


CREATE PROCEDURE [dbo].[exeGetExerciseWithReviews]
	@UserId int,
	@Id int -- This parameter name is shared with dbo.exeGetReview
AS
BEGIN

set nocount on;

-- R.ExerciseId is used as a divider to split rows on exercise and review entities in Runnymede.Website.Controllers.Mvc.ReviewsController.QueryExerciseReviews
select E.Id, E.[Length], E.ServiceType, E.CardId, E.CreationTime, E.ArtifactType, E.Artifact, E.Title, E.Comment, E.Details,
    R.ExerciseId, R.Id, R.RequestTime, R.StartTime, R.FinishTime, R.UserId, U.DisplayName as ReviewerName
from dbo.exeExercises E   
	left join dbo.exeReviews as R on E.Id = R.ExerciseId
    left join dbo.appUsers U on R.UserId = U.Id
where E.Id = @Id 
    and E.UserId = @UserId;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetExerciseWithReviews] TO [websiterole]
    AS [dbo];

