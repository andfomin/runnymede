CREATE PROCEDURE [dbo].[sysInitializeLibDescriptions] 
AS
BEGIN
SET NOCOUNT ON;

/*
select
'insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values ('
+ cast(Id as nvarchar(50)) +  ', '
+ cast(TitleId as nvarchar(50)) +  ', '
+ coalesce('''' + CategoryIds + '''', 'null') + ', '
+ '''' + SourceId + ''', '
+ cast(HasExplanation as nvarchar(1)) +  ', '
+ cast(HasExample as nvarchar(1)) +  ', '
+ cast(HasExercise as nvarchar(1)) +  ', '
+ cast(HasText as nvarchar(1)) +  ', '
+ cast(HasPicture as nvarchar(1)) +  ', '
+ cast(HasAudio as nvarchar(1)) +  ', '
+ cast(HasVideo as nvarchar(1)) +  ');'
from dbo.libDescriptions
where SourceId in ('BCEQ', 'BCEG')
order by Id;

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

set identity_insert dbo.libDescriptions on;

insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (1, 7416639458334771266, '003_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (2, -8680246132519874907, '004_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (3, 5702690858243800613, '005_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (4, -5002929643886824863, '006_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (5, 7836254309288466957, '007_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (6, 8759755828286356799, '008_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (7, 1788284944448409162, '009_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (8, 6753848695597600676, '010_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (9, 6770884126306592066, '011_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (10, 43616310597398454, '012_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (11, -8120788868590283338, '013_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (12, -274167051874833440, '014_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (13, 9182050378239210917, '015_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (14, -228844091874440201, '016_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (15, -7686909544431825130, '017_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (16, -4526199655317884568, '018_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (17, 6751896523708426138, '019_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (18, 3364318309145323469, '020_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (19, 8694731109662868673, '021a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (20, -1531874002592340322, '021b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (21, 5003276059054196313, '022_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (22, 3221422838569585831, '023_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (23, -3979728403063531583, '024_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (24, 4306935786110469753, '025_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (25, -7639028510344422999, '026_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (26, -2491473138775331183, '027_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (27, -2402107680041752072, '028a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (28, 256715726021397072, '028b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (29, 6463447341635942984, '029a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (30, 9016378555826639551, '029b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (31, -3081709895901760775, '030_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (32, -5479309793246422225, '031_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (33, 7274430355298382458, '032_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (34, 5013597848281289964, '033a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (35, -6661194178216229816, '033b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (36, -583728343050767954, '034a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (37, 7983690615715284712, '034b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (38, -1529717128386441586, '034c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (39, -3118531843585442935, '035_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (40, -6896443318216500794, '036_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (41, 4132114942321708598, '037_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (42, -3852874651877188209, '038_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (43, -7714496603488755026, '040a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (44, -1683137454300555118, '040b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (45, 6409407496691060353, '041a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (46, -8326642157107327562, '041b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (47, 3525618347300530855, '042a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (48, 1083337306204125835, '042b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (49, 1716955901124299789, '042c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (50, 188295261074535354, '042d', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (51, -5619979841503474714, '043a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (52, -6843794530889350413, '043b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (53, -688018299871995911, '043c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (54, -7298074511949858644, '043d', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (55, 1066668643376387534, '044a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (56, 5909351351333357261, '044b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (57, -4252858744934789960, '044c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (58, -3048091070961142723, '045_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (59, -2790035044616702902, '047_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (60, -5138972509311419468, '048a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (61, 5465936077879235865, '048b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (62, -8675390058912746079, '048c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (63, 8242145677599808833, '049_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (64, 4154979169707009739, '050_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (65, -5728357618673336993, '051_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (66, 7697821552869570890, '052_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (67, 6848269571089141256, '053_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (68, -7846458793823402387, '054_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (69, 4690774284185228958, '056_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (70, -6740449066197505183, '057_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (71, 2887257009990012393, '058_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (72, -2054386740226670768, '060_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (73, -8090157946838125029, '061_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (74, -5994114394662101010, '062_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (75, -1122639844157929294, '064_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (76, -6818791449015808895, '065_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (77, 105544948524000807, '067a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (78, -5984492765712832375, '067b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (79, 3340436072086282373, '067c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (80, -3688949115739695710, '068a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (81, 7796577791389348852, '068b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (82, 6485184588789399856, '069a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (83, 6153769761824008038, '069b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (84, 8271483057824164855, '070a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (85, 7089998647883989789, '070b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (86, 1491219466198181203, '071a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (87, -5452493219196891215, '071b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (88, -8288207501593397296, '072a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (89, 815505597383446918, '072b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (90, 6624302949427388108, '074_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (91, -3966066913872207461, '075_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (92, 4489305899644438673, '076a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (93, 5873760822419484001, '076b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (94, -8017468920234059223, '077a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (95, -8603804619585674901, '077b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (96, 3923804142791231250, '078a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (97, 6783346585337264077, '078b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (98, -3420381538091235011, '079a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (99, -90874572973706625, '079b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (100, 7432924408131946033, '081_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (101, -5577810732172678280, '082_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (102, -2149070210286016029, '083_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (103, 434993705376447091, '085_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (104, -2714610332882235823, '086_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (105, 2585265068461807553, '086a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (106, -525160243177308999, '086b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (107, 8603604296266708293, '087_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (108, -3320957142568909335, '088_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (109, 6358340207123665652, '090_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (110, 8442513246160048406, '091_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (111, -2329213737876091187, '092a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (112, -7708362328053160663, '092b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (113, 3686164986218312766, '093a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (114, -116662130313560846, '093b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (115, -2973344025736463521, '095_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (116, -995537959680572866, '096_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (117, -8237042830066733658, '096a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (118, -3062983731527429559, '098_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (119, -642700977890738808, '099_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (120, 9012879036803591098, '101_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (121, -8898642122396178432, '102_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (122, 1111562734526714548, '104_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (123, -4051936302726007450, '105_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (124, -6033825621118189026, '107_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (125, 5977966951569439604, '108_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (126, 4596013362480371913, '109_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (127, -5791309909035461552, '110_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (128, 5072255204405174626, '112_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (129, 204802640528783177, '113_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (130, 4764841491322736678, '114_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (131, 4725962075042364455, '115_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (132, 5215655926891542183, '116_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (133, -7154065948265689649, '117_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (134, 5530577301023598858, '119_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (135, 1269842082127312827, '120_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (136, 1798191288027405287, '122a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (137, 3845995624298539947, '122b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (138, 388204091351770093, '123_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (139, -6757637426049951294, '125_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (140, -2319273093541155115, '127_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (141, 8554682737201882750, '128_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (142, 5513550786850135537, '129_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (143, -5656705582964372870, '131_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (144, -7429001834936418592, '132_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (145, 6005020894420681044, '132a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (146, 6553016707619376292, '133_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (147, -8025858027268559047, '133a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (148, -8608238679074527334, '135_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (149, 2434787268009732957, '136_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (150, -5792892074219216769, '137_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (151, -4202082133772585032, '138_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (152, 4776001172529276544, '139_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (153, 7152859233284181874, '141_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (154, -8441796772822133877, '142_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (155, 7113073016077110033, '143_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (156, 512108315461030874, '145_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (157, 8300199914744661609, '146_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (158, -955952142274262631, '147_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (159, -3549826038382058267, '148_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (160, -2334433310549319195, '149a', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (161, 6151699190506960775, '149b', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (162, 6046903774730667629, '149c', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (163, -702030645702755615, '150_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (164, 6266458773635537094, '152_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (165, 852164672913395011, '153_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (166, 3340089687907842354, '154_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (167, -7913324192584477961, '155_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (168, -8105764012755022702, '156_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (169, -8999383102668879578, '157_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (170, -4532053765753412866, '158_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (171, -6127992302490396264, '159_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (172, 272165786783113972, '161_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (173, 7657748372801509682, '162_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (174, -3353349153972948000, '163_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (175, -7743378854516121670, '164_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (176, -2106208847834064990, '165_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (177, 7951767259701783085, '167_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (178, 5645950323007848837, '168_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (179, 1529388503704957612, '169_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (180, 880319149921203195, '170_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (181, -5917722104707679806, '171_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (182, -4298381228413781977, '172_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (183, 3499889591571523965, '173_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (184, 3504002244196771857, '174_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (185, 1209052600905417242, '175_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (186, 2450374870123398711, '176_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (187, -1119846029547564569, '177_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (188, 9002374801877103957, '178_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (189, 9155073269319630450, '179_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (190, 2837363255886499877, '180_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (191, -7169977731791488229, '181_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (192, 4236462640181055420, '182_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (193, -2295312313798414882, '183_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (194, 8520208688092890985, '184_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (195, 8189211518540843399, '186_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (196, -656817059570134343, '187_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (197, 688644789301603355, '188_', 'BCEQ', 0, 1, 0, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (198, -4376684851069895853, '145_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (199, 1776559653752900257, '147_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (200, -6892438140754050377, '149a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (201, -4055692037663420660, '164_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (202, -3799789388523732190, '162_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (203, -3082104361793998879, '412_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (204, -2077305038012185479, '148_ 420_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (205, 163523494587672411, '151_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (206, -5748392531990375412, '153_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (207, -2877061621297261953, '153_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (208, -2485965731821162130, '153_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (209, 6526917196365239598, '153_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (210, 7061527993945665645, '156_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (211, 7711249388923056480, '153_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (212, 7086237967054811244, '154_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (213, -1474122481065733122, '154_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (214, -5139068008030344851, '152_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (215, 7660070115915129860, '154_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (216, 8544411564920526456, '153_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (217, 105359387578957033, '157_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (218, 7913342238390512955, '155_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (219, 4870183890894401625, '157_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (220, 3870643189295366866, '421_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (221, 7401164959071304534, '421_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (222, 6539309548241538582, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (223, 5102624670481437214, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (224, 8022711065370850774, '421_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (225, -4076022824133266968, '420_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (226, 2921920492011470730, '420_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (227, -6409642195233506000, '421_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (228, 5190788971902491722, '421_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (229, 318782680002457817, '425_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (230, 6583523966790419698, '426_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (231, -5297139540768333398, '422_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (232, 866267687879876594, '423_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (233, 2738200811726241811, '102_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (234, 57910377699789093, '101_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (235, 6083465559708734317, '101_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (236, 1192345324183040172, '422_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (237, 6670532070512860836, '421_ 096a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (238, -8705094263899702237, '421_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (239, -4810377706346120904, '088_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (240, -5818586837267774562, '086_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (241, 4904265964443424732, '060_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (242, 7324669989542256371, '060_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (243, 535671556595398224, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (244, -3330055583389342665, '143_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (245, -5162207976756006370, '141_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (246, 8773966455132076215, '141_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (247, -4010765184449565102, '141_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (248, -7858600600611392958, '143_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (249, 20410865714303934, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (250, 6308234464969266770, '122b', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (251, -7608613800090868192, '122b', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (252, 2336795450289762633, '413_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (253, -7621491276677511639, '122b', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (254, 5928345312414544000, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (255, 8495537207971508947, '127_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (256, 5579072601819551705, '128_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (257, 3543813131459625752, '129_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (258, 7577505250491829941, '129_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (259, -460312632350630470, '129_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (260, 8401609786624936055, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (261, 4926989009583169816, '427_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (262, -1852178594486107890, '125_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (263, 1143851644121411701, '125_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (264, 5890994332526231891, '125_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (265, 2800453231821919763, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (266, -6142505955309605584, '415_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (267, -6809603584569075364, '414_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (268, 9130598162915652921, '417_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (269, 5690970685417652049, '416_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (270, -5838999736284086835, '125_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (271, 1143475734355541106, '125_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (272, 22557937045427206, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (273, -6553673245874340831, '099_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (274, -8282409410030279170, '065_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (275, 5376723943302752424, '418_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (276, 866646141503567140, '426_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (277, 8673649159284105110, '087_ 088_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (278, -6831953709769869832, '086_ 086a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (279, 3324804866351890717, '429_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (280, -8543292225306042487, '423_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (281, 9110904921500813289, null, 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (282, 6055261883358910443, '104_ 105_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (283, 4893057613763276246, '105_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (284, 8820770485463850736, '104_ 105_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (285, 2873883792322800198, '109_ 119_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (286, -3938926707911534027, '105_ 107_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (287, 4411964689608660078, '119_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (288, -6810585368598771754, '119_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (289, -4520636788036197773, '109_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (290, 7071421500392417072, '067a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (291, -4494843726450416420, '068a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (292, 2783564482955662593, '071a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (293, 2474716739342180713, '067a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (294, 5774223813224379939, '081_ 071a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (295, 9090181830020182260, '096a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (296, -7052503686635105117, '064_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (297, -7813504156500126756, '065_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (298, 2622244909148029027, '081_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (299, -2126921903504439393, '064_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (300, 1824948355973694898, '060_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (301, 5742378761142572114, '428_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (302, 8438054343721658736, '074_ 075_ 076a 076b', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (303, -183401239254807536, '067b', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (304, 5187773225656893207, '064_ 065_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (305, 8080676687582593234, '056_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (306, 8570745242089752144, '420_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (307, -1072210289414486855, '088_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (308, 917712351600161193, '086a', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (309, 6974841004752042153, '430_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (310, 1404367971906918262, '090_', 'BCEG', 1, 1, 1, 1, 0, 0, 0);
insert dbo.libDescriptions (Id, TitleId, CategoryIds, SourceId, HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo) values (311, 1323854411912397648, '093b', 'BCEG', 1, 1, 1, 1, 0, 0, 0);

set identity_insert dbo.libDescriptions off;

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