//
//  CBPeripheralConverter.swift
//  Iotize Monitoring
//
//  Created by dev@iotize.com on 04/09/2018.
//

import CoreBluetooth

struct DiscoveredDeviceType : Codable{
    public var name: String = ""
    public var address: String = ""
    public var rssi: Int?
    
    init(newName: String, newAddress: String, newRssi: Int?) {
        name = newName
        address = newAddress
        if (newRssi != nil) {
            rssi = newRssi
        }
    }
    
    func ToJSON() -> [AnyHashable: Any] {
        return ["name": name, "address": address]
    }
    
    func toJSONString() throws -> String {
        let jsonEncoder = JSONEncoder()
        let jsonData = try jsonEncoder.encode(self)
        
        return String(data: jsonData, encoding: .utf8)!
        
    }
}


import Foundation

class CBPeripheralConverter {
    
    public static func toDiscoveredDeviceType(device: CBPeripheral) -> DiscoveredDeviceType {
        return DiscoveredDeviceType(newName: device.name!, newAddress: device.identifier.uuidString as String, newRssi: nil)
    }
    public static func toJSONObject(device: CBPeripheral) -> [AnyHashable: Any] {
        return toDiscoveredDeviceType(device: device).ToJSON()
    }
    public static func toJSONString(device: CBPeripheral) throws -> String {
        return try toDiscoveredDeviceType(device: device).toJSONString()
    }
}
