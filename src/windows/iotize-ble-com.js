    var iotizeProxy = null;
    
    //checking that the winmd is properly loaded
    function checkProxy() {
            
        return ( (iotizeProxy != null) || ( (iotizeProxy = IoTizeBLE.BLEManager()) != null) );
    }

    //scanning devices
    var discoveryCallback = null;

    function handleDiscoveryCallback( jsonResponse ) {
        
        if (discoveryCallback != null){            
            discoveryCallback( jsonResponse, { keepCallback: true });
        }
    }

    //connection
    var connectionErrorCallback = null;

    function handleConnectionErrorCallback( ) {
        
        if (connectionErrorCallback != null){            
            connectionErrorCallback( iotizeProxy.getLastError(), { keepCallback: true });
        }
    }

    cordova.commandProxy.add("BLECom", {


        getLastError: function (successCallback, errorCallback) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }

            successCallback(iotizeProxy.getLastError());

        },

        startScan: function (successCallback, errorCallback) {           

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }
           
            discoveryCallback = successCallback;

            var res = iotizeProxy.startScan(handleDiscoveryCallback);
            if (res == true) {
                handleDiscoveryCallback("Ok");
            }
            else {
                errorCallback(iotizeProxy.getLastError());
            }
        },

        stopScan: function (successCallback, errorCallback) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }  

          
            var res = iotizeProxy.stopScan();
            if (res == true) {
                successCallback("Ok");
            }
            else {
                errorCallback(iotizeProxy.getLastError());
            }
        },

        connect: async function (successCallback, errorCallback, device) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }
            
            connectionErrorCallback = errorCallback;

            var success = false;
            try {
                success = await iotizeProxy.connect(device, handleConnectionErrorCallback);
            } catch (e) {
                errorCallback(e, { keepCallback: true });
            }

            if (success){
                successCallback("Ok");
            }
            else {
                handleConnectionErrorCallback()
            }

        },

        disConnect: async function (successCallback, errorCallback, device) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }
            var success = false;
            try {
                success = await iotizeProxy.connect(device);
            } catch (e) {
                errorCallback(e);
            }

            if (success){
                successCallback("Ok");
            }
            else {
                errorCallback(iotizeProxy.getLastError())
            }

        },

        sendRequest: async function (successCallback, errorCallback, device, request) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }
            
            try {
                var response = await iotizeProxy.sendRequest(device, request);
                successCallback(response);
            } catch (e) {                
                errorCallback(iotizeProxy.getLastError());            
            }

        }

    });
