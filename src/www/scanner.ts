//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  scanner.ts
//  device-com-ble.cordova BLE Cordova Plugin
//

import { Observable, BehaviorSubject } from 'rxjs';
import { DeviceScanner, DeviceScannerOptions } from '@iotize/device-client.js/device/scanner/device-scanner';
import { CordovaInterface } from './cordova-interface';
import { debug } from './logger';
import { CordovaBLEScanResult } from './definitions';

declare var iotizeBLE: CordovaInterface;

/**
 * 
 */
export class BLEScanner implements DeviceScanner<CordovaBLEScanResult> {

    private _results = new BehaviorSubject<CordovaBLEScanResult[]>([]);
    private _scanning$ = new BehaviorSubject<boolean>(false);

    constructor() {
    }

    get scanning(): Observable<boolean> {
        return this._scanning$.asObservable();
    }

    get isScanning(): boolean {
        return this._scanning$.value;
    }

    /**
     * Gets the observable on the devices$ Subject
     * @return
     */
    get results(): Observable<CordovaBLEScanResult[]> {
        return this._results.asObservable();
    }


    /**
     * Launches the scan for BLE devices
     * Throws if BLE is not available
     */
    start(options?: DeviceScannerOptions): Promise<void> {
        return this.checkAvailable().then(
            isAvailable => {
                if (!isAvailable) {
                    return Promise.reject("BLE is not available");
                }
                debug("Start Scanning ...");
                this._scanning$.next(true);
                return new Promise<void>((resolve, reject) => {
                    iotizeBLE.startScan((result) => {
                        debug(result);
                        if (result == 'Ok') {
                            resolve();
                            return;
                        }
                        this.addOrRefreshDevice(result);
                    }, (error) => {
                        iotizeBLE
                        .getLastError((lasterror) => {
                            debug("let ble error " + lasterror);
                        }, (err) => {
                            debug('cannot get last ble error: ', err);
                        });
                        reject(error);
                        this._scanning$.next(false);
                    });
                }
            )
        });
    }

    /**
     * 
     */
    stop(): Promise<void> {
        debug("Stop Scanning ...");
        return new Promise<void>((resolve, reject) => {
            iotizeBLE
                .stopScan((result) => {
                    this._scanning$.next(false);
                    resolve();
                },
                    (error) => {
                        this._scanning$.next(false);
                        reject(error);
                    });
        });
    }

    /**
     * Returns true if this scanner is available
     */
    checkAvailable(): Promise<boolean> {
        return new Promise<boolean>((resolve, reject) => {
            iotizeBLE.checkAvailable((result) => {
                debug('checkAvailable result', result);
                resolve(result);
            }, (error) => {
                reject(error);
            });
        })
    }

    private get devices() {
        return this._results.value;
    }

    private addOrRefreshDevice(newDevice: CordovaBLEScanResult) {
        let storedDeviceIndex = this.devices.findIndex((entry) => entry.address == newDevice.address);
        if (storedDeviceIndex >= 0) {
            let storedDevice = this.devices[storedDeviceIndex];
            if (storedDevice.name != newDevice.name || storedDevice.rssi != newDevice.rssi) {
                debug(`Updating device at index ${storedDeviceIndex}, name=${storedDevice.name} with rssi=${storedDevice.rssi}`);
                this.devices[storedDeviceIndex] = newDevice;
                // this.devices = [...this.devices];
                this._results.next(this.devices);
            }
        }
        else {
            debug(`Adding new device name=${newDevice.name} with rssi=${newDevice.rssi}`);
            this.devices.push(newDevice);
            this._results.next(this.devices);
        }
    }
}
