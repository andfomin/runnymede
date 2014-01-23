var App;
(function (App) {
    (function (Exercises_Topics) {
        var Ctrl = (function () {
            function Ctrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                $scope.vm = this;
                this.topics = Exercises_Topics.TopicList.topics;
                this.ownTopic = { topic: 'own', title: null, lines: null };
            }
            Ctrl.prototype.isOwnTopic = function () {
                return this.selectedTopic === this.ownTopic.topic;
            };

            Ctrl.prototype.save = function () {
                var topic;

                if (this.isOwnTopic()) {
                    topic = this.ownTopic;
                } else {
                    var selected = this.selectedTopic;
                    this.topics.some(function (element, index, array) {
                        var found = element.topic === selected;
                        if (found) {
                            topic = element;
                        }
                        ;
                        return found;
                    });
                }

                if (topic) {
                    App.Utils.ngHttpPost(this.$http, App.Utils.exercisesApiUrl('SaveTopic'), {
                        type: 'text',
                        title: topic.title,
                        lines: topic.lines
                    }, function (data) {
                        window.location.assign(App.Utils.exercisesUrl('record?topic=' + data.id));
                    });
                }
            };
            Ctrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return Ctrl;
        })();
        Exercises_Topics.Ctrl = Ctrl;
    })(App.Exercises_Topics || (App.Exercises_Topics = {}));
    var Exercises_Topics = App.Exercises_Topics;
})(App || (App = {}));

var app = angular.module('app', []);
app.controller('Ctrl', App.Exercises_Topics.Ctrl);
//# sourceMappingURL=topics-ctrl.js.map
