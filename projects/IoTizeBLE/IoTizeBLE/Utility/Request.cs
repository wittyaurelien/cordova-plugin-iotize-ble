using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace IoTizeBLE.Utility
{
    enum RequestState
    {
        _undefined = 0,
        _created = 1,
        _sent = 2,
        _responded = 3
    };

    internal class Request
    {
        //Sent data
        byte[] Data;

        //Response from IoTize
        byte[] Response;

        //State of request
        RequestState State=RequestState._undefined;

        //Unique Id
        Guid UUID;


        public Request()
        {
            UUID = Guid.NewGuid();
            Data = null;
            Response = null;
            State = RequestState._undefined;
        }

        public void Send(byte[] data)
        {
            Data = new byte[data.Length];
            Array.Copy(data, Data, data.Length);
            State = RequestState._sent;
        }

        public void SetResponse(byte[] response)
        {
            Response = new byte[response.Length];
            Array.Copy(response, Response, response.Length);
            State = RequestState._responded;
        }

        public byte[] GetResponse()
        {
            return Response;
        }

        public byte[] GetCommand()
        {
            return Data;
        }

        public int index = 0;

        public async Task<bool> IsAnswered()
        {
            int counter = 0;
            while ((counter < 1000) && (State != RequestState._responded))
            {
                await Task.Delay(60);

                if (State == RequestState._responded)
                {
                    return true;
                }
                counter++;

            }
            return (State == RequestState._responded);
        }

        internal Guid GetId()
        {
            return UUID;
        }
    }
}
