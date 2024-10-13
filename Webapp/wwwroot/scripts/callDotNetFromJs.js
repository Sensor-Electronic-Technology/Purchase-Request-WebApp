function callDotNetFunctionFromJS() {
    DotNet.invokeMethod('Webapp', 'PageAboutToBeReloaded').then(_=>{
        console.log('Called ' + functionName + ' from JS');
    });
}