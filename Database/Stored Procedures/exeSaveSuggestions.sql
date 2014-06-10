



CREATE PROCEDURE [dbo].[exeSaveSuggestions]
	@UserId int,
	@Suggestions dbo.exeSuggestionsType readonly
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;
--raiserror('%s,%d::', 16, 1, @ProcName, @);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

    -- Ensure that the user is the actual reviewer and the review is not finished.
	if exists (
		select * 
		from ( 
			select distinct ReviewId from @Suggestions 
			) S
			left join dbo.exeReviews RV on S.ReviewId = RV.Id
		where coalesce(RV.UserId, 0) != @UserId
			or RV.FinishTime is not null
	)
		raiserror('%s,%d:: The user cannot save suggestions for the review.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;

		merge dbo.exeSuggestions as target
		using (
			select ReviewId, CreationTime, [Text] from @Suggestions
			) as source (ReviewId, CreationTime, [Text])
		on (target.ReviewId = source.ReviewId and target.CreationTime = source.CreationTime)
		when matched then 
			update set [Text] = source.[Text]
		when not matched then	
			insert (ReviewId, CreationTime, [Text])
			values (source.ReviewId, source.CreationTime, source.[Text]);

	if @ExternalTran = 0
		commit;
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
    ON OBJECT::[dbo].[exeSaveSuggestions] TO [websiterole]
    AS [dbo];

