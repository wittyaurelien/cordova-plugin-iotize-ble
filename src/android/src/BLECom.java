package com.iotize.plugin;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.provider.Settings;
import android.util.Log;

import com.iotize.android.communication.protocol.ble.BLEProtocol;
import com.iotize.android.communication.protocol.ble.DeviceManager;
import com.iotize.android.communication.protocol.ble.scanner.BLEScanner;
import com.iotize.android.core.util.Helper;
import com.iotize.android.device.api.device.scanner.IOnDeviceDiscovered;

import org.apache.cordova.CallbackContext;
import org.apache.cordova.CordovaInterface;
import org.apache.cordova.CordovaPlugin;
import org.apache.cordova.CordovaWebView;
import org.apache.cordova.LOG;
import org.apache.cordova.PermissionHelper;
import org.json.JSONArray;

import static android.Manifest.permission.ACCESS_COARSE_LOCATION;

public class BLECom extends CordovaPlugin {
    // actions
    private static final String CONNECT = "connect";
    private static final String DISCONNECT = "disConnect";
    private static final String SEND_REQUEST = "sendRequest";
    private static final String GET_LAST_ERROR = "getLastError";
    private static final String START_SCAN = "startScan";
    private static final String STOP_SCAN = "stopScan";
    private static final String IS_ENABLED = "isEnabled";
    private static final String IS_CONNECTED  = "isConnected";
    private static final String ENABLE = "enable";

    private static final int REQUEST_ENABLE_BLUETOOTH = 1;
    private static final int REQUEST_SCAN_PERMISSIONS = 2;

    private static final String TAG = "BLECom";
    private DeviceManager<BLEProtocol> peripherals;
    private BluetoothAdapter bluetoothAdapter;
    private PluginResponse enableBluetoothCallback;
    private PluginResponse pluginResponseDiscoverDevice;
    private PluginResponse permissionCallback;
    private BLEScanner scanner;
    private IOnDeviceDiscovered<BLEScanner.BluetoothDeviceAdapter> onDeviceDiscoveredCallback;

    public void initialize(CordovaInterface cordova, CordovaWebView webView) {
        super.initialize(cordova, webView);

        this.peripherals = new DeviceManager<>();
        Log.d(TAG, "Initializing IoTizeBLE Plugin");
    }

    @Override
    public void onStart() {
        super.onStart();
    }

    @Override
    public void onStop() {
        super.onStop();
    }

    @SuppressLint("MissingPermission")
    public boolean execute(String action, JSONArray args, final CallbackContext callbackContext) {

        PluginResponse pluginResponse = new PluginResponse(
                action,
                args,
                callbackContext
        );
        ArgsHelper argsHelper = new ArgsHelper(args);

        try {
            this.setupBluetoothAdapter();

            switch (action) {
                case STOP_SCAN:
                    Log.d(TAG, STOP_SCAN);
                    this.stopScan(pluginResponse);
                    break;
                case START_SCAN:
                    Log.d(TAG, START_SCAN);
                    this.startScan(pluginResponse);
                    break;
                case CONNECT: {
                    Log.d(TAG, CONNECT);
                    String macAddress = argsHelper.getString(0);
                    this.connect(pluginResponse, macAddress);
                    break;
                }
                case DISCONNECT: {
                    Log.d(TAG, DISCONNECT);
                    String macAddress = argsHelper.getString(0);
                    this.disconnect(pluginResponse, macAddress);
                    break;
                }
                case GET_LAST_ERROR:
                    Log.d(TAG, GET_LAST_ERROR);
                    this.getLastError(pluginResponse);
                    break;
                case SEND_REQUEST:
                    String deviceId = argsHelper.getString(0);
                    String hexString = argsHelper.getString(1);
                    this.sendRequest(pluginResponse, deviceId, hexString);
                    break;
                case IS_ENABLED:
                    pluginResponse.success(bluetoothAdapter.isEnabled());

                    break;
                case IS_CONNECTED: {
                    String macAddress = argsHelper.getString(0);
                    boolean isConnected = peripherals.get(macAddress).isConnected();
                    pluginResponse.success(isConnected);
                    break;
                }
                case ENABLE:
                    if (enableBluetoothCallback != null) {
                        Log.w(TAG, "There is already an enable request pending...");
                    }
                    enableBluetoothCallback = pluginResponse;
                    Intent intent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
                    cordova.startActivityForResult(this, intent, REQUEST_ENABLE_BLUETOOTH);
                    break;
                default:
                    throw new BLEComError(BLEComError.Code.ILLEGAL_ACTION, "Illegal action " + action);
            }
            return true;
        }
        catch (BLEComError err) {
            pluginResponse.error(err);
            return false;
        }
        catch (Throwable err) {
            pluginResponse.error(new BLEComError(
                BLEComError.Code.UNKNOWN,
                err
            ));
            return false;
        }
    }

    private void setupBluetoothAdapter() throws BLEComError {
        if (bluetoothAdapter == null) {
            Activity activity = cordova.getActivity();
            boolean hardwareSupportsBLE = activity.getApplicationContext()
                                .getPackageManager()
                                .hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE);
            if (!hardwareSupportsBLE) {
                LOG.w(TAG, "This hardware does not support Bluetooth Low Energy.");
                throw new BLEComError(BLEComError.Code.BLE_ADPATER_NOT_AVAILABLE);
            }
            BluetoothManager bluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
            if (bluetoothManager == null){
                throw new BLEComError(BLEComError.Code.BLE_ADPATER_NOT_AVAILABLE);
            }
            bluetoothAdapter = bluetoothManager.getAdapter();
        }

    }

    private boolean locationServicesEnabled() {
        int locationMode = 0;
        try {
            locationMode = Settings.Secure.getInt(cordova.getActivity().getContentResolver(), Settings.Secure.LOCATION_MODE);
        } catch (Settings.SettingNotFoundException e) {
            LOG.e(TAG, "Location Mode Setting Not Found", e);
        }
        return (locationMode > 0);
    }


    private void startScan(PluginResponse pluginResponse) {
        if (!locationServicesEnabled()) {
            pluginResponse.error(new BLEComError(BLEComError.Code.LOCATION_SERVICE_DISABLED, "Location service must be enabled to scan low energy device"));
            return;
        }
        if (pluginResponseDiscoverDevice != null && pluginResponseDiscoverDevice != pluginResponse){
            Log.w(TAG, "Scan is already started...");
        }
        pluginResponseDiscoverDevice = pluginResponse;
        if(!PermissionHelper.hasPermission(this, ACCESS_COARSE_LOCATION)) {
            permissionCallback = pluginResponse;
            PermissionHelper.requestPermission(this, REQUEST_SCAN_PERMISSIONS, ACCESS_COARSE_LOCATION);
            return;
        }

        if (scanner == null){
            this.initBLEScanner();
        }
        scanner.start();
        pluginResponse.newResult("Ok");
    }


    private void initBLEScanner() {
        Log.d(TAG, "initBLEScanner()");
        scanner = new BLEScanner(cordova.getContext());
        if (onDeviceDiscoveredCallback == null){
            this.onDeviceDiscoveredCallback = new IOnDeviceDiscovered<BLEScanner.BluetoothDeviceAdapter>() {
                @Override
                public void onDeviceDiscovered(BLEScanner.BluetoothDeviceAdapter device) {
                    Log.d(TAG, "Device discovered: " + device);
                    if (pluginResponseDiscoverDevice != null) {
                        try {
                            pluginResponseDiscoverDevice.newResult(
                                    JSONBuilder.toJSONObject(device)
                            );
                        } catch (Throwable e) {
                            pluginResponseDiscoverDevice.newResult(new BLEComError(
                                    BLEComError.Code.INTERNAl_ERROR,
                                    e
                            ));
                        }
                    }
                    else{
                        Log.w(TAG, "Discovered device but no listener has been setup");
                    }
                }

                @Override
                public void onScanFailed() {
                    Log.w(TAG, "Scan failed");
                }
            };
        }
        scanner.setOnDeviceDiscoveredCallback(this.onDeviceDiscoveredCallback);
    }

    @SuppressLint("NewApi")
    private void stopScan(PluginResponse pluginResponse) {
        pluginResponseDiscoverDevice = null;
        if (scanner == null || !scanner.isEnabled()){
            Log.w(TAG, "Scanner is not running");
            pluginResponse.success();
            return;
        }
        scanner.stop();
        pluginResponse.success();
    }

    private void getLastError(PluginResponse pluginResponse) {
        throw new Error("Not implemented yet");
    }

    private void disconnect(final PluginResponse pluginResponse, String macAddress) {
        BLEProtocol peripheral = peripherals.get(macAddress);

        executeAsync(() -> {
            try {
                peripheral.disconnect();
                pluginResponse.success();
            } catch (Exception e) {
                pluginResponse.error(new BLEComError(BLEComError.Code.DISCONNECTION_ERROR, e));
            }
        });
    }

    private void sendRequest(PluginResponse pluginResponse, String deviceId, String hexString) throws BLEComError {
        if (deviceId == null) {
            throw new IllegalArgumentException("Device id must not be null");
        }

        byte[] data = hexStringToByteArray(hexString);
        Log.d(TAG, SEND_REQUEST + " " + deviceId + " " + hexString);

        BLEProtocol peripheral = peripherals.get(deviceId);
        executeAsync(() -> {
            try {
                byte[] response = peripheral.send(data);
                pluginResponse.success(response);
            } catch (Exception e) {
                pluginResponse.error(new BLEComError(BLEComError.Code.CONNECTION_ERROR, e));
            }
        });
    }

    private void executeAsync(Runnable runnable) {
        cordova.getThreadPool().execute(runnable);
        //runnable.run();
    }

    private byte[] hexStringToByteArray(String hexString) throws BLEComError {
        try{
            return Helper.HexStringToByteArray(hexString);
        }
        catch (Throwable ex){
            throw new BLEComError(BLEComError.Code.ILLEGAL_ARGUMENT, "Should be a valid hexadecimal string");
        }
    }

    private void connect(PluginResponse pluginResponse, String macAddress) throws BLEComError {
        BLEProtocol peripheral = peripherals.getIfExists(macAddress);
        if (peripheral == null){
            if (!BluetoothAdapter.checkBluetoothAddress(macAddress)) {
                throw new BLEComError(BLEComError.Code.INVALID_MAC_ADDRESS);
            }
            BluetoothDevice device = bluetoothAdapter.getRemoteDevice(macAddress);
            peripheral = new BLEProtocol(cordova.getActivity(), device);
            peripherals.put(macAddress, peripheral);
        }
        BLEProtocol finalPeripheral = peripheral;
        if (finalPeripheral.isConnected()){
            pluginResponse.success();
            return;
        }
        executeAsync(() -> {
            try {
                finalPeripheral.connect();
                pluginResponse.success();
            } catch(Exception e) {
                pluginResponse.error(new BLEComError(BLEComError.Code.CONNECTION_ERROR, e));
            }
        });
    }

    /* @Override */
    public void onRequestPermissionResult(int requestCode, String[] permissions, int[] grantResults) {
        for(int result:grantResults) {
            if(result == PackageManager.PERMISSION_DENIED) {
                LOG.d(TAG, "User *rejected* Coarse Location Access");
                if (permissionCallback != null){
                    this.permissionCallback.error("Location permission not granted.");
                }
                return;
            }
        }

        switch(requestCode) {
            case REQUEST_SCAN_PERMISSIONS:
                LOG.d(TAG, "User granted scan permissions");
                this.startScan(pluginResponseDiscoverDevice);
                this.permissionCallback = null;
                break;
        }
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {

        if (requestCode == REQUEST_ENABLE_BLUETOOTH) {

            if (resultCode == Activity.RESULT_OK) {
                LOG.d(TAG, "User enabled Bluetooth");
                if (enableBluetoothCallback != null) {
                    enableBluetoothCallback.success();
                }
            } else {
                LOG.d(TAG, "User did *NOT* enable Bluetooth");
                if (enableBluetoothCallback != null) {
                    enableBluetoothCallback.error("User did not enable Bluetooth");
                }
            }

            enableBluetoothCallback = null;
        }
    }

}