//
//  BLEManager.swift
//  IoTize
//
//  Created by IoTize on 26/05/2017.
//  Copyright © 2017 IoTize. All rights reserved.
//

import CoreBluetooth

typealias Completion = (IoTizeBleError?) -> ()
typealias CompletionWithResponse = (Any , IoTizeBleError?) -> ()


/// The functions within this class are all callbacks from the BLE device and serves the purpose of correctly connecting with the device and retrieving its informations
class BLEManager: NSObject, CBCentralManagerDelegate
{
    //ios GATT
    private var centralManager: CBCentralManager!
    
    //IoTize devices are filtered using this service
    let serviceUUID = CBUUID(string: "6C7B16C2-2A5B-8C9F-CF42-D31425470E7B")
 
    var discoveredPeripherals = [CBPeripheral]()  // table of discoveredPeripherals, initially it is empty
    var stateChangeCompletion: Completion?        //callback when the state is retreived
    var connectionChangeCompletion: Completion?   //callback when the connection state is retreived
    var discoveryCompletion: CompletionWithResponse?   //callback when the connection state is retreived
    
    private var connectedDevice: BLEPeripheral?
  
     override init()
    {
        super.init()

        centralManager = CBCentralManager(delegate: self, queue: nil, options: [CBCentralManagerOptionShowPowerAlertKey: true])
        stateChangeCompletion = nil
        connectionChangeCompletion = nil
        connectedDevice = nil
        discoveryCompletion = nil
    }
    
    func checkState(completion: @escaping Completion){
        stateChangeCompletion = completion
    }

    /* search for devices, scanForPeripherals function is been called
     */
    func beginScan(completion: @escaping CompletionWithResponse){
        if #available(iOS 10.0, *) {
            if (centralManager.state == CBManagerState.poweredOn){
                discoveryCompletion = completion
                discoveredPeripherals.removeAll()
                centralManager.scanForPeripherals(withServices: [serviceUUID], options: nil)
                
                print("--> Start Scanning")
                discoveryCompletion!("Ok", nil)
            } else {
                print("#######> ---- Bluetooth not available yet")
            }
        } else {
            // Fallback on earlier versions
        }
    }
    
    func stopScan(){        
        centralManager.stopScan()
        print("--> Stop Scanning")
    }
    
    /* to connect to a device we need to stop scanning at first
     */
    func connect(_ device: CBPeripheral){        
        centralManager.stopScan()

        if (connectedDevice == nil){
            connectedDevice = BLEPeripheral()
        }
        //connectedDevice!.connect(device: device, manager: self)
        centralManager.connect(device, options: nil)
        print("--> Start Connecting")
    }
    
    /* to connect to a device using its name
     */
    func connectWithUUID( device: String, completion: @escaping Completion)
    {
        centralManager.stopScan()
        
        connectionChangeCompletion = completion
        for item : CBPeripheral in discoveredPeripherals{
            
            let name = item.identifier.uuidString
            print("name: " + name)
            print("device: " + device)
            if (name != nil) {
                
                if (name == device){
                    
                    connect(item)
                    break;
                    
                }                    
            }            
        }
    }

    /* call the function cancelPeripheralConnection to disconnect
     */
    func disconnect( completion: @escaping Completion){
        
        connectionChangeCompletion = completion
        //for the moment we just handle one device
        if ( connectedDevice != nil ){
            if connectedDevice!.bleDevice != nil {
                centralManager.cancelPeripheralConnection(connectedDevice!.bleDevice)                
            }
            connectedDevice!.disconnect()
            connectedDevice = nil
        }
    }
    
    func checkConnection() -> Bool {
        return (connectedDevice != nil)
    }

    /*if bluetooth manager not powered on then no bluetooth is available
     */
    func centralManagerDidUpdateState (_ central: CBCentralManager){
        var error: IoTizeBleError?         
        switch centralManager.state{
            case .poweredOn:
                print("------------> Central Manager is On")
                break
            case .poweredOff:
                error = IoTizeBleError.BlePoweredOff()
                break
            case .resetting:
                error = IoTizeBleError.BleResetting()
                break
            case .unauthorized:
                error = IoTizeBleError.BleUnauthorized()
                break
            case .unsupported:
                error = IoTizeBleError.BleUnsupported()
                break
            case .unknown:
                error = IoTizeBleError.BleUnknown()
                break
            
        }
        if (self.stateChangeCompletion != nil){
            self.stateChangeCompletion!(error)
            self.stateChangeCompletion = nil
        }
    }
    
    /* this function aim to discover the CBPeripheral
     * we define peripheral and name then we call discoveredDevice using these two
     */
    func centralManager(_ central: CBCentralManager, didDiscover peripheral: CBPeripheral, advertisementData: [String : Any], rssi RSSI: NSNumber)
    {
        if (!discoveredPeripherals.contains(peripheral))  // if the table discoveredPeripherals does not contain peripheral
        {
            if let name = advertisementData["kCBAdvDataLocalName"] as? String //define name
            {  
                discoveredPeripherals.append(peripheral)  // add peripheral to the table
                if ( discoveryCompletion != nil){
                    discoveryCompletion!( peripheral , nil)
                }
            }
        }
    }
    
    /* returns the currently discovered list of devices names
     */
    func getDeviceList() -> String {
        var listStr:String = ""; //string list to return
        //names are separated by \n
        for item : CBPeripheral in discoveredPeripherals{
            let name = item.name
            if ( (name != nil) && (name!.count != 0) ) {
                listStr = name! + "\n" + listStr
            }
        }
        return listStr;
    }
    
    /* this function aim to connect the CBPeripheral
     * we call bluetoothConnected using peripheral as paramater
     */
    func centralManager(_ central: CBCentralManager, didConnect peripheral: CBPeripheral){
       
        if ( connectionChangeCompletion != nil){
            connectionChangeCompletion!(nil)
            connectionChangeCompletion = nil
            
            print("--> Did Connect to \(String(describing: peripheral.name))")
            if (connectedDevice != nil){
                connectedDevice!.connect(device: peripheral, manager: self)
            }
           
        }
    }
    
    /* this function aim to check if connection is failed the CBPeripheral
     * we call connectionFailed
     */
    func centralManager(_ central: CBCentralManager, didFailToConnect peripheral: CBPeripheral, error: Error?){
        
        if ( connectionChangeCompletion != nil){
            connectionChangeCompletion!(IoTizeBleError.PeripheralConnectionFailed(peripheral: peripheral, error: error))
            connectionChangeCompletion = nil
        }
        print("--> Error in connecting to device. Error: \(String(describing: error?.localizedDescription))")
        connectedDevice = nil
    }
    
    
    /* this function is called the CBPeripheral did disconnect
     *
     */
    func centralManager (_ central: CBCentralManager, didDisconnectPeripheral peripheral: CBPeripheral, error: Error?){
        
        if ( connectionChangeCompletion != nil){
            connectionChangeCompletion!(nil)
            connectionChangeCompletion = nil
            return
        }
        connectionChangeCompletion!(IoTizeBleError.PeripheralConnectionFailed(peripheral: peripheral, error: error))
        print("--> Error in disconnecting device. Error: \(String(describing: error?.localizedDescription))")
        connectedDevice = nil
    }

    func checkConnection(completion: @escaping Completion){
        if (connectedDevice == nil){
           
            completion(IoTizeBleError.NoDeviceConnected())
        }else {
            connectedDevice!.checkConnection( completion:  completion )
        }
    }

    func sendRequest(data: String, completion: @escaping CompletionWithResponse){
        if (connectedDevice == nil){
            completion("",IoTizeBleError.NoDeviceConnected())
        }else {            
            connectedDevice!.send(data: data, completion: completion)
        }
    }
    
    func isReady() -> Bool {
        return centralManager.state == .poweredOn
    }
}
