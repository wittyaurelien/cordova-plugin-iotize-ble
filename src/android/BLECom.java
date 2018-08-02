package com.iotize.plugin;

import org.apache.cordova.CallbackContext;
import org.apache.cordova.CordovaInterface;
import org.apache.cordova.CordovaPlugin;
import org.apache.cordova.CordovaWebView;
import org.apache.cordova.PluginResult;
import org.apache.cordova.PluginResult.Status;
import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;

import android.util.Log;

public class BLECom extends CordovaPlugin {
  
  private static final String TAG = "BLECom";

  public void initialize(CordovaInterface cordova, CordovaWebView webView) {
    super.initialize(cordova, webView);

    Log.d(TAG, "Initializing IoTizeBLE Plugin");
  }

  public boolean execute(String action, JSONArray args, final CallbackContext callbackContext) throws JSONException {
    
    if(action.equals("checkAvailable")) {
         
      Log.d(TAG, "checkAvailable");

    } else if(action.equals("startScan")) {

      Log.d(TAG, "startScan");

      // An example of returning data back to the web layer
      //final PluginResult result = new PluginResult(PluginResult.Status.OK, (new Date()).toString());
      //callbackContext.sendPluginResult(result);
    
    } else if(action.equals("stopScan")) {

      Log.d(TAG, "stopScan");
     
    } else if(action.equals("connect")) {

      Log.d(TAG, "connect");
     
    } else if(action.equals("disConnect")) {

      Log.d(TAG, "disConnect");
     
    } else if(action.equals("getLastError")) {

      Log.d(TAG, "getLastError");
     
    } else if(action.equals("sendRequest")) {

      Log.d(TAG, "sendRequest");
     
    }
    return true;
  }

}