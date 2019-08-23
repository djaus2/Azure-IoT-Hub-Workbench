﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AzIoTHubDeviceStreams
{

    public class DeviceAndSvcCurrentSettings 
    {
        public static class Info
        {
            //Signals from svc to device 
            //Prepended to sent message if relevant state required
            public const  char KeepAliveChar = '`';
            public const  char RespondChar = '~';
            public const char KeepDeviceListeningChar = '@';
            public const char UnKeepDeviceListeningChar = '#';
            public const char AutoStartDevice = '$';
            public const char UnAutoStartDevice = '%';
            public const char Delimeter = '!';
            public static List<char> SpecialsKs = new List<char> { KeepAliveChar , RespondChar , KeepDeviceListeningChar , UnKeepDeviceListeningChar, AutoStartDevice, UnAutoStartDevice , Delimeter };

        }

        private bool responseExpected = false;
        private bool keepAlive = false;
        private bool autoStartDevice = false;
        private bool keepDeviceListening = false;
        /// <summary>
        /// If true then device needs to respond when message received.
        /// </summary>
        public bool ResponseExpected { get => responseExpected; set => responseExpected = value; }
        /// <summary>
        /// If true then keep socket alive and wait for another message when post reception processing is finished.
        /// </summary>
        public bool KeepAlive { get => keepAlive; set => keepAlive = value; }

        public bool KeepDeviceListening { get => keepDeviceListening; set => keepDeviceListening = value; } 
        public bool AutoStartDevice { get => autoStartDevice; set => autoStartDevice = value; }

        public bool KeepDeviceListeningChanged = false;
        public bool AutoStartDeviceChanged = false;

        // Called to get flags
        /// <summary>
        /// Called on reception of message to get flags
        /// </summary>
        /// <param name="msgIn">Message with flags embedded</param>
        /// <returns>Message without "flags"</returns>
        public virtual string ProcessMsgIn(string msgIn)
        {
            KeepAlive = false;
            ResponseExpected = false;
            KeepDeviceListeningChanged = false;
            AutoStartDeviceChanged = false;

            if (!string.IsNullOrEmpty(msgIn))
            {
                int devmode;
                if (int.TryParse(msgIn.Substring(1,2),out devmode))
                {
                    AzureConnections.MyConnections.DeviceAction = devmode;

                }
                msgIn = msgIn[0] + msgIn.Substring(4);
                int i = 0;
                while ((msgIn != "")&& (Info.SpecialsKs.Contains(msgIn[i])))
                {
                    if (msgIn[i] == Info.Delimeter)
                        break;
                    else
                    {
                        switch (msgIn[i])
                        {
                            case Info.AutoStartDevice:
                                AutoStartDevice = true;
                                AutoStartDeviceChanged = true;
                                msgIn = msgIn.Substring(1);
                                break;
                            case Info.KeepDeviceListeningChar:
                                KeepDeviceListening = true;
                                KeepDeviceListeningChanged = true;
                                msgIn = msgIn.Substring(1);
                                break;
                            case Info.KeepAliveChar:
                                KeepAlive = true;
                                msgIn = msgIn.Substring(1);
                                break;
                            case Info.RespondChar:
                                ResponseExpected = true;
                                msgIn = msgIn.Substring(1);
                                break;
                            case Info.UnAutoStartDevice:
                                AutoStartDevice = false;
                                AutoStartDeviceChanged = true;
                                msgIn = msgIn.Substring(1);
                                break;
                            case Info.UnKeepDeviceListeningChar:
                                KeepDeviceListening = false;
                                KeepDeviceListeningChanged = true;
                                msgIn = msgIn.Substring(1);
                                break;;
                        }
                    }
                }


            }

            /*    if (!string.IsNullOrEmpty(msgIn))
            {
                // ?Keepalive possibly followed by respond chars
                if (msgIn.ToLower()[0] == Info.KeppAliveChar)
                {
                    KeepAlive = true;
                    msgIn = msgIn.Substring(1);
                    if (!string.IsNullOrEmpty(msgIn))
                    {
                        if (msgIn.ToLower()[0] == Info.RespondChar)
                        {
                            ResponseExpected = true;
                            msgIn = msgIn.Substring(1);
                        }
                        else
                            ResponseExpected = false;
                    }
                }

                // ?Respond possibly followed by keepalive chars
                if (msgIn.ToLower()[0] == Info.RespondChar)
                {
                    ResponseExpected = true;
                    msgIn = msgIn.Substring(1);
                    if (!string.IsNullOrEmpty(msgIn))
                    {
                        if (msgIn.ToLower()[0] == Info.KeppAliveChar)
                        {
                            KeepAlive = true;
                            msgIn = msgIn.Substring(1);
                        }
                        else
                            KeepAlive = false;
                    }
                }
            }*/
            return msgIn;
        }


        //
        /// <summary>
        /// Called prior to message being sent by svc. Embed property flags in message
        /// </summary>
        /// <param name="msgOut"></param>
        /// <param name="keepAlive"></param>
        /// <param name="responseExpected"></param>
        /// <returns>Message to be sent</returns>
        public virtual string ProcessMsgOut(string msgOut, bool keepAlive = false, bool responseExpected = true, int DevKeepListening=2,int DevAutoStart=2)
        {
            msgOut = AzureConnections.MyConnections.DeviceAction.ToString("00") + "-" + msgOut;
            KeepAlive = keepAlive;
            ResponseExpected = responseExpected;
            //Prepend message with indicative chars (for device to interpret as abvoe) if relevant flags are true
            if (keepAlive)
                msgOut = Info.KeepAliveChar + msgOut;
            if (responseExpected)
                msgOut = Info.RespondChar + msgOut;
            switch (DevKeepListening)
            {
                case 0:
                    msgOut = DeviceAndSvcCurrentSettings.Info.KeepDeviceListeningChar + msgOut;
                    KeepDeviceListening = false;
                    KeepDeviceListeningChanged = true;
                    break;
                case 1:
                    msgOut = DeviceAndSvcCurrentSettings.Info.UnKeepDeviceListeningChar + msgOut;
                    KeepDeviceListening = true;
                    KeepDeviceListeningChanged = true;
                    break;
                case 2:
                    KeepDeviceListeningChanged = false;
                    break; ;
            }
            switch (DevAutoStart)
            {
                case 0:
                    msgOut = DeviceAndSvcCurrentSettings.Info.AutoStartDevice + msgOut;
                    AutoStartDevice = false;
                    AutoStartDeviceChanged = true;
                    break;
                case 1:
                    msgOut = DeviceAndSvcCurrentSettings.Info.UnAutoStartDevice + msgOut;
                    AutoStartDevice = true;
                    AutoStartDeviceChanged = true;
                    break;
                case 2:
                    AutoStartDeviceChanged = false;
                    break;
            }
            return msgOut;
        }

    }
}
