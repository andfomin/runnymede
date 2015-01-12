

CREATE FUNCTION [dbo].[friGetReviewers]
(
	@UserId int
)
RETURNS TABLE 	
AS
RETURN 
	select U.Id, U.DisplayName, F1.RecordingRate, F1.WritingRate
	from dbo.friFriends F1
		inner join dbo.friFriends F2 on F1.UserId = F2.FriendUserId
		inner join dbo.appUsers U on F1.UserId = U.Id
	where F1.FriendUserId = @UserId
		and F2.UserId = @UserId
		and F1.IsActive = 1	 
		and F2.IsActive = 1;
GO
GRANT SELECT
    ON OBJECT::[dbo].[friGetReviewers] TO [websiterole]
    AS [dbo];

