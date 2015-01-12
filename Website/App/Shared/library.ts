module app.library {

    export interface ICategory {
        id: string;
        parentId: string;
        pathIds: string;
        position: number;
        level: number;
        name: string;
    }

    export interface IExponent {
        categoryId: string;
        referenceLevel: string;
        text: string;
        lines: string[]; // Splitted text.
    }

    export interface IFormat {
        id: string;
        urlBase: string;
    }

    // Corresponds to dbo.appTypes (FR....) and Runnymede.Website.Utils.LibraryUtils.Formats
    export class Formats {
        static Youtube: IFormat = { id: 'FRYTVD', urlBase: null, };
        static BcGrammar: IFormat = { id: 'FRBCEG', urlBase: 'http://learnenglish.britishcouncil.org/en/english-grammar/', };
        static Exponent: IFormat = { id: 'FRCIEX', urlBase: null };
    }

    export class ResourceList {

        resources: IResource[] = null;
        selection: IResource = null;
        exponents: IExponent[] = null;
        levelRating: number = null;
        private levelRatingKnownValue: number = null; // We use it to distinguish whether levelRating is set by an user action or programmatically.

        /* Do not move the iframe element by element.append(). Otherwise it gets destoyed in the old place and recreated in the new place, the player fires the onReady event on each move.
         * We use the trick with upperList and lowerList and items moving over the player to visually imitate the player movement.
         */
        player: YT.Player = null;

        static Clear = 'resourceList.clear';
        static Display = 'resourceList.display';
        static Selected = 'resourceList.selected'; // Notify the host page of an item selection
        static PersonalRemoved = 'resourceList.personalRemoved'; // Notify the host page.

        static $inject = [app.ngNames.$scope, app.ngNames.$rootScope, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$window];

        constructor(
            // ----- Constructor ----- 
            private $scope: app.IScopeWithViewModel,
            private $rootScope: ng.IRootScopeService,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $window: ng.IWindowService
            ) {
            $scope.vm = this;

            $scope.$on(ResourceList.Clear, () => { this.clear(); });
            $scope.$on(ResourceList.Display, (event, args) => { this.displayResources(args.resources); });

            createYouTubePlayer(this.$window, 420, 420, 'toBeReplacedByYoutubeIframe',
                (event: YT.EventArgs) => { this.player = event.target; },
                (event: YT.EventArgs) => { toastr.error('Player error ' + event.data); }
                );

            $scope.$watch(() => { return this.levelRating; },
                (newValue, oldValue, scope) => {
                    if (newValue !== this.levelRatingKnownValue) {
                        this.levelRatingKnownValue = newValue;
                        this.logLanguageLevelRating();
                    }
                }
                );
            // ----- End of constructor ----- 
        }

        private clear = () => {
            this.select(null);
            this.resources = null;
            this.levelRating = null;
            this.levelRatingKnownValue = null;
        };

        private displayResources = (resources: IResource[]) => {
            this.clear();
            this.resources = resources;
        };

        select = (resource: IResource) => {
            this.selection = resource;
            this.exponents = null;

            if (this.player) {
                this.player.pauseVideo();
            }

            if (resource) {
                this.$rootScope.$broadcast(ResourceList.Selected, { resource: resource });
                // The user may just stress-test the app by switching resource items. She should invest at least 10 seconds of her time to count the resource as viewed.
                this.assumeResourceView(resource);

                if (isYoutube(resource) && this.player) {
                    this.player.cueVideoById(resource.naturalKey);
                    // Mobile browsers do not allow a script to start a video to prevent unauthorized download costs.
                    //this.player.loadVideoById(resource.naturalKey);
                    //this.player.playVideo();
                }
                else if (isExponent(resource)) {
                    this.loadExponents();
                }
                else if (isExternal(resource)) {
                    // To precisely control how the link will be opened is impossible. +http://stackoverflow.com/questions/4907843/open-a-url-in-a-new-tab-using-javascript
                    // Chrome blocks popups initiated by scripts. Thus we cannot open an external link automaticaly.
                    this.$window.open(resource.url, '_blank');
                }
            }
        };

        private loadExponents = () => {
            if (isExponent(this.selection)) {
                this.$http.get(app.libraryApiUrl('category/' + this.selection.categoryIds + '/exponents'))
                    .success((data: any) => {
                        this.exponents = data;
                        this.exponents.forEach((i) => {
                            i.lines = i.text.split('\n\n');
                        });
                    }
                    );
            }
        };

        private assumeResourceView = (resource: IResource) => {
            var hostPage = getHostPage();
            var logViews = hostPage && (hostPage !== HostPage.HostPage_History);
            if (logViews && resource && (this.selection === resource)) {
                // The user may just stress-test the app by switching resource items. She should invest at least 10 seconds of her time to count the resource as viewed.
                this.$window.setTimeout(() => {
                    if (this.selection === resource) {
                        logResourceView(this.$http, resource);
                    }
                }, 10000);
            }
        };

        private logLanguageLevelRating = () => {
            if (!app.isAuthenticated()) {
                this.levelRating = null;
                this.levelRatingKnownValue = null;
                toastr.warning(app.notAuthenticatedMessage);
                return;
            }

            var resourceId = this.selection ? this.selection.id : null;
            if (resourceId) {
                app.ngHttpPost(this.$http,
                    app.libraryApiUrl('language_level_rating'),
                    {
                        resourceId: resourceId,
                        languageLevelRating: this.levelRating
                    }
                    )
                    .catch(() => {
                        this.levelRating = null;
                        this.levelRatingKnownValue = null;
                    }
                    );
            }
        };

        togglePersonal = () => {
            this.player && this.player.pauseVideo();

            if (this.selection && this.selection.id) {
                if (this.selection.isPersonal) {
                    app.Modal.openModal(this.$modal,
                        'deletePersonalResourceModal.html',
                        DeletePersonalResourceModal,
                        {
                            resource: this.selection
                        },
                        () => {
                            this.selection.isPersonal = false;
                            this.selection.comment = null;
                            this.$rootScope.$broadcast(ResourceList.PersonalRemoved, { resources: this.resources, resource: this.selection });
                            toastr.success('The personal resouce is removed.');
                        }
                        );
                }
                else {
                    app.Modal.openModal(this.$modal,
                        'addCommonResourceModal.html',
                        AddCommonResourceModal,
                        {
                            resource: this.selection
                        },
                        () => {
                            this.selection.isPersonal = true;
                            toastr.success('The resource is added.');
                        },
                        'static',
                        'lg'
                        );
                }
            }
        };

        getUpperList = () => {
            if (this.resources) {
                if (this.selection) {
                    var index = this.resources.indexOf(this.selection);
                    return this.resources.slice(0, index);
                }
                else {
                    return this.resources;
                }
            }
            else {
                return null;
            }
        };

        getLowerList = () => {
            if (this.resources && this.selection) {
                var index = this.resources.indexOf(this.selection);
                return this.resources.slice(index + 1);
            }
            else {
                return null;
            }
        };

        isPersonal = () => {
            return this.selection && this.selection.isPersonal;
        }

        isFeedbackVisible = () => {
            // We need the Id to send feedback. Ad-hoc resources have no Id.
            return this.selection && this.selection.id;
        };

        getIconName = (resource) => {
            return app.library.getIconName(resource);
        };

        getIconTitle = (resource) => {
            return app.library.getIconTitle(resource);
        };

        isPlayerVisible = () => {
            return this.selection && app.library.isYoutube(this.selection) && this.player;
        };

        isExponentVisible = () => {
            return app.library.isExponent(this.selection);
        };

        isLevelRatingDisabled = () => {
            return this.levelRating;
        };

        showReportProblemModal = () => {
            app.Modal.openModal(this.$modal,
                'reportProblemModal.html',
                ReportProblemModal,
                {
                    resource: this.selection,
                },
                null,
                'static',
                'lg'
                );
        };

    } // end of class ResourceList

    export interface ICategorySelection {
        level1: string;
        level2: string;
        level3: string;
    }

    export interface ITag {
        text: string;
    }

    export class AddResourceModal extends app.Modal {

        resource: IResource;
        selections: ICategorySelection[] = [];
        tags: ITag[] = [];
        categoriesL1: ICategory[];
        showUrl: boolean = true;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.categoriesL1 = Categories.filter((i) => { return (i.level === 1); });
        } // ctor

        getCategories = (parentId: string) => {
            return parent ? Categories.filter((i) => { return (i.parentId === parentId); }) : [];
        };

        clearL2L3 = (s: ICategorySelection) => {
            s.level3 = s.level2 = null;
        }

        clearL3 = (s: ICategorySelection) => {
            s.level3 = null;
        }

        canAddSelection = () => {
            return this.selections.every((i) => { return angular.isString(i.level1); });
        };

        addSelection = () => {
            this.selections.push({
                level1: null,
                level2: null,
                level3: null
            });
        };

        getCategoryIds = () => {
            return this.selections
                .map((i) => { return i.level3 || i.level2 || i.level1; })
                .filter((i) => { return angular.isString(i); })
                .join(' ');
        };

        gatTags = () => {
            return this.tags
                .map((i) => { return i.text; })
                .join(' ');
        };

        canOk = () => {
            // We allow the user to post a resource without a category. Title may be ommited for YouTube videos, it will be populated during validation.
            return !this.busy && this.authenticated && !!this.resource.url;
        };

        internalOk = () => {
            this.resource.categoryIds = this.getCategoryIds();
            this.resource.tags = this.gatTags();

            return app.ngHttpPost(this.$http,
                app.libraryApiUrl(),
                this.resource
                );
        };

    }; // end of class AddResourceModal

    export class AddNewResourceModal extends AddResourceModal {
        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.resource = <IResource>{
                url: null,
            };
            this.addSelection();
        } // // ctor

    } // end of class AddNewResourceModal

    export class AddCommonResourceModal extends AddResourceModal {

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.showUrl = false;
            // The user is going to edit the description.
            this.resource = angular.copy(modalParams.resource);
            delete this.resource.languageLevelRating;
            delete this.resource.viewed;
            delete this.resource.isPersonal;

            // Populate category selection arrays. Build the tree for each category.
            (this.resource.categoryIds || '')
                .split(' ')
                .filter((i) => { return i.length > 0; })
                .forEach((i) => {
                    var sel = {};
                    var catId = i;
                    while (catId) {
                        var cat = app.arrFind(Categories, (j) => { return j.id === catId; });
                        sel['level' + cat.level] = catId;
                        catId = cat.parentId;
                    };
                    this.selections.push(<ICategorySelection>sel);
                });

            this.tags = (this.resource.tags || '')
                .split(' ')
                .filter((i) => { return i.length > 0; })
                .map((i) => { return { text: i }; });

        } // ctor

    }; // end of class AddCommonResourceModal

    export class DeletePersonalResourceModal extends app.Modal {

        resource: IResource;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.resource = modalParams.resource;
        } // ctor

        canOk = () => {
            return !this.busy;
        };

        internalOk = () => {
            return this.$http.delete(app.libraryApiUrl('personal/' + this.resource.id));
        };

    } // end of class DeletePersonalResourceModal

    export class ReportProblemModal extends AddCommonResourceModal {

        report: any = {};
        reportedVersion: IResource;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.showUrl = true;
            this.reportedVersion = angular.copy(this.resource);
        } // ctor

        canOk = () => {
            return !this.busy && this.authenticated;
        };

        internalOk = () => {
            this.resource.categoryIds = this.getCategoryIds();
            this.resource.tags = this.gatTags();

            return app.ngHttpPost(this.$http,
                app.libraryApiUrl('problem_report'),
                {
                    report: this.report,
                    reportedVersion: this.reportedVersion,
                    suggestedVersion: this.resource,
                },
                () => { toastr.success('Thank you for reporting the problem!'); }
                );
        };

    } // end of class ReportProblemModal

    export class TagsInputConfig {
        constructor(tagsInputConfigProvider: any) {
            tagsInputConfigProvider.setDefaults('tagsInput', {
                placeholder: '',
                addOnEnter: true,
                addOnSpace: true,
                addOnComma: true,
                addOnBlur: true,
                allowedTagsPattern: /^[0-9a-zA-Z]+$/,
            });
        }
    }; // end of class TagsInputConfig

    export function isYoutube(resource: IResource) {
        return (!!resource) && (resource.format === Formats.Youtube.id) && !!resource.naturalKey;
    }

    export function isExponent(resource: IResource) {
        return resource && (resource.format === Formats.Exponent.id);
    }

    export function isExternal(resource: IResource) {
        return resource && !(isYoutube(resource) || isExponent(resource));
    }

    export function getIconName(resource) {
        if (isYoutube(resource)) {
            return 'fa-youtube-play';
        }
        else if (isExponent(resource)) {
            return 'fa-quote-left';
        }
        else if (isExternal(resource)) {
            return 'fa-external-link';
        }
        else {
            return null;
        }
    };

    export function getIconTitle(resource) {
        if (isYoutube(resource)) {
            return 'Video';
        }
        else if (isExponent(resource)) {
            return 'Examples';
        }
        else if (isExternal(resource)) {
            return 'External link';
        }
        else {
            return null;
        }
    };

    export class HostPage {
        // These constants are used for logging resource views.
        static HostPage_Null: number = 0; // Reserved. Do not use as a page value
        static HostPage_LibraryCommon: number = 1;
        static HostPage_LibraryPersonal: number = 2;
        static HostPage_Library: number = 3;
        static HostPage_History: number = 4;
        static HostPage_Friend: number = 5;
        static HostPage_Recording: number = 6;
        static HostPage_ViewRecording: number = 7;
        static HostPage_ViewWriting: number = 8;
        static HostPage_Copycat: number = 9;
    } // end of class HostPage

    export function getHostPage() {
        // Initially passed via the page. May be dynamically changed on the library/index page
        var hostPageParam = app['hostPageParam'];
        return HostPage[hostPageParam] || HostPage.HostPage_Null;
    };

    export function logResourceView($http: ng.IHttpService, resource: IResource) {
        resource.viewed = true;
        // A suggestion may have resource.id == null(0?) when the resource is made in code and points to Google
        app.ngHttpPost($http,
            app.libraryApiUrl('resource_view/' + (resource.id || 0)),
            {
                localTime: app.getLocalTimeInfo().time,
                hostPage: getHostPage(),
                resource: resource,
            }
            );
    };

    export function createYouTubePlayer($window: ng.IWindowService, width: number, height: number, idOfElementToReplace: string,
        onReadyCallback: (event: YT.EventArgs) => void,
        onErrorCallback?: (event: YT.EventArgs) => void,
        onStateChangeCallback?: (event: YT.EventArgs) => void
        ) {
        // Event handler which will be called by the YouTube script after the script is loaded.
        var onReady = () => {
            var origin = $window.location.protocol + '//' + $window.location.hostname;

            var playerVars: YT.PlayerVars = {
                enablejsapi: 1,
                iv_load_policy: 3,
                modestbranding: 1,
                origin: origin,
                rel: 0,
                showinfo: 0,
                theme: 'light'
            };

            var events = <YT.Events>{
                onReady: (event) => {
                    onReadyCallback(event);
                },
                onStateChange: (event) => {
                    (onStateChangeCallback || angular.noop)(event);
                },
                onError: (event) => {
                    (onErrorCallback || angular.noop)(event);
                },
            };

            var playerOptions: YT.PlayerOptions = {
                width: width,
                height: height, // ratio 4:3 + 35px for the toolbar
                videoId: null,
                playerVars: playerVars,
                events: events
            };

            new YT.Player(idOfElementToReplace, playerOptions);
        }

         // onYouTubeIframeAPIReady() is called by the YouTube script after the script is loaded.
        (<any>$window).onYouTubeIframeAPIReady = onReady;

        if ((<any>$window).YT === undefined) {
            var document = $window.document;
            // As in the example on +https://developers.google.com/youtube/iframe_api_reference
            var tag = document.createElement('script');
            tag.src = 'https://www.youtube.com/iframe_api';
            var firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        }
        else {
            onReady();
        }
    }


} // end of module app.library
