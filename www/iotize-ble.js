// (c) 2018 IoTize SAS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

/* global cordova, module */
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

    send: function (device_id, data, success, failure) {
        
        cordova.exec(success, failure, 'BLECom', 'sendRequest', [device_id, data]);
    
    },
    
    getLastError: function (success, failure) {
    
        cordova.exec(success, failure, 'BLECom', 'getLastError', []);
    
    },
};
