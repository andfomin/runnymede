




CREATE FUNCTION [dbo].[exeGetNewExerciseId]() 
RETURNS int
AS
BEGIN
/*
20141026 AF
We are not allowed to call crypt_gen_random() from within a scalar-valued function. The error is "Invalid use of a side-effecting operator 'Crypt_Gen_Random' within a function."
However a view can call a side-effect function and we can call a view from within a function. We use dbo.sysRandomTenDigitPositiveInt as a workaround.
We generate random Ids and test them against dbo.exeExercises until we find a vacant one.
*/

declare @Id int;

with MyCTE (Id, Attempt) as (
	select Random as Id, 1 as Attempt from dbo.sysRandomTenDigitPositiveInt
	union all 
	select (select Random from dbo.sysRandomTenDigitPositiveInt) as Id, MC.Attempt + 1 as Attempt
	from MyCTE MC
	where exists (select * from dbo.exeExercises where Id = MC.Id)
)
select top(1) @Id = Id 
from MyCTE MC
order by MC.Attempt desc;

return @Id;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetNewExerciseId] TO [websiterole]
    AS [dbo];

