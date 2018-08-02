
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
    
  static func PeripheralConnectionFailed(peripheral: CBPeripheral, error: Error?) -> IoTizeBleError { return IoTizeBleError(code:200, message:"Connection to \(peripheral.name) failed: \(error?.localizedDescription ?? "unknown")" )}
  static func NoDeviceConnected() -> IoTizeBleError { return IoTizeBleError(code:201, message:"No Device Connected")}
  
  static func ServiceDiscoveryFailed(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:300, message:"Failed to discover services for \(peripheral.name)" )}
  static func CharacteristicsDiscoveryFailed(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:301, message:"Failed to discover characteristics for \(peripheral.name)" )}
  static func CharacteristicSPPNotFound(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:302, message:"Characteristic SPP not found for \(peripheral.name)" )}
  static func CharacteristicNotifyChangeFailed() -> IoTizeBleError { return IoTizeBleError(code:304, message:"Failed to set notification for characteristic" )}
  static func BleVersionIsOld(version: String) -> IoTizeBleError { return IoTizeBleError(code:303, message:"BLE firmware version is too old: \(version)" )}
  
  static func InvalidWriteData(peripheral: CBPeripheral) -> IoTizeBleError { return IoTizeBleError(code:403, message:"Invalid write data for \(peripheral.name)" )}
  static func TimedOutRequest() -> IoTizeBleError { return IoTizeBleError(code:404, message:"Waiting for response timed out" )}


}

@objc(BLECom) class BLECom : CDVPlugin {

  var bleController: BLEManager!
  var lastError: IoTizeBleError?

  //
  override func pluginInitialize(){
    bleController = BLEManager()
  }

  func sendSuccess(command: CDVInvokedUrlCommand, result: String){
    let pluginResult =  CDVPluginResult(
              status: CDVCommandStatus_OK,
              messageAs: result
              )
    self.commandDelegate!.send( pluginResult, callbackId: command.callbackId)
  }
 
  func sendSuccessWithResponse(command: CDVInvokedUrlCommand, result: String){
    let pluginResult =  CDVPluginResult(
              status: CDVCommandStatus_OK,
              messageAs: result
              )
    pluginResult.setKeepCallbackAsBool(true);
    self.commandDelegate!.send( pluginResult, callbackId: command.callbackId, )
  }

  func sendError(command: CDVInvokedUrlCommand, result: String){
    let pluginResult =  CDVPluginResult(
              status: CDVCommandStatus_ERROR,
              messageAs: result
              )
    self.commandDelegate!.send( pluginResult, callbackId: command.callbackId)
  }

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
            self.sendSuccess(command: command, result: "Ok")
          }
        } 
      })
    } 
  }

  //Start scanning for IoTize devices
  @objc(startScan:)
  func startScan(command: CDVInvokedUrlCommand) {        
     
      self.bleController.startScan(completionWithResponse: {
        (result: string, error: IoTizeBleError?) -> () in
        
         DispatchQueue.main.async {
            
          if (error != nil){
            self.lastError = error
            self.sendError(command: command, result: error!.message)
          }
          else {
            self.sendSuccessWithResult(command: command, result: string)
          }
        } 
      })
  }

  //Stop scanning for IoTize devices
  @objc(stopScan:)
  func stopScan(command: CDVInvokedUrlCommand) {
    bleController.stopScan();
    self.sendSuccess(command: command, result: "Ok")
  }


  //Connect to a device using its name
  @objc(connect:)
  func connect(command: CDVInvokedUrlCommand) {
    
    //we need the name of the device
    if (command.arguments.count == 0){
        self.sendError(command: command, result: "Connection parameter error")
        return
    }

    let nameDevice = command.arguments[0] as? String ?? ""
    
    bleController.connectWithName(device: nameDevice, completion: {
        (error: IoTizeBleError?) -> () in
        
        DispatchQueue.main.async {
            
            if (error != nil){
              self.lastError = error
              self.sendError(command: command, result: error!.message)
            }
            else {
              print("##> Sending Connected Ok")
              self.sendSuccess(command: command, result: "Ok")
            }
        }
    })
  }
  
  //Disconnect to a device using its name
  @objc(disConnect:)
  func disConnect(command: CDVInvokedUrlCommand) {
    
    //we need the name of the device
    if (command.arguments.count == 0){
        self.sendError(command: command, result: "disConnection parameter error")
        return
    }

    let nameDevice = command.arguments[0] as? String ?? ""
    
    bleController.disConnect(device: nameDevice, completion: {
        (error: IoTizeBleError?) -> () in
        
        DispatchQueue.main.async {
            
            if (error != nil){
              self.lastError = error
              self.sendError(command: command, result: error!.message)
            }
            else {
              print("##> Sending Disconnected Ok")
              self.sendSuccess(command: command, result: "Ok")
            }
        }
    })
  }

  @objc(getLastError:)
   func getLastError(command: CDVInvokedUrlCommand) {
     let msg: String = (lastError != nil) ? (lastError!.message) : ""
     self.sendSuccess(command: command, result: msg)
    }

  //Send Data to device
  @objc(sendRequest:)
  func sendRequest(command: CDVInvokedUrlCommand) {
    
    //we need data to send
    if (command.arguments.count == 1){
        self.sendError(command: command, result: "SendRequest parameter error")
        return
    }

    let device = command.arguments[0] as? String ?? ""
    let data = command.arguments[1] as? String ?? ""
    
    bleController.sendRequest(data: data, completion: {
        (response: String, error: IoTizeBleError?) -> () in
        
        DispatchQueue.main.async {
            
            if (error != nil){
              self.lastError = error
              self.sendError(command: command, result: error!.message)
            }
            else {
              print("##> Sending Request Ok " + response)
              self.sendSuccess(command: command, result: response)
            }
        }
    })
  }
}
