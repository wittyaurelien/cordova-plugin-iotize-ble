package com.iotize.plugin;

import android.annotation.SuppressLint;
import android.bluetooth.BluetoothDevice;
import android.util.Log;

import com.iotize.android.communication.protocol.ble.scanner.BLEScanner;

import org.json.JSONException;
import org.json.JSONObject;

public class JSONBuilder {

    private static final String TAG = "JSONBuilder";

    @SuppressLint("MissingPermission")
    public static JSONObject toJSONObject(BLEScanner.BluetoothDeviceAdapter device) {
        try {
            JSONObject json = new JSONObject();

            BluetoothDevice bluetoothDevice = device.getDevice();

            json.put("name", bluetoothDevice.getName());
            json.put("address", bluetoothDevice.getAddress());
            json.put("type", bluetoothDevice.getType());
            //        if (advertisingData != null) {
            //            json.put("advertising", byteArrayToJSON(advertisingData));
            //        }
            // TODO real RSSI if we have it, else
            //        if (advertisingRSSI != FAKE_PERIPHERAL_RSSI) {
            //            json.put("rssi", advertisingRSSI);
            //        }
            return json;
        }
        catch (JSONException e) {
            Log.e(TAG, "Internal error", e);
            throw new Error("INTERNAL ERROR: " + e.getMessage(), e);
        }
    }

    public static JSONObject toJSONObject(BLEComError error) {
        try {
            JSONObject json = new JSONObject();

            json.put("code", error.getCode());
            json.put("message", error.getMessage());

            return json;
        }
        catch (JSONException e) {
            Log.e(TAG, "Internal error", e);
            throw new Error("INTERNAL ERROR: " + e.getMessage(), e);
        }
    }

}
