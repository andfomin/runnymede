CREATE FUNCTION [dbo].[relGetRandomTeachers]
(
	@ViewSession int,
	@Bucket int
)
/*
Distributes all rows in pre-determined number of backets randomly.
*/
RETURNS 
	@t TABLE 
	(
		RowNumber int identity primary key clustered,
		Id int,
		DisplayName nvarchar(100),
		ReviewRate decimal(18, 2),
		SessionRate decimal(18, 2),
		Announcement nvarchar(1000),
		ExtId uniqueidentifier
	)
AS
BEGIN
/* AF 20140115.
This is not paging. I believe, paging is more expensive than randomly put items into the fixed number of buckets. Bucketing does not invole sort.

Discriminator calculation should be deterministic and evenly distributed across the full range of int PK. 
Checksum T-SQL algorithms available in SQL Server are just a simple projection of the argument and have a correlated non-uniform result distribution.
At the same time good T-SQL hashing algorithms are expensive in terms of performance.
We use instead a custom hasher with good distribution.

@ViewSession comes from the client and should be deterministic, i.e. the same during the paging session.

The admin should periodically update 'Relationships.Teachers.BuketCount' according to the total table row count and expected average number of items on a page.
*/

	declare @BacketCount int = dbo.appGetConstantAsInt('Relationships.Teachers.BuketCount');
	declare @NormalizedBucket int = @Bucket % @BacketCount;

/* +http://blogs.msdn.com/b/sqltips/archive/2005/07/20/441053.aspx
 INSERT queries that use SELECT with ORDER BY to populate rows guarantees how identity values are computed but not the order in which the rows are inserted
 We do order by RowNumber in the calling query.
*/
	insert @t (Id, DisplayName, ReviewRate, SessionRate, Announcement, ExtId)
		select Id, DisplayName, ReviewRate, SessionRate, Announcement, ExtId
		from dbo.appUsers 
		where IsTeacher = 1
		and abs(checksum(cast((sin(Id) + sin(@ViewSession)) as nvarchar(100)))) % @BacketCount = @NormalizedBucket
		order by checksum(cast((sin(Id) + sin(@ViewSession)) as nvarchar(100)));
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetRandomTeachers] TO [websiterole]
    AS [dbo];

