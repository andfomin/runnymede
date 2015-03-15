module app.exercises {

    export interface IPoint {
        x: number;
        y: number;
    }

    export class Canvas {

        exercise: IExercise;
        remark: IRemark;
        images: HTMLImageElement[] = [];
        page: number = 1; // IRemark.page first page is 1. The pagination control assumes that the first page is 1.
        canvas: HTMLCanvasElement;
        pointSize = 20; // For click detection
        scales = [0.25, 0.5, 0.75, 1.0];
        scaleIndex: number = 0;

        constructor(
            public $appRemarks: app.exercises.IRemarksService,
            private $document: angular.IDocumentService,
            public $modal: angular.ui.bootstrap.IModalService,
            public $scope: app.IScopeWithViewModel,
            private $window: angular.IWindowService
            )
        /* ----- Constructor  ------------ */
        {
            (<any>$scope).vma = this;

            this.exercise = app['exerciseParam'];
            this.loadImages();

            this.canvas = <HTMLCanvasElement>($window.document.getElementById('myCanvas'));            
            this.canvas.onclick = (event: MouseEvent) => { this.onCanvasClick(event); }; // onCanvasClick is overrided in the descendant, thus do not assign the function directly, wrap it in a call.

            $scope.$on(app.exercises.RemarksService.remarksChanged,() => { this.onRemarksChanged(); });
            $scope.$on(app.exercises.RemarksService.unselectRemark, () => { this.selectRemark(null); });

            $document.on('scroll', () => { this.scrollSpy(); });

        }
        /* ----- End of constructor  ----- */

        clear = () => {
            this.selectRemark(null);
            this.repaint();
        };

        private loadImages = () => {
            var blobNames = this.exercise.artifact.split(',');
            this.images = [];
            blobNames.forEach((i, idx) => {
                var image = new Image();
                this.images.push(image);
                image.onload = () => {
                    if (idx == this.page - 1) {
                        this.repaint();
                    };
                }
                image.src = app.getBlobUrl('writing-photos', i);
            });
        };

        onCanvasClick = (event: MouseEvent) => {
            var point = this.relativeCoordinates(event);
            var remark = this.findRemark(point);
            this.selectRemark(remark);
            this.$scope.$apply();
        };

        zoomIn = () => {
            this.scaleIndex = Math.min(this.scaleIndex + 1, this.scales.length - 1);
            this.clear();
        };

        zoomOut = () => {
            this.scaleIndex = Math.max(this.scaleIndex - 1, 0);
            this.clear();
        };

        repaint = () => {
            // The first page is 1.
            var image = this.images[this.page - 1];
            if (image && image.complete && image.naturalWidth && image.naturalHeight) {
                var scale = this.scales[this.scaleIndex];
                // For substantial downscaling, do it in steps. +http://stackoverflow.com/questions/17861447/html5-canvas-drawimage-how-to-apply-antialiasing
                var width = image.width * scale;
                var height = image.height * scale;
                /* IE, Safari, Opera treat canvas like a block level element and so we need to style both width and height. CSS height:auto does not work in IE. */
                this.canvas.width = width;
                this.canvas.height = height;
                var context = this.canvas.getContext('2d');
                context.drawImage(image, 0, 0, image.width, image.height, 0, 0, width, height);

                this.$appRemarks.remarks
                    .filter((i) => { return i.page == this.page; })
                    .forEach((i) => {
                        this.drawRemark(context, i);
                    });
            }
        };

        private drawRemark = (context: CanvasRenderingContext2D, remark: IRemark) => {
            var ctx = context || this.canvas.getContext('2d');
            var x = remark.x * this.canvas.width;
            var y = remark.y * this.canvas.height;
            ctx.beginPath();

            //ctx.fillStyle = 'rgba(0,0,255,1.0)';
            //ctx.fillRect(x - this.pointSize / 2, y - this.pointSize / 2, this.pointSize, this.pointSize);

            switch (remark.like) {
                case true:
                    var rgb = '0,255,0';
                    var text = '\uf164'; // fa-thumbs-up
                    var shift = 7;
                    break;
                case false:
                    var rgb = '255,0,0';
                    var text = '\uf165'; // fa-thumbs-down
                    var shift = 10;
                    break;
                default:
                    var rgb = '255,255,0';
                    var text = '\uf111'; // fa-circle
                    var shift = 10;
            };

            var opacity = (remark === this.remark) ? 0.1 : 0.3;

            //ctx.arc(x, y, this.pointSize / 2, 0, 2 * Math.PI, false);
            //ctx.fillStyle = 'rgba(' + rgb + ',' + opacity + ')';
            //ctx.fill();

            ctx.font = '28px FontAwesome';
            ctx.textAlign = 'center';
            ctx.fillStyle = 'rgba(' + rgb + ',' + opacity + ')';
            ctx.fillText(text, x, y + shift);

            if (remark === this.remark) {
                //ctx.lineWidth = 2;
                //ctx.strokeStyle = 'rgba(' + rgb + ',0.3)';
                //ctx.stroke();

                switch (remark.like) {
                    case true:
                        text = '\uf087'; // fa-thumbs-o-up
                        shift = 8;
                        break;
                    case false:
                        text = '\uf088'; // fa-thumbs-o-down
                        shift = 11;
                        break;
                    default:
                        text = '\uf10c'; // fa-circle-o
                        shift = 10;
                };

                ctx.fillStyle = 'rgba(' + rgb + ',0.3)';
                ctx.fillText(text, x, y + shift);
            }
        };

        relativeCoordinates = (event: MouseEvent) => {
            // Copied from +http://stackoverflow.com/a/27204937/2808337
            // This is the current screen rectangle of canvas
            var rect = this.canvas.getBoundingClientRect();
            var top = rect.top;
            var bottom = rect.bottom;
            var left = rect.left;
            var right = rect.right;
            var width = right - left;
            var height = bottom - top;
            // Recalculate mouse offsets to relative offsets
            var x = event.clientX - left;
            var y = event.clientY - top;
            // Round to 3 decimals.
            x = Math.round(1000 * x / width) / 1000;
            y = Math.round(1000 * y / height) / 1000;
            return <IPoint>{ x: x, y: y };
        };

        findRemark = (p: IPoint) => {
            return app.arrFind(this.$appRemarks.remarks, (i) => {
                /* In our coordinate system both unequal sides of the image have the same relative extent 1. So a circle on the image would be projected to an oval in our coordinate system.
                We cannot calculate the distance using a radius as Math.sqrt(x * x + y * y). We use a bounding rectangle. */
                var horz = this.pointSize / this.canvas.width / 2;
                var vert = this.pointSize / this.canvas.height / 2;
                var left = p.x - horz;
                var top = p.y - vert;
                var right = p.x + horz;
                var bottom = p.y + vert;
                var hit = (i.x > left) && (i.y > top) && (i.x < right) && (i.y < bottom);
                return hit;
            });
        };

        selectRemark = (remark) => {
            if (remark !== this.remark) {
                this.remark = remark;
                this.repaint();
            }
        }

        isEditing = (remark: app.IRemark) => {
            var unfinished = this.exercise.reviews.some((i) => { return !i.finishTime; });
            return (remark === this.remark) && unfinished;
        }

        scrollSpy = () => {
            if (this.remark) {
                // If the canvas goes out of view (actually above the middle of the screen), close the editor panel.
                var rect = this.canvas.getBoundingClientRect();
                var canvasAbovePanel = rect.bottom < this.$window.innerHeight / 2;
                // If remark goes out of screen, unselect it.
                var remarkY = rect.top + (rect.bottom - rect.top) * this.remark.y;
                var remarkOutOfView = (remarkY < 0) || (remarkY > this.$window.innerHeight);

                if (canvasAbovePanel || remarkOutOfView) {
                    this.selectRemark(null);
                    this.$scope.$apply();
                }
            }
        };

        onRemarksChanged = () => {
            var old = this.remark;
            this.clear();
            if (old) {
                // Old remark items are replaced with new ones in the list. Aarray.indexOf() will not find the old one.
                var r = app.arrFind(this.$appRemarks.remarks,(i) => {
                    return (i.id === old.id) && (i.reviewId === old.reviewId);
                });
                if (r) {
                    this.selectRemark(r);
                }
            }
        };

    } // end of class Canvas

    export function WritingsComparer(a: IRemark, b: IRemark) {
        var page = (a.page === b.page) ? 0 : (a.page < b.page ? -1 : 1);
        var y = (a.y === b.y) ? 0 : (a.y < b.y ? -1 : 1);
        var x = (a.x === b.x) ? 0 : (a.x < b.x ? -1 : 1);
        return (page !== 0) ? page : ((y !== 0) ? y : x);
    };

} // end of module app.exercises
