

CREATE PROCEDURE [dbo].[exeGetExercises]
	@UserId int,
	@RowOffset int, 
	@RowLimit int
AS
BEGIN

set nocount on;

declare @RowCount int;

declare @t table (
	Id int,
	CreationTime datetime2(0),
	ServiceType char(6),
	ArtifactType char(6),
	Title nvarchar(100),
	[Length] int
);

insert into @t (Id, CreationTime, ServiceType, ArtifactType, Title, [Length])
	select Id, CreationTime, ServiceType, ArtifactType, Title, [Length]
	from dbo.exeExercises
	where UserId = @UserId;

select @RowCount = count(*) from @t;

delete @t 
	where Id not in 
		(
			select Id
			from @t 
			order by CreationTime desc
			offset @RowOffset rows
			fetch next @RowLimit rows only
		);

set nocount off;

-- The order of the recordsets does matter.

select Id, CreationTime, ServiceType, ArtifactType, Title, [Length]
from @t
order by CreationTime desc;

select R.ExerciseId, R.Id, R.RequestTime, R.StartTime, R.FinishTime
from dbo.exeReviews R 
	inner join @t T on R.ExerciseId = T.Id;

select @RowCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetExercises] TO [websiterole]
    AS [dbo];

