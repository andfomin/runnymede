


CREATE FUNCTION [dbo].[conGetOffers]
(
	@MinCreateTime datetime2(2),
	@UserId int
)
RETURNS TABLE 	
AS
RETURN 

	select U.Id, U.DisplayName, U.Announcement, 
		iif(I.InvitationTo & I.InvitationFrom = 1, U.SkypeName, null) as SkypeName,
		I.InvitationTo, I.InvitationFrom, UO.CreateTime
	from (
		-- Find all other users who was active recently. CreateTime is used for sorting in an outer query.
		select UserId as OtherUserId, min(CreateTime) as CreateTime
		from dbo.conOffers
		where CreateTime > @MinCreateTime
			and UserId != @UserId
		group by UserId
	) UO
		inner join dbo.appUsers U on UO.OtherUserId = U.Id
		left join (
			-- Find mutual invitations with the user
			select 
				I0.OtherUserId,
				cast(max(isnull(I0.ToUserId, 0)) as bit) as InvitationTo,
				cast(max(isnull(I0.UserId, 0)) as bit) as InvitationFrom
			from (
				select coalesce(I1.ToUserId, I2.UserId) as OtherUserId, I1.ToUserId, I2.UserId
				from (
					select ToUserId
					from dbo.conOffers
					where CreateTime > @MinCreateTime
						and UserId = @UserId
				) I1
				full outer join (
					select UserId
					from dbo.conOffers
					where CreateTime > @MinCreateTime
						and ToUserId = @UserId
				) I2 on I1.ToUserId = I2.UserId
			) I0
			group by I0.OtherUserId	
		) I on UO.OtherUserId = I.OtherUserId
	;
GO
GRANT SELECT
    ON OBJECT::[dbo].[conGetOffers] TO [websiterole]
    AS [dbo];

