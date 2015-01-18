module app.library {

    export interface IRootCategory extends ICategory {
        active: boolean;
    }

    export interface IPersonalCategory extends ICategory {
        personal: boolean;
    }

    export class Index extends app.CtrlBase {

        categoriesL1: IRootCategory[] = [];
        categoriesL1KnownState: string;
        selectedL1: ICategory = null;
        selectedL2: ICategory = null;
        selectedL3: ICategory = null;
        filter: ICategory = null;
        query: string;
        isEmpty: boolean = false;

        // These state names are used in StateConfig and as URL path in pgLoad(). They correspond to routes in Runnymede.Website.Controllers.Api.LibraryApiController
        static Common = 'common';
        static Personal = 'personal';

        static $inject = [app.ngNames.$scope, app.ngNames.$rootScope, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$window, app.ngNames.$state];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $rootScope: ng.IRootScopeService,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService
            ) {
            /* ----- Constructor  ----- */
            super($scope);

            this.isEmpty = !this.authenticated;

            $rootScope.$on('$stateChangeSuccess', (event, toState, toParams, fromState, fromParams) => {
                if (toState.name === Index.Personal) {
                    this.clear();
                    this.clearList();
                    this.loadPersonalCategoryIds(0);
                    app['hostPageParam'] = HostPage.HostPage_LibraryPersonal;
                }
                else {
                    this.setCategoriesL1();
                    app['hostPageParam'] = HostPage.HostPage_LibraryCommon;
                }
            });

            $scope.$on(ResourceList.PersonalRemoved, (event, args) => { this.onPersonalRemoved(args.resources, args.resource); });

            //Watch when an accordion section is toggled manually by the user.
            $scope.$watch(() => { return angular.toJson(this.categoriesL1); },
                (newValue) => {
                    if (newValue !== this.categoriesL1KnownState) {
                        this.categoriesL1KnownState = newValue;
                        this.selectL1(app.arrFind(this.categoriesL1, (i) => { return i.active; }));
                    }
                });

            /* ----- End of constructor  ----- */
        }

        clear = () => {
            this.categoriesL1.forEach((i) => { i.active = false; });
            this.categoriesL1KnownState = angular.toJson(this.categoriesL1);
            this.selectL1(null);
        }

        clearList = () => {
            this.$rootScope.$broadcast(ResourceList.Clear);
            this.pgReset();
        };

        setCategoriesL1 = () => {
            var all = !this.isPersonal();
            this.categoriesL1 = <IRootCategory[]>Categories.filter((i) => { return (i.level === 1) && (all || (<IPersonalCategory>i).personal); });
            this.clear();
        }

        selectL1 = (category: ICategory) => {
            this.selectedL1 = category;
            this.selectL2(null);
        };

        selectL2 = (category: ICategory) => {
            this.selectedL2 = category;
            this.selectL3(null);
        };

        selectL3 = (category: ICategory) => {
            this.selectedL3 = category;
            this.filter = this.selectedL3 || this.selectedL2 || this.selectedL1;
            this.query = null;
            this.clearList();
            if (this.selectedL3 || (this.isPersonal() && this.authenticated && this.filter)) {
                this.pgLoad();
            }
        };

        getCategories = (category: ICategory) => {
            var all = !this.isPersonal();
            return category
                ? Categories.filter((i) => { return (i.parentId === category.id) && (all || (<IPersonalCategory>i).personal); })
                : null;
        };

        getFilterText = () => {
            var text = this.filter && this.filter.name;
            var n = 40;
            // truncate at the last word boundary.du
            if (text && (text.length > n)) {
                var s_ = text.substr(0, n - 1);
                s_ = s_.substr(0, s_.lastIndexOf(' '));
                return s_ + ' ...';
            }
            else
                return text;
        };

        canSearch = () => {
            return (this.filter || this.query) && !this.busy;
        };

        pgLoad = () => {
            if (!this.busy) {
                this.busy = true;
                app.ngHttpGet(this.$http,
                    app.libraryApiUrl(this.$state.current.name),
                    {
                        offset: this.pgOffset(),
                        limit: this.pgLimit,
                        categoryId: this.filter && this.filter.id,
                        q: this.query,
                    },
                    (data) => {
                        if (data && angular.isArray(data.value)) {
                            if (this.isPersonal()) {
                                data.value.forEach((i) => {
                                    // Personal results come from Azure Search, we do not query the database. These properties are implied for the Personal mode
                                    i.viewed = true;
                                    i.isPersonal = true;
                                });
                            }
                            this.$rootScope.$broadcast(ResourceList.Display, { resources: data.value });
                            this.pgTotal = data['@odata.count'] || 0;
                        }
                    },
                    () => { this.busy = false; }
                    );
            }
        }

        isCommon = () => {
            return this.$state.current.name === Index.Common;
        };

        isPersonal = () => {
            return this.$state.current.name === Index.Personal;
        };

        loadPersonalCategoryIds = (delay: number) => {
            var adjustCategories = (items: any[]) => {
                this.findPersonalCategories(items);
                this.setCategoriesL1();
            };

            if (this.authenticated) {
                this.$window.setTimeout(() => {
                    app.ngHttpGet(this.$http,
                        app.libraryApiUrl('personal_categories'),
                        null,
                        (data) => {
                            adjustCategories(data.value);
                            this.isEmpty = angular.isArray(data.value) && (data.value.length === 0);
                        }
                        );
                }, delay || 0);
            }
            else {
                // Show a faked category list.
                adjustCategories(Categories);
            }
        };

        findPersonalCategories = (items: any[]) => {
            // Iterate through the nested Id arrays. Make a hash table of distinct ids.
            var hash = {};
            items.forEach((i) => {
                i.categoryPathIds.forEach((id) => {
                    if (!hash.hasOwnProperty(id)) {
                        hash[id] = true;
                    }
                });
            });

            Categories.forEach((i) => { (<IPersonalCategory>i).personal = hash.hasOwnProperty(i.id); });
        };

        onPersonalRemoved = (resources, resource) => {
            if (this.isPersonal()) {
                var idx = resources.indexOf(resource);
                if (idx !== -1) {
                    resources.splice(idx, 1);
                    // Refresh the resource list.
                    this.$rootScope.$broadcast(ResourceList.Display, { resources: resources });
                    if (resources.length === 0) {
                        // Ensure Azure Search keeps pace deleteing the document and re-enumerating categories.
                        this.loadPersonalCategoryIds(1000);
                    }
                }
            }
        };

        showAddResourceModal = () => {
            this.clear();
            app.Modal.openModal(this.$modal,
                'addNewResourceModal',
                AddNewResourceModal,
                null,
                (data) => {
                    // log a resource view.
                    logResourceView(this.$http, data);
                    // Ensure Azure Search keeps pace adding the document and re-enumerating categories.
                    this.loadPersonalCategoryIds(2000);
                },
                'static',
                'lg'
                );
        };

    } // end of class Ctrl

    class StateConfig {
        static $inject = [app.ngNames.$stateProvider, app.ngNames.$urlRouterProvider];
        constructor(private $stateProvider: ng.ui.IStateProvider, private $urlRouterProvider: ng.ui.IUrlRouterProvider) {
            var makeState = (s: string) => {
                return {
                    name: s,
                    url: '/' + s.toLowerCase(),
                    data: {
                        secondaryTitle: s.charAt(0).toUpperCase() + s.slice(1) + ' collection of learning and practicing resources',
                    },
                };
            };
            // These state names are used as URL path in pgLoad(). They correspond to routes in Runnymede.Website.Controllers.Api.LibraryApiController
            var common: ng.ui.IState = makeState(Index.Common);
            var personal: ng.ui.IState = makeState(Index.Personal);
            $stateProvider
                .state(common)
                .state(personal)
            ;
            $urlRouterProvider.otherwise(common.url);
        }
    }; // end of class StateConfig

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ui.router', 'ngAnimate', 'angular-loading-bar', 'ngTagsInput'])
        .config(StateConfig)
        .run(app.StateTitleSyncer)
        .config(TagsInputConfig)
        .controller('Index', Index)
        .controller('ResourceList', ResourceList)
    ;

} // end of module app.library
