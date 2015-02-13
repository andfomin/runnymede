module app.exercises {

    //export function isMobileDevice() {
    //    var ver = window.navigator.appVersion;
    //    var ua = window.navigator.userAgent.toLowerCase();
    //    var mobile = (ver.indexOf("iPad") != -1)
    //        || (ver.indexOf("iPhone") != -1)
    //        || (ua.indexOf("android") != -1)
    //        || (ua.indexOf("ipod") != -1)
    //        || (ua.indexOf("windows ce") != -1)
    //        || (ua.indexOf("windows phone") != -1);
    //    return !!mobile;
    //}

    export function captureSupported(accept: string) {
        // element will be garbage-collected on function return.
        //var element = (<Document>(<any>this.$document[0])).createElement('input'); 
        var element = document.createElement('input');
        element.setAttribute('type', 'file');
        element.setAttribute('accept', accept);
        /* Working Draft +http://www.w3.org/TR/2012/WD-html-media-capture-20120712/ described string values for the capture attribute.
         * Candidate Recommendation +http://www.w3.org/TR/2013/CR-html-media-capture-20130509/ specifies that the capture attribute is of type boolean.
         */
        element.setAttribute('capture', <any>true);
        return element.hasAttribute('accept') && element.hasAttribute('capture');
    }

} // end of module exercises
