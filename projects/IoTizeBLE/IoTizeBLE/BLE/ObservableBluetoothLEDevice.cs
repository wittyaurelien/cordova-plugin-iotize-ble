// <copyright file="ObservableBluetoothLEDevice.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using System.Collections;
using System.Collections.Generic;
using IoTizeBLE.Utility;


namespace IoTizeBLE
{
    /// <summary>
    /// Wrapper around <see cref="BluetoothLEDevice"/> to make it easier to use
    /// </summary>
    internal class ObservableBluetoothLEDevice : INotifyPropertyChanged, IEquatable<ObservableBluetoothLEDevice>
    {

        /// <summary>
        /// Compares RSSI values between ObservableBluetoothLEDevice. Sorts based on closest to furthest where 0 is unknown
        /// and is sorted as furthest away
        /// </summary>
        public class RSSIComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                ObservableBluetoothLEDevice a = x as ObservableBluetoothLEDevice;
                ObservableBluetoothLEDevice b = y as ObservableBluetoothLEDevice;

                if (a == null || b == null)
                {
                    throw new InvalidOperationException("Compared objects are not ObservableBluetoothLEDevice");
                }

                // If they're equal
                if (a.RSSI == b.RSSI)
                {
                    return 0;
                }

                // RSSI == 0 means we don't know it. Always make that the end.
                if (b.RSSI == 0)
                {
                    return -1;
                }

                if (a.RSSI < b.RSSI || a.rssi == 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Source for <see cref="BluetoothLEDevice"/>
        /// </summary>
        private BluetoothLEDevice bluetoothLEDevice;

        /// <summary>
        /// Gets the bluetooth device this class wraps
        /// </summary>
        public BluetoothLEDevice BluetoothLEDevice
        {
            get
            {
                return bluetoothLEDevice;
            }

            private set
            {
                bluetoothLEDevice = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BluetoothLEDevice"));
            }
        }


        /// <summary>
        /// Source for <see cref="DeviceInfo"/>
        /// </summary>
        private DeviceInformation deviceInfo;

        /// <summary>
        /// Gets the device information for the device this class wraps
        /// </summary>
        public DeviceInformation DeviceInfo
        {
            get
            {
                return deviceInfo;
            }

            private set
            {
                deviceInfo = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DeviceInfo"));
            }
        }

        /// <summary>
        /// Source for <see cref="IsConnected"/>
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// Gets or sets a value indicating whether this device is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsConnected"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsPaired"/>
        /// </summary>
        private bool isPaired;

        /// <summary>
        /// Gets or sets a value indicating whether this device is paired
        /// </summary>
        public bool IsPaired
        {
            get
            {
                return isPaired;
            }

            set
            {
                if (isPaired != value)
                {
                    isPaired = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsPaired"));
                }
            }
        }

        private bool isSecureConnection;

        public bool IsSecureConnection
        {
            get
            {
                return isSecureConnection;
            }

            set
            {
                if (isSecureConnection != value)
                {
                    isSecureConnection = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsSecureConnection"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Services"/>
        /// </summary>
        private ObservableCollection<ObservableGattDeviceService> services = new ObservableCollection<ObservableGattDeviceService>();

        /// <summary>
        /// Gets the services this device supports
        /// </summary>
        public ObservableCollection<ObservableGattDeviceService> Services
        {
            get
            {
                return services;
            }

            private set
            {
                services = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Services"));
            }
        }

        /// <summary>
        /// Source for <see cref="ServiceCount"/>
        /// </summary>
        private int serviceCount;

        /// <summary>
        /// Gets or sets the number of services this device has
        /// </summary>
        public int ServiceCount
        {
            get
            {
                return serviceCount;
            }

            set
            {
                if (serviceCount < value)
                {
                    serviceCount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ServiceCount"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets the name of this device
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            private set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="ErrorText"/>
        /// </summary>
        private string errorText;

        /// <summary>
        /// Gets the error text when connecting to this device fails
        /// </summary>
        public string ErrorText
        {
            get
            {
                return errorText;
            }

            private set
            {
                errorText = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ErrorText"));
            }
        }

        private Queue<int> RssiValue = new Queue<int>(10);

        /// <summary>
        /// Source for <see cref="RSSI"/>
        /// </summary>
        private int rssi;

        /// <summary>
        /// Gets the RSSI value of this device
        /// </summary>
        public int RSSI
        {
            get
            {
                return rssi;
            }

            private set
            {
                if (RssiValue.Count >= 10)
                {
                    RssiValue.Dequeue();
                }
                RssiValue.Enqueue(value);

                int newValue = (int)Math.Round(RssiValue.Average(), 0);

                if (rssi != newValue)
                {
                    rssi = newValue;
                    OnPropertyChanged(new PropertyChangedEventArgs("RSSI"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="BatteryLevel"/>
        /// </summary>
        private int batteryLevel = -1;

        /// <summary>
        /// Gets or sets the Battery level of this device. -1 if unknown.
        /// </summary>
        public int BatteryLevel
        {
            get
            {
                return batteryLevel;
            }

            set
            {
                if (batteryLevel != value)
                {
                    batteryLevel = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BatteryLevel"));
                }
            }
        }

        private string bluetoothAddressAsString;
        /// <summary>
        /// Gets the bluetooth address of this device as a string
        /// </summary>
        public string BluetoothAddressAsString
        {
            get
            {
                return bluetoothAddressAsString;
            }

            private set
            {
                if (bluetoothAddressAsString != value)
                {
                    bluetoothAddressAsString = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BluetoothAddressAsString"));
                }
            }
        }

        private ulong bluetoothAddressAsUlong;
        /// <summary>
        /// Gets the bluetooth address of this device
        /// </summary>
        public ulong BluetoothAddressAsUlong
        {
            get
            {
                return bluetoothAddressAsUlong;
            }

            private set
            {
                if (bluetoothAddressAsUlong != value)
                {
                    bluetoothAddressAsUlong = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BluetoothAddressAsUlong"));
                }
            }
        }

        private const string SPPOverLEGUID = "6C7B16C2-2A5B-8C9F-CF42-D31425470E7B";
        private const string SPPoverLE_BUFFER_CHAR_UUID = "cc5c5491-b3be-9287-cb42-f7a6a29a50d5";
        private GattCharacteristic SPPOverLECharacteristic = null;

        //we enqueue up to 100 commands
        private Queue<Request> RequestedCommands = new Queue<Request>();
        private Request CurrentRequest;

        /// <summary>
        /// Initializes a new instance of the<see cref="ObservableBluetoothLEDevice" /> class.
        /// </summary>
        /// <param name="deviceInfo">The device info that describes this bluetooth device"/></param>
        public ObservableBluetoothLEDevice(DeviceInformation deviceInfo)
        {
            DeviceInfo = deviceInfo;
            Name = DeviceInfo.Name;

            string ret = String.Empty;

            if (DeviceInfo.Properties.ContainsKey("System.Devices.Aep.DeviceAddress"))
            {
                BluetoothAddressAsString = ret = DeviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString();
                BluetoothAddressAsUlong = Convert.ToUInt64(BluetoothAddressAsString.Replace(":", String.Empty), 16);
            }

            IsPaired = DeviceInfo.Pairing.IsPaired;


            this.PropertyChanged += ObservableBluetoothLEDevice_PropertyChanged;
        }

        ~ObservableBluetoothLEDevice()
        {
            this.PropertyChanged -= ObservableBluetoothLEDevice_PropertyChanged;
            if (BluetoothLEDevice != null)
            {

                BluetoothLEDevice.ConnectionStatusChanged -= BluetoothLEDevice_ConnectionStatusChanged;
                BluetoothLEDevice.NameChanged -= BluetoothLEDevice_NameChanged;
                foreach (var service in Services)
                {
                    if (service.UUID.ToUpper() == SPPOverLEGUID.ToUpper())
                    {
                        foreach (var item in service.Characteristics)
                        {
                            if (item.UUID.ToUpper() == SPPoverLE_BUFFER_CHAR_UUID.ToUpper())
                            {
                                item.ResponseEvent -= Item_ResponseEvent;
                            }
                        }
                    }
                }

            }

        }

        private void ObservableBluetoothLEDevice_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeviceInfo")
            {
                if (DeviceInfo.Properties.ContainsKey("System.Devices.Aep.SignalStrength") && DeviceInfo.Properties["System.Devices.Aep.SignalStrength"] != null)
                {
                    RSSI = (int)DeviceInfo.Properties["System.Devices.Aep.SignalStrength"];
                }
            }
        }

        /// <summary>
        /// result of finding all the services
        /// </summary>
        private GattDeviceServicesResult result;

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Connect to this bluetooth device
        /// </summary>
        /// <returns>Connection task</returns>
        public async Task<bool> Connect()
        {
            bool ret = false;
            string debugMsg = String.Format("Connect: ");
      
            Log.WriteLine(debugMsg + "Entering");

            //Start checking requests
            Task.Run(() => ManageRequests()).AsAsyncAction();

            RequestedCommands.Clear();
            CurrentRequest = null;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(async () =>
            {
                Log.WriteLine(debugMsg + "In UI thread");
                try
                {

                    if (BluetoothLEDevice == null)
                    {
                        Log.WriteLine(debugMsg + "Calling BluetoothLEDevice.FromIdAsync");
                        BluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(DeviceInfo.Id);
                    }
                    else
                    {
                        Log.WriteLine(debugMsg + "Previously connected, not calling BluetoothLEDevice.FromIdAsync");
                    }

                    if (BluetoothLEDevice == null)
                    {
                        ret = false;
                        Log.WriteLine(debugMsg + "BluetoothLEDevice is null");

                        MessageDialog dialog = new MessageDialog("No permission to access device", "Connection error");
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        Log.WriteLine(debugMsg + "BluetoothLEDevice is " + BluetoothLEDevice.Name);

                        // Setup our event handlers and view model properties
                        BluetoothLEDevice.ConnectionStatusChanged += BluetoothLEDevice_ConnectionStatusChanged;
                        BluetoothLEDevice.NameChanged += BluetoothLEDevice_NameChanged;

                        IsPaired = DeviceInfo.Pairing.IsPaired;
                        IsConnected = BluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;

                        Name = BluetoothLEDevice.Name;

                        // Get all the services for this device
                        CancellationTokenSource GetGattServicesAsyncTokenSource = new CancellationTokenSource(5000);
                        var GetGattServicesAsyncTask = Task.Run(() => BluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached), GetGattServicesAsyncTokenSource.Token);

                        result = await GetGattServicesAsyncTask.Result;

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            // In case we connected before, clear the service list and recreate it
                            Services.Clear();
                            SPPOverLECharacteristic = null;

                            Log.WriteLine(debugMsg + "GetGattServiceAsync SUCCESS");
                            foreach (var serv in result.Services)
                            {
                                ObservableGattDeviceService obsService = new ObservableGattDeviceService(serv);
                                await obsService.GetAllCharacteristics();
                                Services.Add(obsService);
                            }

                            ServiceCount = Services.Count();


                            ret = true;
                        }
                        else if (result.Status == GattCommunicationStatus.ProtocolError)
                        {
                            ErrorText = debugMsg + "GetGattServiceAsync Error: Protocol Error - " + result.ProtocolError.Value;
                            Log.WriteLine(ErrorText);
                            string msg = "Connection protocol error: " + result.ProtocolError.Value.ToString();
                            var messageDialog = new MessageDialog(msg, "Connection failures");
                            await messageDialog.ShowAsync();

                        }
                        else if (result.Status == GattCommunicationStatus.Unreachable)
                        {
                            ErrorText = debugMsg + "GetGattServiceAsync Error: Unreachable";
                            Log.WriteLine(ErrorText);
                            string msg = "Device unreachable";
                            var messageDialog = new MessageDialog(msg, "Connection failures");
                            await messageDialog.ShowAsync();
                        }

                      
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(debugMsg + "Exception - " + ex.Message);
                    string msg = String.Format("Message:\n{0}\n\nInnerException:\n{1}\n\nStack:\n{2}", ex.Message, ex.InnerException, ex.StackTrace);

                    var messageDialog = new MessageDialog(msg, "Exception");
                    await messageDialog.ShowAsync();

                    // Debugger break here so we can catch unknown exceptions
                    Debugger.Break();
                }
            });

            if (ret)
            {
                Log.WriteLine(debugMsg + "Exiting (0)");
            }
            else
            {
                Log.WriteLine(debugMsg + "Exiting (-1)");
            }

            return ret;
        }


        /// <summary>
        /// DisConnect to this bluetooth device
        /// </summary>
        /// <returns>DiscConnection task</returns>
        public async Task<bool> Disconnect()
        {
            IsConnected = false;
            
            RequestedCommands.Clear();
            CurrentRequest = null;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(async () =>
            {
                try
                {

                    if (BluetoothLEDevice != null)
                    {
                        BluetoothLEDevice.ConnectionStatusChanged -= BluetoothLEDevice_ConnectionStatusChanged;
                        BluetoothLEDevice.NameChanged -= BluetoothLEDevice_NameChanged;
                        BluetoothLEDevice = null;
                        IsConnected = false;
                        IsPaired = false;
                        Name = "";
                        Services.Clear();
                        SPPOverLECharacteristic = null;
                        ServiceCount = 0;
                    }

                }
                catch (Exception ex)
                {
                    Log.WriteLine("Exception - " + ex.Message);
                    string msg = String.Format("Message:\n{0}\n\nInnerException:\n{1}\n\nStack:\n{2}", ex.Message, ex.InnerException, ex.StackTrace);
                }
            });


            return true;
        }


        public async Task<bool> DoInAppPairing()
        {
            Log.WriteLine("Trying in app pairing");

            // BT_Code: Pair the currently selected device.
            DevicePairingResult result = await DeviceInfo.Pairing.PairAsync();

            Log.WriteLine($"Pairing result: {result.Status.ToString()}");

            if (result.Status == DevicePairingResultStatus.Paired ||
                result.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                return true;
            }
            else
            {
                MessageDialog d = new MessageDialog("Pairing error", result.Status.ToString());
                await d.ShowAsync();
                return false;
            }
        }

        /// <summary>
        /// Executes when the name of this devices changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BluetoothLEDevice_NameChanged(BluetoothLEDevice sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                Name = BluetoothLEDevice.Name;
            });
        }

        /// <summary>
        /// Executes when the connection state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BluetoothLEDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                IsPaired = DeviceInfo.Pairing.IsPaired;
                IsConnected = BluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
            });
        }


        /// <summary>
        /// Executes when a property is changed
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Overrides the ToString function to return the name of the device
        /// </summary>
        /// <returns>Name of this characteristic</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Compares this device to other bluetooth devices by checking the id
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true for equal</returns>
        bool IEquatable<ObservableBluetoothLEDevice>.Equals(ObservableBluetoothLEDevice other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.DeviceInfo.Id == other.DeviceInfo.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Updates this device's deviceInformation
        /// </summary>
        /// <param name="deviceUpdate"></param>
        public void Update(DeviceInformationUpdate deviceUpdate)
        {
            DeviceInfo.Update(deviceUpdate);

            OnPropertyChanged(new PropertyChangedEventArgs("DeviceInfo"));
        }

        public void setUpSPP()
        {
            if (Services.Count == 0)
            {
                throw new InvalidOperationException("No service available: connection error?");
            }
                
            String servicesStr = "";
            String characteresticsStr = "";
            foreach (var service in Services)
            {
                if (service.UUID.ToUpper() == SPPOverLEGUID.ToUpper())
                {
                    servicesStr += service.UUID + "-";

                    if (service.Characteristics.Count == 0)
                    {
                        throw new InvalidOperationException("Empty Characteristics");
                    }

                    foreach (var item in service.Characteristics)
                    {
                        characteresticsStr += "charc:" + item.UUID + "-";
                        if (item.UUID.ToUpper() == SPPoverLE_BUFFER_CHAR_UUID.ToUpper())
                        {
                            item.ResponseEvent += Item_ResponseEvent;
                            SPPOverLECharacteristic = item.Characteristic;
                        }
                    }
                }
            }
            if (SPPOverLECharacteristic == null)
            {
                throw new InvalidOperationException( servicesStr + "     " + characteresticsStr );
            }

        }
        int m_TX_buffer_len;
        byte[] m_TX_buffer;
        int LEN_PACKET = 19;
        int m_currentTxPacket = 0;
        int m_TX_Buffer_rest = 0;

        byte[] receivedResponse = null;

        private static int bufferLength = 300;
        private byte[] rxBuffer = new byte[bufferLength];
        private int rxBufferLength = 0;
       

        private async Task ManageRequests()
        {
            
            while (true)
            {
                while (IsConnected && (RequestedCommands.Count != 0))
                {
                    Request current = RequestedCommands.Dequeue();

                    Log.WriteLine("--->Manage request :" + current.index + " in " + Environment.CurrentManagedThreadId.ToString());

                    await ActualSendRequest(current);
                }

                await Task.Delay(50);

            }
        }

        public Request RegisterRequest(byte[] request)
        {
            Request newrequest = new Request();
            newrequest.Send(request);
            RequestedCommands.Enqueue(newrequest);
            return newrequest;
        }

        

        public async Task<bool> ActualSendRequest(Request request)
        { 
            receivedResponse = null;
            rxBufferLength = 0;
            for (int i = 0; i < rxBuffer.Length; i++)
            {
                rxBuffer[i] = 0;
            }

            CurrentRequest = request;
            await send_All_TX_Packets(request.GetCommand());

            return await request.IsAnswered();
        }


        // Send all the BLE packets
        private async Task send_All_TX_Packets(byte[] data)
        {
            byte chksum = 0;

            if (data.Length > 0)
            {

                m_TX_buffer_len = data.Length;
                m_TX_buffer = new byte[m_TX_buffer_len + 1];

                // Compute check sum
                uint tmp = 0;
                for (int i = 0; i < m_TX_buffer_len; i++)
                {
                    m_TX_buffer[i] = data[i];
                    tmp = tmp + (uint)m_TX_buffer[i];
                }

                chksum = (byte)(tmp & 0xFF);

                m_TX_buffer[m_TX_buffer_len] = chksum;
                m_TX_buffer_len = m_TX_buffer_len + 1;

                // Compute number of packet to send
                m_currentTxPacket = m_TX_buffer_len / LEN_PACKET;
                m_TX_Buffer_rest = m_TX_buffer_len % LEN_PACKET;

                if (m_TX_Buffer_rest == 0)
                {
                    m_currentTxPacket = m_currentTxPacket - 1;
                    m_TX_Buffer_rest = LEN_PACKET;
                }

                // launch the first packet
                await send_one_TX_Packet(m_currentTxPacket);

            } // if (TX_Buffer_length > 0)
        }

        // Send one packet of BLE frame
        private async Task send_one_TX_Packet(int num)
        {
            int len = 0;
            int offset = 0;

            if (SPPOverLECharacteristic == null)
            {
                setUpSPP();
            }
            if (SPPOverLECharacteristic == null)
            {
                throw new InvalidOperationException("Invalid BLE Characteristics");                
            }

            await SPPOverLECharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);

            // Check if last packet
            if (num > 0)
            {
                offset = m_TX_Buffer_rest + ((num - 1) * LEN_PACKET);
                len = LEN_PACKET;
            }
            else
            {
                offset = 0;
                len = m_TX_Buffer_rest;
            }

            // Extract frame from buffer
            byte[] packet = new byte[len + 1];
            packet[0] = (byte)offset;

            for (int i = 0; i < len; i++)
            {
                packet[i + 1] = m_TX_buffer[offset + i];
            }

            Log.WriteLine(" ---------------------- send: " + CurrentRequest.index + " data=[" + BitConverter.ToString(packet, 0, packet.Length) + "]");

            Windows.Storage.Streams.IBuffer packetBuffer = GattConvert.ToIBufferFromArray(packet);

            if (SPPOverLECharacteristic != null)
            {
                await SPPOverLECharacteristic.WriteValueAsync(packetBuffer, GattWriteOption.WriteWithoutResponse);
            }

        }

       


        private void Item_ResponseEvent(object sender, ReceivedEventArgs e)
        {
            rxBufferLength += (e.Response.Length - 1);
            

            if (rxBufferLength > ObservableBluetoothLEDevice.bufferLength)
            {
                rxBufferLength = ObservableBluetoothLEDevice.bufferLength;
            }

            int offset = (e.Response[0] & 0xFF) - 1;

            if ((offset + e.Response.Length) < ObservableBluetoothLEDevice.bufferLength)
            {
                Array.Copy(e.Response, 1, rxBuffer, (offset+1), (e.Response.Length - 1));
            }

            if (offset == -1)
            {
                
                {
                    byte checksum = 0;

                    // Remove checksum to length
                    rxBufferLength = rxBufferLength - 1;

                    // Compute checksum
                    uint tmp = 0;
                    for (int i=0; i < rxBufferLength; i++)
                    {
                        tmp = tmp + (uint)(rxBuffer[i]);
                    }

                    // Test checksum
                    checksum = (byte)(tmp & 0xFF);
                    if (checksum != rxBuffer[rxBufferLength])
                    {
                        // wrong checksum APDU error code
                        rxBuffer[rxBufferLength - 2] = 0x66;
                        rxBuffer[rxBufferLength - 1] = 0x02;
                    }
                }

                receivedResponse = new byte[rxBufferLength];
                for (int i = 0; i < rxBufferLength; i++)
                {
                    receivedResponse[i] = rxBuffer[i];
                }

                CurrentRequest.SetResponse(receivedResponse);
                Log.WriteLine(" ---------------------- get: "+ CurrentRequest.index + " data=[" + BitConverter.ToString(e.Response, 0, e.Response.Length) + "]");

            }

        }
     

    }
}
