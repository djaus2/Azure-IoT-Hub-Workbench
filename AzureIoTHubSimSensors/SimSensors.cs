﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;


namespace Azure_IoTHub_Sensors
{
    public class Sensors
    {

        private static Sensors CurrentSensor = null;
        private static int currentSensorIndex = -1;
        private static List<string> sensors = null;

        public static string ProcessMsgRecvdByDevice(string msgIn)
        {
            string msgOut = "";
            string[] msg = msgIn.Split(new char[] { '-', ' ' });
            if ((msg.Length > 1) || (msg[0].ToLower()=="help"))
            {
                switch (msg[0].ToLower().Substring(0, 3))
                {
                    case "set":
                        if (msg.Length > 2)
                        {
                            msgOut = SetVal(msg[1], msg[2]);
                        }
                        break;
                    case "get":
                        msgOut = GetVal(msg[1]);
                        break;
                    case "hel":
                        msgOut = Help();
                        break;
                    case "sen":
                        int sensorClassIndex;
                        if (int.TryParse(msg[1], out sensorClassIndex))
                        {
                            msgOut = SetSensorClass(sensorClassIndex);
                        }
                        else {
                            msgOut = SetSensorClass(msg[1]);
                        };
                        break;
                    default:
                        msgOut = "Invalid request";
                        break;
                }
            }
            return msgOut;
        }

        public static string Help()
        {
            if (CurrentSensor != null)
            {
                return CurrentSensor.help();
            }
            else return "Help: No sensor selected.";
        }

        public static string GetVal(string sensor)
        {
            if (string.IsNullOrEmpty(sensor))
            {
                return "GetVal: Invalid Sensor";
            }
            if (CurrentSensor != null)
            {
                    return CurrentSensor.getval(sensor);
            }
            else return "GetVal: No sensor selected.";
        }

        public static string SetVal(string sensor, string strnVal)
        {
            int val;
            bool bval;
            if (string.IsNullOrEmpty(sensor))
            {
                return "SetVal: Invalid Sensor";
            }
            if (!string.IsNullOrEmpty(strnVal))
            {
                if (int.TryParse(strnVal, out val))
                {
                    if (CurrentSensor != null)
                    {
                        return CurrentSensor.setval(sensor, val);
                    }
                }
                else if (bool.TryParse(strnVal, out bval))
                {
                    if (CurrentSensor != null)
                    {
                        return CurrentSensor.setval(sensor, bval);
                    }
                }
            }
            return "SetVal: Invalid setting as string";
        }
        public static string SetVal(string sensor , int val)
        {
            if (string.IsNullOrEmpty(sensor))
            {
                return "SetVal: Invalid Sensor";
            }
            if (CurrentSensor != null)
            {
                return CurrentSensor.setval(sensor, val);
            }
            else return "SetVal: No sensor selected.";
        }

        public static string SetVal(string sensor, bool bval)
        {
            if (string.IsNullOrEmpty(sensor))
            {
                return "SetVal: Invalid Sensor";
            }
            if (CurrentSensor != null)
            {
                return CurrentSensor.setval(sensor, bval);
            }
            else return "SetVal: No sensor selected.";
        }

        internal static void Load()
        {
            sensors = new List<string>();
            //It would be nice to use reflection here
            sensors.Add(Sensor1.ClassName.ToLower());
            sensors.Add(Sensor2.ClassName.ToLower());
            sensors.Add(Weather1.ClassName.ToLower());
            sensors.Add(Weather2.ClassName.ToLower());
        }


        public static string SetSensorClass(int index)
        {
            if (sensors == null)
                Load();
            if ((index > -1) && (index < sensors.Count))
            {
                CurrentSensor = null;
                currentSensorIndex = index;
                //It would be nice to use reflection here
                switch (index)
                {
                    case 0:
                        CurrentSensor = new Sensor1();
                        break;
                    case 1:
                        CurrentSensor = new Sensor2();
                        break;
                }
                return "SetSensor: OK";
            }
            
            return "SetSensor: Sensor class not found";
        }

        public static string SetSensorClass(string className)
        {
            if(string.IsNullOrEmpty(className))
                return "SetSensor: Sensor Class not found";

            if (sensors == null)
                Load();
            if (sensors.Contains(className.ToLower()))
            {
                int index = sensors.IndexOf(className.ToLower());
                return SetSensorClass(index);
            }

            return "SetSensor: Sensor Class not found";
        }

        public virtual string help() { return "Not yet implemented"; }
        public virtual string getval(string sensor) { return "Not yet implemented"; }
        public virtual string setval(string sensor, int val) { return "Not yet implemented"; }
        public virtual string setval(string sensor, bool bval) { return "Not yet implemented"; }
    }

    /// <summary>
    /// Simpel example
    /// </summary>
    public class Sensor1 : Sensors
    {
        public static string ClassName = "Sensor1";
        //A couple of values that can be set and returned.
        int state = -1;
        bool toggle = false;

        public Sensor1()
        {
            //Setup code
        }

        ~Sensor1()
        {
            //Shutdown code
        }
        public override string getval(string sensor)
        {
            string msgOut = "Invalid. Try Help";
            switch (sensor.Substring(0,3).ToLower())
            {
                case "tem":
                    msgOut = "45 C";
                    break;
                case "pre":
                    msgOut = "1034.0 hPa";
                    break;
                case "hum":
                    msgOut = "67%";
                    break;
                case "sta":
                    msgOut = string.Format("state = {0}",state);
                    break;
                case "tog":
                    msgOut = string.Format("toggle = {0}", toggle);
                    break;
                default:
                    msgOut = "Invalid request. Try Help";
                    break;
            }
            return msgOut;
        }

        public override string help()
        {
            return  "Only first three characters of each word required.\r\nget:temperature,pressure,humidity,state,toggle,help\r\nset:state <int value>,toggle <0|1> (true|false)";
        }


        public override string setval(string sensor, int val)
        {
            string msgOut = "Invalid";
            switch (sensor.Substring(0,3).ToLower())
            {
                case "sta":
                    state = val;
                    msgOut = "setVal: OK";
                    break;
                case "tog":
                    toggle = val > 0 ? true:false;
                    msgOut = "setVal: OK";
                    break;
                default:
                    msgOut = "Invalid request. Try Help";
                    break;
            }
            return msgOut;
        }

        public override string setval(string sensor, bool bval)
        {
            toggle = bval;
            string msgOut = "setVal: OK";
            return msgOut;
        }

    }

  
    public class Weather1 : Sensors
    {
        public static string ClassName = "Sensor2";

        private double minTemperature = 20;
        private double minHumidity = 60;
        Random rand = new Random();
        public Weather1()
        {
            rand = new Random();
        }

        ~Weather1()
        {

        }
        public override string getval(string val)
        {
            double currentTemperature = minTemperature + rand.NextDouble() * 15;
            double currentHumidity = minHumidity + rand.NextDouble() * 20;
            double currentPressure = 0 + rand.NextDouble() * 100;
            //Create JSON message
            var telemetryDataPoint = new TelemetryDataPoint
            {
                city = "",
                pressure = (int) currentPressure,
                temperature = (int) currentTemperature,
                humidity = (int) currentHumidity
            };
            return  JsonConvert.SerializeObject(telemetryDataPoint);
        }
        public override string setval(string sensor, int val)
        {
            return "Sensor2: setVal() Not yet implemented";
        }
    }

    public class Weather2 : Sensors
    {
        public static string ClassName = "Sensor2";
        public Weather2()
        {
            
        }

        ~Weather2()
        {

        }
        public override string getval(string val)
        {
            return "XYZ"; // Weather_FromCities.GetWeather().GetAwaiter().GetResult();
        }
        public override string setval(string sensor, int val)
        {
            return "Sensor2: setVal() Not yet implemented";
        }
    }

    public class Sensor2 : Sensors
    {
        public static string ClassName = "Sensor2";
        public Sensor2()
        {
            //Setup code
        }

        ~Sensor2()
        {

        }
        public override string getval(string val)
        {
            return "Sensor2: getVal() Not yet implemented";
        }
        public override string setval(string sensor, int val)
        {
            return "Sensor2: setVal() Not yet implemented";
        }
    }
}
