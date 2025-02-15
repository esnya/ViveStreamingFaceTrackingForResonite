using System;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;
using ViveStreamingFaceTrackingModule;

namespace ViveStreamingFaceTrackingForResonite
{
    public sealed class ViveStreamingFaceTrackingDriver : IInputDriver, IDisposable
    {
        private const string HMD_NAME = "2004";
        private const string VS_SERVER_VERSION = "2007";
        private const string VS_SERVER_STATE = "2105";
        private const string EYE_DATA = "5001";
        private const string LIP_DATA = "5002";
        private InputInterface? inputInterface;
        private ViveStreamingEyes? eyes;
        private ViveStreamingMouth? mouth;

        //private string? settings;
        //private string? serverVersion;
        private static string? hmdName;
        private static bool connected;
        private static string? eyeData;
        private static string? lipData;

        private bool _active;
        public bool Active
        {
            set
            {
                if (_active == value)
                {
                    return;
                }

                _active = value;
                if (value)
                {
                    if (!VS_PC_SDK.VS_StartFaceTracking())
                    {
                        throw new InvalidOperationException("Failed to start face tracking");
                    }
                }
                else
                {
                    if (!VS_PC_SDK.VS_StopFaceTracking())
                    {
                        throw new InvalidOperationException("Failed to stop face tracking");
                    }
                }
            }
            get
            {
                return _active;
            }
        }
        public ViveStreamingFaceTrackingDriver()
        {
            ResoniteMod.Debug($"Initializing Vive Streaming Face Tracking");

            VS_PC_SDK.VS_SetCallbackFunction(OnStatusUpdate, OnSettingChange, OnDebugLog);

            var init_result = VS_PC_SDK.VS_Init();
            if (init_result != 0)
            {
                throw new InvalidOperationException($"Failed to initialize Vive Streaming Face Tracking SDK: {init_result}");
            }

            ResoniteMod.Msg($"Vive Streaming Face Tracking Initialized. Server: v{VS_PC_SDK.VS_Version()}");
        }

        public int UpdateOrder => 100;

        public void CollectDeviceInfos(DataTreeList list)
        {
            if (eyes is not null)
            {
                var dict = new DataTreeDictionary();
                dict.Add("Name", "Vive Streaming Face Tracker");
                dict.Add("Type", "Eye Tracking");
                dict.Add("Model", hmdName ?? "Unknown");
            }

            if (mouth is not null)
            {
                var dict = new DataTreeDictionary();
                dict.Add("Name", "Vive Streaming Face Tracker");
                dict.Add("Type", "Mouth Tracking");
                dict.Add("Model", hmdName ?? "Unknown");
            }
        }

        public void RegisterInputs(InputInterface inputInterface)
        {
            this.inputInterface = inputInterface;
            eyes = new ViveStreamingEyes(inputInterface, "Vive Streaming Eyes", true);
            mouth = new ViveStreamingMouth(inputInterface, "Vive Streaming Mouth");
        }

        private static void OnStatusUpdate(string status, string value)
        {
            switch (status)
            {
                case "2498":
                case EYE_DATA:
                    eyeData = value;
                    break;

                case "2499":
                case LIP_DATA:
                    lipData = value;
                    break;

                case VS_SERVER_VERSION:
                    //serverVersion = value;
                    ResoniteMod.Msg($"Vive Streaming Server v{value} connected.");
                    break;
                case HMD_NAME:
                    hmdName = value;
                    ResoniteMod.Msg($"HMD Name: {value}");
                    break;
                case VS_SERVER_STATE:
                    if (int.TryParse(value, out var state))
                    {
                        switch (state)
                        {
                            case 0:
                                if (!connected)
                                {
                                    ResoniteMod.Msg("HMD connected");
                                    connected = true;
                                }
                                break;
                            case 2:
                                if (connected)
                                {
                                    ResoniteMod.Msg("HMD disconnected");
                                    connected = false;
                                }
                                break;
                            default:
                                // ResoniteMod.Warn($"Unknown VS Server state: {state}");
                                break;
                        }
                    }
                    break;
                default:
                    //ResoniteMod.Warn($"Unknown status: {status} {value}");
                    break;
            }
        }

        private static void OnSettingChange(string settings)
        {
            //this.settings = settings;
            ResoniteMod.Debug(settings);
        }

        private static void OnDebugLog(string message)
        {
            ResoniteMod.Debug(message);
        }

        public void UpdateInputs(float deltaTime)
        {
            if (eyes is not null)
            {
                eyes.IsDeviceActive = connected;
                eyes.IsEyeTrackingActive = inputInterface?.VR_Active ?? false;

                if (connected && eyes.IsEyeTrackingActive)
                {

                    if (eyeData is not null)
                    {
                        eyes.UpdateStatus(eyeData);
                    }
                    eyes.UpdateInputs();
                }
            }

            if (mouth is not null)
            {
                mouth.IsDeviceActive = connected;
                if (mouth.IsTracking = inputInterface?.VR_Active ?? false)
                {
                    if (lipData is not null)
                    {
                        mouth.UpdateStatus(lipData);
                    }
                    mouth.UpdateInputs();
                }
            }
        }

        public void Dispose()
        {
            Active = false;

            var result = VS_PC_SDK.VS_Release();
            if (result != 0)
            {
                ResoniteMod.Warn($"Failed to release Vive Streaming Face Tracking SDK: {result}");
            }
        }
    }
}
