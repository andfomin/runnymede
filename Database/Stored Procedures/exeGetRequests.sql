

CREATE PROCEDURE [dbo].[exeGetRequests]
	@UserId int,
	@UserIsTeacher bit = null -- We get the value from the authorization cookie. Save on requests to dbo.appUsers
AS
/*
If the user has an unfinished review, do not show them requests. Return the ReviewId without a Price to indicate that case (Price is not null.)
*/
BEGIN

declare @ReviewId int;

/* There is CREATE UNIQUE NONCLUSTERED INDEX [UXF_UserId_FinishTime] ON [dbo].[exeReviews]
([UserId] ASC) WHERE ([UserId] IS NOT NULL AND [FinishTime] IS NULL) */
select @ReviewId = Id
from dbo.exeReviews 
where UserId = @UserId 
	and FinishTime is null;

if (@ReviewId is not null) begin

	select @ReviewId as ReviewId;

end
else begin

	/* There is CREATE NONCLUSTERED INDEX [IXFC_ReviewerUserId_Active_ReviewId_Price] ON [dbo].[exeRequests]
	([ReviewerUserId] ASC) INCLUDE ([ReviewId],[Price]) WHERE ([IsActive]=(1)) */
	-- If there are both a personal and a common requests simultaneously, the personal price wins.
	select RQ.ReviewId, RQ.ReviewerUserId, RQ.Price, 
		RV.ExerciseType, RV.ExerciseLength, RV.AuthorUserId, RV.AuthorName, RV.RequestTime
	from (
		select ReviewId, max(ReviewerUserId) as ReviewerUserId,	
			iif(
				count(*) = 1, 
				max(Price), 
				max(iif(ReviewerUserId = @UserId, Price, null))
			) as Price
		from (
			select ReviewId, ReviewerUserId, Price
			from dbo.exeRequests
			where ReviewerUserId = @UserId 
				and IsActive = 1
			union all
			select ReviewId, ReviewerUserId, Price
			from dbo.exeRequests
			where ReviewerUserId is null
				and IsActive = 1
				-- If the user is not a teacher, filter out requests to "Any teacher". Return only the direct ones.
				and @UserIsTeacher = 1
		) t
		group by ReviewId
	) RQ
		inner join dbo.exeReviews RV on RQ.ReviewId = RV.Id;

end

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetRequests] TO [websiterole]
    AS [dbo];

