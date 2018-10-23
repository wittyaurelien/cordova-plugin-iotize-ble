//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  ble-com-protocol.ts
//  device-com-ble.cordova BLE Cordova Plugin
//

import { QueueComProtocol } from '@iotize/device-client.js/protocol/impl/queue-com-protocol';
import { ComProtocolConnectOptions, ComProtocolDisconnectOptions, ComProtocolSendOptions } from '@iotize/device-client.js/protocol/api/com-protocol.interface';
import { FormatHelper } from '@iotize/device-client.js/core/format/format-helper';
import { from, Observable } from 'rxjs';
import { CordovaInterface } from './cordova-interface';

declare var iotizeBLE: CordovaInterface;

export class BLEComProtocol extends QueueComProtocol {

    log(level: 'info' | 'error' | 'debug', ...args: any[]): void {
        console[level](level.toUpperCase() + " [IoTizeBLEProtocol] | " + args.map(entry => {
            if (typeof entry === 'object'){
                return JSON.stringify(entry);
            }
            else {
                return entry;
            }
        }).join(" "));
    }

   private deviceName: string = "";

   constructor(name: string) {
       super();
       this.deviceName = name;
       this.options.connect.timeout = 60000;
   }


    _connect(options?: ComProtocolConnectOptions): Observable<any> {
        this.log('info', '_connect', options);
        return from(this._cordovaCallToPromise(
            iotizeBLE.connect,
            this.deviceName
        ));
    }


    _disconnect(options?: ComProtocolDisconnectOptions): Observable<any> {
        return from(this._cordovaCallToPromise(
            iotizeBLE.disConnect,
            this.deviceName
        ));
    }

    write(data: Uint8Array): Promise<any> {
        throw new Error("Method not implemented.");
    }

    read(): Promise<Uint8Array> {
        throw new Error("Method not implemented.");
    }

    send(data: Uint8Array, options ?: ComProtocolSendOptions): Observable<any>{
        let promise = this._cordovaCallToPromise<string>(
                iotizeBLE.send,
                this.deviceName,
                FormatHelper.toHexString(data)
            )
            .then((hexString: string) => FormatHelper.hexStringToBuffer(hexString));
        return from(promise);
    }

    protected _cordovaCallToPromise<T>(cordovaFct: (...args: any[]) => any, ...args: any[]): Promise<T>{
        if (!cordovaFct){
            this.log('error', 'INTERNAL ERROR UNKOWN CORDOVA FUNCTION');
        }
        this.log('debug', 'Call to ', cordovaFct.name, ...args);
        return new Promise<T>((resolve: any, reject: any) => {
            args.push((result: any) => {
                this.log('debug', 'success handler ', result);
                resolve(result);
            });
            args.push((err: any) => {
                this.log('error', 'error handler ', err);
                reject(err);
            });

            cordovaFct.apply(iotizeBLE, args);
        });
    }

 };

