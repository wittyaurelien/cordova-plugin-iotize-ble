    var iotizeProxy = null;
    
    //checking that the winmd is properly loaded
    function checkProxy() {
            
        return ( (iotizeProxy != null) || ( (iotizeProxy = IoTizeBLE.BLEManager()) != null) );
    }

    //scanning devices
    var discoveryCallback = null;

    function handleDiscoveryCallback( jsonResponse ) {
        
        if (discoveryCallback != null){            
            discoveryCallback( JSON.parse(jsonResponse), { keepCallback: true });
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
                success = await iotizeProxy.disConnect(device);
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

        isConnected: async function (successCallback, errorCallback, device) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }
            var success = false;
            try {
                success = await iotizeProxy.isConnected(device);
            } catch (e) {
                errorCallback(e);
            }
         
            successCallback(success);            

        },
        
        checkAvailable: async function (successCallback, errorCallback) {

            if (!checkProxy()){
                errorCallback("iotize-ble Plugin is not available for this version of Windows! Minimal version required is 'Window10 Fall Creators Update, version 1709'");
                return;
            }
            var success = false;
            try {
                success = await iotizeProxy.checkAvailable();
                successCallback(success);      
            } catch (e) {
                errorCallback(e);
            }
         
                
        },
        
        sendRequest: async function (successCallback, errorCallback, request) {

            if (!checkProxy()){
                errorCallback("Internal Error!");
                return;
            }
            
            try {
                var response = await iotizeProxy.sendRequest(request[0], request[1]);
                successCallback(response);
            } catch (e) {                
                errorCallback(iotizeProxy.getLastError());            
            }

        }

    });
