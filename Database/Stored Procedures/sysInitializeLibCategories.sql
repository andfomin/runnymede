﻿CREATE PROCEDURE [dbo].[sysInitializeLibCategories] 
AS
BEGIN
SET NOCOUNT ON;

/*
select
'insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values (' 
+ '''' + Id + ''', '
+ coalesce('''' + ParentId + '''', 'null') + ', '
+ 'N''' + replace(Name, '''', '''''') + ''', '
+ coalesce(cast(Position as nvarchar(4)), 'null') +  ', '
+ coalesce('''' + FeatureCode + '''', 'null') + ', '
+ cast(Display as nvarchar(1)) +  ')'
from dbo.libCategories;

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

insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('000A', null, N'Communication', 3, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('000B', null, N'Grammar', 1, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('000C', null, N'Vocabulary', 2, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('000D', null, N'Media', 5, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('000E', null, N'Pronunciation', 4, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('002_', '000A', N'Functions', 1, '2', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('003_', '002_', N'Using numbers', null, '3', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('004_', '002_', N'Using prices', null, '4', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('005_', '002_', N'Telling the time', null, '5', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('006_', '002_', N'Directions', null, '6', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('007_', '002_', N'Greetings', null, '7', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('008_', '002_', N'Giving personal information', null, '8', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('009_', '002_', N'Describing habits and routines', null, '9', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('010_', '002_', N'Describing people', null, '10', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('011_', '002_', N'Describing things', null, '11', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('012_', '002_', N'Requests', null, '12', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('013_', '002_', N'Suggestions', null, '13', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('014_', '002_', N'Advice', null, '14', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('015_', '002_', N'Invitations', null, '15', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('016_', '002_', N'Offers', null, '16', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('017_', '002_', N'Arrangements to meet people', null, '17', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('018_', '002_', N'Obligations and necessity', null, '18', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('019_', '002_', N'Describing places', null, '19', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('020_', '002_', N'Describing past experiences and storytelling', null, '20', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('021a', '002_', N'Describing feelings, emotions, attitudes', null, '21a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('021b', '002_', N'Describing feelings, emotion, attitudes precisely', null, '21b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('022_', '002_', N'Describing hopes and plans', null, '22', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('023_', '002_', N'Giving precise information', null, '23', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('024_', '002_', N'Expressing abstract ideas', null, '24', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('025_', '002_', N'Expressing certainty, probability, doubt', null, '25', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('026_', '002_', N'Generalising and qualifying', null, '26', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('027_', '002_', N'Synthesizing, evaluating, glossing information', null, '27', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('028a', '002_', N'Speculating', null, '28a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('028b', '002_', N'Speculating and hypothesising about causes, consequences etc.', null, '28b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('029a', '002_', N'Expressing opinions', null, '29a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('029b', '002_', N'Expressing opinions tentatively, hedging', null, '29b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('030_', '002_', N'Expressing shades of opinion and certainty', null, '30', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('031_', '002_', N'Expressing agreement/disagreement', null, '31', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('032_', '002_', N'Expressing reaction, e.g. indifference', null, '32', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('033a', '002_', N'Critiquing and reviewing. Talking about films and books', null, '33a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('033b', '002_', N'Critiquing and reviewing', null, '33b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('034a', '002_', N'Developing an argument', null, '34a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('034b', '002_', N'Developing an argument in academic discourse style', null, '34b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('034c', '002_', N'Developing an argument systematically', null, '34c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('035_', '002_', N'Conceding a point', null, '35', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('036_', '002_', N'Emphasizing a point, feeling, issue', null, '36', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('037_', '002_', N'Defending a point of view persuasively', null, '37', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('038_', '002_', N'Responding to counterarguments', null, '38', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('039_', '000A', N'Discourse Functions', 2, '39', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('040a', '039_', N'Initiating conversation', null, '40a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('040b', '039_', N'Closing conversation', null, '40b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('041a', '039_', N'Checking understanding from speaker''s point of view', null, '41a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('041b', '039_', N'Checking understanding from listener''s point of view', null, '41b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('042a', '039_', N'Managing interaction. Interrupting', null, '42a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('042b', '039_', N'Managing interaction. Changing the topic', null, '42b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('042c', '039_', N'Managing interaction. Resuming', null, '42c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('042d', '039_', N'Managing interaction. Continuing', null, '42d', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('043a', '039_', N'Taking the initiative in interaction. Control and delegation at start', null, '43a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('043b', '039_', N'Taking the initiative in interaction. During the meeting', null, '43b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('043c', '039_', N'Taking the initiative in interaction. Keeping interaction participants on topic', null, '43c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('043d', '039_', N'Taking initiative in non-control situation', null, '43d', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('044a', '039_', N'Encouraging another speaker to continue.', null, '44a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('044b', '039_', N'Inviting another speaker to come in in one-to-one interaction', null, '44b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('044c', '039_', N'Inviting another speaker to come in in group interaction.', null, '44c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('045_', '039_', N'Interacting informally, reacting, expressing interest, sympathy, surprise', null, '45', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('046_', '000C', N'Discourse Markers', 2, '46', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('047_', '046_', N'Connecting words (and, but, because)', null, '47', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('048a', '046_', N'Linkers: sequential - past time (first, finally)', null, '48a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('048b', '046_', N'Linkers: sequential - past time (later)', null, '48b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('048c', '046_', N'Linkers: sequential - past time (subsequently)', null, '48c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('049_', '046_', N'Connecting words expressing cause and effect, contrast etc.', null, '49', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('050_', '046_', N'Linkers: although, in spite of, despite', null, '50', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('051_', '046_', N'Linking devices: logical markers', null, '51', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('052_', '046_', N'Markers to structure informal spoken discourse', null, '52', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('053_', '046_', N'Discourse markers to structure formal speech', null, '53', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('054_', '046_', N'Markers to structure and signpost informal speech and writing', null, '54', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('055_', '000B', N'Simple Verb Forms', 1, '55', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('056_', '055_', N'To be (including questions and negatives)', null, '56', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('057_', '055_', N'Have got (British)', null, '57', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('058_', '055_', N'Imperatives (+/-)', null, '58', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('059_', '000B', N'Questions', 2, '59', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('060_', '059_', N'Questions forms', null, '60', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('061_', '059_', N'Wh-questions in the past', null, '61', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('062_', '059_', N'Complex question tags', null, '62', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('063_', '000B', N'Verbs: Present', 3, '63', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('064_', '063_', N'Present simple', null, '64', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('065_', '063_', N'Present continuous', null, '65', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('066_', '000B', N'Verbs: Past', 4, '66', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('067a', '066_', N'Past simple', null, '67a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('067b', '066_', N'Past simple (narrative)', null, '67b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('067c', '066_', N'Past simple (to be)', null, '67c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('068a', '066_', N'Past continuous', null, '68a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('068b', '066_', N'Past continuous (narrative)', null, '68b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('069a', '066_', N'Used to', null, '69a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('069b', '066_', N'Used to (narrative)', null, '69b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('070a', '066_', N'Would expressing habit in the past', null, '70a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('070b', '066_', N'Would expressing habit in the past (narrative)', null, '70b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('071a', '066_', N'Past perfect', null, '71a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('071b', '066_', N'Past perfect (narrative)', null, '71b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('072a', '066_', N'Past perfect continuous', null, '72a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('072b', '066_', N'Past perfect continuous (narrative)', null, '72b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('073_', '000B', N'Verbs: Future', 5, '73', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('074_', '073_', N'Going to', null, '74', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('075_', '073_', N'Present continuous for the future (arrangements)', null, '75', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('076a', '073_', N'Future time (will, going to)', null, '76a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('076b', '073_', N'Future time (will, going to) (Prediction)', null, '76b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('077a', '073_', N'Future continuous', null, '77a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('077b', '073_', N'Future continuous (Prediction)', null, '77b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('078a', '073_', N'Future perfect', null, '78a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('078b', '073_', N'Future perfect (Prediction)', null, '78b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('079a', '073_', N'Future perfect continuous', null, '79a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('079b', '073_', N'Future perfect continuous (Prediction)', null, '79b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('080_', '000B', N'Verbs: Present Perfect', 6, '80', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('081_', '080_', N'Present perfect', null, '81', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('082_', '080_', N'Present perfect, past simple', null, '82', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('083_', '080_', N'Present perfect continuous', null, '83', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('084_', '000B', N'Gerund and Infinitive', 7, '84', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('085_', '084_', N'I''d like', null, '85', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('086_', '084_', N'Gerunds', null, '86', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('086a', '084_', N'Verb + -ing (like/hate/love)', null, '86a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('086b', '084_', N'Verb + -ing/infinitive (like / want - would like)', null, '86b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('087_', '084_', N'To + infinitive (express purpose)', null, '87', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('088_', '084_', N'Verb + to + infinitive', null, '88', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('089_', '000B', N'Conditionals', 8, '89', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('090_', '089_', N'Zero and first conditional', null, '90', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('091_', '089_', N'Second and third conditional', null, '91', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('092a', '089_', N'Mixed conditionals', null, '92a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('092b', '089_', N'Mixed conditionals in the past, present and future', null, '92b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('093a', '089_', N'Wish', null, '93a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('093b', '089_', N'Wish / if only, regrets', null, '93b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('094_', '000B', N'Phrasal Verbs', 9, '94', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('095_', '094_', N'Common phrasal verbs', null, '95', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('096_', '094_', N'Extended phrasal verbs', null, '96', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('096a', '094_', N'Extended phrasal verbs (splitting)', null, '96a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('097_', '000B', N'Passives', 10, '97', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('098_', '097_', N'Simple passive', null, '98', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('099_', '097_', N'All passive forms', null, '99', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('100_', '000B', N'Other Verb Forms', 11, '100', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('101_', '100_', N'Reported speech', null, '101', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('102_', '100_', N'Relative clauses', null, '102', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('103_', '000B', N'Modals: Can', 12, '103', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('104_', '103_', N'Can/can''t (ability)', null, '104', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('105_', '103_', N'Can/could (functional)', null, '105', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('106_', '000B', N'Modals: Possibility', 13, '106', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('107_', '106_', N'Might, may', null, '107', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('108_', '106_', N'Possibly, probably, perhaps', null, '108', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('109_', '106_', N'Might, may, will, probably', null, '109', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('110_', '106_', N'Must/can''t (deduction)', null, '110', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('111_', '000B', N'Modals: Obligation and Necessity', 14, '111', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('112_', '111_', N'Must/mustn''t', null, '112', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('113_', '111_', N'Have to', null, '113', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('114_', '111_', N'Must/have to', null, '114', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('115_', '111_', N'Should', null, '115', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('116_', '111_', N'Ought to', null, '116', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('117_', '111_', N'Need to/needn''t', null, '117', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('118_', '000B', N'Modals: Past', 15, '118', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('119_', '118_', N'Deduction and speculation (should have, might have etc.)', null, '119', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('120_', '118_', N'Can''t have, needn''t have', null, '120', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('121_', '000B', N'Nouns', 16, '121', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('122a', '121_', N'Very common countable and uncountable (much/many)', null, '122a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('122b', '121_', N'Countable and uncountable (much/many)', null, '122b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('123_', '121_', N'There is / there are', null, '123', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('124_', '000B', N'Pronouns', 17, '124', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('125_', '124_', N'Personal pronouns', null, '125', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('126_', '000B', N'Possessives', 18, '126', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('127_', '126_', N'Possessive adjectives', null, '127', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('128_', '126_', N'Use of ''s and s''', null, '128', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('129_', '126_', N'Possessive pronouns', null, '129', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('130_', '000B', N'Prepositions and Prepositional Phrases', 19, '130', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('131_', '130_', N'Common prepositions', null, '131', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('132_', '130_', N'Prepositional phrases (time and movement)', null, '132', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('132a', '130_', N'Prepositions of time (on/at/in)', null, '132a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('133_', '130_', N'Prepositional phrases (place, time and movement)', null, '133', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('133a', '130_', N'Prepositions of place', null, '133a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('134_', '000B', N'Articles', 20, '134', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('135_', '134_', N'Definite, indefinite articles', null, '135', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('136_', '134_', N'Zero article with uncountable nouns', null, '136', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('137_', '134_', N'Definite article with superlatives', null, '137', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('138_', '134_', N'Articles with countable and uncountable nouns', null, '138', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('139_', '134_', N'Articles with abstract nouns', null, '139', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('140_', '000B', N'Determiners', 21, '140', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('141_', '140_', N'Basic (e.g. any, some, a lot of)', null, '141', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('142_', '140_', N'Wider range (e.g. all, none, not (any), enough, (a) few)', null, '142', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('143_', '140_', N'Broad range (e.g. all the, most, both)', null, '143', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('144_', '000B', N'Adjectives', 22, '144', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('145_', '144_', N'Common', null, '145', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('146_', '144_', N'Demonstrative', null, '146', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('147_', '144_', N'Ending in ''-ed'' and ''-ing''', null, '147', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('148_', '144_', N'Collocation of adjective', null, '148', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('149a', '144_', N'Comparative, superlative', null, '149a', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('149b', '144_', N'Comparative, - use of than', null, '149b', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('149c', '144_', N'Superlative, - use of definite article', null, '149c', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('150_', '144_', N'Comparisons with fewer and less', null, '150', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('151_', '000B', N'Adverbs', 23, '151', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('152_', '151_', N'Adverbs of frequency', null, '152', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('153_', '151_', N'Simple adverbs of place, manner and time', null, '153', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('154_', '151_', N'Adverbial phrases of time, place and frequency including word order', null, '154', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('155_', '151_', N'(Adjectives and) adverbs', null, '155', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('156_', '151_', N'Adverbial phrases of degree, extent, probability', null, '156', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('157_', '151_', N'Comparative and superlative of adverbs', null, '157', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('158_', '151_', N'Attitudinal adverbs', null, '158', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('159_', '151_', N'Inversion (negative adverbials)', null, '159', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('160_', '000B', N'Intensifiers', 24, '160', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('161_', '160_', N'Very basic (very, really)', null, '161', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('162_', '160_', N'Basic (quite, so, a bit)', null, '162', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('163_', '160_', N'Broader range of intensifiers (such as too, so, enough)', null, '163', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('164_', '160_', N'Wide range (such as extremely, much too)', null, '164', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('165_', '160_', N'Collocation of intensifiers', null, '165', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('166_', '000C', N'Lexicon', 1, '166', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('167_', '166_', N'Nationalities and countries', null, '167', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('168_', '166_', N'Personal information', null, '168', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('169_', '166_', N'Food and drink', null, '169', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('170_', '166_', N'Things in the town, shops and shopping', null, '170', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('171_', '166_', N'Travel and services vocabulary', null, '171', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('172_', '166_', N'Basic verbs', null, '172', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('173_', '166_', N'Clothes', null, '173', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('174_', '166_', N'Colours', null, '174', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('175_', '166_', N'Dimensions', null, '175', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('176_', '166_', N'Ways of travelling', null, '176', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('177_', '166_', N'Objects and rooms', null, '177', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('178_', '166_', N'Adjectives: personality, description, feelings', null, '178', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('179_', '166_', N'Contrasting opinions (on the one hand . . . )', null, '179', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('180_', '166_', N'Summarising exponents (briefly, all in all . . . )', null, '180', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('181_', '166_', N'Collocation', null, '181', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('182_', '166_', N'Colloquial language', null, '182', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('183_', '166_', N'Approximating (vague language)', null, '183', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('184_', '166_', N'Differentiating to choose the best in context', null, '184', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('186_', '166_', N'Eliminating false friends', null, '186', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('187_', '166_', N'Formal and informal registers', null, '187', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('188_', '166_', N'Idioms', null, '188', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('189_', '000C', N'Topics', 3, '189', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('190_', '189_', N'Family life', null, '190', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('191_', '189_', N'Hobbies and pastimes', null, '191', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('192_', '189_', N'Holidays', null, '192', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('193_', '189_', N'Work and jobs', null, '193', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('194_', '189_', N'Shopping', null, '194', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('195_', '189_', N'Leisure activities', null, '195', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('196_', '189_', N'Education', null, '196', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('197_', '189_', N'Film', null, '197', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('198_', '189_', N'Books and literature', null, '198', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('199_', '189_', N'News, lifestyles and current affairs', null, '199', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('200_', '189_', N'Mass media', null, '200', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('201_', '189_', N'Arts', null, '201', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('202_', '189_', N'Scientific development', null, '202', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('203_', '189_', N'Technical and legal language', null, '203', 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('300_', '000D', N'Regions', 3, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('301_', '300_', N'Eastern Africa', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('302_', '300_', N'Middle Africa', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('303_', '300_', N'Northern Africa', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('304_', '300_', N'Southern Africa', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('305_', '300_', N'Western Africa', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('306_', '300_', N'South America', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('307_', '300_', N'Caribbean', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('308_', '300_', N'Central America', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('309_', '300_', N'Northern America', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('310_', '300_', N'Central Asia', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('311_', '300_', N'Eastern Asia', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('312_', '300_', N'Southern Asia', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('313_', '300_', N'South-Eastern Asia', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('314_', '300_', N'Western Asia', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('315_', '300_', N'Eastern Europe', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('316_', '300_', N'Northern Europe', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('317_', '300_', N'Southern Europe', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('318_', '300_', N'Western Europe', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('319_', '300_', N'Australia and Oceania', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('320_', '000D', N'Programs', 2, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('321_', '320_', N'"Real Russia" by Sergey Baklykov', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('322_', '320_', N'Family Album USA', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('323_', '000E', N'Vowel sounds, British accent', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('324_', '000E', N'Vowel sounds, American accent', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('325_', '000E', N'Consonant sounds', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('327_', '000E', N'Stress in words', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('328_', '000E', N'Stress and intonation in sentences', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('329_', '000E', N'Accents', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('330_', '000E', N'Influence of native language ', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('331_', '330_', N'Arabic', 1, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('332_', '330_', N'Chinese', 2, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('333_', '330_', N'French', 3, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('334_', '330_', N'Russian', 7, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('335_', '330_', N'Indian', 4, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('336_', '330_', N'Japanese', 5, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('337_', '330_', N'Korean', 6, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('338_', '330_', N'Spanish', 8, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('339_', '327_', N'Vowels in stressed syllables', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('340_', '327_', N'Vowels in unstressed syllables', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('341_', '327_', N'Linking words', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('342_', '328_', N'Falling and rising tones', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('343_', '328_', N'Focus words', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('344_', '328_', N'Thought groups and pausing', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('345_', '328_', N'Connected speech', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('346_', '328_', N'Stressing important words, reducing unimportant words', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('347_', '325_', N'/m/ man, him, ham, map', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('348_', '325_', N'/n/ no, tin, nice, keen', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('349_', '325_', N'/ŋ/ ringer, sing, finger, drink, king', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('350_', '325_', N'/p/ pack, pen, spin, tip, pit', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('351_', '325_', N'/b/ but, web, bit', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('352_', '325_', N'/t/ two, sting, bet, tick', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('353_', '325_', N'/d/ do, odd', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('354_', '325_', N'/k/ cat, kill, skin, queen, unique, thick, kiss, cut', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('355_', '325_', N'/ɡ/ go, get, beg, gut', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('356_', '325_', N'/tʃ/ chair, nature, teach, cheap', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('357_', '325_', N'/dʒ/ gin, joy, edge, jump', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('358_', '325_', N'/f/ fool, enough, leaf, off, photo, fill, fat', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('359_', '325_', N'/v/ voice, have, of', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('360_', '325_', N'/θ/ thing, teeth', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('361_', '325_', N'/ð/ this, breathe, father', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('362_', '325_', N'/s/ see, city, pass, sand', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('363_', '325_', N'/z/ zoo, rose, size', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('364_', '325_', N'/ʃ/ she, sure, session, emotion, leash, sheep', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('365_', '325_', N'/ʒ/ vision, pleasure, beige, equation, seizure', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('366_', '325_', N'/h/ high, ham', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('367_', '325_', N'British /r/ red, run, very', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('368_', '325_', N'American /r/ red, run, very', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('369_', '325_', N'/j/ yes, you', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('370_', '325_', N'/w/ we, queen', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('371_', '325_', N'/l/ left, bell, let', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('372_', '323_', N'British /ʊ/ foot, full, look, could', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('373_', '323_', N'British /ɑː/ bath, staff, clasp, dance', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('374_', '323_', N'British /ɒ/ cloth, cough, long, laurel, origin', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('375_', '323_', N'British /ɜː/ nurse, hurt, term, work', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('376_', '323_', N'British /iː/ fleece, seed, key, seize', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('377_', '323_', N'British /eɪ/ face, weight, rein, steak', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('378_', '323_', N'British /ɑː/ palm, calm, bra, father', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('379_', '323_', N'British /ɔː/ thought, taut, hawk, broad', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('380_', '323_', N'British /əʊ/ goat, soap, soul, home', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('381_', '323_', N'British /uː/ goose, who, group, few', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('382_', '323_', N'British /aɪ/ price, ripe, tribe, aisle, choir', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('383_', '323_', N'British /ɔɪ/ choice, boy, void, coin', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('384_', '323_', N'British /aʊ/ mouth, pouch, noun, crowd, flower', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('385_', '323_', N'British /ɪə/ near, beer, pier, fierce, serious', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('386_', '323_', N'British /ɛə/ square, care, air, wear, Mary', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('387_', '323_', N'British /ɑː/ start, far, sharp, farm, safari', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('388_', '323_', N'British /ɔː/ north, war, storm, for, aural', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('389_', '323_', N'British /ɔː/ force, floor, coarse, ore, oral', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('390_', '323_', N'British /ʊə/ cure, poor, tour, fury', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('391_', '324_', N'American /ʊ/ foot, full, look, could', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('392_', '324_', N'American /æ/ bath, staff, clasp, dance', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('393_', '324_', N'American /ɔ/ cloth, cough, long, laurel, origin', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('394_', '324_', N'American /ɜr/ nurse, hurt, term, work', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('395_', '324_', N'American /i/ fleece, seed, key, seize', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('396_', '324_', N'American /eɪ/ face, weight, rein, steak', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('397_', '324_', N'American /ɑ/ palm, calm, bra, father', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('398_', '324_', N'American /ɔ/ thought, taut, hawk, broad', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('399_', '324_', N'American /o/ goat, soap, soul, home', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('400_', '324_', N'American /u/ goose, who, group, few', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('401_', '324_', N'American /aɪ/ price, ripe, tribe, aisle, choir', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('402_', '324_', N'American /ɔɪ/ choice, boy, void, coin', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('403_', '324_', N'American /aʊ/ mouth, pouch, noun, crowd, flower', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('404_', '324_', N'American /ɪr/ near, beer, pier, fierce, serious', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('405_', '324_', N'American /ɛr/ square, care, air, wear, Mary', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('406_', '324_', N'American /ɑr/ start, far, sharp, farm, safari', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('407_', '324_', N'American /ɔr/ north, war, storm, for, aural', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('408_', '324_', N'American /or/ force, floor, coarse, ore, oral', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('409_', '324_', N'American /ʊr/ cure, poor, tour, fury', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('410_', '325_', N'Consonant clusters', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('411_', '325_', N'Word endings', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('APP1', null, N'Game "Copycat"', null, null, 0)
-- Additions 201502209.
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('412_', '144_', N'Noun modifiers', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('413_', '121_', N'Proper nouns', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('414_', '124_', N'Reciprocal pronouns (each other, one another)', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('415_', '124_', N'Interrogative pronouns (who, whose, what, which)', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('416_', '124_', N'Relative pronouns (who, whose, what, which, that)', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('417_', '124_', N'Reflexive pronouns (myself, themselves)', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('418_', '100_', N'Delexical verbs (have, take, make, give)', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('419_', '000B', N'Phrases, clauses, sentences', 26, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('420_', '419_', N'Phrases', 1, null, 1);
update dbo.libCategories set ParentId = '419_' where Id = '101_'; -- 'Reported speech'
update dbo.libCategories set ParentId = '419_' where Id = '102_'; -- 'Relative clauses'
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('421_', '419_', N'Sentence structure', 2, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('422_', '100_', N'Transitive and intransitive verbs', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('423_', '419_', N'Link verbs', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('424_', '419_', N'Double object verbs', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('425_', '419_', N'Short forms', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('426_', '419_', N'Double object', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('427_', '124_', N'Indefinite pronouns (anybody, nothing)', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('428_', '100_', N'Reflexive verbs', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('429_', '100_', N'Irregular verbs', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('430_', '100_', N'Verbs followed by ''that'' clause', null, null, 1)
----- Deployed.
-- Additions 20150320.
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('431_', '166_', N'Business terms', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('432_', '189_', N'Business', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('433_', '000F', N'IELTS', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('434_', '433_', N'IELTS Listening', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('436a', '433_', N'IELTS Reading Academic', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('436b', '433_', N'IELTS Reading General', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('437a', '433_', N'IELTS Writing Academic', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('437b', '433_', N'IELTS Writing General', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('438_', '433_', N'IELTS Speaking', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('439_', '000F', N'TOEFL iBT', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('440_', '439_', N'TOEFL Reading', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('441_', '439_', N'TOEFL Listening', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('442_', '439_', N'TOEFL Speaking', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('443_', '439_', N'TOEFL Writing', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('444_', '000F', N'TOEIC', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('445_', '444_', N'TOEIC Listening', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('446_', '444_', N'TOEIC Reading', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('447_', '444_', N'TOEIC Speaking', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('448_', '444_', N'TOEIC Writing', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('449_', '000F', N'Cambridge English: First (FCE)', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('451_', '449_', N'FCE Reading and Use of English', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('452_', '449_', N'FCE Writing', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('453_', '449_', N'FCE Listening', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('454_', '449_', N'FCE Speaking', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('455_', '000F', N'Cambridge English: Advanced (CAE)', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('456_', '455_', N'CAE Reading and Use of English', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('457_', '455_', N'CAE Writing', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('458_', '455_', N'CAE Listening', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('459_', '455_', N'CAE Speaking', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('460_', '000F', N'Cambridge English: Proficiency (CPE)', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('461_', '460_', N'CPE Reading and Use of English', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('462_', '460_', N'CPE Writing', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('463_', '460_', N'CPE Listening', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('464_', '460_', N'CPE Speaking', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('465_', '000F', N'STEP Eiken', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('466_', '465_', N'Eiken First stage', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('467_', '465_', N'Eiken Second stage (speaking)', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('468_', '000F', N'EGE English (ЕГЭ)', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('469_', '468_', N'EGE Part A', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('470_', '468_', N'EGE Part B', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('471_', '468_', N'EGE Part C', null, null, 1);
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('472_', '468_', N'EGE Speaking', null, null, 1);
----- Deployed.
-- Additions 20150323.
update dbo.libCategories set Position = 26 where Id = '419_'; -- 'Phrases, clauses, sentences'
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('473_', '000B', N'Conjunctions', 25, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('474_', '473_', N'Coordinating conjunctions', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('475_', '473_', N'Correlative conjunctions', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('476_', '473_', N'Subordinating conjunctions', null, null, 1)
----- Deployed.
-- Additions 20150330.
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('477_', '419_', N'Punctuation: Comma, ampersand', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('478_', '419_', N'Punctuation: Period, exclamation point, question mark', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('479_', '419_', N'Punctuation: Colon, semicolon, dash, hyphen, parenthesis, quotation marks, slash, ellipsis', null, null, 1)
----- Deployed.
-- Additions 20150401.
delete dbo.libCategories where Id = '434_';
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('435_', '433_', N'IELTS Listening', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('435a', '433_', N'IELTS Listening - Section 1', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('435b', '433_', N'IELTS Listening - Section 2', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('435c', '433_', N'IELTS Listening - Section 3', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('435d', '433_', N'IELTS Listening - Section 4', null, null, 1)

insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('436_', '433_', N'IELTS Reading', null, null, 1)

insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('437_', '433_', N'IELTS Writing', null, null, 1)
update dbo.libCategories set Name = 'IELTS Writing Academic - Task 1' where Id = '437a'; 
update dbo.libCategories set Name = 'IELTS Writing Academic - Task 2' where Id = '437b'; 
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('437c', '433_', N'IELTS Writing General - Task 1', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('437d', '433_', N'IELTS Writing General - Task 2', null, null, 1)

insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('438a', '433_', N'IELTS Speaking - Part 1', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('438b', '433_', N'IELTS Speaking - Part 2', null, null, 1)
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('438c', '433_', N'IELTS Speaking - Part 3', null, null, 1)
----- Deployed.
----- Deployed.
-- Additions 20150409.
update dbo.libCategories set Name = 'IELTS Writing General - Task 1' where Id = '437b'; 
update dbo.libCategories set Name = 'IELTS Writing - Task 2' where Id = '437c'; 
delete dbo.libCategories where Id = '437d';
----- Deployed.
----- Deployed.
-- Additions 20150409.
insert dbo.libCategories (Id, ParentId, Name, Position, FeatureCode, Display) values ('480_', '473_', N'Conjunctive adverbs', null, null, 1)
----- Deployed.



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