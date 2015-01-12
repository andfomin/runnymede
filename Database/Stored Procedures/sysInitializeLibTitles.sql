﻿CREATE PROCEDURE [dbo].[sysInitializeLibTitles] 
AS
BEGIN
SET NOCOUNT ON;

/*
select
'insert dbo.libTitles ([Title]) values (N''' + replace(Title, '''', '''''') +  ''');'
from dbo.libTitles T
	inner join dbo.libDescriptions D on T.Id = D.TitleId
where D.SourceId in ('BCEQ', 'BCEG');

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

insert dbo.libTitles ([Title]) values (N'Adverbs. Comparative and superlative of adverbs');
insert dbo.libTitles ([Title]) values (N'Other Verb Forms. Relative clauses');
insert dbo.libTitles ([Title]) values (N'Sentences: Adverbials');
insert dbo.libTitles ([Title]) values (N'Functions. Using prices');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Linkers: sequential - past time (subsequently)');
insert dbo.libTitles ([Title]) values (N'Articles. Definite, indefinite');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future continuous (Prediction)');
insert dbo.libTitles ([Title]) values (N'Link verbs');
insert dbo.libTitles ([Title]) values (N'Determiners. Wider range (e.g. all, none, not (any), enough, (a) few)');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Checking understanding from listener''s point of view');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past perfect continuous');
insert dbo.libTitles ([Title]) values (N'Verbs: Continuous aspect');
insert dbo.libTitles ([Title]) values (N'Phrasal Verbs. Extended phrasal verbs (splitting)');
insert dbo.libTitles ([Title]) values (N'Functions. Suggestions');
insert dbo.libTitles ([Title]) values (N'Adverbs. Adverbial phrases of degree, extent, probability');
insert dbo.libTitles ([Title]) values (N'Questions. Wh-questions in the past');
insert dbo.libTitles ([Title]) values (N'Prepositions and Prepositional Phrases. Prepositions of place');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future continuous');
insert dbo.libTitles ([Title]) values (N'Adverbs. (Adjectives and) adverbs');
insert dbo.libTitles ([Title]) values (N'Quantifiers');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Markers to structure and signpost informal speech and writing');
insert dbo.libTitles ([Title]) values (N'Verbs: Present continuous');
insert dbo.libTitles ([Title]) values (N'Intensifiers. Wide range (such as extremely, much too)');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Initiating conversation');
insert dbo.libTitles ([Title]) values (N'Conditionals. Mixed conditionals in the past, present and future');
insert dbo.libTitles ([Title]) values (N'Functions. Arrangements to meet people');
insert dbo.libTitles ([Title]) values (N'Functions. Generalising and qualifying');
insert dbo.libTitles ([Title]) values (N'Uncount nouns');
insert dbo.libTitles ([Title]) values (N'Count nouns');
insert dbo.libTitles ([Title]) values (N'Prepositions and Prepositional Phrases. Prepositional phrases (time and movement)');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Taking initiative in non-control situation');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Collocation');
insert dbo.libTitles ([Title]) values (N'Modals: Obligation and Necessity. Need to/needn''t');
insert dbo.libTitles ([Title]) values (N'Verbs: Present tense');
insert dbo.libTitles ([Title]) values (N'Functions. Emphasizing a point, feeling, issue');
insert dbo.libTitles ([Title]) values (N'Comparative and superlative adjectives');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Taking the initiative in interaction. During the meeting');
insert dbo.libTitles ([Title]) values (N'Verbs: -ing forms');
insert dbo.libTitles ([Title]) values (N'Verbs: Present. Present continuous');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Will have / would have');
insert dbo.libTitles ([Title]) values (N'Reciprocal pronouns: each other / one another');
insert dbo.libTitles ([Title]) values (N'Pronouns. Personal - subject');
insert dbo.libTitles ([Title]) values (N'Simple Verb Forms. Have got (British)');
insert dbo.libTitles ([Title]) values (N'Functions. Critiquing and reviewing');
insert dbo.libTitles ([Title]) values (N'Verbs: Active and passive voice');
insert dbo.libTitles ([Title]) values (N'Sentence structure');
insert dbo.libTitles ([Title]) values (N'pronouns/questions');
insert dbo.libTitles ([Title]) values (N'Adverbs. Inversion (negative adverbials)');
insert dbo.libTitles ([Title]) values (N'Modals: Possibility. Might, may');
insert dbo.libTitles ([Title]) values (N'Questions. Complex question tags');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past simple (narrative)');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Travel and services vocabulary');
insert dbo.libTitles ([Title]) values (N'Pronouns: this, that, these, those');
insert dbo.libTitles ([Title]) values (N'Sentences: Verbs with -ing forms');
insert dbo.libTitles ([Title]) values (N'Articles. Definite article with superlatives');
insert dbo.libTitles ([Title]) values (N'Modals: Possibility. Must/can''t (deduction)');
insert dbo.libTitles ([Title]) values (N'Adverbials of place');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Linking devices: logical markers');
insert dbo.libTitles ([Title]) values (N'Prepositions and Prepositional Phrases. Common prepositions');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Taking the initiative in interaction. Control and delegation at start');
insert dbo.libTitles ([Title]) values (N'Verbs: Present Perfect. Present perfect, past simple');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing agreement/disagreement');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past perfect (narrative)');
insert dbo.libTitles ([Title]) values (N'Sentences: Intransitive verbs');
insert dbo.libTitles ([Title]) values (N'Definite article: the');
insert dbo.libTitles ([Title]) values (N'Adverbials of time: how often');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Linkers: sequential - past time (first, finally)');
insert dbo.libTitles ([Title]) values (N'Functions. Directions');
insert dbo.libTitles ([Title]) values (N'Sentences: Verbs with to + infinitive');
insert dbo.libTitles ([Title]) values (N'Adverbs. Attitudinal adverbs');
insert dbo.libTitles ([Title]) values (N'Functions. Obligations and necessity');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Will/would');
insert dbo.libTitles ([Title]) values (N'Verbs: Past continuous');
insert dbo.libTitles ([Title]) values (N'Adjectives');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Basic verbs');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Inviting another speaker to come in in group interaction.');
insert dbo.libTitles ([Title]) values (N'Articles. With countable and uncountable nouns');
insert dbo.libTitles ([Title]) values (N'Noun phrase');
insert dbo.libTitles ([Title]) values (N'Adjectives: Intensifiers');
insert dbo.libTitles ([Title]) values (N'Modals: Can. Can/could (functional)');
insert dbo.libTitles ([Title]) values (N'Interrogative determiners: which, what');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing abstract ideas');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Present continuous for the future (arrangements)');
insert dbo.libTitles ([Title]) values (N'Modal verbs: May, might, may have, might have');
insert dbo.libTitles ([Title]) values (N'Functions. Responding to counterarguments');
insert dbo.libTitles ([Title]) values (N'Adjectives: Mitigators');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past continuous');
insert dbo.libTitles ([Title]) values (N'Adjectives. Collocation of adjective');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future perfect continuous');
insert dbo.libTitles ([Title]) values (N'Intensifiers. Broader range of intensifiers (such as too, so, enough)');
insert dbo.libTitles ([Title]) values (N'Determiners and quantifiers');
insert dbo.libTitles ([Title]) values (N'Gerund and Infinitive. Verb + to + infinitive');
insert dbo.libTitles ([Title]) values (N'Functions. Conceding a point');
insert dbo.libTitles ([Title]) values (N'Adjectives: Noun modifiers');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing shades of opinion and certainty');
insert dbo.libTitles ([Title]) values (N'Passives. Simple passive');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Interacting informally, reacting, expressing interest, sympathy, surprise');
insert dbo.libTitles ([Title]) values (N'Phrasal Verbs. Common phrasal verbs');
insert dbo.libTitles ([Title]) values (N'Adverbials of direction');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Connecting words (and, but, because)');
insert dbo.libTitles ([Title]) values (N'Gerund and Infinitive. Gerunds');
insert dbo.libTitles ([Title]) values (N'Functions. Synthesizing, evaluating, glossing information');
insert dbo.libTitles ([Title]) values (N'Adverbials of distance');
insert dbo.libTitles ([Title]) values (N'Functions. Speculating');
insert dbo.libTitles ([Title]) values (N'Adjectives. Comparative, superlative');
insert dbo.libTitles ([Title]) values (N'Conditionals. Mixed conditionals');
insert dbo.libTitles ([Title]) values (N'Possessives. Possessive adjectives');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Approximating (vague language)');
insert dbo.libTitles ([Title]) values (N'Verbs: Present Perfect. Present perfect continuous');
insert dbo.libTitles ([Title]) values (N'Verbs: Present simple');
insert dbo.libTitles ([Title]) values (N'Intensifiers. Collocation of intensifiers');
insert dbo.libTitles ([Title]) values (N'Order of adjectives');
insert dbo.libTitles ([Title]) values (N'Questions. Questions forms');
insert dbo.libTitles ([Title]) values (N'Pronouns: it, there');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Closing conversation');
insert dbo.libTitles ([Title]) values (N'Functions. Describing feelings, emotions, attitudes precisely');
insert dbo.libTitles ([Title]) values (N'Functions. Developing an argument systematically');
insert dbo.libTitles ([Title]) values (N'Adverbials of time: how long');
insert dbo.libTitles ([Title]) values (N'Verbs: Present. Present simple');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Objects and rooms');
insert dbo.libTitles ([Title]) values (N'Verbs followed by to + infinitive');
insert dbo.libTitles ([Title]) values (N'Phrasal Verbs. Extended phrasal verbs');
insert dbo.libTitles ([Title]) values (N'Adjectives. Ending in ''-ed'' and ''-ing''');
insert dbo.libTitles ([Title]) values (N'Adjectives. Comparisons with fewer and less');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Taking the initiative in interaction. Keeping interaction participants on topic');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Formal and informal registers');
insert dbo.libTitles ([Title]) values (N'Passives. All passive forms');
insert dbo.libTitles ([Title]) values (N'Functions. Developing an argument');
insert dbo.libTitles ([Title]) values (N'Gerund and Infinitive. Verb + -ing/infinitive (like / want - would like)');
insert dbo.libTitles ([Title]) values (N'Possessives: Reciprocal pronouns');
insert dbo.libTitles ([Title]) values (N'Functions. Advice');
insert dbo.libTitles ([Title]) values (N'Functions. Offers');
insert dbo.libTitles ([Title]) values (N'Verbs: Talking about the past');
insert dbo.libTitles ([Title]) values (N'Conditionals. Wish / if only, regrets');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future perfect continuous (Prediction)');
insert dbo.libTitles ([Title]) values (N'Nouns');
insert dbo.libTitles ([Title]) values (N'Verbs');
insert dbo.libTitles ([Title]) values (N'Functions. Requests');
insert dbo.libTitles ([Title]) values (N'Reporting: reports and summaries');
insert dbo.libTitles ([Title]) values (N'Comparative adverbs');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past simple');
insert dbo.libTitles ([Title]) values (N'Adverbials');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Managing interaction. Continuing');
insert dbo.libTitles ([Title]) values (N'Modals: Obligation and Necessity. Have to');
insert dbo.libTitles ([Title]) values (N'Functions. Speculating and hypothesising about causes, consequences etc.');
insert dbo.libTitles ([Title]) values (N'Intensifiers. Very basic (very, really)');
insert dbo.libTitles ([Title]) values (N'Clauses: Short forms');
insert dbo.libTitles ([Title]) values (N'Nouns. There is / there are');
insert dbo.libTitles ([Title]) values (N'Gerund and Infinitive. I''d like');
insert dbo.libTitles ([Title]) values (N'Adjectives. Common');
insert dbo.libTitles ([Title]) values (N'Verb phrase');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Idiomatic expressions');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past perfect continuous (narrative)');
insert dbo.libTitles ([Title]) values (N'Adverbs. Simple adverbs of place, manner and time');
insert dbo.libTitles ([Title]) values (N'Sentences: Link verbs');
insert dbo.libTitles ([Title]) values (N'Double object verbs');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Things in the town, shops and shopping');
insert dbo.libTitles ([Title]) values (N'Verbs followed by -ing clauses');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Encouraging another speaker to continue.');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Managing interaction. Changing the topic');
insert dbo.libTitles ([Title]) values (N'Modals: Can. Can/can''t (ability)');
insert dbo.libTitles ([Title]) values (N'Pronouns: you, they');
insert dbo.libTitles ([Title]) values (N'Pronouns: one, ones');
insert dbo.libTitles ([Title]) values (N'Sentences: Transitive verbs');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Dimensions');
insert dbo.libTitles ([Title]) values (N'Modals: Past. Can''t have, needn''t have');
insert dbo.libTitles ([Title]) values (N'Verbs: Wishes, hypotheses');
insert dbo.libTitles ([Title]) values (N'Verbs in time clauses and if clauses');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past perfect');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Food and drink');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Managing interaction. Resuming');
insert dbo.libTitles ([Title]) values (N'Adjectives: -ed, -ing');
insert dbo.libTitles ([Title]) values (N'Functions. Describing habits and routines');
insert dbo.libTitles ([Title]) values (N'Nouns. Very common countable and uncountable (much/many)');
insert dbo.libTitles ([Title]) values (N'Verbs: Question forms');
insert dbo.libTitles ([Title]) values (N'Proper nouns');
insert dbo.libTitles ([Title]) values (N'Articles. Zero article with uncountable nouns');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Ways of travelling');
insert dbo.libTitles ([Title]) values (N'Verbs: Past simple');
insert dbo.libTitles ([Title]) values (N'Gerund and Infinitive. Verb + -ing (like/hate/love)');
insert dbo.libTitles ([Title]) values (N'Verbs: Present perfect');
insert dbo.libTitles ([Title]) values (N'Relative clauses');
insert dbo.libTitles ([Title]) values (N'Verbs: Past perfect');
insert dbo.libTitles ([Title]) values (N'Possessive pronouns');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Summarising exponents (briefly, all in all . . . )');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Certain, probable, possible');
insert dbo.libTitles ([Title]) values (N'Simple Verb Forms. Imperatives (+/-)');
insert dbo.libTitles ([Title]) values (N'Prepositional phrases');
insert dbo.libTitles ([Title]) values (N'Functions. Giving precise information');
insert dbo.libTitles ([Title]) values (N'Irregular verbs');
insert dbo.libTitles ([Title]) values (N'Adverbs. Adverbial phrases of time, place and frequency including word order');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past simple (to be)');
insert dbo.libTitles ([Title]) values (N'Functions. Describing past experiences and storytelling');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Clothes');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Colours');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Managing interaction. Interrupting');
insert dbo.libTitles ([Title]) values (N'Possessives: Pronouns');
insert dbo.libTitles ([Title]) values (N'Conditionals. Wish');
insert dbo.libTitles ([Title]) values (N'Nouns. Countable and uncountable (much/many)');
insert dbo.libTitles ([Title]) values (N'Adverbials: where they go in a sentence');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future perfect');
insert dbo.libTitles ([Title]) values (N'Functions. Defending a point of view persuasively');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Linkers: although, in spite of, despite');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Colloquial language');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing certainty, probability, doubt');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Modals + have');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future time (will, going to)');
insert dbo.libTitles ([Title]) values (N'Modals: Possibility. Might, may, will, probably');
insert dbo.libTitles ([Title]) values (N'Simple Verb Forms. To be (including questions and negatives)');
insert dbo.libTitles ([Title]) values (N'Modals: Obligation and Necessity. Should');
insert dbo.libTitles ([Title]) values (N'Modals: Obligation and Necessity. Must/have to');
insert dbo.libTitles ([Title]) values (N'Articles. With abstract nouns');
insert dbo.libTitles ([Title]) values (N'Superlative adverbs');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Can, could, could have');
insert dbo.libTitles ([Title]) values (N'Sentences: questions and negatives');
insert dbo.libTitles ([Title]) values (N'Indefinite pronouns');
insert dbo.libTitles ([Title]) values (N'Functions. Describing hopes and plans');
insert dbo.libTitles ([Title]) values (N'Functions. Critiquing and reviewing. Talking about films and books');
insert dbo.libTitles ([Title]) values (N'Modals: Obligation and Necessity. Must/mustn''t');
insert dbo.libTitles ([Title]) values (N'Adverbial phrases');
insert dbo.libTitles ([Title]) values (N'Verbs: Talking about the present');
insert dbo.libTitles ([Title]) values (N'Clause, phrase, sentence: Verb patterns');
insert dbo.libTitles ([Title]) values (N'Modals: Obligation and Necessity. Ought to');
insert dbo.libTitles ([Title]) values (N'Delexical verbs: have, take, make, give');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Linkers: sequential - past time (later)');
insert dbo.libTitles ([Title]) values (N'Possessives. Possessive pronouns');
insert dbo.libTitles ([Title]) values (N'Modals: Past. Deduction and speculation (should have, might have etc.)');
insert dbo.libTitles ([Title]) values (N'Possessives: Nouns');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Personal information');
insert dbo.libTitles ([Title]) values (N'Relative pronouns');
insert dbo.libTitles ([Title]) values (N'Functions. Telling the time');
insert dbo.libTitles ([Title]) values (N'Reflexive and ergative verbs');
insert dbo.libTitles ([Title]) values (N'Verbs: Perfective aspect');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future time (will, going to) (Prediction)');
insert dbo.libTitles ([Title]) values (N'Personal Pronouns');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Inviting another speaker to come in in one-to-one interaction');
insert dbo.libTitles ([Title]) values (N'Possessives');
insert dbo.libTitles ([Title]) values (N'Modals: Possibility. Possibly, probably, perhaps');
insert dbo.libTitles ([Title]) values (N'Prepositions and Prepositional Phrases. Prepositions of time (on/at/in)');
insert dbo.libTitles ([Title]) values (N'Adjectives. Superlative, - use of definite article');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Ability, permission, requests, advice');
insert dbo.libTitles ([Title]) values (N'Reporting verbs with that, wh-, if clauses');
insert dbo.libTitles ([Title]) values (N'Adjectives. Comparative, - use of than');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Used to (narrative)');
insert dbo.libTitles ([Title]) values (N'Adverbs. Adverbs of frequency');
insert dbo.libTitles ([Title]) values (N'Problems with count/uncount nouns');
insert dbo.libTitles ([Title]) values (N'Conditionals. Zero and first conditional');
insert dbo.libTitles ([Title]) values (N'Discourse Functions. Checking understanding from speaker''s point of view');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing opinions');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Used to');
insert dbo.libTitles ([Title]) values (N'Adverbials of location');
insert dbo.libTitles ([Title]) values (N'Adjective phrases');
insert dbo.libTitles ([Title]) values (N'Prepositions and Prepositional Phrases. Prepositional phrases (place, time and movement)');
insert dbo.libTitles ([Title]) values (N'Sentences: Double object verbs');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Going to');
insert dbo.libTitles ([Title]) values (N'Sentences: Two- and three-part verbs');
insert dbo.libTitles ([Title]) values (N'Functions. Describing places');
insert dbo.libTitles ([Title]) values (N'Functions. Describing people');
insert dbo.libTitles ([Title]) values (N'Functions. Describing things');
insert dbo.libTitles ([Title]) values (N'Verbs: Future. Future perfect (Prediction)');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Discourse markers to structure formal speech');
insert dbo.libTitles ([Title]) values (N'Verbs followed by that clause');
insert dbo.libTitles ([Title]) values (N'Adverbials of probability');
insert dbo.libTitles ([Title]) values (N'Verbs: Past tense');
insert dbo.libTitles ([Title]) values (N'Adverbials of time: already, still, yet, no longer');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Would expressing habit in the past (narrative)');
insert dbo.libTitles ([Title]) values (N'Determiners. Broad range (e.g. all the, most, both)');
insert dbo.libTitles ([Title]) values (N'Determiners. Basic (e.g. any, some, a lot of)');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing reaction, e.g. indifference');
insert dbo.libTitles ([Title]) values (N'Wh- clauses');
insert dbo.libTitles ([Title]) values (N'Clause, phrase, sentence');
insert dbo.libTitles ([Title]) values (N'Functions. Using numbers');
insert dbo.libTitles ([Title]) values (N'Verbs: Present Perfect. Present perfect');
insert dbo.libTitles ([Title]) values (N'Possessives: Questions');
insert dbo.libTitles ([Title]) values (N'Intensifiers. Basic (quite, so, a bit)');
insert dbo.libTitles ([Title]) values (N'Adverbials of time: time and dates');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Markers to structure informal spoken discourse');
insert dbo.libTitles ([Title]) values (N'Adverbials of time');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Past continuous (narrative)');
insert dbo.libTitles ([Title]) values (N'Functions. Greetings');
insert dbo.libTitles ([Title]) values (N'How we make adverbials');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Nationalities and countries');
insert dbo.libTitles ([Title]) values (N'Functions. Developing an argument in academic discourse style');
insert dbo.libTitles ([Title]) values (N'Clause structure');
insert dbo.libTitles ([Title]) values (N'The verb be');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Eliminating false friends');
insert dbo.libTitles ([Title]) values (N'Discourse Markers. Connecting words expressing cause and effect, contrast etc.');
insert dbo.libTitles ([Title]) values (N'Verbs: Past. Would expressing habit in the past');
insert dbo.libTitles ([Title]) values (N'Adjectives. Demonstrative');
insert dbo.libTitles ([Title]) values (N'Pronouns');
insert dbo.libTitles ([Title]) values (N'Verbs: Talking about the future');
insert dbo.libTitles ([Title]) values (N'Conditionals. Second and third conditional');
insert dbo.libTitles ([Title]) values (N'Possessives: Adjectives');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Differentiating to choose the best in context');
insert dbo.libTitles ([Title]) values (N'Adverbs of manner');
insert dbo.libTitles ([Title]) values (N'Possessives. Use of ''s and s''');
insert dbo.libTitles ([Title]) values (N'Verb phrases');
insert dbo.libTitles ([Title]) values (N'Gerund and Infinitive. To + infinitive (express purpose)');
insert dbo.libTitles ([Title]) values (N'Verbs: to + infinitive');
insert dbo.libTitles ([Title]) values (N'Functions. Describing feelings, emotions, attitudes');
insert dbo.libTitles ([Title]) values (N'Functions. Giving personal information');
insert dbo.libTitles ([Title]) values (N'Indefinite article: a/an');
insert dbo.libTitles ([Title]) values (N'Modal verbs: Can/could');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Adjectives: personality, description, feelings');
insert dbo.libTitles ([Title]) values (N'Other Verb Forms. Reported speech (range of tenses)');
insert dbo.libTitles ([Title]) values (N'Functions. Expressing opinions tentatively, hedging');
insert dbo.libTitles ([Title]) values (N'Phrasal verbs');
insert dbo.libTitles ([Title]) values (N'Modal verbs');
insert dbo.libTitles ([Title]) values (N'Reflexive pronouns');
insert dbo.libTitles ([Title]) values (N'Vocabulary. Contrasting opinions (on the one hand . . . )');
insert dbo.libTitles ([Title]) values (N'Functions. Invitations');

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