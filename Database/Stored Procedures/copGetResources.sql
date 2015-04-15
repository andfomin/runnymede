
CREATE PROCEDURE [dbo].[copGetResources]
	@UserId int,
	@RowOffset int,
	@RowLimit int,
	@Viewed bit,
	@Session int
AS
BEGIN
/*
-- Get resources viewed/not_viewed by the user. Group by NaturalKey. Order by CopycatPriority, then by consistent/repeatable random.
*/
set nocount on;

declare @TotalCount int;

declare @Seed float = sin(@Session);

declare @t table (
	RowNumber int identity primary key clustered,
	NaturalKeyChecksum bigint,
	ResourceId int
);

/*
-- The IsForCopycat condition hints the query optimizer to get all we need from FX_NaturalKeyChecksum. Be carefull, do not read anything extra from the table.
-- The computed column NaturalKeyChecksum is not persisted. It is procesed from FX_NaturalKeyChecksum. Observe the execution plan to decide whether to persist it.
-- Consistent/repeatable random. Convertion through decimal(19,17) keeps max available presision. Direct convertion from float trancates decimal digits.

INSERT query that uses SELECT with ORDER BY to populate rows guarantees how identity values are computed but not the order in which the rows are inserted
+http://blogs.msdn.com/b/sqltips/archive/2005/07/20/441053.aspx .
*/

insert @t (NaturalKeyChecksum, ResourceId)
	select R.NaturalKeyChecksum, max(R.Id)
	from dbo.libResources R
		left join dbo.libUserResources UR on @UserId = UR.UserId and R.Id = UR.ResourceId
	where iif(UR.ResourceId is not null, 1, 0) = @Viewed
		and R.IsForCopycat = 1
	group by R.NaturalKeyChecksum
	order by 
		max(coalesce(UR.CopycatPriority, 0)) desc, 
		avg(100000 * coalesce(UR.CopycatPriority, 0)) desc, 
		binary_checksum(convert(nvarchar(20), convert(decimal(19,17), sin(R.NaturalKeyChecksum) + @Seed)));

select @TotalCount = count(*) from @t;

set nocount off;

;with Cte (RowNumber, NaturalKeyChecksum, ResourceId)
as (
	select RowNumber, NaturalKeyChecksum, ResourceId
	from @t
	order by RowNumber
	offset @RowOffset rows
		fetch next @RowLimit rows only
)
select Id, [Format], NaturalKey, Segment, [Priority], Title
from (
	-- Segments
	select R.Id, R.[Format], R.NaturalKey, R.Segment, UR.CopycatPriority as [Priority], null as Title, T.RowNumber
	from Cte T
		inner join dbo.libResources R on T.NaturalKeyChecksum = R.NaturalKeyChecksum
		left join dbo.libUserResources UR on @UserId = UR.UserId and R.Id = UR.ResourceId
	-- Although the IsForCopycat condition seems redundand, it hints the query optimizer to perform index seek in FX_NaturalKeyChecksum instead of clusterd index scan.
	where R.IsForCopycat = 1
	union all
	-- Titles
	select null, null, R.NaturalKey, null, null, TT.Title, T.RowNumber
	from Cte T
		inner join dbo.libResources R on T.ResourceId = R.Id
		left join dbo.libDescriptions D on R.DescriptionId = D.Id
		left join dbo.libTitles TT on D.TitleId = TT.Id
) q
order by RowNumber;

-- Do not return @TotalCount as a column in the main query, because the main query may return no rows for a big @RowOffset.
select @TotalCount as TotalCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[copGetResources] TO [websiterole]
    AS [dbo];

