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
import { debug } from './logger';
import { ConnectionState } from '@iotize/device-client.js/protocol/api';

declare var iotizeBLE: CordovaInterface;

export class BLEComProtocol extends QueueComProtocol {
    private _connectionStateSubject?: Subject<any>;
    _connectionChangeObservable: any;
    private _connectionStateSubscription?: Subscription;
    private _connectionObservable?: Observable<any>;
    private _connectionStateCallBack?: (state:any) => void;

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
        debug('_connect', options);
        // return from(this._cordovaCallToPromise(
        //     iotizeBLE.connect,
        //     this.deviceId
        // ));

        if (!this._connectionStateSubject) {
            this._connectionStateSubject = new Subject();
        }

        this._connectionStateCallBack = (state: string) => {
            debug('_connectionState', state);
            debug('_connect observable: onConnect');
            const connexionState = ConnectionState[state] as ConnectionState;
            this.setConnectionState(connexionState);
            if (state == 'CONNECTED') {
                debug('_connectionStateSubject: next')
                this._connectionStateSubject.next('OK');
            }
        };
        const onError = (error: any) => {
            debug('_connect observable: onError');
            this._connectionStateSubject.error(error);
        };
        iotizeBLE.connect(this.deviceId, this._connectionStateCallBack ,onError);

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
            console.warn('INTERNAL ERROR UNKOWN CORDOVA FUNCTION');
            return;
        }
        debug('Call to ', cordovaFct.name, ...args);
        return new Promise<T>((resolve: any, reject: any) => {
            args.push((result: any) => {
                debug('success handler ', result);
                resolve(result);
            });
            args.push((err: any) => {
                debug('error handler ', err);
                reject(err);
            });

            cordovaFct.apply(iotizeBLE, args);
        });
    }

    // protected _connectionChangeObservable(): Observable<any>{
    //     this.log('info', '_connectionChangeObservable');

    //     return Observable.create((subscriber: Subscriber<any>) => {
    //         const onConnectionChange = (val: any) => {
    //             debug('_connect observable: onConnect');
    //             subscriber.next(val);
    //         };
    //         const onError = (error: any) => {
    //             debug('_connect observable: onError');
    //             subscriber.error(error);
    //         };
    //         iotizeBLE.connectionChangeListener(onConnectionChange ,onError);
    //     });
    // }

 };

