// (c) 2018 IoTize SAS
//
"use strict";


module.exports = {

    checkAvailable: function (success, failure){
    
        cordova.exec(success, failure, 'BLECom', 'checkAvailable', []);
    
    },

    startScan: function (success, failure) {

        cordova.exec(success, failure, 'BLECom', 'startScan', []);
    
    },

    stopScan: function (success, failure) {

        cordova.exec(success, failure, 'BLECom', 'stopScan', []);
    
    },
            
    connect: function (device_id, success, failure) {

        cordova.exec(success, failure, 'BLECom', 'connect', [device_id]);    
    
    },      
         
    disConnect: function (device_id, success, failure) {

        cordova.exec(success, failure, 'BLECom', 'disConnect', [device_id]);    
    
    },
    
    isConnected: function (device_id, success, failure) {

        cordova.exec(success, failure, 'BLECom', 'isConnected', [device_id]);    
    
    },
    
    send: function (device_id, data, success, failure) {
        
        cordova.exec(success, failure, 'BLECom', 'sendRequest', [device_id, data]);
    
    },
    
    getLastError: function (success, failure) {
    
        cordova.exec(success, failure, 'BLECom', 'getLastError', []);
    
    },
};
