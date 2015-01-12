


CREATE FUNCTION [dbo].[exeGetAnyTeacherReviewRate]
(
	@ExerciseType char(6)
)
RETURNS decimal(9,2)
AS
BEGIN
/*
AF 20140806
This function is called twice, as review conditions before a review requested and then on the review request. 
It must be determined, i.e. return the same value for same arguments.
*/

-- We produce here a pseudo-random value. The range is 100...300
--return convert(decimal(9,2), abs((sin(@UserId) + sin(@ExerciseId))) * 100 + 100);

return dbo.appGetConstantAsFloat('AnyTeacher.ReviewRate.' + @ExerciseType);

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetAnyTeacherReviewRate] TO [websiterole]
    AS [dbo];

