
CREATE FUNCTION [dbo].[friGetFriends]
(
	@UserId int
)
RETURNS TABLE 	
AS
RETURN 
	select U.Id, U.DisplayName, U.Announcement,
		F.UserIsActive, F.FriendIsActive,
		F.UserRecordingRate, F.FriendRecordingRate, 
		F.UserWritingRate, F.FriendWritingRate, 
		F.UserSessionRate, F.FriendSessionRate, 
		F.UserLastContactDate, F.FriendLastContactDate, 
		F.UserLastContactType, F.FriendLastContactType
	from (
		select
			coalesce(F1.FriendUserId, F2.UserId) as OtherUserId, 
			F1.IsActive as UserIsActive, F2.IsActive as FriendIsActive,
			F1.RecordingRate as UserRecordingRate, F2.RecordingRate as FriendRecordingRate,
			F1.WritingRate as UserWritingRate, F2.WritingRate as FriendWritingRate,
			F1.SessionRate as UserSessionRate, F2.SessionRate as FriendSessionRate,
			F1.LastContactDate as UserLastContactDate, F2.LastContactDate as FriendLastContactDate, 
			F1.LastContactType as UserLastContactType, F2.LastContactType as FriendLastContactType
		from (
			select UserId, FriendUserId, IsActive, RecordingRate, WritingRate, SessionRate, LastContactDate, LastContactType
			from dbo.friFriends
			where UserId = @UserId
		) F1
		full outer join (
			select UserId, FriendUserId, IsActive, RecordingRate, WritingRate, SessionRate, LastContactDate, LastContactType
			from dbo.friFriends
			where FriendUserId = @UserId
		) F2 on F1.FriendUserId = F2.UserId
	) F
		inner join dbo.appUsers U on F.OtherUserId = U.Id;
GO
GRANT SELECT
    ON OBJECT::[dbo].[friGetFriends] TO [websiterole]
    AS [dbo];

