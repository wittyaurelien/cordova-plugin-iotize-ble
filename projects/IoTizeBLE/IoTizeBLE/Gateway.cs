using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using IoTizeBLE.Utility;
using System.Collections.ObjectModel;

namespace IoTizeBLE
{
    enum ConnectionState
    {
        _notConnected = 0,
        _inConnection = 1,
        _connected = 2,
        _inDisconnection = 3,
        _error = 4
    }

    //Callback called when a new device is detected
    public delegate void IoTizeDiscoveryCallback(string jsonResult);

    public delegate void IoTizeDisconnectionCallback();

    //
    public sealed class BLEManager
    {

        internal String lastError;

        internal ConnectionState connectionState = ConnectionState._notConnected;
        
        private static IoTizeDiscoveryCallback _discoveryCallback;

        private static IoTizeDisconnectionCallback _disconnectionCallback;

        private GattSampleContext Context;


        private ObservableBluetoothLEDevice SelectedDevice = null;

        public bool IsDeviceConnected { get; private set; }

        public BLEManager()
        {
            lastError = "";
            Context = GattSampleContext.Context;

            //debugging log file
            Log.CreateLog();
        }

        ~BLEManager()
        {
            //comment this to keep the debug log file
            Log.RemoveLog();
        }

        public string getLastError()
        {
            return lastError;
        }

        public bool isConnected()
        {
            return (connectionState == ConnectionState._connected);
        }


        public bool startScan(IoTizeDiscoveryCallback callback)
        {
            try
            {
                Log.WriteLine("-->Start Enumeration");
                Context.StartEnumeration();
                Context.PropertyChanged += Context_PropertyChanged;
                _discoveryCallback = callback;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }

            return true;
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeviceList")
            {
                if ( (_discoveryCallback != null) && (Context.BluetoothLEDevices.Last() != null) )
                {

                    Log.WriteLine("---> found device" + Context.BluetoothLEDevices.Last().Name);
                    _discoveryCallback(Context.BluetoothLEDevices.Last().Name);
                }
            }
        }

        public bool stopScan()
        {
            try
            {
                Context.StopEnumeration();
                Context.PropertyChanged -= Context_PropertyChanged;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }

            return true;
        }

        public IAsyncOperation<bool> connect(string device_id, IoTizeDisconnectionCallback callback)
        {
            Context.StopEnumeration();
            _disconnectionCallback = callback;
           

            if (connectionState == ConnectionState._inConnection)
            {
                Log.WriteLine("---> wait for Connecting");
                return Task.Run(() => doWaitConnection())
                   .AsAsyncOperation();
                
            }
            else
            {
                connectionState = ConnectionState._inConnection;
                Log.WriteLine("---> Start Connecting");
                return Task.Run(() => doStartConnection(device_id))
                   .AsAsyncOperation();
            }
        }

        public IAsyncOperation<bool> disConnect(string device_id)
        {
            Context.StopEnumeration();

            if (connectionState == ConnectionState._inDisconnection)
            {
                Log.WriteLine("---> wait for Disconnecting");
                return Task.Run(() => doWaitDisconnection())
                   .AsAsyncOperation();

            }
            else
            {
                connectionState = ConnectionState._inDisconnection;
                Log.WriteLine("---> Start Disconnecting");
                return Task.Run(() => doStartDisConnection(device_id))
                   .AsAsyncOperation();
            }

        }

        private async Task<bool> doWaitConnection()
        {
            while (connectionState == ConnectionState._inConnection)
            {
                await Task.Delay(50);

                if (connectionState == ConnectionState._error)
                {
                    return false;
                }

                if (connectionState == ConnectionState._connected)
                {
                    return true;
                }

            }

            return (connectionState == ConnectionState._connected);
        }

        private async Task<bool> doWaitDisconnection()
        {
            while (connectionState == ConnectionState._inDisconnection)
            {
                await Task.Delay(50);

                if (connectionState == ConnectionState._error)
                {
                    return false;
                }

                if (connectionState == ConnectionState._notConnected)
                {
                    return true;
                }

            }

            return (connectionState == ConnectionState._notConnected);
        }

        private async Task<bool> doStartConnection(string device_id)
        {
            bool successfull = false;

            lastError = "Connecting ...";

            //step 1: connect to device
            try
            {
                SelectedDevice = Context.BluetoothLEDevices.FirstOrDefault(item => item.Name == device_id);
                if (SelectedDevice != null)
                {
                    Log.WriteLine("---> In Connection");
                    IsDeviceConnected = await SelectedDevice.Connect();
                    successfull = IsDeviceConnected;
                    Log.WriteLine("---> End of Connection " + successfull);
                    SelectedDevice.PropertyChanged += SelectedDevice_PropertyChanged;
                }
                else
                {
                    IsDeviceConnected = false;
                    successfull = false;
                }
                
            }
            catch (Exception e)
            {
                lastError = "Exception in connection " + e.Message;
                Log.WriteLine("---> "+ lastError);
                connectionState = ConnectionState._error;
                return false;
            }

            if (successfull == false)
            {
                lastError = "Failed in connection " + device_id;
                Log.WriteLine("---> " + lastError);
                connectionState = ConnectionState._error;
                return false;
            }

            lastError = "Successfull connection";

            connectionState = ConnectionState._connected;
            Log.WriteLine("---> " + lastError);
            return true;
        }

        private async Task<bool> doStartDisConnection(string device_id)
        {
            bool successfull = false;

            lastError = "Disconnection ...";

            //step 1: connect to device
            try
            {
                if (SelectedDevice != null)
                {
                    SelectedDevice.PropertyChanged -= SelectedDevice_PropertyChanged;
                    _disconnectionCallback = null;
                    successfull = await SelectedDevice.Disconnect();
                    IsDeviceConnected = !successfull;
                    
                }

            }
            catch (Exception e)
            {
                lastError = "Exception in disconnection " + e.Message;
                Log.WriteLine("---> " + lastError);
                connectionState = ConnectionState._error;
                return false;
            }

            if (successfull == false)
            {
                lastError = "Failed in disconnection " + device_id;
                Log.WriteLine("---> " + lastError);
                connectionState = ConnectionState._error;
                return false;
            }

            lastError = "Successfull disconnection";
            SelectedDevice = null;

            connectionState = ConnectionState._connected;
            Log.WriteLine("---> " + lastError);
            return true;
        }

        private void SelectedDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsConnected")
            {
                if ((_disconnectionCallback != null) && (SelectedDevice != null))
                {

                    Log.WriteLine("---> device disconnected" + SelectedDevice.Name);
                    _disconnectionCallback();
                }
            }
           
        }

        private int index = 0;

        public IAsyncOperation<string> sendRequest(string device, string data)
        {
            IBuffer mybuffer = GattConvert.ToIBufferFromHexString(data);
            byte[] mybyteArray = GattConvert.ToByteArray(mybuffer);

            Request req = (SelectedDevice != null) ? SelectedDevice.RegisterRequest(mybyteArray) : null;
            if (req != null)
            {
                req.index = index;
                index++;
            }

            return Task.Run(() => waitSendRequest(req))
               .AsAsyncOperation();
        }

        
        private async Task<string> waitSendRequest(Request req)
        {
            if (connectionState != ConnectionState._connected)
                return (lastError = "Error: No connection available");

            if (req == null)
                return (lastError = "Error: Unable to send request");

            bool isanswered = await req.IsAnswered();

            if (!isanswered)
                return (lastError = "Error: Did not received an answer");

            IBuffer myresponse = GattConvert.ToIBufferFromArray(req.GetResponse());
            string strresponse = GattConvert.ToHexString(myresponse);

            Log.WriteLine("--- Response : " + req.index + " in "+ Environment.CurrentManagedThreadId + "with " + strresponse);
            return strresponse;            
        }
    }

}
