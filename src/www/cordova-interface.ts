//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  cordova-interface.ts
//  device-com-ble.cordova BLE Cordova Plugin
//

// import { exec } from "cordova";
export interface CordovaInterface {

    checkAvailable (success: (data: any) => any, failure: (err: any) => any);

    startScan(success: (data: any) => any, failure: (err: any) => any);

    stopScan(success: (data: any) => any, failure: (err: any) => any);
            
    connect(device_id: string, success: (data: any) => any, failure: (err: any) => any);
        
    disConnect(device_id: string, success: (data: any) => any, failure: (err: any) => any);
    
    isConnected(device_id: string, success: (data: any) => any, failure: (err: any) => any);
    
    send(device_id: string, data: any, success: (data: any) => any, failure: (err: any) => any);
    
    getLastError(success: (data: any) => any, failure: (err: any) => any);
    
}