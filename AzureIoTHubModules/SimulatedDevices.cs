﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Azure_IoTHub_Telemetry;


namespace Azure_IoTHub_Telemetry
{

    public class SimulatedDevice
    {
        public delegate void ActionReceivedText(string recvTxt);

        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private static string s_connectionString = "{Your device connection string here}";

        public static bool ContinueLoop {get; set;}=false;

        public static string MessageString { get; set; } = "";

        public static Microsoft.Azure.Devices.Client.Message Message = null;
        public static string IOTMess { get; set; } = "";

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync()
        {
            ContinueLoop = true;
            while (ContinueLoop)
            {
                Azure_IoTHub_Sensors.TelemetryDataPoint telemetryDataPoint;
                Azure_IoTHub_Sensors.Weather_FromCities wer = (Azure_IoTHub_Sensors.Weather_FromCities)(Azure_IoTHub_Sensors.Weather.CurrentWeather);
                if (wer != null)
                    telemetryDataPoint =  await wer.GetWeatherAsync();
                else
                    telemetryDataPoint =  Azure_IoTHub_Sensors.Weather.CurrentWeather.GetWeather();

                MessageString = JsonConvert.SerializeObject(telemetryDataPoint);
                
                Message = new Message(Encoding.ASCII.GetBytes(MessageString));
                //Message.UserId = Azure_IoTHub_Connections.MyConnections.IoTHubName;
                Message.Properties.Add("temperatureAlert", (telemetryDataPoint.temperature > 30) ? "true" : "false");
                Message.Properties.Add("humidityAlert", (telemetryDataPoint.humidity > 80) ? "true" : "false");
                Message.Properties.Add("pressureAlert", (telemetryDataPoint.pressure > 1010) ? "true" : "false");
                Azure_IoTHub_Telemetry.SyntheticIoTMessage iotmessage = new Azure_IoTHub_Telemetry.SyntheticIoTMessage(Message);
                MessageString = iotmessage.Serialise();

                

                System.Diagnostics.Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, MessageString);
                OnDeviceStatusUpdateD?.Invoke(string.Format("{0} > Sending message: {1}", DateTime.Now, MessageString));

                // Send the telemetry message
                if (!IsDeviceStreaming)
                {
                    await s_deviceClient.SendEventAsync(Message);
                    Delay = 1000* Azure_IoTHub_Connections.MyConnections.TelemetryDelayBtwReadings;
                    await Task.Delay(Delay);
                    if (!ContinueLoop)
                        OnDeviceStatusUpdateD?.Invoke("Cancelled Telemetry - Device end");
                }
                else
                {
                    ContinueLoop= false;
                }
                
            }
        }

        private static int Delay = Azure_IoTHub_Connections.MyConnections.TelemetryDelayBtwReadings;

        private static bool IsDeviceStreaming = false;

        public static bool IsConfigured { get; set; } = false;

        private static ActionReceivedText OnDeviceStatusUpdateD;


        public static void Configure(string device_cs, bool isDeviceStreaming, TransportType transportType, bool loop, ActionReceivedText onDeviceStatusUpdateD = null, int delay=1000)
        {
            Delay = delay;
            OnDeviceStatusUpdateD = onDeviceStatusUpdateD;
            IsDeviceStreaming = isDeviceStreaming;

            s_connectionString = device_cs;

            if (!IsDeviceStreaming)
            {
                s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, transportType);
            }
            ContinueLoop = true;
        }

        
        public static async Task<string>  Run()
        {
            if (Azure_IoTHub_Sensors.Weather.CurrentWeather == null)
                Azure_IoTHub_Sensors.Weather.CurrentWeather = new Azure_IoTHub_Sensors.Weather_Random();
             MessageString = "";
            System.Diagnostics.Debug.WriteLine("IoT Hub Quickstarts #1 - Simulated device started.");
            // Connect to the IoT hub using the MQTT protocol
           
            await SendDeviceToCloudMessagesAsync();
            if (!IsDeviceStreaming)
            {
                System.Diagnostics.Debug.WriteLine("Simulated Device Done");
                await s_deviceClient.CloseAsync();
                MessageString = "";
            }

            return MessageString;
        }
    }
}

