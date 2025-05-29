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
        private ViveStreamingEyes? eyes;
        private ViveStreamingMouth? mouth;

        private static string? hmdName;
        private static bool connected;
        private static string? eyeData;
        private static string? lipData;

        private bool _tracking;
        public bool IsActive { get; set; }

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
            if (list is null)
            {
                return;
            }

            if (eyes is not null)
            {
                var dict = new DataTreeDictionary();
                dict.Add("Name", "Vive Streaming Face Tracker");
                dict.Add("Type", "Eye Tracking");
                dict.Add("Model", hmdName ?? "Unknown");
                list.Add(dict);
            }

            if (mouth is not null)
            {
                var dict = new DataTreeDictionary();
                dict.Add("Name", "Vive Streaming Face Tracker");
                dict.Add("Type", "Mouth Tracking");
                dict.Add("Model", hmdName ?? "Unknown");
                list.Add(dict);
            }
        }

        public void RegisterInputs(InputInterface inputInterface)
        {
            eyes = new ViveStreamingEyes(inputInterface);
            mouth = new ViveStreamingMouth(inputInterface);
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
                    ResoniteMod.Msg($"Vive Streaming Server v{value} connected.");
                    break;
                case HMD_NAME:
                    hmdName = value;
                    ResoniteMod.Msg($"HMD Name: {value}");
                    break;
                case VS_SERVER_STATE:
                    HandleServerState(value);
                    break;
                default:
                    break;
            }
        }

        private static void HandleServerState(string value)
        {
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
                        break;
                }
            }
        }

        private static void OnSettingChange(string settings)
        {
            ResoniteMod.Debug(settings);
        }

        private static void OnDebugLog(string message)
        {
            ResoniteMod.Debug(message);
        }

        public void UpdateInputs(float deltaTime)
        {

            if (!connected && _tracking)
            {
                VS_PC_SDK.VS_StopFaceTracking();
                _tracking = false;
            }

            if (connected && !_tracking && IsActive)
            {
                if (!VS_PC_SDK.VS_StartFaceTracking())
                {
                    ResoniteMod.Error("Failed to start face tracking");
                    throw new InvalidOperationException("Failed to start face tracking");
                }
                _tracking = true;
            }

            if (connected && _tracking && !IsActive)
            {
                if (!VS_PC_SDK.VS_StopFaceTracking())
                {
                    ResoniteMod.Error("Failed to stop face tracking");
                    throw new InvalidOperationException("Failed to stop face tracking");
                }
                _tracking = false;
            }

            if (!connected)
            {
                _tracking = false;
            }

            eyes?.UpdateInputs(connected, ref eyeData, deltaTime);
            mouth?.UpdateInputs(connected, ref lipData);
        }

        public void Dispose()
        {
            IsActive = false;

            var result = VS_PC_SDK.VS_Release();
            if (result != 0)
            {
                ResoniteMod.Warn($"Failed to release Vive Streaming Face Tracking SDK: {result}");
            }
        }
    }
}
