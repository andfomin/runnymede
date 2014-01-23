var App;
(function (App) {
    (function (Exercises_Topics) {
        // The topic list is quite static, so we wrap it in a JavaScript file to use the browser caching capability.
        var TopicList = (function () {
            function TopicList() {
            }
            TopicList.topics = [
                { topic: "you", title: "You", lines: ["Describe yourself."] },
                { topic: "family", title: "Family", lines: ["Describe your family.", "Do you have a large or small family?", "How much time do you spend with your family?", "What do you like to do together as a family?", "Do you get along well with your family?", "Are people in your country generally close to their families?"] },
                { topic: "work", title: "Work", lines: ["What do you do?", "What are your responsibilities?", "How many hours do you work each day?", "Do you enjoy your work?", "Is there some other kind of work you would rather do?", "If you could change your job or profession, what would you do?", "Describe the process of getting a job in your country.", "Describe the company or organization you work for.", "What is your position?", "What do you like about your job?", "What do you dislike about your job?"] },
                { topic: "education", title: "Education", lines: ["Describe your education.", "What kind of school did you go to as a child?", "Did you go to a co-educational school?", "What was your favourite subject as a child?", "Who was your favourite teacher?", "What is the education system like in your country?", "Do you think your country has an effective education system?"] },
                { topic: "studies", title: "Studies", lines: ["What are you studying now?", "What is your area of specialization?"] },
                { topic: "hometown", title: "Hometown", lines: ["Describe your hometown.", "What's special about it?", "Where is your hometown located?", "Is it easy to travel around your hometown?", "What is it known for?", "What do people in your town do?", "What are the main industries in your hometown?", "What problems face your hometown?", "What languages are spoken in your hometown?", "What are the advantages of living in your hometown?", "What are some problems faced by your hometown?", "Compare your hometown with another city.", "What are some environmental problems faced by your hometown?"] },
                { topic: "weather", title: "Weather", lines: ["What's the weather like in your country?", "Does the weather affect your mood?", "How do rainy days make you feel?", "What's your favourite season of the year?", "What do you like to do when it's hot?", "What do you usually do in the winter?", "How many seasons does your country have?"] },
                { topic: "home", title: "Home", lines: ["Describe your home.", "What kind of building do you live in?", "How long have you lived there?"] },
                { topic: "wedding", title: "Wedding", lines: ["Have you ever been to a wedding?", "Whose wedding was it?", "Where was it held?", "What clothes do people wear?", "Describe the wedding ceremony.", "What sort if gifts do people buy for the bridal couple?", "What kind of clothes did the bride and groom wear?"] },
                { topic: "travel", title: "Travel", lines: ["Do you like to travel?", "What kind of places have you visited in your life?", "Which place would you really like to visit? Why?", "What's the best place you've ever visited?"] },
                { topic: "computers", title: "Computers", lines: ["Do you think computers help society?", "Do you think computers are bad for health?", "How do you think computers have changed the world?"] },
                { topic: "internet", title: "Internet", lines: ["Do you use the Internet much during the day?", "What do you usually do on the Internet?", "What are some advantages of the Internet?", "What are some disadvantages?", "Do people in your country use the Internet a lot?", "Do you do any shopping on the Internet?"] },
                { topic: "email", title: "Email", lines: ["Do you send and receive email regularly?", "Who do you usually communicate with?", "How often do you check your email?", "Do you think writing email has strengthened or weakened people's writing skills?", "What are some disadvantages of email?"] },
                { topic: "friend", title: "Friend", lines: ["Describe a friend.", "How long have you known each other?", "What do usually do together?", "What do you like the most about him / her?", "How often do you see each other?"] },
                { topic: "place", title: "Place", lines: ["Describe a place you like to go.", "Why is this place special to you?", "When did you first visit this place?", "Where is this place located?", "What language is spoken here? Do you speak this language?"] },
                { topic: "smoking", title: "Smoking", lines: ["What do you feel about smoking in public places?", "Do you think smoking should be banned in people's homes?"] },
                { topic: "marriage", title: "Marriage", lines: ["What is the attitude toward marriage in your country?", "Do most young people plan on getting married in your country?", "What are some of the advantages of marriage?", "What are some of the disadvantages?", "Is the divorce rate high in your country?", "Do you think people should be allowed to get divorced?"] },
                { topic: "hobbies", title: "Hobbies", lines: ["Do you have any hobbies?", "What are some of your hobbies?", "When did you first develop tis hobby?", "What are some of the advantages of having a hobby?", "How much time do you spend on your hobby?"] },
                { topic: "films", title: "Films", lines: ["Do you enjoy watching movies?", "What's your favourite film?", "Who are your favourite actors?", "How often do you watch films?"] }
            ];
            return TopicList;
        })();
        Exercises_Topics.TopicList = TopicList;
    })(App.Exercises_Topics || (App.Exercises_Topics = {}));
    var Exercises_Topics = App.Exercises_Topics;
})(App || (App = {}));
//# sourceMappingURL=topics-topicList.js.map
