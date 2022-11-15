window.blazr_setExitCheck = function (dotNetHelper, set) {
    if (set) {
        window.addEventListener("beforeunload", blazr_spaExit);
        blazrDotNetExitHelper = dotNetHelper;
    }
    else {
        window.removeEventListener("beforeunload", blazr_spaExit);
        blazrDotNetExitHelper = null;
    }
}

var blazrDotNetExitHelper;

window.blazr_spaExit = function (event) {
    event.preventDefault();
    blazrDotNetExitHelper.invokeMethodAsync("SpaExit");
    return event.returnValue="Do you really want to go!";
}
