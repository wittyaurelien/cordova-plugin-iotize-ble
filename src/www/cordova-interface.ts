//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  cordova-interface.ts
//  device-com-ble.cordova BLE Cordova Plugin
//

// import { exec } from "cordova";
export interface CordovaInterface {

    checkAvailable (success: (data: any) => any, failure: (err: any) => any): void;

    startScan(success: (data: any) => any, failure: (err: any) => any): void;

    stopScan(success: (data: any) => any, failure: (err: any) => any): void;
            
    connect(device_id: string, success: (data: any) => any, failure: (err: any) => any): void;
        
    disConnect(device_id: string, success: (data: any) => any, failure: (err: any) => any): void;
    
    isConnected(device_id: string, success: (data: any) => any, failure: (err: any) => any): void;
    
    send(device_id: string, data: any, success: (data: any) => any, failure: (err: any) => any): void;
    
    getLastError(success: (data: any) => any, failure: (err: any) => any): void;
    
}