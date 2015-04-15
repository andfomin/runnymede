


CREATE PROCEDURE [dbo].[sesAcceptProposals]
	@UserId int,
	@Proposals xml
AS
BEGIN
/*
declare	@Proposals xml = '
<Proposals>
	<Proposal Id="4" Cost="10" />
	<Proposal Id="5" Cost="10" />
</Proposals>
';

*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;	

	declare @MinStart smalldatetime = dateadd(minute, dbo.appGetConstantAsInt('Sessions.BookingAdvance.Minutes'), sysutcdatetime());

	if @ExternalTran = 0
		begin transaction;

		-- Expect conflicts due to the concurrent nature of the task.
		if exists (
			select 1
			from @Proposals.nodes('/Offers/Offer') T(C)
				left join dbo.sesSessions S on 
					T.C.value('@Id[1]', 'int') = S.Id 
					and T.C.value('@Cost[1]', 'decimal(9,2)') = S.Cost
					and TeacherUserId is null
					and nullif(ProposedTeacherUserId, @UserId) is null
					and S.Start > @MinStart
			where S.Id is null
		)
			raiserror('%s,%d:: Outdated data.', 16, 1, @ProcName, @UserId);

		update S set TeacherUserId = @UserId, ProposalAcceptanceTime = sysutcdatetime(), Price = P.Price
			from dbo.sesSessions S 
				inner join @Proposals.nodes('/Offers/Offer') T(C) on
					S.Id = T.C.value('@Id[1]', 'int') 
					and S.Cost = T.C.value('@Cost[1]', 'decimal(9,2)')
				cross apply dbo.sesGetSessionPrice(S.Start, S.[End]) P
			where TeacherUserId is null
				and nullif(ProposedTeacherUserId, @UserId) is null
				and S.Start > @MinStart;

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
    ON OBJECT::[dbo].[sesAcceptProposals] TO [websiterole]
    AS [dbo];

