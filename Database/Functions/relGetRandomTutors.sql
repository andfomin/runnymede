CREATE FUNCTION [dbo].[relGetRandomTutors]
(
	@Session int,
	@Bucket int
)
/*
Returns rows in random order.
*/
RETURNS 
	@t TABLE 
	(
		RowNumber int identity primary key clustered,
		Id int,
		DisplayName nvarchar(100),
		Rate decimal(18, 2),
		Announcement nvarchar(1000)
	)
AS
BEGIN
/* AF 20140115.
This is not paging. I believe, paging is more expensive than randomly put items into the fixed number of buckets. Bucketing does not invole sort.

Discriminator calculation should be deterministic and evenly distributed across the full range of int PK. 
Checksum T-SQL algorithms available in SQL Server are just a simple projection of the argument and have a correlated non-uniform result distribution.
At the same time good T-SQL hashing algorithms are expensive in terms of performance.
We use instead a custom hasher with good distribution.

@Session comes from the client and should be deterministic, i.e. the same during the paging session.

The admin should periodically update 'Relationships.Tutors.BuketCount' according to the total table row count and expected average number of items on a page.
*/

	declare @BacketCount int = dbo.appGetConstantAsInt('Relationships.Tutors.BuketCount');
	declare @NormalizedBucket int = @Bucket % @BacketCount;

/* +http://blogs.msdn.com/b/sqltips/archive/2005/07/20/441053.aspx
 INSERT queries that use SELECT with ORDER BY to populate rows guarantees how identity values are computed but not the order in which the rows are inserted
*/
	insert @t (Id, DisplayName, Rate, Announcement)
		select Id, DisplayName, Rate, Announcement
		from dbo.appUsers 
		where IsTutor = 1
		and abs(checksum(cast((sin(Id) + sin(@Session)) as nvarchar(100)))) % @BacketCount = @NormalizedBucket
		order by checksum(cast((sin(Id) + sin(@Session)) as nvarchar(100)));
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].relGetRandomTutors TO [websiterole]
    AS [dbo];

