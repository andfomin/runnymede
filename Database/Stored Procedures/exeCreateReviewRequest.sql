

CREATE PROCEDURE [dbo].[exeCreateReviewRequest]
	@Request xml
AS
BEGIN
/*
declare	@Request xml = '
<Request>
	<User Id="4" />
	<Exercise Id="555" />
	<Reviewer UserId="4" Price="0" />
	<Reviewer UserId="5" Price="2.22" />
	<Reviewer Price="3.33" />
</Request>
';

Publishes exercise for reviewing. 
We allow for an arbitrary reviewer, in that case UserId is a null
We allow for self-reviewing.

20121113 AF. Initial release
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @UserId int, @ExerciseId int, @ExerciseType char(6), @ExerciseLength int, @MaxPrice decimal(18, 2), @IsTeacher bit,
			@AuthorName nvarchar(100), @ReviewerNames nvarchar(500), @Attribute nvarchar(100), @RequestText nvarchar(1000),
			@ReviewId int;

	declare @R table (
		UserId int,
		DisplayName nvarchar(100),
		IsTeacher bit,
		Price decimal(9,2),
		CalculatedPrice decimal(9,2)
	);

	set @RequestText = convert(nvarchar(1000), @Request);

	set @UserId = @Request.value('(/Request/User/@Id)[1]', 'int' );

	set @ExerciseId = @Request.value('(/Request/Exercise/@Id)[1]', 'int' );

	-- Only the owner of the exercise can request a review. 
	select @ExerciseType = [Type], @ExerciseLength = [Length] 
	from dbo.exeExercises 
	where Id = @ExerciseId 
		and UserId = @UserId;
	
	-- Type is non-nullable.
	if (@ExerciseType is null)
		raiserror('%s,%d,%d:: The user cannot request a review of the exercise.', 16, 1, @ProcName, @UserId, @ExerciseId);

	insert @R (UserId, Price, CalculatedPrice)
		select R.UserId, R.Price,
			case
				-- We allow for an arbitrary reviewer.
				when R.UserId is null then dbo.exeCalculateReviewPrice(@ExerciseType, @ExerciseLength, dbo.exeGetAnyTeacherReviewRate(@ExerciseType))
				-- We allow for self-reviewing.
				when R.UserId = @UserId then 0
				else dbo.exeCalculateReviewPrice(@ExerciseType, @ExerciseLength,
					iif(dbo.exeIsTypeRecording(@ExerciseType) = 1, F.RecordingRate, iif(dbo.exeIsTypeWriting(@ExerciseType) = 1, F.WritingRate, null)))
			end		
		from (
			select T.C.value('@UserId[1]', 'int') as UserId, T.C.value('@Price[1]', 'decimal(9,2)') as Price
			from @Request.nodes('/Request/Reviewer') T(C)
		) R
			left join dbo.friFriends F on R.UserId = F.UserId and F.FriendUserId = @UserId and F.IsActive = 1;

	-- The reviewer has to setup a personal review rate for the exercise author.
	if exists (
		select * from @R where CalculatedPrice is null
	)
		raiserror('%s,%s:: The reviewer has not set a personal review rate for the exercise author.', 16, 1, @ProcName, @RequestText);

	if exists (
		select * 
		from @R
		where (Price is null)
			or (Price < 0)
			or (Price <> CalculatedPrice)
	)
		raiserror('%s,%s:: Wrong price.', 16, 1, @ProcName, @RequestText);

	if exists (
		select UserId
		from @R
		group by UserId
		having count(*) > 1
	) 
		raiserror('%s,%s:: The reviewer is proposed twice.', 16, 1, @ProcName, @RequestText);

	if exists (
		select * 
		from @R R
			inner join dbo.exeRequests RQ on R.UserId = RQ.ReviewerUserId
			inner join dbo.exeReviews RV on RQ.ReviewId = RV.Id
		where RV.ExerciseId = @ExerciseId
			and RQ.IsActive = 1
	)
		raiserror('%s,%s:: There is already a request to the proposed reviewer for reviewing the exercise.', 16, 1, @ProcName, @RequestText);

	update R
	set DisplayName = U.DisplayName, IsTeacher = U.IsTeacher
	from @R R
		left join dbo.appUsers U on R.UserId = U.Id;

	if exists (
		select *
		from @R
		where UserId is not null
			and Price > 0
			and IsTeacher = 0
	)
		raiserror('%s,%s:: The price must be zero for a reviewer who is not a teacher.', 16, 1, @ProcName, @RequestText);

	-- Concatenate reviewer names to store temporarily in the ReviewerName column until the review starts.
	-- 'Any teacher' corresponds to AnyTeacherDisplayName in utils.js
	set @ReviewerNames =
		stuff((
			select N', ' + coalesce(DisplayName, N'Any teacher')
			from @R
			for xml path('')
		), 1, 2, '');

	select @AuthorName = DisplayName 
	from dbo.appUsers 
	where Id = @UserId;

	-- We store intially the maximal price in dbo.exeReviews. We store the personal prices in dbo.exeRequests.
	-- We will adjust it and refund the user on the review start based on the actual price of the reviewer.
	select @MaxPrice = max(Price)
	from @R;

	select @ReviewId = dbo.exeGetNewReviewId();

	if @ExternalTran = 0
		begin transaction;

		insert dbo.exeReviews (Id, ExerciseId, Price, ExerciseType, ExerciseLength, RequestTime, AuthorUserId, AuthorName, ReviewerName)
			values (@ReviewId, @ExerciseId, @MaxPrice, @ExerciseType, @ExerciseLength, sysutcdatetime(), @UserId, @AuthorName, @ReviewerNames);		

		insert dbo.exeRequests (ReviewerUserId, IsActive, ReviewId, Price)
			select R.UserId, 1, @ReviewId, R.Price
			from @R R;

		if (@MaxPrice > 0) begin

			set @Attribute = cast(@ReviewId as nvarchar(100));
		
			exec dbo.accChangeEscrow @UserId = @UserId, @Amount = @MaxPrice, @TransactionType = 'TRRVRQ', @Attribute = @Attribute, @Details = @Request;
		end

		update F  
		set LastContactType = 'CN__RR', LastContactDate = sysutcdatetime()
		from dbo.friFriends F
			inner join @R R on F.FriendUserId = R.UserId
		where F.UserId = @UserId

	if @ExternalTran = 0
		commit;

	select Id, ExerciseId, Price, RequestTime, ReviewerName
	from dbo.exeReviews 
	where Id = @ReviewId;

end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback;
	if @XState = -1
		rollback;
	throw;
end catch



END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeCreateReviewRequest] TO [websiterole]
    AS [dbo];

