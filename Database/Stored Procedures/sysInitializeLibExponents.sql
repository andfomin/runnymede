CREATE PROCEDURE [dbo].[sysInitializeLibExponents] 
AS
BEGIN
SET NOCOUNT ON;

/*
!!!!!!!!  DO NOT NORMALIZE LINE ENDINGS  !!!!!!!!  They are inside data strings.

Use the "Tasks/Generate Scripts" wizard, save to file and delete GO commands. They are not SQL and they switch mode to execution if occur in a stored procedure code on saving.
*/

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);
--raiserror('%s: ', 16, 1, @ProcName);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;


INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'003_', N'A1', N'We have three cats and one dog.

My father is 45 years old.

There are 500 people in our village.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'004_', N'A1', N'How much does the room cost? - 45 Euros per night.

The train ticket to York is 7 pounds 50 (pence).

I spend about 50 dollars a day.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'005_', N'A1', N'What''s the time? A quarter to seven.

Do you have the time please?

Can you tell me the time, please? - It''s 9.45. (nine forty-five)

The train leaves at three o''clock.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'006_', N'A1', N'The hotel is on the left.

Go to the end of the street and turn right.

Where is the supermarket? - It''s straight ahead.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'007_', N'A1', N'Hi John, how are you today?

Good evening, Mr Jones.

This is Mary. Pleased to meet you.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'008_', N'A1', N'My name is Carlos.

I am from the north of China.

I live in Beirut.

I have two sisters and one brother.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'009_', N'A1', N'My brother goes to work at 8 o''clock.

I get the bus to college every day.

I always go swimming on Tuesdays.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'009_', N'A2', N'On Sundays I visit my mother.

I phone my family at the weekend.

The director comes to our office every Tuesday.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'010_', N'A2', N'Marco has blue eyes.

Ekaterina is tall and slim.

Mary has long blonde hair.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'011_', N'A2', N'It''s green and it''s made of plastic.

It''s small, round and made of rubber.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'012_', N'A2', N'Can you give me that book, please?

Can you open the window?

Could I have a glass of milk, please?

Could you pass the sugar, please?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'013_', N'A2', N'Shall we go home now?

Let''s go to the cinema?

Why don''t we phone Jim?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'014_', N'A2', N'You should ask the teacher.

You could try the Internet')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'015_', N'A2', N'Would you like to come to my party?

Do you fancy going to the club tonight?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'016_', N'A2', N'Can I help you?

Shall I carry your bag?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'017_', N'A2', N'We are meeting John at 8 o''clock.

They are seeing Helen later tonight.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'018_', N'A2', N'We have to get home. Grandad is waiting for us.

We must hurry. We are late.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'019_', N'A2', N'Edinburgh is the capital city of Scotland.It has lots of old buildings. It is famous for its castle and its architecture. Every year in summer it holds an international arts festival which brings performers and visitors from around the world.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'019_', N'B1', N'Cairo is the capital city of Egypt. It is on the banks of the River Nile. It has a population of more than 10 million people. Cairo has a rich history. The famous pyramids and the sphinx are located just outside the city.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'020_', N'A2', N'Last year I went to Spain for a walking holiday. Sometimes I stayed in local mountain hostels and sometimes I slept in my tent. One evening I was far from any village, so I camped at the edge of a forest beside a small river. I ate some food and watched the sun go down. I heard a small noise at the edge of the forest. I turned and saw two pairs of eyes. Wolves!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'020_', N'B1', N'Last year we went to Thailand for our holidays. We visited many interesting places. I went scuba-diving while my boyfriend went on an elephant ride. We also tried lots of different kinds of food. We had a great time.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'020_', N'B2', N'I was walking the dog in the park when I heard a loud crash. I looked in the direction of the noise and saw that a huge tree had fallen down. There were some people screaming and calling for help and some children were trapped under one of the fallen branches.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'021a', N'B1', N'He felt a little nervous about the exam.

I''m fed up with this British food.

We should all use public transport as much as possible.

I''m sorry to hear that.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'021a', N'B2', N'He was furious when he saw that his new bicycle had been damaged.

She screamed in anger at how stupid her brother had been.

Heather was delighted with her shot and her face glowed with pride.

Why should we suffer just because our neighbours like loud parties?

I don''t think it''s right for passengers to put their feet on the seats.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'021b', N'C1', N'Well it would be all right if they came out and said it, but I have a bit of a problem with . . .

I don''t really feel comfortable with . . .

I couldn''t care less whether . . . or not.

I''m afraid this is something I feel quite strongly about.

Cristina became a vegetarian and her father had rather mixed feelings about this.

Michael felt completely devastated. Somebody had deliberately sabotaged his research but he did not know who could do such a thing.

When I reached the summit of the mountain I felt a great burden had been lifted from my shoulders. My childhood dream had finally come true. Looking down into the valleys far below I felt a sense of pride in my achievement. I wanted to shout out loud from the top of the world.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'022_', N'B2', N'I am having a meeting with my boss on Friday.

How are you going to get to France?

How long are you going to Jamaica for? I''d love to see the photos when you get back.

I''ll call you soon.

I am going to go around the world when I''ve saved enough money

I hope to get a job in Australia next year.

I''ve always wanted to visit the Taj Mahal.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'023_', N'B2', N'I''ll meet you at 2pm in the children''s section of Waterstones in Oxford Street.

In my job I mainly have to deal with clients, particularly arranging and following up on orders.

He was born in a little village in the North East of Estonia on the 22nd of October, 1928.

My degree was in economics, specialising in finance.

You need to place the pizza dough in a warm bowl, cover it with a cloth or place it somewhere warm, leave it to rise for 30 minutes or until it doubles in size.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'024_', N'B2', N'Corruption is widespread in that part of the world.

There is little respect for human rights during war time.

Education is the way out of poverty for many young people.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'025_', N'B2', N'I''m absolutely certain it''s going to rain.

It''s impossible to get him out of bed before 10 o''clock.

He''s probably gone to the library.

We''re definitely not going to Spain this year.

Are you sure we will arrive in time?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'025_', N'C1', N'It is highly likely that the airport will be closed again tomorrow.

No doubt he''ll bring his dog as usual.

There''s bound to be trouble at the meeting.

Is that settled, then? Yes. It''s settled.

It looks as if she''s going to be late.

Surely, you don''t think it was my fault?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'026_', N'B2', N'On the whole I think it is a good idea.

Generally speaking, the teachers are very helpful

More often than not he shops in the High Street.

Taking into consideration the cost of travel, you might not want to buy a flat so far away.

We''ll stay for a week or two, depending on the cost.

Provided that there is no rain, the concert will go ahead as planned.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'027_', N'B2', N'To sum up, the government will need to cut spending for the next five years.

All in all, it was a miserable performance.

To be fair, it was his own fault for parking where he shouldn''t have.

In short, they were better than us at promoting their ideas')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'027_', N'C1', N'In a nutshell, it was the headmaster who had to take responsibility.

To cut a long story short, he ended up sleeping on my floor.

All things considered, I think we''ve made the best decision.

Another way of putting this would be to say . . .

To recap on what has been said so far, . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'028a', N'B2', N'I wonder if John will be going to the party.

If she got the nomination, she could probably win if she gathered enough support from the community.

What do you think would happen if they did discover oil there?

What if Teresa hadn''t turned up?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'028b', N'C1', N'Supposing he had missed his train?

If the pound did drop to parity with the euro, Britain might be better off in the long run.

Well if we don''t do something about the oil spill, there could be a lot more fallout than just dead fish. I mean, the water could be polluted for decades.

If you''d arrived on time, we would probably have missed the traffic.

If she didn''t get so excited, she might get more work done.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'029a', N'B1', N'I think England will win the World Cup.

I don''t think he is old enough to get married.

In my opinion, it''s too expensive.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'029a', N'B2', N'If I were you, I''d just say no.

From her point of view, we have to do this as soon as possible

The way I see it is that you''ll have to study very hard.

I feel we should do it.

I really don''t think it''s a good idea.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'029b', N'C1', N'I assume you''ll be going home for Christmas.

Am I right to think you''re responsible?

It''s supposed to be good.

I''m just not so sure, it could be okay.

Maybe she is the best person for the job.

I should think he''ll be delighted with the surprise.

It could well be the best solution.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'030_', N'C1', N'There''s no doubt about the fact that there is going to be inflation.

No, I''m absolutely sure. I mean look at the figure for X.

I may be wrong, but I think higher inflation is almost certain.

I have a feeling there may be a problem here.

I suppose that could be an option.

I rather doubt that he''ll come.

It''s not something I feel strongly about

Well one option/possibility might be to go earlier.

I really think that the people who produce our food should not be exploited. But the problem is that sometimes fair trade goods are more than double the price. When this is the case I tend to buy the cheaper product. I am not proud of this, but I am sure there are many other people exactly like me.

I thought the meeting was a missed opportunity to actually do something good for a change. But I know that when it comes to environmental issues, governments tend to talk a lot and make grand promises, and then go back and carry on just the same as before. If they really wanted to make a difference they wouldn''t just set so-called ''green objectives'' but would pass laws which would have an immediate impact on the environment.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'031_', N'B1', N'I think so too.

You''re right

Exactly!

Yes, I agree.

I think you are absolutely right.

So do/am I.

Neither do I.

Well, actually . . .

Well not really.

I''m sorry but I think you''re wrong.

I see what you mean but . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'031_', N'B2', N'That''s just what I was thinking.

You know, that''s exactly what I think.

I totally agree.

That''s a good point.

No I''m afraid I can''t agree with you there.

You can''t be serious!

Don''t be silly!

That''s ridiculous.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'032_', N'B2', N'Why ask me?

Thank gooodness

Fantastic idea!

Brilliant!

Great!

Whatever.

That''s ridiculous!

How''s that possible?

Really?

No way! I don''t believe it.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'032_', N'C1', N'To be honest, I simply don''t care.

Why bother?

It''s not such a big issue.

I don''t really mind/have an opinion, one way or the other.

What are you trying to say?

Absolutely!

I don''t believe it.

That''s amazing!

Oh, you poor thing.

You can say that again!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'033a', N'B1', N'Meryl Streep was brilliant.

It was difficult to follow.

It was set in Chicago.

It was about a woman who went around the world.

If you like action movies you will like this one

It had a happy ending.

I think you should read this book.

I liked this book because''')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'033b', N'B2', N'I think that ''Treasure Island'' is still popular with children even though the language is rather dated.

In spite of its popularity I feel that ''The Beach'' is a very overrated book which appeals mainly to gap-year students.

The film was a bit disappointing, really.

The best part was when ''..

It was really good when ''.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'033b', N'C1', N'The (report) highlights some important issues but it does not, to my mind, get to the bottom of the problem.

It''s an excellent summary, but I think it would be improved by a deeper consideration of X.

The (report) sets out to do X, and it does parts of this well, but it seems to me to lack . . .

The good/best thing about (the report) is that it is so concise. It really hits the nail on the head.

Well, it starts well, but then after x pages/in the section on X, I had the impression that it . . .

The plot involves the disappearance of a sacred sword and introduces us to various levels of castle intrigue. Stephenson weaves his usual magic by giving us snippets of information here and there, now from the royal chambers, now from the castle kitchens. The master of gothic science fiction has provided us with a real page-turner. The only criticism that might be levelled here is that the author assumes that the reader is already familiar with characters and the world they inhabit. To get the most out of this book one needs to have read the previous books in the series.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'034a', N'B2', N'To begin with it''s a bigger problem than you think.

As far as I am concerned this has nothing to do with the issue.

The way I see it, the family is more important.

That''s the reason I don''t want to work there anymore.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'034b', N'B2', N'One reason why''

Another argument for/against . . . is . . .

X maintains that . . .

Y states that . . .

It could be argued/asserted that . . .

In conclusion . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'034c', N'C1', N'It is frequently argued that . . . , however

X is clearly a topic of concern to many people nowadays

There are several reasons for this: one . . . , two . . . , and finally . . .

The main reason for this . . . is/may be . . .

Some people might argue that . . . However . . .

Opponents of this idea try to suggest that . . . However . . .

It''s clear that . . .

No one would dispute that . . .

It is generally accepted that . . .

All the evidence/data indicates/suggests that . . .

Thus to conclude, the central issues are .

In conclusion, before we . . . we need to . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'035_', N'C1', N'I see what you mean, but . . .

I take your point. I agree we need . . . /It''s certainly true that . . .

I have to admit that . . .

It is true that . . .

Though I hate to say it, I think you are right that . . .

That may be true, but . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'036_', N'C1', N'The main point I''d like to emphasize here is the fact that parents have an important role to play in a child''s education

75% of those interviewed said that public transport was not as safe. Yes, 75% think public transport is now more dangerous.

After turning the whole house upsidedown, the police found nothing. Absolutely nothing.

He''s not Roger Federer but he is a very good tennis player.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'037_', N'C1', N'I know this may not be a popular conclusion, but it seems to me we have to face (facts/ the fact that . . . )

I do appreciate that what I proposed may be expensive/painful/a surprise to some people, but I really am convinced the evidence shows we need to''

I recognise that this may . . . , but . . .

But one should not lose sight of the fact that . . .

But surely one still needs to take X into account')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'038_', N'C1', N'What you say may be true in some contexts, however in this case''.

You may be right, but I still think that . . ..

Whilst it may indeed be true that . . . , I still think . . .

There is no evidence to show that ''.

On the contrary, . . .

I think you have misunderstood the point I was making . . .

I can see where you are coming from but there are problems with your analysis of the situation.

In some circumstances, I would agree with you entirely, but in this case . . .

Even so, he still has a long way to go before he is suitable management material.

No matter how you look at it, he made a mistake.

All the same, she deserves another chance.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'040a', N'B1', N'Hi! My name''s Paula. What''s your name?

Excuse me, can I talk to you for a minute?

Excuse me, please. Have you got a minute?

Excuse me, please. I wonder if you could help me.

Let me introduce myself.

Guess what!

You will never believe what I saw yesterday')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'040b', N'B1', N'It''s been nice talking to you. Bye.

I''m sorry. I''ve got to go now.

Must go - see you later.

See you later. Take care.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'041a', N'B1', N'Is that clear?

Do you follow me?

Do you know what I mean?

Do you understand?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'041a', N'B2', N'Are you following me?

Let me know if you have any questions?

Does that make sense?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'041b', N'B1', N'I''m sorry, did you say ''. . .''?

Is this what you are saying? . . .

I''m not sure I understand. Are you saying that . . . ?

Do you mean . . . ?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'041b', N'B2', N'If I understood/understand you correctly, there are no planes at all on Saturday.

Do you mean I can''t talk to the boss right now?

Are you trying to say you don''t want to go out with me anymore?

Let me see whether I''ve understood you correctly.

So what you''re really saying is . . .

Am I right in assuming . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042a', N'B1', N'Sorry, to interrupt you but . . .

I have a question.

Could I interrupt here?

Do you mind if I say something?

Could I just say something?

Sorry, I just wanted to say . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042a', N'B2', N'Actually, . . .

I''m sorry but . . .

Just a minute!

Yes, I know, but . . . !

Hang on!

Hold on!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042b', N'B1', N'Anyway, . . .

By the way, there''s something else I wanted to tell you.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042b', N'B2', N'Oh, by the way . . .

That reminds me . . .

This has nothing to do with what we are talking about but . . .

On another subject..

Talking about holidays, did you know that I''m off to Florida next week?

Before I forget . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042c', N'B1', N'Anyway, . . .

Anyway, what was I saying?

What were we talking about?

To get back to what I was saying . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042c', N'B2', N'Anyway, I was telling you about John''s party''

To get back to what I was saying''')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'042d', N'B1', N'Anyway, . . .

So, as I was saying . . .

Okay, . . .')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'043a', N'B2', N'Andre, would you like begin?

Pilar, would you like to kick off?

Shall we begin?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'043b', N'B2', N'Jenny, can you tell us how the Human Resources reorganisation is coming along?

How does that affect your department, Rosa?

Let''s move on, shall we?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'043c', N'B2', N'We don''t have time to go into that matter right now.

Let''s get back to the issue under discussion, shall we?

hat''s another topic, really.

Can we keep to the point, please.

Let''s not get distracted.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'043d', N'B2', N'I''d like to say a few words here.

Yes, I think I can contribute to this point.

My expertise in this area might help to clarify the situation.

Perhaps, I could say something here.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'044a', N'B2', N'Carry on.

Go on.

Really?

Mmm''mmm''.

Don''t stop.

Tell me more''

What makes you say that?

What makes you think that?

I''m all ears.

I''m listening.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'044b', N'B2', N'Don''t you agree?

Is that okay with you?

How about you?

What do you reckon/think?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'044c', N'B2', N'What do you think, Mario?

Let''s hear what Gabriella has to say.

James might have something to say on this.

Fiona knows a lot about this.

Hey, you did something like that, didn''t you?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'045_', N'B2', N'Wow, that''s fantastic.

Really? Tell me more.

Tell me all about it.

I don''t believe it!

Oh wow!

Oh you poor thing.

That''s awful. What a shame!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'047_', N'A1', N'She lives in Switzerland and she goes skiing a lot.

I don''t like Indian food but I like Chinese.

I go to bed early because my job starts at 7.00.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'048a', N'A2', N'First we went to Naples. We stayed there 5 days and visited Heracleum and then Pompeii. After that we went to Progida, but I didn''t like it. Finally we stayed a week in Capri.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'048b', N'B1', N'He finished the e-mail and then went out for a while.

Later, he looked at it again, to see if he had missed anything important.

After that, he changed the text a little.

Finally he spellchecked it and sent it.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'048c', N'B2', N'Subsequently, he went on to be one of our best salesmen.

Following this he decided to leave the country.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'049_', N'B1', N'On the other hand, we could stay at home and watch television.

However, this depends on the number of people you''ve invited.

Therefore, it is cheaper to take the bus.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'049_', N'B2', N'I know it would be good fun to watch the late-night film. Nevertheless, I think we should all get an early night before the big event tomorrow.

I would like to tell you more. However, that is as much as I am allowed to reveal at this time.

Consequently, he moved to London to be closer to his family.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'050_', N'B2', N'In spite of her illness during the course, she managed to qualify successfully.

Despite the rain we all had a great time.

Although I was very young at the time, I remember what happened quite clearly.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'051_', N'C1', N'The pound is not as strong as it was two years ago. Moreover, the cost of flights has gone up . . .

Profits are likely to fall this year. Consequently, we need to prepare our shareholders for some bad news.

He was warned many times about the dangers of mountain climbing in winter. Nevertheless, he continued to tackle some of the toughest peaks.

Despite the clear danger that was pointed out to him, he insisted on continuing so he is at least partly responsible for what happened.

The cost of fuel has gone up. Therefore it is hardly surprising that there has been an increase in the use of public transport.

Whereas that is the case in Brazil, in Columbia it is more a question of . . .

Certainly the car is here to stay, but the question is to what extent it will be the same concept of car.

The policy was correct is so far as it was applied; the problem is that it wasn''t applied systematically.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'052_', N'B1', N'Right.

Really?

Well, anyway . . .

Oh I know.

Yes, I suppose so.

I know how you feel

You know, I don''t like her either.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'053_', N'B2', N'To begin, I would like to introduce my colleagues.

Furthermore, I believe that the best way forward is to provide more training.

Moreover, the idea that depression can only be cured by medication is now being challenged.

Consequently, we have to be prepared for a fall in profits next year.

Regarding our position on nuclear power, that has not changed.

Additionally, we will also provide support throughout the process.

In conclusion, we have agreed to give ''3,000 to the charity.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'054_', N'C1', N'We''ve a bit more money coming in than we had last year. Mind you, we''ll still need to be careful with the heating bills.

He''d spent all his money without realising. So, he couldn''t afford a taxi and had to walk home.

Then guess what happened?

On top of that his girlfriend was really angry with him.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'056_', N'A1', N'We are from South America.

No I''m not tired.

France is a wonderful country.

I am a psychology student.

Are you French? No I''m not.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'057_', N'A1', N'Have you got any money?

I''ve got all of his CDs

We''ve got lots of time.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'058_', N'A1', N'Sit down, please.

Go away!

Don''t talk to the driver.

Don''t spend too much money.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'058_', N'A2', N'Somebody stop him!

Push the bar.

Please don''t smoke in here.

Break the glass in an emergency.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'060_', N'A1', N'Is she from Egypt?

Do you like dancing?

What is your name?

Why are we waiting?

What time is it?

How much does it cost?

When did you arrive?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'060_', N'A2', N'Did you pass your driving test?

Have you seen my new car?

Is Sasha arriving today?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'061_', N'A2', N'Where did she go to university?

How did they travel?

When did it happen?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'061_', N'B1', N'Who did you see at the party?

How long have you been studying English?

Why did you get the tattoo?

What happened then?

What have you been doing since you left school?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'062_', N'B1', N'He hasn''t come home yet, has he?

He built the house himself, didn''t he?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'064_', N'A1', N'She eats fruit every day.

We go to the beach on Sundays.

They live near Edinburgh.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'064_', N'A2', N'Do you like British food?

The plane lands at six.

I love this programme.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'065_', N'A1', N'Ibrahim is studying medicine at Bristol University.

John''s working in France now.

It''s raining again.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'065_', N'A2', N'I am staying with Hilary at the moment.

What is he wearing?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'067a', N'A1', N'After the meal we went to a club.

She fell and broke her leg.

I lived in Paris for 6 months.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'067a', N'A2', N'He gave me a nice present.

She bought some flowers for her mother.

I began to play chess when I was 5.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'067a', N'B1', N'When he fell, he cut his leg

I went to London on Sunday and someone stole my camera.

They had so much fun that they forgot to check what the time was.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'067b', N'B2', N'To help pay for his keep and to help his family, Andrew, who was still only 15 years old, began working ten-hour days at a Kensington hotel washing dishes and cleaning the kitchen. He earned just 6 pounds per week. The harsh working conditions and the cruelty of the kitchen staff had a strong influence on his later political outlook, and informed his work when he began his literary career, particularly the novel that made him famous, ''Working Boy''.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'067b', N'C1', N'I went home that evening in a very sombre mood. I tried to relax. I made myself a cup of coffee and turned on the television. But I just could not get the incident out of my mind. The more I thought about things, the more certain I was that something just didn''t make sense. I decided to go back over everything the next day.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'067c', N'A1', N'It was very good.

I moved to Madrid when I was 15.

We were happy there.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'068a', N'A2', N'I was living in Spain when I met her.

It was raining, so we decided to get a taxi.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'068a', N'B1', N'It happened while I was watching television yesterday.

I was coming home from work when the car in front of me suddenly stopped.

Car ''A'' was coming from a side street. The driver wanted to turn left. The other car was coming along the road. It was moving really fast. The driver of car ''A'' didn''t see it. They hit each other.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'068b', N'B2', N'Antonio was walking away from the crowd when the trouble started. He was trying to get home but the buses were not running. He was just crossing the bridge to safety when he heard the sound of breaking glass. He was telling himself not to get involved when a bottle smashed right beside him.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'068b', N'C1', N'They were late as usual, hoping the guests would be a little late. Miriam was still in the kitchen preparing enormous bowls of salad. Her father was tidying away all his papers which were usually scattered over every available space in the dining room. John was keeping a lookout at the front gate, kicking pebbles along the path.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'069a', N'A2', N'She used to be a ballet dancer.

He used to wear glasses but now he uses contacts.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'069a', N'B1', N'They used to live in Portugal.

I used to have a really nice wallet, but I lost it.

When I went to primary school I used to walk to school with a friend, but my mother used to collect me in the afternoons.

I never used /didn''t use to like olives, but now I love them.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'069b', N'B2', N'We used to play at the park at the edge of the town.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'069b', N'C1', N'That bit of the coastline used to be much less busy than it is these days. Lisa and her brothers loved exploring the coves and beaches for miles in both directions. They used to get up really early, run down the rocky path that led to Shell Bay and go for a swim before breakfast.es, those days were fun.

I had a proper tricycle when I was a small child. It had a boot and I used to keep all sorts of toys in it. We used to go all over the place, using the trike as a mobile base.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'070a', N'B1', N'Every night I would tell my little brother a story and he would fall asleep in the middle.

During the summer holiday we would get up early and go to the beach.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'070a', N'B2', N'Every autumn we would steal apples from their garden. We would eat the sour fruit and come home holding our stomachs.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'070b', N'C1', N'My grandmother used to live by the seaside and we would go there every Easter. My Dad would drive, my mother would navigate and we would sit in the back fighting.

In the summer we went to Devon for years. My Dad would ask us to navigate. It was a way of keeping us quiet. We would watch out for named pubs and read the road signs. Usually we counted cars too. I would count VWs; my more sophisticated brother counted Jags.ould usually win.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'071a', N'B1', N'The train had left when I got to the station.

When I got home, Joan had already cooked supper.

They had already paid by the time I asked for the bill.

Ahmed had just arrived.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'071b', N'B2', N'When I''d climbed to the top of the hill, I looked back down and saw something I hadn''t seen before.

He had broken the vase when he had come in through the window.

He had had a terrible day up until that point.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'071b', N'C1', N'When he returned to the UK things were very different. Maria had given up her job in the library and gone back to university. Reza had finally left home and had moved in with a rather odd group of postgrads who had very strict house rules about everything from the storage of food to when guests were allowed to visit. Brigitte seemed to have completely disappeared. Just six months before they had been inseparable. Although he had known that it couldn''t last, it surprised him just how quickly things had changed.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'072a', N'B2', N'I was tired. I''d been working for sixteen hours.

They had been driving so fast that the police had difficulty stopping them.

Had they been waiting long?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'072b', N'C1', N'Whoever it had been must have had a key. So if his parents had been visiting their friends in Lyon, and his sister had been out celebrating the end of term with her boyfriend, then there was only one person who would have been able to get into the house that night.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'074_', N'A1', N'We are going to make a pizza this evening.

They''re going to visit London tomorrow.

Are you going to study this weekend?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'074_', N'A2', N'He''s going to buy a car next year.

She''s going to have an operation in October.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'075_', N'A2', N'Nareene''s playing basketball tonight.

I''m seeing him at 11.00 this morning.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'076a', N'A2', N'I''m going to see John on Saturday ('' already decided)

I''ll tell him about the party (... you are deciding as you speak)

A: I am going to lose my match. B: No you won''t. I''ll help you.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'076b', N'B1', N'If they continue to play this badly, Liverpool are going to lose the cup.

Spurs will probably win the league this season.

Look at those clouds. It''s going to rain.

He will pass his driving test eventually.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'076b', N'B2', N'You will succeed where I have failed.

Here comes the bus now. We aren''t going to be late after all.

Don''t worry. He''ll be here on time.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'076b', N'C1', N'My brother and his girlfriend are getting married in August. They''re not going to go on honeymoon. They''re going to do up the flat they bought with the money they''ll save. They''ll probably have some kind of reception or party for the wedding but I don''t think it will be a very grand affair.

Oh no. Another goal for United! Bar''a is going to lose.

I just got a phone call from Raoul. He''s in a taxi. He''s going to get here in about five minutes.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'077a', N'B1', N'I''ll be working late tomorrow.

He''ll be arriving on the last train from Manchester.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'077b', N'B2', N'This time next year, I''ll be working in Japan and earning good money.

I''ll be visiting my mum on Thursday. Can you come another time?

Will you be using the car tomorrow?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'077b', N'C1', N'This time next year I''ll be sun bathing on my yacht in Antibes. I''ll be mixing with celebrities from all over the world. I''ll be driving a look-at-me car and going to fancy restaurants.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'078a', N'B2', N'I''d better go and collect the girls. They''ll have finished school by now.

I''ll call you at six. Will you have arrived by then?

She won''t have left by then.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'078b', N'C1', N'At the speed things are moving, the case will have expired before it is brought to court.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'079a', N'B2', N'Julia will have been studying Economics for 5 years when she graduates next year.

You''ll have been travelling for 4 days when you get to Bangkok. You''d better book a hotel and have a couple of days rest.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'079b', N'C1', N'At the end of next year, I''ll have been working here for 5 years!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'081_', N'A2', N'He has lost his wallet.

Have you got your results yet?

Have you ever been to Greece?

They''ve gone to Italy on holiday.

Have they come back form the supermarket?

She hasn''t been to school this week.

I''ve known him for 5 years/since 2005.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'081_', N'B1', N'She''s just gone to the shop.

I''ve started but I haven''t finished it yet.

He still hasn''t arrived.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'082_', N'B1', N'I''ve been to Thailand twice. I went there in 2003 and 2007. Have you been there?

He''s won every match so far.

He won every tournament last year.

I''ve had about 9 cars.

We went out together for six months.

When I was at school I studied French for about 5 years.

I have studied French since I was 14 years old.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'083_', N'B1', N'How long have you been playing tennis?

It''s been raining non-stop for two days now.

He''s been working on the report all morning.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'083_', N'B2', N'You''ve been spending a lot of time on the Internet recently.

They''ve been working very hard to get building completed on time.

Honestly, we haven''t been wasting our time.

Have you been seeing Julie behind my back?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'085_', N'A1', N'I''d like a cup of coffee.

I''d like to go home.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'086_', N'A2', N'Walking is the best exercise.

He goes jogging every morning.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'086a', N'A1', N'I love swimming.

I don''t like waiting for buses.

I hate being late.

I like sitting in the sun and doing nothing.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'086b', N'A2', N'I love playing tennis.

I hate washing up.

I enjoy dancing

Would you like to go to the cinema?

I want another drink.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'087_', N'A2', N'I go jogging to get fit.

They are going to Scotland to see the Loch Ness monster.

I went to the post office to buy stamps.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'088_', N'A2', N'She wants to go home now.

I forgot to lock the door.

They hope to arrive at 9 o''clock.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'090_', N'A2', N'If I stay in the sun I get a headache.

If I eat eggs I feel sick.

If I fail my exams, my father will be angry.

I''ll stay in if it rains this afternoon.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'090_', N'B1', N'If you heat water, it boils.

If you press this button, it switches off.

If we don''t tell him, he''ll be angry.

What will he do if he doesn''t find a job?

We''ll go swimming if the water is warm enough.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'091_', N'B1', N'If I won the lottery I''d buy a big house in the countryside.

What would you do if they asked you to work in America?

I would have told Jim, if I had seen him.

If we hadn''t gone out last night, we wouldn''t have missed them.

My girlfriend would have killed me if I''d forgotten her birthday.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'092a', N'B2', N'If I had studied harder, I''d be at university now.

If I''d got that job I applied for I''d be working in Istanbul.

I would have driven you to the match if I didn''t have so much work.

If I wasn''t working in July, I would have suggested we go camping in France.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'092b', N'C1', N'If she had taken her studies more seriously last year, she''d have more job opportunities now.

If Lola had given me the information earlier, she''d be coming with us on holiday.

If I were rich, I would have bought that holiday.

If Nareene didn''t come with us to Glastonbury, everyone would be disappointed.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'093a', N'B2', N'I wish I was rich.

I wish today wasn''t Monday.

I wish I wasn''t going into hospital tomorrow.

She wished she hadn''t hurt his feelings.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'093b', N'C1', N'I wish I''d studied a bit harder.

You wish you''d kept your mouth shut, don''t you?

If only he''d take more care of his health.

If only I had behaved a bit better, she might have given me a chance.

If only the sun would come out!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'095_', N'A2', N'He got up at 6 o''clock.

Put your coat on, it''s raining.

The plane takes off in few minutes.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'096_', N'B1', N'He turned the jobs down.

They made the story up.

She switched the light on.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'096_', N'B2', N'Let''s splash out on a bottle of champagne.

Watching that programme has put me off chicken.

I''ll take you up on that offer.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'096a', N'C1', N'The policeman broke the fight up very quickly.

She talked me into going to her parents'' place for the weekend.

I can''t make anything out; it''s really dark.

She knew that her mother had put John up to it.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'098_', N'B1', N'The lock was broken.

The trees were damaged by the storm.

Rome wasn''t built in a day.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'099_', N'B2', N'I''m being eaten alive by these mosquitoes.

I wasn''t told about the new rules.

I thought that I was being followed.

Did you think that you were being criticised?

The new treatment for malaria has been found to be very effective.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'099_', N'C1', N'He''ll be given a warning.

You''ll be being transferred to your new job tomorrow.

The seats will all have been taken by the time we get there!

He''s going to be given an award.

He ought to be sacked for behaviour like that.

Having been beaten so many times, he decided to fight back.

He might have been hurt.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'101_', N'B1', N'She said she liked brown bread.

He asked if she wanted to go home.

John told them the machine was working.

She explained that she''d lost my telephone number.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'101_', N'B2', N'She said she''d been waiting for ages.

I knew we''d be late.

She thought she could do it all herself.

They reported that the volcano might erupt at any time.

They said it should be fun.

I told her I had to go.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'102_', N'B2', N'I''ve lost the books that I borrowed from the library.

Where is the man that sells second-hand records?

The children he played with thought he was much younger.

This is my cousin Verena , who teaches music.

Shelly and Byron''s poetry, which used to be compulsory, has now been dropped from the syllabus.

She told us all about her new boyfriend, whom none of us knew anything about.

They ran quickly through the streets, all of which were covered in a thick blanket of snow.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'104_', N'A1', N'I can''t swim.

He can speak Spanish, French and Italian.

She can play chess.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'105_', N'A1', N'Can/could I use your phone?

Can/could I have a return?

Can I help?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'105_', N'A2', N'Could I use your computer? - Yes. Of course you can.

This could be England''s best chance.

Can I have some more spaghetti, please?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'107_', N'A2', N'She might come. I don''t know.

John may know the answer to your question.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'108_', N'A2', N'I''ll probably see you later.

Lionel Messi is probably Argentina''s most famous footballer.

Perhaps she''s late.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'109_', N'B1', N'I might be half an hour late.

Petra will probably be late too. She''s usually late.

We may go to Egypt this year.

Are you going to have a party in your new flat? I don''t know. I may, I may not.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'110_', N'B1', N'Mohamed can''t be at home yet, I saw him leave just a few minutes ago.

I don''t believe it. It can''t be true.

That must be Brigitte''s father. She told me he was coming.

You''ve just walked all the way from Oxford Street. You must be tired.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'112_', N'A2', N'You must get to work on time.

I must go to bed. I''m really tired.

You mustn''t smoke here.

I must phone her tonight. It''s her birthday.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'113_', N'A2', N'Students have to fill in a form if they want to leave early.

I have to go to Madrid tomorrow. I''ve got a job interview.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'114_', N'B1', N'I really must lose some weight before the holiday.

Passengers must not put their feet on the seats.

I can''t come tonight because I have to meet my cousin.

I have to make an appointment this week. It hurts!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'115_', N'A2', N'You should stay in and study tonight. You''ve got an exam on Friday.

You shouldn''t drink so much cola. It''s bad for your teeth.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'116_', N'B1', N'I really ought to spend less money.

You ought to inform the police.

My parents will be worried. I ought to phone.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'117_', N'B1', N'I need to get back to work.

Do you really need to wear such old jeans?

Do we need to buy tickets before we get on the train?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'119_', N'B1', N'We had a great time in Crete. You should have come with us.

The letter should have come yesterday.

They might have arrived already.

She might have gone home.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'119_', N'B2', N'You shouldn''t have read her journal. It should be private.

You should have asked her earlier. It''s too late now.

He shouldn''t have any problem doing such a simple task. (Assumption)

The plane should have arrived by now. (Assumption)

I knew we might have to pay to get in.

You shouldn''t have shouted at him. He might have hit you.

The weather could have been better but we still had a good time.

You could have told me!')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'119_', N'C1', N'You shouldn''t have told her. She''ll be very upset.

I should have warned him about the traffic, but I forgot.

You might have told me it was her birthday. I felt embarrassed I didn''t take a present.

Things might have turned out differently, if she had asked first.

I don''t think anyone could have done anything. He had decided.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'120_', N'B2', N'It can''t have been John you saw, because he was with me.

What can he have done with the keys? He can''t have lost them again.

You needn''t have bothered getting here on time. He''s always late.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'120_', N'C1', N'He can''t have got my message. He would never be this late.

You needn''t have bought any potatoes. We had some.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'122a', N'A1', N'How much money do you have?

How many sisters do you have?

Do you like cheese?

I bought an apple and some bread.

Mira has very short hair.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'122b', N'A2', N'She has eight chairs in her lounge.

How much furniture does he have?

I need some help/advice.

Would you like a piece of cake/cup of tea?

I need as much information as possible.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'123_', N'A1', N'There''s a bank near the station.

There are a lot of seats at the front.

Is there a supermarket near here?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'125_', N'A1', N'I bought a dictionary.

They live in Newcastle.

Sorry, I dropped it..')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'127_', N'A1', N'This is my seat.

Is this your pen?

That''s our house.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'128_', N'A1', N'It''s Mary''s turn to buy coffee.

The girl''s hair was bright red.

This is the students'' room.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'128_', N'A2', N'I''ll meet you outside Mary''s house.

That''s John''s car.

The children''s clothes are all dirty.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'129_', N'A1', N'This is my laptop.

That is her coat.

No. It''s mine.

Is that their car?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'129_', N'A2', N'That''s not our ball, it''s theirs.

It always wags its tail when it''s happy.

Is Heather a friend of yours?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'131_', N'A1', N'He is sitting at the table.

We went to Sardinia last year.

He comes from Scotland')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'132_', N'A1', N'The holidays begin in July.

They like to play football in the evening.

On Tuesdays she goes to college.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'132a', N'A1', N'I''ll see you in December.

It starts at 6 o''clock.

They lived there for ten years.

My sister is coming on Tuesday.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'132a', N'A2', N'He was born on Christmas Day.

I''ll read the book during the holidays.

The train arrives at 17.15.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'133_', N'A2', N'He went inside the building.

We walked along the beach.

They arrived at the station in the middle of the night.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'133a', N'A1', N'Our shop is on the High Street.

They live in Reading.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'135_', N'A1', N'She has a dog, but I don''t have a pet.

I''d like an apple juice, please.

Your jacket is on the chair.

I live by the sea.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'138_', N'A2', N'She has blonde hair.

I love pizza, but the pizzas at Gino''s are not very good.

I''ve got bad news for you.

Everybody wants coffee.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'138_', N'B1', N'Don''t go in the water. It''s freezing.

I would like milk in my tea.

He was wearing black jeans.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'139_', N'B2', N'History tended to be uninteresting when I was at school.

The early history of Scotland is full of betrayal.

Happiness in marriage is something you have to work at.

Education is not compulsory in many developing countries.

The education I received was first-rate.

Charity begins at home.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'141_', N'A1', N'I need a lot of sleep.

Do you have any cheese?

I''d like some vegetables, please.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'141_', N'A2', N'I don''t have any money.

He spends a lot of time in his garage.

Can I have some water, please?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'142_', N'A2', N'I am going to have a party for a few friends.

We don''t have enough eggs to make our cake.

None of my friends are going to the club.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'143_', N'B1', N'All the seats are taken.

We haven''t got enough paper for everyone.

Several people are waiting.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'145_', N'A1', N'She is wearing a red skirt.

That''s a beautiful phone.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'146_', N'A1', N'This pizza is really good.

What did that man say?

Those oranges look very nice.

These people want to talk to us.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'146_', N'A2', N'Those children over there are very noisy.

These shoes are killing me.

He left for the city on 19th February. That night the volcano erupted.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'147_', N'A2', N'The film was really boring.

Her story was really amusing.

The journey was really exciting.

The crowd was already excited.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'148_', N'B1', N'I didn''t want to wake him from his deep sleep.

The student produced some really high quality work.

We couldn''t get to work because of the heavy snow.

There was a strong smell of coffee in the room.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'149a', N'A1', N'She''s taller than Michelle.

I am better at writing.

Tom is the oldest in the class.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'149b', N'A2', N'This book is more interesting than these ones.

My sister is much older than me')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'149c', N'A2', N'The fastest mammal in the world is the cheetah.

Maths is the most difficult subject for me.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'152_', N'A1', N'We always go shopping on Saturdays.

We sometimes meet Susan here.

I never go to the gym after work.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'152_', N'A2', N'Have you ever been to the United States?

He often visits his family.

He usually stays here with us.

He always carries a bag.

She hardly ever leaves her room.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'153_', N'A2', N'There''s water everywhere.

He quickly opened the door.

I am going to London tomorrow.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'154_', N'A2', N'He went home yesterday.

They were here today.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'154_', N'B1', N'We usually go abroad in summer.

I have never been abroad.

He stayed behind yesterday.

They often play upstairs.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'155_', N'B1', N'She''s a good singer. She sings really well.

The instructions were not very clear.

My mother has been working too hard recently.

This cheese is a bit hard.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'155_', N'B2', N'He scored a direct hit.

The train goes direct to London without even stopping at York.

There''s no such thing as a free lunch.

Feel free to use it whenever you want.

He went straight to work.

Next draw a straight line across the top of the paper.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'156_', N'B1', N'The water was extremely cold.

He speaks very quickly.

He speaks too quickly.

There will probably be some speeches after the meal.

He''ll definitely win.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'157_', N'B1', N'Paula got ready more quickly than the others.

Jenson Button was faster in practice.

I''m afraid he''s getting worse.

The person who most frequently got ill was Angela.

Stig worked the hardest.

Marie did the worst in the exam.

Paulo did the best at maths.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'158_', N'B2', N'Frankly, I couldn''t care less

Clearly, he was in the wrong.

Apparently, he was in line for promotion.

Fortunately, he had a spare pair of shoes with him.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'159_', N'C1', N'Little did I know that he had already left the company.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'161_', N'A1', N'She''s a very tall girl.

John is a really good friend.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'161_', N'A2', N'She was left very unhappy.

I am really sorry for losing your book.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'162_', N'A2', N'The water is quite cold.

I am so happy with my new flat.

It is getting a bit cold now. I want to go home.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'163_', N'B1', N'He''s a really good driver.

Do we have enough cake to go round?

He came back so suddenly.

She''s so intelligent it''s scary.

The ball was just too fast.

He''s quite good at science.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'164_', N'B1', N'We did quite well.

I''ve got a terribly difficult decision to make.

The maths test was unbelievably easy.

That''s much too difficult for a B1 test.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'165_', N'B2', N'After working all day and all night he was totally exhausted.

He was absolutely horrified when he realised what he had done.

She''s completely hopeless when it comes to housework.

I am entirely satisfied that he followed the correct procedure.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'167_', N'A1', N'Pedro is Spanish but he works in France.

She comes from China but her husband is English.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'168_', N'A1', N'She''s married and has three children.

I am 26 years old, single and I work in a bank.

He''s an engineer.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'169_', N'A1', N'I like fresh fruit for breakfast.

Vegetables are good for you.

What kind of coffee do you want?

I have a cup of tea every morning.

I don''t like fish.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'169_', N'A2', N'I love strawberries and cream.

Let''s get some fish and chips.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'170_', N'A1', N'Where is the supermarket?

How much does this cost?

Where is the nearest internet caf''?

Where can I buy a . . . ?

I''m looking for a bank/chemist.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'170_', N'A2', N'Keep left.

Insert exact money.

Do you know where the post office is? I want to buy some stamps.

Where can I buy some coffee beans?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'170_', N'B1', N'It fits really well but do you think it suits me?

I need to get some toothpaste from the chemist''s in the shopping mall.

Can you hold on while I get a magazine from this newsagent?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'171_', N'A1', N'Does this bus go to the town centre?

I want to buy a phone.

Where is the train station?')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'171_', N'A2', N'A return ticket to Brighton, please.

Can you tell me the way to IKEA?

What time do you close?

I''m looking for the bus station.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'171_', N'B1', N'I''m sorry, we don''t accept cheques. Do you have a debit or credit card?

You need to check in at least two hours before departure.

The gate number will be announced on the monitor in the departure lounge.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'172_', N'A1', N'He is a student.

They live in Brighton.

I work in a factory.

They like shopping.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'173_', N'A1', N'You can''t wear jeans at work.

I bought a new T-shirt.

I don''t like wearing skirts or dresses. I prefer jeans.

My father wears a suit and tie to work.

Is it cotton?

I lost my new leather jacket.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'174_', N'A1', N'My favourite colours are red and green.

He always wears black.

The houses near the sea are all blue or pink.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'175_', N'A1', N'My room is very small.

It''s a long street.

Scottish mountains are not very high.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'176_', N'A1', N'My friends get the bus to work but I take the train.

I usually fly to France, but sometimes I drive.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'177_', N'A2', N'The fruit is in a bowl in the dining room.

The kitchen is the warmest room in the house.

The tools are in the garage.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'178_', N'A2', N'Pedro is a quiet and serious boy.

She is tall, blonde and wears very smart clothes.

I am very happy with my new job, but my boss is very strict.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'179_', N'B1', N'On the one hand, he is good with people. On the other hand he does not think before he speaks.

Even though he earns very little he is always very generous.

Mind you, he is still very fit.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'179_', N'B2', N'The weather forecast is good. Nevertheless, you always need to be careful in the mountains.

In spite of his age, he is still goes camping in the wild.

The story has been told many times before. Nonetheless, it is still a warning to us all.

Some students continue to live with their parents. However, I prefer to be independent.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'180_', N'B2', N'In a word, it was a disaster.

We felt that the idea was in general a good one; the more we spend on advertising the higher our sales will be.

To sum up, if we can''t make more money some people will have to lose their jobs')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'181_', N'B1', N'I''m going to take a quick shower.

Its midnight but I still feel wide awake.

There''s a good chance he''ll be late.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'181_', N'B2', N'I''d prefer a dry wine.

The resort has a range of luxury accommodation to offer.

He''s a very heavy smoker.

I''m retaking the exam next week.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'181_', N'C1', N'It''s there in black and white.

He was in excruciating pain.

The suspense is palpable.

I did physics at university.

The situation is untenable.

It''s a no go area.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'182_', N'B1', N'That''s a really cool top you''re wearing.

My boss is nice but he talks really posh.

The kids had a brilliant time at the zoo.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'182_', N'B2', N'She''s just been dumped by her boyfriend.

There''s no hurry. Let''s just chill out for an hour or two.

I can''t be bothered with the hassle.

She fell and landed on her bum.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'182_', N'C1', N'I am absolutely knackered.

She was gobsmacked when he turned up at the party.

The whole thing was a cock-up from beginning to end.

He tried to flog me an old banger.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'183_', N'C1', N'There will be about 30 odd people - well 30 to 40.

I think he an accountant or something like that.

The book is sort of similar to his first one.

Can you pass me the thingummyjig for taking nails out?

All the painting stuff is in the garage.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'184_', N'C1', N'She was really upset when she failed her exams. I think she is still in shock.

It''s not that I don''t like her; I detest her.

It''s really good. It''s concise, focused, readable.

I wouldn''t say she''s antisocial, just a bit shy.

It wasn''t bad, just a bit disappointing.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'186_', N'C1', N'In the chemistry class they performed an interesting experiment (as opposed to experience which is French for experiment)')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'187_', N'C1', N'Mr. John Wilson passed away peacefully at his home in Nottingham last week.

John Wilson died in his sleep last week

John kicked the bucket a few days back.')
INSERT [dbo].[libExponents] ([CategoryId], [ReferenceLevel], [Text]) VALUES (N'188_', N'C1', N'I wish I could remember her name. It''s on the tip of my tongue.

Everybody wants work with Marion. She really is the flavour of the month.

If you want a shoulder to cry on, I''ll always be here for you.')




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