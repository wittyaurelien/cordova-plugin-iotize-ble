//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  ArgsHelper.java
//  device-com-ble.cordova BLE Cordova Plugin
//

package com.iotize.plugin;

import org.json.JSONArray;
import org.json.JSONException;

class ArgsHelper {
    private final JSONArray args;

    public ArgsHelper(JSONArray args) {
        this.args = args;
    }

    public String getString(int i) throws BLEComError {
        if (this.args == null || i >= this.args.length()){
            throw new BLEComError(BLEComError.Code.ILLEGAL_ARGUMENT, "Missing argument n°" + (i+1));
        }
        try {
            return this.args.getString(i);
        } catch (JSONException e) {
            throw new Error("INTERNAL ERROR", e);
        }
    }
}
