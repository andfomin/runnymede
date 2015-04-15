

CREATE FUNCTION [dbo].[exeCalculateReviewPrice]
(
	@ExerciseType char(6),
	@Length int
)
RETURNS decimal(9,2)
AS
BEGIN
/*
20140807 AF
20141230 AF Added writing and refactored
Mirrors app.exercises.CreateReviewRequestModal.calcPrice() in app/exercises/createReviewRequest.ts. Refactor in both places.
Length is milliseconds/words. Rate is $/min / $/100words. Minimal price is for 1 minute / 100 words. Round down to full cents.
20150313
@Rate is calculated within this procedure.
*/

declare @Rate decimal(9,2) = dbo.appGetConstantAsFloat('Exercises.ReviewRate.' + @ExerciseType);

declare @unit float = 
	case 
		when (dbo.exeIsTypeRecording(@ExerciseType) = 1) then 60000.0
		when (dbo.exeIsTypeWriting(@ExerciseType) = 1) then 100.0
		else null
	end;

declare @units float = @Length / @unit;

declare @price float = iif((@units > 1 or @units is null), @units, 1) * @Rate;
declare @cents float = floor(@price * 100);
declare @result decimal(9,2) = @cents / 100;

--select @units as units, @price as price, @cents as cents, @result as result; 
return @result;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeCalculateReviewPrice] TO [websiterole]
    AS [dbo];

