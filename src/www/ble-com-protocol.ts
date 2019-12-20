//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  ble-com-protocol.ts
//  device-com-ble.cordova BLE Cordova Plugin
//
import { FormatHelper } from '@iotize/device-client.js/core/format/format-helper';
import { ConnectionState } from '@iotize/device-client.js/protocol/api';
import {
    ComProtocolConnectOptions,
    ComProtocolDisconnectOptions,
    ComProtocolOptions,
    ComProtocolSendOptions,
} from '@iotize/device-client.js/protocol/api/com-protocol.interface';
import { QueueComProtocol } from '@iotize/device-client.js/protocol/impl/queue-com-protocol';
import { from, Observable, Subject, Subscription } from 'rxjs';
import { filter, first } from 'rxjs/operators';

import { CordovaInterface } from './cordova-interface';
import { debug } from './logger';

declare var iotizeBLE: CordovaInterface;

export class BLEComProtocol extends QueueComProtocol {
    _connectionChangeObservable: any;

    private deviceId: string = "";
    _connectionStateSubject: Subject<ConnectionState>;
    _connectionStateSubscription: Subscription;

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
        if (!this._connectionStateSubject) {
            this._connectionStateSubject = new Subject();
        }

        this._connectionStateSubscription = this._connectionStateSubject.subscribe((val) => this.setConnectionState(val));

        const onConnectionStateChange = (val: string) => {
            debug('_connect observable: onConnect "', val , '"');
            this._connectionStateSubject.next(ConnectionState[val]);
        };
        const onError = (error: any) => {
            debug('_connect observable: onError', error);
            this._connectionStateSubject.error(error);
        };
        iotizeBLE.connect(this.deviceId, onConnectionStateChange, onError);

        return this._connectionStateSubject.pipe(
            filter(state => state == ConnectionState.CONNECTED),
            first()
        );
    }

    _disconnect(options?: ComProtocolDisconnectOptions): Observable<any> {
        debug('_disconnect', options);
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

    send(data: Uint8Array, options?: ComProtocolSendOptions): Observable<any> {
        let promise = this._cordovaCallToPromise<string>(
            iotizeBLE.send,
            this.deviceId,
            FormatHelper.toHexString(data)
        )
            .then((hexString: string) => FormatHelper.hexStringToBuffer(hexString));
        return from(promise);
    }

    protected _cordovaCallToPromise<T>(cordovaFct: (...args: any[]) => any, ...args: any[]): Promise<T> {
        if (!cordovaFct) {
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

};

