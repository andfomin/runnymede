

CREATE PROCEDURE [dbo].[exeGetRequests]
	@UserId int
AS
/*
If the user has an unfinished review, do not show them requests. Return the Review Id without a Type to indicate that case (Type is not null.)
*/
BEGIN

declare @ReviewId int;

-- There is IX_UserId_FinishTime

select @ReviewId = Id
from dbo.exeReviews 
where UserId = @UserId 
	and FinishTime is null;

if (@ReviewId is not null) begin

	select @ReviewId as Id, N'unfinished' as ServiceType;

end
else begin

	select R.Id, E.ServiceType
	from dbo.exeReviews R
		inner join dbo.exeExercises E on R.ExerciseId = E.Id
	where R.UserId is null;

end

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetRequests] TO [websiterole]
    AS [dbo];

