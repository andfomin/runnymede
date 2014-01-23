CREATE FUNCTION [dbo].[relGetRandomTutors]
(
	@Session int,
	@Bucket int
)
RETURNS 
	@t TABLE 
	(
		Id int,
		DisplayName nvarchar(100),
		RateARec decimal(18, 2)
	)
AS
BEGIN
/* AF 20140115.
This is not paging. I believe, paging is more expensive than randomly put items into the fixed number of buckets. Backeting does not invole sort.
Discriminator calculation should be deterministic and evenly distributed across the full range of int. 
Checksum T-SQL algorithms available in SQL Server are just a simple projection of the argument and have a correlated non-uniform result distribution.
At the same time good T-SQL hashing algorithms are expensive in terms of performance.

@Session comes from the client and should be deterministic, i.e. the same during the paging session.

The admin should periodically update 'Relationships.Tutors.BuketCount' according to the total table row count and expected average number of items on a page.
*/
	declare @BacketCount int = dbo.appGetConstantAsInt('Relationships.Tutors.BuketCount');
	declare @NormalizedBucket int = @Bucket % @BacketCount;

	insert @t
		select Id, DisplayName, RateARec
		from dbo.appUsers 
		where IsTutor = 1
		and abs(binary_checksum(cast(sin(Id + @Session) as nvarchar(100)))) % @BacketCount = @NormalizedBucket
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].relGetRandomTutors TO [websiterole]
    AS [dbo];

