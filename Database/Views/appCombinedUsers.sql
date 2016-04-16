



CREATE VIEW [dbo].[appCombinedUsers]
AS
select A.Id, A.UserName, A.Email, A.EmailConfirmed, U.CreationTime, U.IsTeacher, U.DisplayName, U.SkypeName, U.ExtId, L.LoginProvider
from dbo.aspnetUsers A
	inner join dbo.appUsers U on A.Id = U.Id
	left join dbo.aspnetUserLogins L on A.Id = L.UserId