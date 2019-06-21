//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  ble-com-protocol.ts
//  device-com-ble.cordova BLE Cordova Plugin
//

import { QueueComProtocol } from '@iotize/device-client.js/protocol/impl/queue-com-protocol';
import { ComProtocolConnectOptions, ComProtocolDisconnectOptions, ComProtocolSendOptions, ComProtocolOptions } from '@iotize/device-client.js/protocol/api/com-protocol.interface';
import { FormatHelper } from '@iotize/device-client.js/core/format/format-helper';
import { from, Observable, Subscriber, Subscription, Subject } from 'rxjs';
import { first } from "rxjs/operators";
import { CordovaInterface } from './cordova-interface';

declare var iotizeBLE: CordovaInterface;

export class BLEComProtocol extends QueueComProtocol {
    private _connectionStateSubject?: Subject<any>;
    _connectionChangeObservable: any;

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

    private _connectionStateSubscription?: Subscription;
    private _connectionObservable?: Observable<any>;

   private deviceId: string = "";

   constructor(deviceId: string, comProtocolOptions?: ComProtocolOptions) {
       super();
       this.deviceId = deviceId;
       if (comProtocolOptions) {
           this.options = comProtocolOptions;
       } else {
           this.options.connect.timeout = 60000;
       }
   }


    _connect(options?: ComProtocolConnectOptions): Observable<any> {
        this.log('info', '_connect', options);
        // return from(this._cordovaCallToPromise(
        //     iotizeBLE.connect,
        //     this.deviceId
        // ));

        if (!this._connectionStateSubject) {
            this._connectionStateSubject = new Subject();
        }

        this._connectionStateSubscription = this._connectionStateSubject.subscribe((val) => this.setConnectionState(val));

        const onConnect = (val: any) => {
            this.log('debug', '_connect observable: onConnect');
            this._connectionStateSubject.next(val);
        };
        const onError = (error: any) => {
            this.log('debug', '_connect observable: onError');
            this._connectionStateSubject.error(error);
        };
        iotizeBLE.connect(this.deviceId, onConnect ,onError);

        return this._connectionStateSubject.pipe(
            first()
        );
    }


    _disconnect(options?: ComProtocolDisconnectOptions): Observable<any> {
        return from(this._cordovaCallToPromise(
            iotizeBLE.disConnect,
            this.deviceId
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
                this.deviceId,
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

    // protected _connectionChangeObservable(): Observable<any>{
    //     this.log('info', '_connectionChangeObservable');

    //     return Observable.create((subscriber: Subscriber<any>) => {
    //         const onConnectionChange = (val: any) => {
    //             this.log('debug', '_connect observable: onConnect');
    //             subscriber.next(val);
    //         };
    //         const onError = (error: any) => {
    //             this.log('debug', '_connect observable: onError');
    //             subscriber.error(error);
    //         };
    //         iotizeBLE.connectionChangeListener(onConnectionChange ,onError);
    //     });
    // }

 };

