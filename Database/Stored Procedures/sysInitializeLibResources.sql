CREATE PROCEDURE [dbo].[sysInitializeLibResources] 
AS
BEGIN
SET NOCOUNT ON;

/*
select
'insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values ('
+ cast((1000000000 + DescriptionId) as nvarchar(50)) +  ', '
+ '''' + [Format] + ''', '
+ '''' + NaturalKey + ''', '
+ '1, '
+ cast(DescriptionId as nvarchar(50)) +  ', '
+ '1);'
from dbo.libResources
where [Format] in ('FRCIEX', 'FRBCEG')
order by DescriptionId;

If using the Generate Script Wizard, watch for GO commands. They are not SQL and they switch mode to execution if occur in a stored procedure code on saving.
*/

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);
--raiserror('%s: ', 16, 1, @ProcName);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000001, 'FRCIEX', '003_', 1, 1, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000002, 'FRCIEX', '004_', 1, 2, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000003, 'FRCIEX', '005_', 1, 3, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000004, 'FRCIEX', '006_', 1, 4, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000005, 'FRCIEX', '007_', 1, 5, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000006, 'FRCIEX', '008_', 1, 6, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000007, 'FRCIEX', '009_', 1, 7, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000008, 'FRCIEX', '010_', 1, 8, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000009, 'FRCIEX', '011_', 1, 9, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000010, 'FRCIEX', '012_', 1, 10, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000011, 'FRCIEX', '013_', 1, 11, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000012, 'FRCIEX', '014_', 1, 12, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000013, 'FRCIEX', '015_', 1, 13, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000014, 'FRCIEX', '016_', 1, 14, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000015, 'FRCIEX', '017_', 1, 15, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000016, 'FRCIEX', '018_', 1, 16, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000017, 'FRCIEX', '019_', 1, 17, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000018, 'FRCIEX', '020_', 1, 18, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000019, 'FRCIEX', '021a', 1, 19, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000020, 'FRCIEX', '021b', 1, 20, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000021, 'FRCIEX', '022_', 1, 21, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000022, 'FRCIEX', '023_', 1, 22, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000023, 'FRCIEX', '024_', 1, 23, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000024, 'FRCIEX', '025_', 1, 24, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000025, 'FRCIEX', '026_', 1, 25, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000026, 'FRCIEX', '027_', 1, 26, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000027, 'FRCIEX', '028a', 1, 27, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000028, 'FRCIEX', '028b', 1, 28, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000029, 'FRCIEX', '029a', 1, 29, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000030, 'FRCIEX', '029b', 1, 30, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000031, 'FRCIEX', '030_', 1, 31, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000032, 'FRCIEX', '031_', 1, 32, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000033, 'FRCIEX', '032_', 1, 33, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000034, 'FRCIEX', '033a', 1, 34, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000035, 'FRCIEX', '033b', 1, 35, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000036, 'FRCIEX', '034a', 1, 36, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000037, 'FRCIEX', '034b', 1, 37, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000038, 'FRCIEX', '034c', 1, 38, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000039, 'FRCIEX', '035_', 1, 39, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000040, 'FRCIEX', '036_', 1, 40, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000041, 'FRCIEX', '037_', 1, 41, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000042, 'FRCIEX', '038_', 1, 42, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000043, 'FRCIEX', '040a', 1, 43, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000044, 'FRCIEX', '040b', 1, 44, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000045, 'FRCIEX', '041a', 1, 45, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000046, 'FRCIEX', '041b', 1, 46, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000047, 'FRCIEX', '042a', 1, 47, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000048, 'FRCIEX', '042b', 1, 48, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000049, 'FRCIEX', '042c', 1, 49, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000050, 'FRCIEX', '042d', 1, 50, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000051, 'FRCIEX', '043a', 1, 51, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000052, 'FRCIEX', '043b', 1, 52, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000053, 'FRCIEX', '043c', 1, 53, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000054, 'FRCIEX', '043d', 1, 54, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000055, 'FRCIEX', '044a', 1, 55, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000056, 'FRCIEX', '044b', 1, 56, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000057, 'FRCIEX', '044c', 1, 57, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000058, 'FRCIEX', '045_', 1, 58, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000059, 'FRCIEX', '047_', 1, 59, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000060, 'FRCIEX', '048a', 1, 60, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000061, 'FRCIEX', '048b', 1, 61, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000062, 'FRCIEX', '048c', 1, 62, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000063, 'FRCIEX', '049_', 1, 63, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000064, 'FRCIEX', '050_', 1, 64, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000065, 'FRCIEX', '051_', 1, 65, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000066, 'FRCIEX', '052_', 1, 66, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000067, 'FRCIEX', '053_', 1, 67, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000068, 'FRCIEX', '054_', 1, 68, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000069, 'FRCIEX', '056_', 1, 69, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000070, 'FRCIEX', '057_', 1, 70, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000071, 'FRCIEX', '058_', 1, 71, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000072, 'FRCIEX', '060_', 1, 72, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000073, 'FRCIEX', '061_', 1, 73, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000074, 'FRCIEX', '062_', 1, 74, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000075, 'FRCIEX', '064_', 1, 75, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000076, 'FRCIEX', '065_', 1, 76, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000077, 'FRCIEX', '067a', 1, 77, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000078, 'FRCIEX', '067b', 1, 78, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000079, 'FRCIEX', '067c', 1, 79, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000080, 'FRCIEX', '068a', 1, 80, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000081, 'FRCIEX', '068b', 1, 81, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000082, 'FRCIEX', '069a', 1, 82, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000083, 'FRCIEX', '069b', 1, 83, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000084, 'FRCIEX', '070a', 1, 84, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000085, 'FRCIEX', '070b', 1, 85, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000086, 'FRCIEX', '071a', 1, 86, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000087, 'FRCIEX', '071b', 1, 87, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000088, 'FRCIEX', '072a', 1, 88, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000089, 'FRCIEX', '072b', 1, 89, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000090, 'FRCIEX', '074_', 1, 90, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000091, 'FRCIEX', '075_', 1, 91, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000092, 'FRCIEX', '076a', 1, 92, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000093, 'FRCIEX', '076b', 1, 93, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000094, 'FRCIEX', '077a', 1, 94, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000095, 'FRCIEX', '077b', 1, 95, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000096, 'FRCIEX', '078a', 1, 96, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000097, 'FRCIEX', '078b', 1, 97, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000098, 'FRCIEX', '079a', 1, 98, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000099, 'FRCIEX', '079b', 1, 99, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000100, 'FRCIEX', '081_', 1, 100, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000101, 'FRCIEX', '082_', 1, 101, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000102, 'FRCIEX', '083_', 1, 102, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000103, 'FRCIEX', '085_', 1, 103, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000104, 'FRCIEX', '086_', 1, 104, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000105, 'FRCIEX', '086a', 1, 105, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000106, 'FRCIEX', '086b', 1, 106, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000107, 'FRCIEX', '087_', 1, 107, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000108, 'FRCIEX', '088_', 1, 108, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000109, 'FRCIEX', '090_', 1, 109, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000110, 'FRCIEX', '091_', 1, 110, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000111, 'FRCIEX', '092a', 1, 111, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000112, 'FRCIEX', '092b', 1, 112, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000113, 'FRCIEX', '093a', 1, 113, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000114, 'FRCIEX', '093b', 1, 114, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000115, 'FRCIEX', '095_', 1, 115, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000116, 'FRCIEX', '096_', 1, 116, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000117, 'FRCIEX', '096a', 1, 117, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000118, 'FRCIEX', '098_', 1, 118, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000119, 'FRCIEX', '099_', 1, 119, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000120, 'FRCIEX', '101_', 1, 120, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000121, 'FRCIEX', '102_', 1, 121, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000122, 'FRCIEX', '104_', 1, 122, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000123, 'FRCIEX', '105_', 1, 123, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000124, 'FRCIEX', '107_', 1, 124, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000125, 'FRCIEX', '108_', 1, 125, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000126, 'FRCIEX', '109_', 1, 126, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000127, 'FRCIEX', '110_', 1, 127, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000128, 'FRCIEX', '112_', 1, 128, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000129, 'FRCIEX', '113_', 1, 129, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000130, 'FRCIEX', '114_', 1, 130, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000131, 'FRCIEX', '115_', 1, 131, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000132, 'FRCIEX', '116_', 1, 132, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000133, 'FRCIEX', '117_', 1, 133, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000134, 'FRCIEX', '119_', 1, 134, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000135, 'FRCIEX', '120_', 1, 135, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000136, 'FRCIEX', '122a', 1, 136, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000137, 'FRCIEX', '122b', 1, 137, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000138, 'FRCIEX', '123_', 1, 138, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000139, 'FRCIEX', '125_', 1, 139, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000140, 'FRCIEX', '127_', 1, 140, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000141, 'FRCIEX', '128_', 1, 141, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000142, 'FRCIEX', '129_', 1, 142, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000143, 'FRCIEX', '131_', 1, 143, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000144, 'FRCIEX', '132_', 1, 144, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000145, 'FRCIEX', '132a', 1, 145, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000146, 'FRCIEX', '133_', 1, 146, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000147, 'FRCIEX', '133a', 1, 147, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000148, 'FRCIEX', '135_', 1, 148, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000149, 'FRCIEX', '136_', 1, 149, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000150, 'FRCIEX', '137_', 1, 150, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000151, 'FRCIEX', '138_', 1, 151, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000152, 'FRCIEX', '139_', 1, 152, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000153, 'FRCIEX', '141_', 1, 153, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000154, 'FRCIEX', '142_', 1, 154, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000155, 'FRCIEX', '143_', 1, 155, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000156, 'FRCIEX', '145_', 1, 156, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000157, 'FRCIEX', '146_', 1, 157, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000158, 'FRCIEX', '147_', 1, 158, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000159, 'FRCIEX', '148_', 1, 159, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000160, 'FRCIEX', '149a', 1, 160, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000161, 'FRCIEX', '149b', 1, 161, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000162, 'FRCIEX', '149c', 1, 162, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000163, 'FRCIEX', '150_', 1, 163, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000164, 'FRCIEX', '152_', 1, 164, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000165, 'FRCIEX', '153_', 1, 165, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000166, 'FRCIEX', '154_', 1, 166, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000167, 'FRCIEX', '155_', 1, 167, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000168, 'FRCIEX', '156_', 1, 168, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000169, 'FRCIEX', '157_', 1, 169, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000170, 'FRCIEX', '158_', 1, 170, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000171, 'FRCIEX', '159_', 1, 171, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000172, 'FRCIEX', '161_', 1, 172, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000173, 'FRCIEX', '162_', 1, 173, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000174, 'FRCIEX', '163_', 1, 174, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000175, 'FRCIEX', '164_', 1, 175, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000176, 'FRCIEX', '165_', 1, 176, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000177, 'FRCIEX', '167_', 1, 177, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000178, 'FRCIEX', '168_', 1, 178, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000179, 'FRCIEX', '169_', 1, 179, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000180, 'FRCIEX', '170_', 1, 180, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000181, 'FRCIEX', '171_', 1, 181, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000182, 'FRCIEX', '172_', 1, 182, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000183, 'FRCIEX', '173_', 1, 183, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000184, 'FRCIEX', '174_', 1, 184, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000185, 'FRCIEX', '175_', 1, 185, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000186, 'FRCIEX', '176_', 1, 186, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000187, 'FRCIEX', '177_', 1, 187, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000188, 'FRCIEX', '178_', 1, 188, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000189, 'FRCIEX', '179_', 1, 189, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000190, 'FRCIEX', '180_', 1, 190, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000191, 'FRCIEX', '181_', 1, 191, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000192, 'FRCIEX', '182_', 1, 192, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000193, 'FRCIEX', '183_', 1, 193, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000194, 'FRCIEX', '184_', 1, 194, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000195, 'FRCIEX', '186_', 1, 195, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000196, 'FRCIEX', '187_', 1, 196, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000197, 'FRCIEX', '188_', 1, 197, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000198, 'FRBCEG', 'adjectives', 1, 198, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000199, 'FRBCEG', 'adjectives/adjectives-ed-and-ing', 1, 199, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000200, 'FRBCEG', 'adjectives/comparative-and-superlative-adjectives', 1, 200, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000201, 'FRBCEG', 'adjectives/intensifiers', 1, 201, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000202, 'FRBCEG', 'adjectives/mitigators', 1, 202, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000203, 'FRBCEG', 'adjectives/noun-modifiers', 1, 203, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000204, 'FRBCEG', 'adjectives/order-adjectives', 1, 204, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000205, 'FRBCEG', 'adverbials', 1, 205, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000206, 'FRBCEG', 'adverbials/adverbials-place', 1, 206, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000207, 'FRBCEG', 'adverbials/adverbials-place/adverbials-direction', 1, 207, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000208, 'FRBCEG', 'adverbials/adverbials-place/adverbials-distance', 1, 208, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000209, 'FRBCEG', 'adverbials/adverbials-place/adverbials-location', 1, 209, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000210, 'FRBCEG', 'adverbials/adverbials-probability', 1, 210, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000211, 'FRBCEG', 'adverbials/adverbials-time', 1, 211, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000212, 'FRBCEG', 'adverbials/adverbials-time/already-still-yet-and-no-longer', 1, 212, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000213, 'FRBCEG', 'adverbials/adverbials-time/how-long', 1, 213, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000214, 'FRBCEG', 'adverbials/adverbials-time/how-often', 1, 214, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000215, 'FRBCEG', 'adverbials/adverbials-time/time-and-dates', 1, 215, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000216, 'FRBCEG', 'adverbials/adverbs-manner', 1, 216, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000217, 'FRBCEG', 'adverbials/comparative-adverbs', 1, 217, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000218, 'FRBCEG', 'adverbials/how-we-make-adverbials', 1, 218, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000219, 'FRBCEG', 'adverbials/superlative-adverbs', 1, 219, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000220, 'FRBCEG', 'adverbials/where-they-go-sentence', 1, 220, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000221, 'FRBCEG', 'clause-phrase-and-sentence', 1, 221, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000222, 'FRBCEG', 'clause-phrase-and-sentence/adjective-phrases', 1, 222, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000223, 'FRBCEG', 'clause-phrase-and-sentence/adverbial-phrases', 1, 223, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000224, 'FRBCEG', 'clause-phrase-and-sentence/clause-structure', 1, 224, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000225, 'FRBCEG', 'clause-phrase-and-sentence/noun-phrase', 1, 225, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000226, 'FRBCEG', 'clause-phrase-and-sentence/prepositional-phrases', 1, 226, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000227, 'FRBCEG', 'clause-phrase-and-sentence/sentence-structure', 1, 227, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000228, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns', 1, 228, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000229, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/clauses-short-forms', 1, 229, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000230, 'FRBCEG', 'double-object-verbs', 1, 230, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000231, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/intransitive-verbs', 1, 231, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000232, 'FRBCEG', 'link-verbs', 1, 232, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000233, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/relative-clauses', 1, 233, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000234, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/reporting-reports-and-summaries', 1, 234, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000235, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/reporting-verbs-that-wh-and-if-clauses', 1, 235, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000236, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/transitive-verbs', 1, 236, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000237, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/two-and-three-part-verbs', 1, 237, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000238, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/verb-patterns-adverbials', 1, 238, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000239, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/verbs-infinitive', 1, 239, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000240, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/verbs-ing-forms', 1, 240, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000241, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/verbs-questions-and-negatives', 1, 241, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000242, 'FRBCEG', 'clause-phrase-and-sentence/verb-patterns/wh-clauses', 1, 242, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000243, 'FRBCEG', 'clause-phrase-and-sentence/verb-phrase', 1, 243, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000244, 'FRBCEG', 'determiners-and-quantifiers', 1, 244, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000245, 'FRBCEG', 'determiners-and-quantifiers/definite-article', 1, 245, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000246, 'FRBCEG', 'determiners-and-quantifiers/indefinite-article-and', 1, 246, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000247, 'FRBCEG', 'determiners-and-quantifiers/interrogative-determiners-which-and-what', 1, 247, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000248, 'FRBCEG', 'determiners-and-quantifiers/quantifiers', 1, 248, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000249, 'FRBCEG', 'nouns', 1, 249, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000250, 'FRBCEG', 'nouns/common-problems-countuncount-nouns', 1, 250, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000251, 'FRBCEG', 'nouns/count-nouns', 1, 251, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000252, 'FRBCEG', 'nouns/proper-nouns', 1, 252, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000253, 'FRBCEG', 'nouns/uncount-nouns', 1, 253, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000254, 'FRBCEG', 'possessives', 1, 254, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000255, 'FRBCEG', 'possessives/possessives-adjectives', 1, 255, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000256, 'FRBCEG', 'possessives/possessives-nouns', 1, 256, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000257, 'FRBCEG', 'possessives/possessives-pronouns', 1, 257, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000258, 'FRBCEG', 'possessives/possessives-questions', 1, 258, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000259, 'FRBCEG', 'possessives/possessives-reciprocal-pronouns', 1, 259, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000260, 'FRBCEG', 'pronouns', 1, 260, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000261, 'FRBCEG', 'pronouns/indefinite-pronouns', 1, 261, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000262, 'FRBCEG', 'pronouns/it-and-there', 1, 262, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000263, 'FRBCEG', 'pronouns/one-and-ones', 1, 263, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000264, 'FRBCEG', 'pronouns/personal-pronouns', 1, 264, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000265, 'FRBCEG', 'pronouns/possessive-pronouns-see-possessives-pronouns', 1, 265, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000266, 'FRBCEG', 'pronouns/questions', 1, 266, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000267, 'FRBCEG', 'pronouns/reciprocal-pronouns-each-other-and-one-another', 1, 267, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000268, 'FRBCEG', 'pronouns/reflexive-pronouns', 1, 268, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000269, 'FRBCEG', 'pronouns/relative-pronouns', 1, 269, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000270, 'FRBCEG', 'pronouns/that-these-and-those', 1, 270, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000271, 'FRBCEG', 'pronouns/you-and-they', 1, 271, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000272, 'FRBCEG', 'verbs', 1, 272, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000273, 'FRBCEG', 'verbs/active-and-passive-voice', 1, 273, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000274, 'FRBCEG', 'verbs/continuous-aspect', 1, 274, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000275, 'FRBCEG', 'verbs/delexical-verbs-have-take-make-and-give', 1, 275, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000276, 'FRBCEG', 'verbs/double-object-verbs', 1, 276, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000277, 'FRBCEG', 'verbs/infinitive', 1, 277, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000278, 'FRBCEG', 'verbs-ing-forms', 1, 278, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000279, 'FRBCEG', 'verbs/irregular-verbs', 1, 279, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000280, 'FRBCEG', 'verbs/link-verbs', 1, 280, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000281, 'FRBCEG', 'verbs/modal-verbs', 1, 281, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000282, 'FRBCEG', 'verbs/modal-verbs/ability-permission-requests-and-advice', 1, 282, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000283, 'FRBCEG', 'verbs/modal-verbs/can-could-and-could-have', 1, 283, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000284, 'FRBCEG', 'verbs/modal-verbs/can-or-could', 1, 284, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000285, 'FRBCEG', 'verbs/modal-verbs/certain-probable-or-possible', 1, 285, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000286, 'FRBCEG', 'verbs/modal-verbs/may-might-may-have-and-might-have', 1, 286, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000287, 'FRBCEG', 'verbs/modal-verbs/modals-have', 1, 287, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000288, 'FRBCEG', 'verbs/modal-verbs/will-have-or-would-have', 1, 288, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000289, 'FRBCEG', 'verbs/modal-verbs/will-or-would', 1, 289, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000290, 'FRBCEG', 'verbs/past-tense', 1, 290, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000291, 'FRBCEG', 'verbs/past-tense/past-continuous', 1, 291, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000292, 'FRBCEG', 'verbs/past-tense/past-perfect', 1, 292, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000293, 'FRBCEG', 'verbs/past-tense/past-simple', 1, 293, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000294, 'FRBCEG', 'verbs/perfective-aspect', 1, 294, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000295, 'FRBCEG', 'verbs/phrasal-verbs', 1, 295, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000296, 'FRBCEG', 'verbs/present-tense', 1, 296, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000297, 'FRBCEG', 'verbs/present-tense/present-continuous', 1, 297, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000298, 'FRBCEG', 'verbs/present-tense/present-perfect', 1, 298, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000299, 'FRBCEG', 'verbs/present-tense/present-simple', 1, 299, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000300, 'FRBCEG', 'verbs/question-forms', 1, 300, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000301, 'FRBCEG', 'verbs/reflexive-and-ergative-verbs', 1, 301, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000302, 'FRBCEG', 'verbs/talking-about-future', 1, 302, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000303, 'FRBCEG', 'verbs/talking-about-past', 1, 303, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000304, 'FRBCEG', 'verbs/talking-about-present', 1, 304, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000305, 'FRBCEG', 'verbs/verb-be', 1, 305, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000306, 'FRBCEG', 'verbs/verb-phrases', 1, 306, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000307, 'FRBCEG', 'verbs/verbs-followed-infinitive', 1, 307, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000308, 'FRBCEG', 'verbs/verbs-followed-ing-clauses', 1, 308, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000309, 'FRBCEG', 'verbs/verbs-followed-that-clause', 1, 309, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000310, 'FRBCEG', 'verbs/verbs-time-clauses-and-if-clauses', 1, 310, 1);
insert dbo.libResources (Id, [Format], NaturalKey, IsCommon, DescriptionId, UserId) values (1000000311, 'FRBCEG', 'verbs/wishes-and-hypotheses', 1, 311, 1);

	if @ExternalTran = 0
		commit;
end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback;
	if @XState = -1
		rollback;
	throw;
end catch

END