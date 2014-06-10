


CREATE PROCEDURE [dbo].[exeSaveRemarks]
	@UserId int,
	@Remarks dbo.exeRemarksType readonly
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
			select distinct ReviewId from @Remarks 
			) RM
			left join dbo.exeReviews RV on RM.ReviewId = RV.Id
		where coalesce(RV.UserId, 0) != @UserId
			or RV.FinishTime is not null
	)
		raiserror('%s,%d:: The user cannot save remarks for the review.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;

		merge dbo.exeRemarks as target
		using (
			select ReviewId, CreationTime, Start, Finish, [Text], Keywords from @Remarks
			) as source (ReviewId, CreationTime, Start, Finish, [Text], Keywords)
		on (target.ReviewId = source.ReviewId and target.CreationTime = source.CreationTime)
		when matched then 
			update set Start = source.Start, Finish = source.Finish, [Text] = source.[Text], Keywords = source.Keywords
		when not matched then	
			insert (ReviewId, CreationTime, Start, Finish, [Text], Keywords)
			values (source.ReviewId, source.CreationTime, source.Start, source.Finish, source.[Text], source.Keywords);

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
    ON OBJECT::[dbo].[exeSaveRemarks] TO [websiterole]
    AS [dbo];

