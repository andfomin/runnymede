module App.Exercises_Topics {

    export class Ctrl {

        public topics: ITopic[];
        ownTopic: ITopic;
        selectedTopic: string;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.topics = TopicList.topics;
            this.ownTopic = { topic: 'own', title: null, lines: null };
        } // end of ctor

        isOwnTopic() {
            return this.selectedTopic === this.ownTopic.topic;
        }

        save() {
            var topic: ITopic;

            if (this.isOwnTopic()) {
                topic = this.ownTopic;
            }
            else {
                var selected = this.selectedTopic;
                this.topics.some(function (element, index, array) {
                    var found = element.topic === selected;
                    if (found) {
                        topic = element;
                    };
                    return found;
                });
            }

            if (topic) {
                App.Utils.ngHttpPost(this.$http,
                    Utils.exercisesApiUrl('SaveTopic'),
                    {
                        type: 'text',
                        title: topic.title,
                        lines: topic.lines
                    },
                    (data) => {
                        window.location.assign(App.Utils.exercisesUrl('record?topic=' + data.id));
                    }
                    );
            }

        }
    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Exercises_Topics.Ctrl);
