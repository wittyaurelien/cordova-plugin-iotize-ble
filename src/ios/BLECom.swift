//
//  Copyright 2018 IoTize SAS Inc.  Licensed under the MIT license. 
//
//  BLECom.swift
//  device-com-ble.cordova BLE Cordova Plugin
//

import Foundation
import CoreBluetooth

struct IoTizeBleError: Error {
    
    let code: Int
    let message: String
    
    init(code: Int, message: String) {
        self.code = code
        self.message = message
    }
    
    static func BleUnsupported() -> IoTizeBleError { return IoTizeBleError(code:100, message:"Bluetooth is not supported")}
    static func BleUnauthorized() -> IoTizeBleError { return IoTizeBleError(code:101, message:"Bluetooth is not authorized")}
    static func BlePoweredOff() -> IoTizeBleError { return IoTizeBleError(code:102, message:"Bluetooth is powered off")}
    static func BleResetting() -> IoTizeBleError { return IoTizeBleError(code:103, message:"Bluetooth is resetting")}
    static func BleUnknown() -> IoTizeBleError { return IoTizeBleError(code:104, message:"Bluetooth is in an unknown state")}
    
    static func PeripheralConnectionFailed(peripheral: CBPeripheral, error: Error?) -> IoTizeBleError { return IoTizeBleError(code:200, message:"Connection to \(peripheral.name ?? "unknown peripheral") failed: \(error?.localizedDescription ?? "unknown")" )}
    static func NoDeviceConnected() -> IoTizeBleError { return IoTizeBleError(code:201, message:"No Device Connected")}
    
    static func ServiceDiscoveryFailed(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:300, message:"Failed to discover services for \(peripheral.name ?? "unknown peripheral")" )}
    static func CharacteristicsDiscoveryFailed(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:301, message:"Failed to discover characteristics for \(peripheral.name ?? "unknown peripheral")" )}
    static func CharacteristicSPPNotFound(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:302, message:"Characteristic SPP not found for \(peripheral.name ?? "unknown peripheral")" )}
    static func CharacteristicNotifyChangeFailed() -> IoTizeBleError { return IoTizeBleError(code:304, message:"Failed to set notification for characteristic" )}
    static func BleVersionIsOld(version: String) -> IoTizeBleError { return IoTizeBleError(code:303, message:"BLE firmware version is too old: \(version)" )}
    
    static func InvalidWriteData(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:403, message:"Invalid write data for \(peripheral.name ?? "unknown peripheral")" )}
    static func TimedOutRequest(msg: String) -> IoTizeBleError { return IoTizeBleError(code:404, message:"Waiting for response timed out, txData: " + msg )}
        
}

//Main class handling the plugin functionalities.
@objc(BLECom) class BLECom : CDVPlugin {
    
    var bleController: BLEManager!
    var lastError: IoTizeBleError?
    
    //
    override func pluginInitialize(){
        bleController = BLEManager()
    }
    
    //helper to return a string 
    func sendSuccess(command: CDVInvokedUrlCommand, result: String){
        let pluginResult =  CDVPluginResult(
            status: CDVCommandStatus_OK,
            messageAs: result
        )
        self.commandDelegate!.send( pluginResult, callbackId: command.callbackId)
    }
    
    //helper to return a boolean 
    private func sendSuccess(command: CDVInvokedUrlCommand, result: Bool) {
        let pluginResult = CDVPluginResult(
            status: CDVCommandStatus_OK,
            messageAs: result
        )
        self.commandDelegate!.send(pluginResult, callbackId: command.callbackId)
    }
    
    //helper to return a JSon object 
    func sendSuccess(command: CDVInvokedUrlCommand, result: DiscoveredDeviceType){
        let pluginResult =  CDVPluginResult(
            status: CDVCommandStatus_OK,
            messageAs: result.ToJSON()
        )
        self.commandDelegate!.send( pluginResult, callbackId: command.callbackId)
    }
    
    //helper to return a String with keeping the callback
    func sendSuccessWithResponse(command: CDVInvokedUrlCommand, result: String){
        let pluginResult =  CDVPluginResult(
            status: CDVCommandStatus_OK,
            messageAs: result
        )
        pluginResult!.setKeepCallbackAs(true);
        self.commandDelegate!.send( pluginResult, callbackId: command.callbackId )
    }
    
    //helper to return a JSon object with keeping the callback
    func sendSuccessWithResponse(command: CDVInvokedUrlCommand, result: DiscoveredDeviceType){
        let pluginResult =  CDVPluginResult(
            status: CDVCommandStatus_OK,
            messageAs: result.ToJSON()
        )
        pluginResult!.setKeepCallbackAs(true);
        self.commandDelegate!.send( pluginResult, callbackId: command.callbackId )
    }
    
    //helper to send back an error
    func sendError(command: CDVInvokedUrlCommand, result: String){
        let pluginResult =  CDVPluginResult(
            status: CDVCommandStatus_ERROR,
            messageAs: result
        )
        self.commandDelegate!.send( pluginResult, callbackId: command.callbackId)
    }
    
    //Is BLE available
    @objc(checkAvailable:)
    func checkAvailable(command: CDVInvokedUrlCommand) {
        
        DispatchQueue.main.async {
            
            //from ios5
            if ( floor(NSFoundationVersionNumber) <= floor(NSFoundationVersionNumber_iOS_5_1) ){
                self.sendError(command: command, result: IoTizeBleError.BleUnsupported().message)
            }
            
            //check State
            self.bleController.checkState(completion: {
                (error: IoTizeBleError?) -> () in
                
                DispatchQueue.main.async {
                    
                    if (error != nil){
                        self.lastError = error
                        self.sendError(command: command, result: error!.message)
                    }
                    else {
                        self.sendSuccess(command: command, result: true)
                    }
                    
                }
            })
        }
    }
    

    //Start scanning  IoTize devices    
    @objc(startScan:)
    func startScan(command: CDVInvokedUrlCommand) {
        
        if (!self.isReady()){
            
            DispatchQueue.main.async {
                Thread.sleep(forTimeInterval: 0.01)
                self.startScan(command: command)
            }
            return
        }
        
        self.bleController.beginScan(completion: {
            (result: Any, error: IoTizeBleError?) -> () in
            
            DispatchQueue.main.async {
                
                if (error != nil){
                    self.lastError = error
                    self.sendError(command: command, result: error!.message)
                }
                else if let resultString = result as? String{
                    self.sendSuccessWithResponse(command: command, result: resultString)
                } else if let resultDiscoveredDevice = result as? DiscoveredDeviceType {
                    self.sendSuccessWithResponse(command: command, result: resultDiscoveredDevice)
                } else if let resultDevice = result as? CBPeripheral {
                    self.sendSuccessWithResponse(command: command, result: CBPeripheralConverter.toDiscoveredDeviceType(device: resultDevice))
                } else {
                }
            }
        })
    }
    
    //Stop scanning 
    @objc(stopScan:)
    func stopScan(command: CDVInvokedUrlCommand) {
        bleController.stopScan();
        self.sendSuccess(command: command, result: "Ok")
    }
    
    
    //Connect to a device using its UUID
    @objc(connect:)
    func connect(command: CDVInvokedUrlCommand) {
        
        //we need the UUID of the device
        if (command.arguments.count == 0){
            self.sendError(command: command, result: "Connection parameter error")
            return
        }
        
        let deviceUUID = command.arguments[0] as? String ?? ""
        
        bleController.connectWithUUID(device: deviceUUID, completion: {
            (state: Any?, error: IoTizeBleError?) -> () in
            
            DispatchQueue.main.async {
                
                if (error != nil){
                    self.lastError = error
                    self.sendError(command: command, result: error!.message)
                }
                else {
                    //print("##> Sending Connected Ok")
                    let _state = state as? String

                    self.sendSuccessWithResponse(command: command, result: _state ?? "") // keep callback
                }
            }
            })
    }
    
    //Disconnect from a device using its name
    @objc(disConnect:)
    func disConnect(command: CDVInvokedUrlCommand) {
        
        //we need the name of the device
        if (command.arguments.count == 0){
            self.sendError(command: command, result: "disConnection parameter error")
            return
        }
                
        bleController.disconnect( completion: {
            (error: IoTizeBleError?) -> () in
            
            DispatchQueue.main.async {
                
                if (error != nil){
                    self.lastError = error
                    self.sendError(command: command, result: error!.message)
                }
                else {
                    self.sendSuccess(command: command, result: "Ok")
                }
            }
        })
    }
    
    //Retrieve additional information
    @objc(getLastError:)
    func getLastError(command: CDVInvokedUrlCommand) {
        let msg: String = (lastError != nil) ? (lastError!.message) : ""
        self.sendSuccess(command: command, result: msg)
    }
    
    //Send Data to device
    @objc(sendRequest:)
    func sendRequest(command: CDVInvokedUrlCommand) {
        print("##> SendRequest")
        //we need data to send
        if (command.arguments.count == 1){
            self.sendError(command: command, result: "SendRequest parameter error")
            return
        }
        
        let data = command.arguments[1] as? String ?? ""
        
        bleController.sendRequest(data: data, completion: {
            (response: Any, error: IoTizeBleError?) -> () in
            
            DispatchQueue.main.async {
                
                if (error != nil){
                    self.lastError = error
                    self.sendError(command: command, result: error!.message)
                }
                else {
                    if let responseString = response as? String {
                        self.sendSuccess(command: command, result: responseString)
                    }
                    else if let responseDiscoveredDevice = response as? DiscoveredDeviceType {
                        self.sendSuccess(command: command, result: responseDiscoveredDevice)
                    } else if let responseDevice = response as? CBPeripheral {
                        self.sendSuccess(command: command, result: CBPeripheralConverter.toDiscoveredDeviceType(device: responseDevice))
                    }
                }
            }
        })
    }
    
    @objc(isConnected:)
    func isConnected(command: CDVInvokedUrlCommand) {
        
        let error: Bool ;
        
        error = bleController.isConnected()
        self.sendSuccess(command: command, result: error)
        
    }

    func isReady() -> Bool {
        return bleController.isReady()
    }

}
