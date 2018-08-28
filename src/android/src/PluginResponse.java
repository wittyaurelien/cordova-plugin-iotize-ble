package com.iotize.plugin;

import com.iotize.android.core.util.Helper;

import org.apache.cordova.CallbackContext;
import org.apache.cordova.PluginResult;
import org.json.JSONArray;
import org.json.JSONObject;

public class PluginResponse {

    private static final String TAG = "PluginResponse";
    private final CallbackContext callbackContext;
    private final String action;

    public PluginResponse(String action, JSONArray args, CallbackContext callbackContext) {
        this.callbackContext = callbackContext;
        this.action = action;

    }

    public void newResult(String data) {
        PluginResult result = new PluginResult(
                PluginResult.Status.OK,
                data
        );
        result.setKeepCallback(true);
        this.callbackContext.sendPluginResult(result);
    }

    public void newResult(JSONObject data) {
        PluginResult result = new PluginResult(
                PluginResult.Status.OK,
                data
        );
        result.setKeepCallback(true);
        this.callbackContext.sendPluginResult(result);
    }

    public void newResult(BLEComError error) {
        this.callbackContext.sendPluginResult(new PluginResult(
                PluginResult.Status.ERROR,
                JSONBuilder.toJSONObject(error)
        ));
    }

    public void success(byte[] response) {
        this.callbackContext.success(Helper.ByteArrayToHexString(response));
    }

    public void error(BLEComError err) {
        JSONObject object = JSONBuilder.toJSONObject(err);
        this.callbackContext.error(object);
    }

    public void success(boolean isConnected) {
        this.callbackContext.success(isConnected ? "true": "false");
    }

    public void success() {
        this.callbackContext.success();
    }

    public void error(String message) {
        this.callbackContext.success(message);
    }

    public void success(String msg) {
        this.callbackContext.success(msg);
    }

    public CallbackContext getCallbackContext() {
        return callbackContext;
    }
}
