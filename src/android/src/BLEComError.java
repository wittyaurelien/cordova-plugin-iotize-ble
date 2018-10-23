//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  BLEComError.java
//  device-com-ble.cordova BLE Cordova Plugin
//
package com.iotize.plugin;

public class BLEComError extends Exception {

    private final String code;

    public BLEComError(String code) {
        super("BLE communication error (code " + code + ")");
        this.code = code;
    }

    public BLEComError(String code, Throwable cause) {
        super(cause.getMessage() + " (code " + code + ")");
        this.code = code;
    }

    public BLEComError(String code, String message) {
        super(message + " (code " + code + ")");
        this.code = code;
    }

    public String getCode() {
        return code;
    }

    public interface Code {
        String UNKNOWN = "Unknown";
        String BLE_ADPATER_NOT_AVAILABLE = "BLENotAvailable";
        String INVALID_MAC_ADDRESS = "InvalidMacAddress";
        String INTERNAl_ERROR = "InternalError";
        String CONNECTION_ERROR = "ConnectionError";
        String ILLEGAL_ARGUMENT = "IllegalArgument";
        String LOCATION_SERVICE_DISABLED = "LocationServiceDisabled";
        String DISCONNECTION_ERROR = "DisconnectError";
        String ILLEGAL_ACTION = "IllegalAction";
    }
}
