//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  scanner.ts
//  device-com-ble.cordova BLE Cordova Plugin
//

import { Subject, Observable } from 'rxjs';
import { DeviceScanner, DeviceScannerOptions } from '@iotize/device-client.js/device/scanner/device-scanner';
import { CordovaInterface } from './cordova-interface';

declare var iotizeBLE: CordovaInterface;

export interface DiscoveredDeviceType {
    name: string;
    address: string;
    rssi?: number;
}

/**
 * 
 */
export class BLEScanner implements DeviceScanner {

    public isScanning: boolean = false;
    private devices$: Subject<DiscoveredDeviceType>;

    constructor() {
        this.devices$ = new Subject();
    }

    /**
     * Launches the scan for BLE devices
     */
    start(options?: DeviceScannerOptions) {
        if (options.timeout){
        }
        console.log("Start Scanning ...");
        iotizeBLE.startScan((result) => {
            console.log(result);
            if (result == 'Ok') {
                this.isScanning = true;
                return;
            }
            this.devices$.next(result);
            // this.devices$.next(JSON.parse(result));
        }, (error) => {
            iotizeBLE
                .getLastError((lasterror) => {
                    console.log("error " + lasterror);
                }, (err) => {
                    console.error(error);
                });
        });
        return this.devices$;
    }

    /**
     * Gets the observable on the devices$ Subject
     * @return {Observable<DiscoveredDeviceType>}
     */
    devicesObservable(): Observable<DiscoveredDeviceType> {
        return this.devices$.asObservable();
    }

    /**
     * 
     */
    stop() {
        console.log("Stop Scanning ...");
        iotizeBLE
            .stopScan((result) => {
                console.log(result);
                this.isScanning = false;
            },
            (error) => {
                console.log("failed : " + error);
            });
    }
}
