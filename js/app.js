/*
 *  Document   : app.js
 *  Author     : pixelcave
 *  Description: Custom scripts and plugin initializations (available to all pages)
 *
 *  Feel free to remove the plugin initilizations from uiInit() if you would like to
 *  use them only in specific pages. Also, if you remove a js plugin you won't use, make
 *  sure to remove its initialization from uiInit().
 */

var App = function() {

    var parseFloatJS = function (numberStr) {


        if (GlobalPreprocessed.ServerDecimalSeparator != SYSTEM_DECIMAL_SEPARATOR) {
            numberStr = (typeof numberStr == 'undefined') ? "" : strReplaceAll( numberStr,GlobalPreprocessed.ServerDecimalSeparator, SYSTEM_DECIMAL_SEPARATOR);
        }
        if (isNaN(numberStr) || numberStr == "" || numberStr.split(SYSTEM_DECIMAL_SEPARATOR).length > 2) {
            return false; // invalid number
        } else {
            //Remove comma's always, if the client was the comma as decimal separator, then the comma was replaced in the above sentence`
            numberStr = strReplaceAll( numberStr,',', '');
            return parseFloat(numberStr);
        }
    };
    var formatFloatServer = function (numberObj) {

        if (typeof numberObj != TYPEOF_NUMBER)
            numberObj = parseFloatJS(numberObj);

        if (!numberObj) {
            return false;
        } else {
            return numberObj.toLocaleString(GlobalPreprocessed.Language);
        }
    };

    var strReplaceAll = function (str,search, replacement) {
        return str.split(search).join(replacement);
    };
    
    var loadUIValidations = function () {
        // Set input validations isNaN(numberStr)
        $(document).on('keypress', '.val-input-integer-not-negative', function (e) {
            var invalid = (e.which != 8 && e.which != 0
                && (e.which < 48 || e.which > 57))
            if (invalid) {
                return false;
            }

        });
        $(document).on('keypress', '.val-input-float-not-negative', function (e) {
            var inputValue = $(this).val();
            //console.log('key pressed on float input '+inputValue);
            var hasDecimal = inputValue.indexOf('.') > -1 ||
                inputValue.indexOf(',') > -1;
            var isDecimal = e.which == 44 || e.which == 46;
            var isDigit = e.which == 8 || e.which == 0 || (e.which >= 48 && e.which <= 57);


            if (!isDigit && !( !hasDecimal && isDecimal )  ) {
                return false;
            }
        });

        $(document).ready(function () {
            $('.modal').on('hidden.bs.modal', function (e) {
                var modal = $('.modal');
                if (modal.hasClass('in')) {
                    $('body').addClass('modal-open');
                }
                $('body').css('padding-right', '0');
            });
            $('body').tooltip({
                selector: '[data-toggle=tooltip]'
            });
        });
    }
    
    var parseJson = function (obj) {
        if (typeof obj == 'string') {
            var str = obj.replace(/\n/g, "\\n")
                .replace(/\r/g, "\\r")
                .replace(/\t/g, "\\t")
                .replace(/\f/g, "\\f");
            return JSON.parse(str);
        }
        return false;
    }
    

    return {
        init: function() {
            if (typeof Page !== 'undefined' && typeof Page.init !== 'undefined') {
                Page.init();
            }
        },
        
        unload: function(e){
            if (typeof Page !== 'undefined' && typeof Page.unload !== 'undefined') {
                return Page.unload(e);
            }
        },
        beforeUnload : function(e){
            if (typeof Page !== 'undefined' && typeof Page.beforeUnload !== 'undefined') {
                var returnValue = Page.beforeUnload(e);
                if (returnValue) { // If a selector is received , then focus that selector 


                    var focusFunc = this.topFocusSelector;

                    setTimeout(function () { // this is executed if the user stayed on page
                        if (returnValue instanceof jQuery && returnValue.length) {
                            focusFunc(returnValue);
                        }
                    }, 100);

                    
                    return returnValue;
                }
            }
        },
        
        loadUIValidations: function() {
            loadUIValidations();
        },
        topFocusSelector: function(selector,margin,fadeInOut,finishCallback){
            var headerH = 0;
            if (!margin) {
                margin = 0;
            }

            if ($('#header-container').length) {
                headerH = $('#header-container').height();
            }
            var scrollValue = selector.offset().top - headerH - margin;

            $("html, body").animate({ scrollTop: scrollValue }, 500, function () {
                
                if (fadeInOut || typeof(fadeInOut) === 'undefined') { // Use fade by default
                    selector.fadeOut(250, "", function () {
                        selector.fadeIn(250, "", function () {
                            selector.focus();
                            if (typeof (finishCallback) != 'undefined')
                                finishCallback;
                        });
                    });
                } else {
                    selector.focus();
                    if (typeof (finishCallback) != 'undefined')
                        finishCallback;
                }

                
            });
        },
        /* Utils */

        // Decode html, useful to show messages generated from Razor in javascript function like alert
        decodeHtml : function (html) {
            return $('<div/>').html(html).text();
        },
        strReplaceAll: function (str, search, replace) {
            return strReplaceAll(str, search, replace);
        },
        parseFloatJS: function (numberStr) {
            return parseFloatJS(numberStr);
        },
        formatFloatServer: function(numberObj) {
            return formatFloatServer(numberObj);
        },

        getTimeZoneOffset : function(){
            return ((new Date()).getTimezoneOffset() / 60) * -1;
        },

        parseJson: function(obj){
            return parseJson(obj);
        },
     
        formAsDictionary: function (formSelector) {
            var formData = {};
            formSelector.find('[name]').each(function() {
                formData[this.name] = this.value;  
            });
            return formData;
        }
    };
}();

/* Initialize App when page loads */
$(function () { App.init(); });

$( document ).ready(function() {
    window.onunload = function (e) {
        return App.unload(e);
    };
    $(window).bind('beforeunload', function (e) {
        return App.beforeUnload(e);
    });
});

