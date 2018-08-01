# Bluetooth Low Energy (BLE) communication with IoTize modules Plugin for Apache Cordova

This plugin enables communication between a phone and a BLE enabled IOTIZE module.

The plugin provides a simple [JavaScript API](#api) for iOS, Android and Windows.

 * Scan for IoTize peripherals
 * Connect to an IoTize peripheral
 * Send a Request and returns the reponse from the peripheral

See the [IoTize Monitoring App](https://github.com/don/cordova-plugin-ble-central/tree/master/examples) for ideas on how this plugin can be used.

## Supported Platforms

* iOS
* Windows (10) 

# Installing

### Cordova

    $ cordova plugin add cordova-plugin-iotize-ble
    $ ionic cordova plugin add C:/Developpement/cordova/cordova-plugin-iotize/cordova-plugin-iotize-ble
    for iOS since the plugin is in swift language, before using iotize-ble plugin , cordova-plugin-add-swift-support:https://github.com/akofman/cordova-plugin-add-swift-support should be installed. 

# API

## Methods

- [iotize-ble.startScan](#startscan)
- [iotize-ble.stopScan](#stopscan)
- [iotize-ble.connect](#connect)
- [iotize-ble.disconnect](#disconnect)
- [iotize-ble.send](#send)

## startScan

Scan and discover IoTize BLE peripherals.

    iotize-ble.startScan(success, failure);

### Description

Function `startScan` scans for IoTize BLE devices.  Scanning will continue until `stopScan` is called.

### Parameters

- __success__: Success callback function that is invoked upon a successfull start of scanning with "Ok" as result, then upon each discovery called with the name of the device as result.
- __failure__: Error callback function, invoked when error occurs. [optional]


## stopScan

Stop scanning for BLE peripherals.

    iotize-ble.stopScan(success, failure);

### Description

Function `stopScan` stops scanning for BLE devices.

### Parameters

- __success__: Success callback function, invoked when scanning is stopped. [optional]
- __failure__: Error callback function, invoked when error occurs. [optional]

### Quick Example

    iotize-ble.startScan([], function(device) {
        console.log(device);
    }, failure);

    setTimeout(function() {
        iotize-ble.stopScan(
            function() { console.log("Scan complete"); },
            function() { console.log("stopScan failed"); }
        );
    }, 5000);
    
## connect

Connect to a peripheral.

    iotize-ble.connect(device_id, connectCallback, connectionErrorCallback);

### Description

Function `connect` connects to a BLE peripheral. The callback is long running. The connect callback will be called when the connection is successful. 

The connectionErrorCallback callback is called if the connection fails, or later if the peripheral disconnects. The connectionErrorCallback callback is only called when the peripheral initates the disconnection. The disconnect callback is not called when the application calls (#disconnect). The disconnect callback is how your app knows the peripheral inintiated a disconnect.

### Scanning before connecting

Devices should be scanned before connection. Please note that for this version only one device at a time could be connected.

### Parameters

- __device_id__: name of the peripheral
- __connectCallback__: Connect callback function that is invoked when the connection is successful.
- __disconnectCallback__: Disconnect callback function, invoked when the peripheral disconnects or an error occurs.

## disconnect

Disconnect.

    iotize-ble.disconnect(device_id, [success], [failure]);

### Description

Function `disconnect` disconnects the selected device.

### Parameters

- __device_id__: name of the peripheral
- __success__: Success callback function that is invoked when the connection is successful. [optional]
- __failure__: Error callback function, invoked when error occurs. [optional]

## send

sends a frame of byte to ioTize device using SPP characteristic.

    iotize-ble.send(device_id, __data__, success, failure);

### Parameters
- __device_id__: name address of the peripheral
- __data__: binary data as a string ex:"A2CA000007010003FFFF0002"
- __success__: Success callback function that is invoked when the connection is successful. The result parameter is the response as a string where bytes are '-' separated ex: "45-49-6F-54-7A-50-72-30-30-34-31-30-30-30-30-31-30-39-42-90-00"
- __failure__: Error callback function, invoked when error occurs. [optional]

### Quick Example

    // read data from a characteristic, do something with received data
    iotize-ble.send(
               name,
               data,
               function successHandler(result) {
                   console.log("received result " + result);            
                   observer.complete();
               },
               function errorHandler(err) {
                   observer.error(err);
               });
       });


# Testing the Plugin

The plugin could be tested using ionic framework.

TODO

# License

Apache 2.0

# Feedback

Try the code. If you find an problem or missing feature, file an issue in Jira .
