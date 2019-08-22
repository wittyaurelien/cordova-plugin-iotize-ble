export interface CordovaBLEScanResult {
    name: string;
    address: string;
    rssi?: number;
}

/**
 * @deprecated
 * use CordovaBLEScanResult instead
 */
export type DiscoveredDeviceType = CordovaBLEScanResult;