





create VIEW [dbo].[sysRandomTenDigitPositiveInt]
AS
/*
20141109 AF
We are not allowed to call crypt_gen_random() from within a scalar-valued function. The error is "Invalid use of a side-effecting operator 'Crypt_Gen_Random' within a function."
However a view can call a side-effect function and we can call a view from within a function. We use this view as a workaround.
-- Max positive int value is 2147483647. To keep uniform distribution the modulo operator should undiscriminatelly cover the full range. / 2 = 1073741823 
*/
select abs(convert(int, crypt_gen_random(4))) % 1073741823 + 1073741823 as Random