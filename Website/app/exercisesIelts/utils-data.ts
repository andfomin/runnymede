module app.exercisesIelts {

    export interface IDescriptor {
        path: string;
        text: string;
        group?: string;
        band?: number;
    };

    //export interface IDescriptorGroup {
    //    path: string;
    //    descs: IDescriptor[];
    //};

    export interface ICriterionArea {
        path: string;
        title: string;
    };

    //export interface ITestComponent {
    //    path: string;
    //    title: string;
    //    areas: ICriterionArea[];
    //};

    //export var criterionAreas: ICriterionArea[] = [
    //    {
    //        path: 'SVRIW1/TA',
    //        title: 'Task achievement',
    //    },
    //    {
    //        path: 'SVRIW1/CC',
    //        title: 'Coherence and cohesion',
    //    },
    //    {
    //        path: 'SVRIW1/LR',
    //        title: 'Lexical resource',
    //    },
    //    {
    //        path: 'SVRIW1/GR',
    //        title: 'Grammatical range and accuracy',
    //    },
    //    {
    //        path: 'SVRIW2/TR',
    //        title: 'Task response',
    //    },
    //    {
    //        path: 'SVRIW2/CC',
    //        title: 'Coherence and cohesion',
    //    },
    //    {
    //        path: 'SVRIW2/LR',
    //        title: 'Lexical resource',
    //    },
    //    {
    //        path: 'SVRIW2/GR',
    //        title: 'Grammatical range and accuracy',
    //    },
    //    {
    //        path: 'SVRIS_/FC',
    //        title: 'Fluency and coherence',
    //    },
    //    {
    //        path: 'SVRIS_/LR',
    //        title: 'Lexical resource',
    //    },
    //    {
    //        path: 'SVRIS_/GR',
    //        title: 'Grammatical range and accuracy',
    //    },
    //    {
    //        path: 'SVRIS_/P_',
    //        title: 'Pronunciation',
    //    },
    //];

    export var criterionAreaTitles = {
        'SVRIW1/TA': 'Task achievement',
        'SVRIW1/CC': 'Coherence and cohesion',
        'SVRIW1/LR': 'Lexical resource',
        'SVRIW1/GR': 'Grammatical range and accuracy',
        'SVRIW2/TR': 'Task response',
        'SVRIW2/CC': 'Coherence and cohesion',
        'SVRIW2/LR': 'Lexical resource',
        'SVRIW2/GR': 'Grammatical range and accuracy',
        'SVRIS_/FC': 'Fluency and coherence',
        'SVRIS_/LR': 'Lexical resource',
        'SVRIS_/GR': 'Grammatical range and accuracy',
        'SVRIS_/P_': 'Pronunciation',
    };

    export var descsW1: IDescriptor[] = [
        // W1/TA - Task achievement
        { path: 'SVRIW1/TA/0/8', text: 'covers all requirements of the task sufficiently' },
        { path: 'SVRIW1/TA/0/7', text: 'covers the requirements of the task' },
        { path: 'SVRIW1/TA/0/6', text: 'addresses the requirements of the task' },
        { path: 'SVRIW1/TA/0/5', text: 'generally addresses the task; the format may be inappropriate in places' },
        { path: 'SVRIW1/TA/0/4', text: 'attempts to address the task but does not cover all key features/bullet points; the format may be inappropriate' },

        { path: 'SVRIW1/TA/1/8', text: 'presents, highlights and illustrates key features/ bullet points clearly and appropriately' },
        { path: 'SVRIW1/TA/1/7', text: 'clearly presents and highlights key features/bullet points' },
        { path: 'SVRIW1/TA/1/6', text: 'presents and adequately highlights key features/ bullet points but details may be irrelevant, inappropriate or inaccurate' },
        { path: 'SVRIW1/TA/1/5', text: 'presents, but inadequately covers, key features/ bullet points; there may be a tendency to focus on details' },
        { path: 'SVRIW1/TA/1/4', text: 'may confuse key features/ bullet points with detail; parts may be unclear, irrelevant, repetitive or inaccurate' },

        { path: 'SVRIW1/TA/2/7', text: '(A) presents a clear overview of main trends, differences or stages' },
        { path: 'SVRIW1/TA/2/6', text: '(A) presents an overview with information appropriately selected' },
        { path: 'SVRIW1/TA/2/5', text: '(A) recounts detail mechanically with no clear overview; there may be no data to support the description' },

        { path: 'SVRIW1/TA/3/7', text: '(GT) presents a clear purpose, with the tone consistent and appropriate' },
        { path: 'SVRIW1/TA/3/6', text: '(GT) presents a purpose that is generally clear; there may be inconsistencies in tone' },
        { path: 'SVRIW1/TA/3/5', text: '(GT) may present a purpose for the letter that is unclear at times; the tone may be variable and sometimes inappropriate' },
        { path: 'SVRIW1/TA/3/4', text: '(GT) fails to clearly explain the purpose of the letter; the tone may be inappropriate' },

        // W1/CC - Coherence and cohesion
        { path: 'SVRIW1/CC/0/8', text: 'sequences information and ideas logically' },
        { path: 'SVRIW1/CC/0/7', text: 'logically organises information and ideas; there is clear progression throughout' },
        { path: 'SVRIW1/CC/0/6', text: 'arranges information and ideas coherently and there is a clear overall progression' },
        { path: 'SVRIW1/CC/0/5', text: 'presents information with some organisation but there may be a lack of overall progression' },
        { path: 'SVRIW1/CC/0/4', text: 'presents information and ideas but these are not arranged coherently and there is no clear progression in the response' },

        { path: 'SVRIW1/CC/1/8', text: 'manages all aspects of cohesion well' },
        { path: 'SVRIW1/CC/1/7', text: 'uses a range of cohesive devices appropriately although there may be some under-/over-use' },
        { path: 'SVRIW1/CC/1/6', text: 'uses cohesive devices effectively, but cohesion within and/or between sentences may be faulty or mechanical' },
        { path: 'SVRIW1/CC/1/5', text: 'makes inadequate, inaccurate or over-use of cohesive devices' },
        { path: 'SVRIW1/CC/1/4', text: 'uses some basic cohesive devices but these may be inaccurate or repetitive' },

        { path: 'SVRIW1/CC/2/8', text: 'uses paragraphing sufficiently and appropriately' },

        { path: 'SVRIW1/CC/3/6', text: 'may not always use referencing clearly or appropriately' },
        { path: 'SVRIW1/CC/3/5', text: 'may be repetitive because of lack of referencing and substitution' },

        // W1/LR - Lexical resource
        { path: 'SVRIW1/LR/0/8', text: 'uses a wide range of vocabulary fluently and flexibly to convey precise meanings' },
        { path: 'SVRIW1/LR/0/7', text: 'uses a sufficient range of vocabulary to allow some flexibility and precision' },
        { path: 'SVRIW1/LR/0/6', text: 'uses an adequate range of vocabulary for the task' },
        { path: 'SVRIW1/LR/0/5', text: 'uses a limited range of vocabulary, but this is minimally adequate for the task' },
        { path: 'SVRIW1/LR/0/4', text: 'uses only basic vocabulary which may be used repetitively or which may be inappropriate for the task' },

        { path: 'SVRIW1/LR/1/8', text: 'skilfully uses uncommon lexical items but there may be occasional inaccuracies in word choice and collocation' },
        { path: 'SVRIW1/LR/1/7', text: 'uses less common lexical items with some awareness of style and collocation' },
        { path: 'SVRIW1/LR/1/6', text: 'attempts to use less common vocabulary but with some inaccuracy' },

        { path: 'SVRIW1/LR/2/8', text: 'produces rare errors in spelling and/or word formation' },
        { path: 'SVRIW1/LR/2/7', text: 'may produce occasional errors in word choice, spelling and/or word formation' },
        { path: 'SVRIW1/LR/2/6', text: 'makes some errors in spelling and/or word formation, but they do not impede communication' },
        { path: 'SVRIW1/LR/2/5', text: 'may make noticeable errors in spelling and/or word formation that may cause some difficulty for the reader' },
        { path: 'SVRIW1/LR/2/4', text: 'has limited control of word formation and/or spelling; errors may cause strain for the reader' },

        // W1/GR - Grammatical range and accuracy
        { path: 'SVRIW1/GR/0/8', text: 'uses a wide range of structures' },
        { path: 'SVRIW1/GR/0/7', text: 'uses a variety of complex structures' },
        { path: 'SVRIW1/GR/0/6', text: 'uses a mix of simple and complex sentence forms' },
        { path: 'SVRIW1/GR/0/5', text: 'uses only a limited range of structures' },
        { path: 'SVRIW1/GR/0/4', text: 'uses only a very limited range of structures with only rare use of subordinate clauses' },

        { path: 'SVRIW1/GR/1/8', text: 'the majority of sentences are error-free' },
        { path: 'SVRIW1/GR/1/7', text: 'produces frequent error-free sentences' },
        { path: 'SVRIW1/GR/1/5', text: 'attempts complex sentences but these tend to be less accurate than simple sentences' },

        { path: 'SVRIW1/GR/2/8', text: 'makes only very occasional errors or inappropriacies' },
        { path: 'SVRIW1/GR/2/7', text: 'has good control of grammar and punctuation but may make a few errors' },
        { path: 'SVRIW1/GR/2/6', text: 'makes some errors in grammar and punctuation but they rarely reduce communication' },
        { path: 'SVRIW1/GR/2/5', text: 'may make frequent grammatical errors and punctuation may be faulty; errors can cause some difficulty for the reader' },
        { path: 'SVRIW1/GR/2/4', text: 'some structures are accurate but errors predominate, and punctuation is often faulty' },
    ];

    export var descsW2: IDescriptor[] = [
        // W2/TR - Task response
        { path: 'SVRIW2/TR/0/8', text: 'sufficiently addresses all parts of the task' },
        { path: 'SVRIW2/TR/0/7', text: 'addresses all parts of the task' },
        { path: 'SVRIW2/TR/0/6', text: 'addresses all parts of the task although some parts may be more fully covered than others' },
        { path: 'SVRIW2/TR/0/5', text: 'addresses the task only partially; the format may be inappropriate in places' },
        { path: 'SVRIW2/TR/0/4', text: 'responds to the task only in a minimal way or the answer is tangential; the format may be inappropriate' },

        { path: 'SVRIW2/TR/1/8', text: 'presents a well-developed response to the question' },
        { path: 'SVRIW2/TR/1/7', text: 'presents a clear position throughout the response' },
        { path: 'SVRIW2/TR/1/6', text: 'presents a relevant position although the conclusions may become unclear or repetitive' },
        { path: 'SVRIW2/TR/1/5', text: 'expresses a position but the development is not always clear and there may be no conclusions drawn' },
        { path: 'SVRIW2/TR/1/4', text: 'presents a position but this is unclear' },

        { path: 'SVRIW2/TR/2/8', text: 'presents relevant, extended and supported ideas' },
        { path: 'SVRIW2/TR/2/7', text: 'presents, extends and supports main ideas, but there may be a tendency to over-generalise and/or supporting ideas may lack focus' },
        { path: 'SVRIW2/TR/2/6', text: 'presents relevant main ideas but some may be inadequately developed/unclear' },
        { path: 'SVRIW2/TR/2/5', text: 'presents some main ideas but these are limited and not sufficiently developed; there may be irrelevant detail' },
        { path: 'SVRIW2/TR/2/4', text: 'presents some main ideas but these are difficult to identify and may be repetitive, irrelevant or not well supported' },

        // W2/CC - Coherence and cohesion
        { path: 'SVRIW2/CC/0/8', text: 'sequences information and ideas logically' },
        { path: 'SVRIW2/CC/0/7', text: 'logically organises information and ideas; there is clear progression throughout' },
        { path: 'SVRIW2/CC/0/6', text: 'arranges information and ideas coherently and there is a clear overall progression' },
        { path: 'SVRIW2/CC/0/5', text: 'presents information with some organisation but there may be a lack of overall progression' },
        { path: 'SVRIW2/CC/0/4', text: 'presents information and ideas but these are not arranged' },

        { path: 'SVRIW2/CC/1/8', text: 'manages all aspects of cohesion well' },
        { path: 'SVRIW2/CC/1/7', text: 'uses a range of cohesive devices appropriately although there may be some under-/over-use' },
        { path: 'SVRIW2/CC/1/6', text: 'uses cohesive devices effectively, but cohesion within and/or between sentences may be faulty or mechanical' },
        { path: 'SVRIW2/CC/1/5', text: 'makes inadequate, inaccurate or over-use of cohesive devices' },
        { path: 'SVRIW2/CC/1/4', text: 'uses some basic cohesive devices but these may be inaccurate or repetitive' },

        { path: 'SVRIW2/CC/2/8', text: 'uses paragraphing sufficiently and appropriately' },
        { path: 'SVRIW2/CC/2/7', text: 'presents a clear central topic within each paragraph' },
        { path: 'SVRIW2/CC/2/6', text: 'uses paragraphing, but not always logically' },
        { path: 'SVRIW2/CC/2/5', text: 'may not write in paragraphs, or paragraphing may be inadequate' },
        { path: 'SVRIW2/CC/2/4', text: 'may not write in paragraphs or their use may be confusing' },

        { path: 'SVRIW2/CC/3/6', text: 'may not always use referencing clearly or appropriately' },
        { path: 'SVRIW2/CC/3/5', text: 'may be repetitive because of lack of referencing and substitution' },

        // W2/LR - Lexical resource
        { path: 'SVRIW2/LR/0/8', text: 'uses a wide range of vocabulary fluently and flexibly to convey precise meanings' },
        { path: 'SVRIW2/LR/0/7', text: 'uses a sufficient range of vocabulary to allow some flexibility and precision' },
        { path: 'SVRIW2/LR/0/6', text: 'uses an adequate range of vocabulary for the task' },
        { path: 'SVRIW2/LR/0/5', text: 'uses a limited range of vocabulary, but this is minimally adequate for the task' },
        { path: 'SVRIW2/LR/0/4', text: 'uses only basic vocabulary which may be used repetitively or which may be inappropriate for the task' },

        { path: 'SVRIW2/LR/1/8', text: 'skilfully uses uncommon lexical items but there may be occasional inaccuracies in word choice and collocation' },
        { path: 'SVRIW2/LR/1/7', text: 'uses less common lexical items with some awareness of style and collocation' },
        { path: 'SVRIW2/LR/1/6', text: 'attempts to use less common vocabulary but with some inaccuracy' },

        { path: 'SVRIW2/LR/2/8', text: 'produces rare errors in spelling and/or word formation' },
        { path: 'SVRIW2/LR/2/7', text: 'may produce occasional errors in word choice, spelling and/or word formation' },
        { path: 'SVRIW2/LR/2/6', text: 'makes some errors in spelling and/or word formation, but they do not impede communication' },
        { path: 'SVRIW2/LR/2/5', text: 'may make noticeable errors in spelling and/or word formation that may cause some difficulty for the reader' },
        { path: 'SVRIW2/LR/2/4', text: 'has limited control of word formation and/or spelling; errors may cause strain for the reader' },

        // W2/GR - Grammatical range and accuracy
        { path: 'SVRIW2/GR/0/8', text: 'uses a wide range of structures' },
        { path: 'SVRIW2/GR/0/7', text: 'uses a variety of complex structures' },
        { path: 'SVRIW2/GR/0/6', text: 'uses a mix of simple and complex sentence forms' },
        { path: 'SVRIW2/GR/0/5', text: 'uses only a limited range of structures' },
        { path: 'SVRIW2/GR/0/4', text: 'uses only a very limited range of structures with only rare use of subordinate clauses' },

        { path: 'SVRIW2/GR/1/8', text: 'the majority of sentences are error-free' },
        { path: 'SVRIW2/GR/1/7', text: 'produces frequent error-free sentences' },
        { path: 'SVRIW2/GR/1/5', text: 'attempts complex sentences but these tend to be less accurate than simple sentences' },
        { path: 'SVRIW2/GR/1/4', text: 'uses only a very limited range of structures with only rare use of subordinate clauses' },

        { path: 'SVRIW2/GR/2/8', text: 'makes only very occasional errors or inappropriacies' },
        { path: 'SVRIW2/GR/2/7', text: 'has good control of grammar and punctuation but may make a few errors' },
        { path: 'SVRIW2/GR/2/6', text: 'makes some errors in grammar and punctuation but they rarely reduce communication' },
        { path: 'SVRIW2/GR/2/5', text: 'may make frequent grammatical errors and punctuation may be faulty; errors can cause some difficulty for the reader' },
        { path: 'SVRIW2/GR/2/4', text: 'some structures are accurate but errors predominate, and punctuation is often faulty' },
    ];

    export var descsS_: IDescriptor[] = [
        // S_/FC - Fluency and coherence
        { path: 'SVRIS_/FC/0/8', text: 'speaks fluently with only occasional repetition or self-correction; hesitation is usually content-related and only rarely to search for language' },
        { path: 'SVRIS_/FC/0/7', text: 'speaks at length without noticeable effort or loss of coherence; may demonstrate language-related hesitation at times, or some repetition and/or self-correction' },
        { path: 'SVRIS_/FC/0/6', text: 'is willing to speak at length, though may lose coherence at times due to occasional repetition, self-correction or hesitation' },
        { path: 'SVRIS_/FC/0/5', text: 'usually maintains flow of speech but uses repetition, self correction and/or slow speech to keep going; produces simple speech fluently, but more complex communication causes fluency problems' },
        { path: 'SVRIS_/FC/0/4', text: 'cannot respond without noticeable pauses and may speak slowly, with frequent repetition and self-correction' },

        { path: 'SVRIS_/FC/1/8', text: 'develops topics coherently and appropriately' },
        { path: 'SVRIS_/FC/1/7', text: 'uses a range of connectives and discourse markers with some flexibility' },
        { path: 'SVRIS_/FC/1/6', text: 'uses a range of connectives and discourse markers but not always appropriately' },
        { path: 'SVRIS_/FC/1/5', text: 'may over-use certain connectives and discourse markers' },
        { path: 'SVRIS_/FC/1/4', text: 'links basic sentences but with repetitious use of simple connectives and some breakdowns in coherence' },

        // S_/LR - Lexical resource
        { path: 'SVRIS_/LR/0/8', text: 'uses a wide vocabulary resource readily and flexibly to convey precise meaning' },
        { path: 'SVRIS_/LR/0/7', text: 'uses vocabulary resource flexibly to discuss a variety of topics' },
        { path: 'SVRIS_/LR/0/6', text: 'has a wide enough vocabulary to discuss topics at length and make meaning clear in spite of inappropriacies' },
        { path: 'SVRIS_/LR/0/5', text: 'manages to talk about familiar and unfamiliar topics but uses vocabulary with limited flexibility' },
        { path: 'SVRIS_/LR/0/4', text: 'is able to talk about familiar topics but can only convey basic meaning on unfamiliar topics and makes frequent errors in word choice' },

        { path: 'SVRIS_/LR/1/8', text: 'uses less common and idiomatic vocabulary skilfully, with occasional inaccuracies' },
        { path: 'SVRIS_/LR/1/7', text: 'uses some less common and idiomatic vocabulary and shows some awareness of style and collocation, with some inappropriate choices' },

        { path: 'SVRIS_/LR/2/8', text: 'uses paraphrase effectively as required' },
        { path: 'SVRIS_/LR/2/7', text: 'uses paraphrase effectively' },
        { path: 'SVRIS_/LR/2/6', text: 'generally paraphrases successfully' },
        { path: 'SVRIS_/LR/2/5', text: 'attempts to use paraphrase but with mixed success' },
        { path: 'SVRIS_/LR/2/4', text: 'rarely attempts paraphrase' },

        // S_/GR - Grammatical range and accuracy
        { path: 'SVRIS_/GR/0/8', text: 'uses a wide range of structures flexibly' },
        { path: 'SVRIS_/GR/0/7', text: 'uses a range of complex structures with some flexibility' },
        { path: 'SVRIS_/GR/0/6', text: 'uses a mix of simple and complex structures, but with limited flexibility' },
        { path: 'SVRIS_/GR/0/5', text: 'produces basic sentence forms with reasonable accuracy' },
        { path: 'SVRIS_/GR/0/4', text: 'produces basic sentence forms and some correct simple sentences but subordinate structures are rare' },

        { path: 'SVRIS_/GR/1/8', text: 'produces a majority of error-free sentences with only very occasional inappropriacies or basic/non-systematic errors' },
        { path: 'SVRIS_/GR/1/7', text: 'frequently produces error-free sentences, though some grammatical mistakes persist' },
        { path: 'SVRIS_/GR/1/6', text: 'may make frequent mistakes with complex structures though these rarely cause comprehension problems' },
        { path: 'SVRIS_/GR/1/5', text: 'uses a limited range of more complex structures, but these usually contain errors and may cause some comprehension problems' },
        { path: 'SVRIS_/GR/1/4', text: 'errors are frequent and may lead to misunderstanding' },

        // S_/P_ - Pronunciation
        { path: 'SVRIS_/P_/0/8', text: 'uses a wide range of pronunciation features' },
        { path: 'SVRIS_/P_/0/6', text: 'uses a range of pronunciation features with mixed control' },
        { path: 'SVRIS_/P_/0/4', text: 'uses a limited range of pronunciation features' },

        { path: 'SVRIS_/P_/1/8', text: 'sustains flexible use of features, with only occasional lapses' },
        { path: 'SVRIS_/P_/1/6', text: 'shows some effective use of features but this is not sustained' },
        { path: 'SVRIS_/P_/1/4', text: 'attempts to control features but lapses are frequent' },

        { path: 'SVRIS_/P_/2/8', text: 'is easy to understand throughout; L1 accent has minimal effect on intelligibility' },
        { path: 'SVRIS_/P_/2/6', text: 'can generally be understood throughout, though mispronunciation of individual words or sounds reduces clarity at times' },
        { path: 'SVRIS_/P_/2/4', text: 'mispronunciations are frequent and cause some difficulty for the listener' },
    ];

}