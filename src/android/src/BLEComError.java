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

    public static BLEComError illegalAction(String msg) {
        return new BLEComError(BLEComError.Code.ILLEGAL_ACTION, msg);
    }

    public static BLEComError connectionError(Exception e) {
        return new BLEComError(BLEComError.Code.CONNECTION_ERROR, e);
    }

    public static BLEComError invalidMacAddress(String macAddress) {
        return new BLEComError(BLEComError.Code.INVALID_MAC_ADDRESS, "Invalid mac address " + macAddress);
    }

    public static BLEComError requestError(Exception e, String deviceId, byte[] data) {
        return new BLEComError(Code.REQUEST_ERROR, e);
    }

    public static BLEComError unknownError(Throwable err) {
        return new BLEComError(
                BLEComError.Code.UNKNOWN,
                err
        );
    }

    public static BLEComError bleNotAvailable() {
        return new BLEComError(BLEComError.Code.BLE_ADPATER_NOT_AVAILABLE);
    }

    public static BLEComError illegalArgument(String msg) {
        return new BLEComError(BLEComError.Code.ILLEGAL_ARGUMENT, msg);
    }

    public static BLEComError internalError(Throwable e) {
        return new BLEComError(
                BLEComError.Code.INTERNAl_ERROR,
                e
        );
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
        String REQUEST_ERROR = "RequestError";
        String ILLEGAL_ARGUMENT = "IllegalArgument";
        String LOCATION_SERVICE_DISABLED = "LocationServiceDisabled";
        String DISCONNECTION_ERROR = "DisconnectError";
        String ILLEGAL_ACTION = "IllegalAction";
    }
}
