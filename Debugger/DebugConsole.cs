using System;
using ViveStreamingFaceTrackingModule;

namespace ViveStreamingFaceTrackingForResonite.Debugger
{
    internal class DebugConsole
    {
        private enum Status : int
        {
            S1001 = 1001, // i.e. "high encode time **.** ms"
            S1002 = 1002, // i.e. "max", "good"

            Resolution = 2001, // i.e. "2048 x 2048 per eye"
            ConnectionType = 2002, // i.e. "WIFI"
            S2003 = 2003, // i.e. "HMD is Ready"
            HMDName = 2004, // i.e. "VIVE XR Series"
            I2005 = 2005, // i.e. "5", "54, "77", "81", "84"
            I2006 = 2006, // i.e. "1"
            B2009 = 2009, // i.e. "True"
            ServerVersion = 2007, // Version
            ServerName = 2008, // HostName, IpAddress

            ConnectionQuality = 2101, // i.e. "Good", "Acceptable", "Poor"
            I2103 = 2103, // i.e. "100"
            I2104 = 2104, // i.e. "0"
            ServerState = 2105, // ViveStreamingServerState
            I2106 = 2106, // i.e. "7", "8", "9", "10", "11", "12", "17"
            I2107 = 2107, // i.e. "0"

            EyeData = 2498, // float0,float1,...
            LipData = 2499, // float0,float1,...
        }

        private enum ViveStreamingServerState : int
        {
            Streaming = 0,
            NoFrameReceivedFromSteamVR = 1,
            Standby = 2,
            VBSDriverNotFound = 3,
            SteamVRNotRunning = 4,
        }

        // Error Code: 404, 501

        private static void OnStatusUpdate(string status, string value)
        {
            if (int.TryParse(status, out int statusNumber))
            {
                if (Enum.IsDefined(typeof(Status), statusNumber))
                {
                    if (int.TryParse(value, out int valueNumber))
                    {
                        if (Enum.IsDefined(typeof(ViveStreamingServerState), valueNumber))
                        {
                            switch ((Status)statusNumber)
                            {
                                case Status.ServerVersion:
                                    Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {value}");
                                    break;
                                case Status.ServerState:
                                    Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {(ViveStreamingServerState)valueNumber}");
                                    break;
                                case Status.EyeData:
                                    Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {value}");
                                    break;
                                case Status.LipData:
                                    Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {value}");
                                    break;
                                default:
                                    Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {value}");
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {valueNumber}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Status: {(Status)statusNumber}\tValue: {value}");
                    }
                }
                else
                {
                    Console.WriteLine($"Status: {statusNumber}\tValue: {value}");
                }
            }
            else
            {
                Console.WriteLine($"Status: {status}\tValue: {value}");
            }
        }

        private static void OnSettingChange(string setting)
        {
            Console.WriteLine($"Setting Change: {setting}");
        }

        private static void OnMessageLog(string message)
        {
            Console.WriteLine($"Message Log: {message}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Initialize ViveStreamingFaceTrackingModule");

            Console.WriteLine("Register Callback Functions");
            VS_PC_SDK.VS_SetCallbackFunction(OnStatusUpdate, OnSettingChange, OnMessageLog);

            if (VS_PC_SDK.VS_Init() != 0)
            {
                throw new InvalidOperationException($"Failed to initialize ViveStreamingFaceTrackingModule with error code {VS_PC_SDK.VS_Init()}");
            }

            Console.WriteLine("Start Face Tracking");
            VS_PC_SDK.VS_StartFaceTracking();
            Console.WriteLine("Face Tracking Started");

            Console.WriteLine("Press any key to stop face tracking");
            while (!Console.KeyAvailable)
            {
                System.Threading.Thread.Sleep(100);
            }

            Console.WriteLine("Stop Face Tracking");
            VS_PC_SDK.VS_StopFaceTracking();
            Console.WriteLine("Face Tracking Stopped");

            Console.WriteLine("Release ViveStreamingFaceTrackingModule");
            VS_PC_SDK.VS_Release();
            Console.WriteLine("ViveStreamingFaceTrackingModule Released");
        }
    }
}
