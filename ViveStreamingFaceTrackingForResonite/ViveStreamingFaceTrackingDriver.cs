using System;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;
using ViveStreamingFaceTrackingModule;

namespace ViveStreamingFaceTrackingForResonite;

/// <summary>
/// Event arguments for status change events.
/// </summary>
public sealed class StatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the connection status.
    /// </summary>
    public string ConnectionStatus { get; }

    /// <summary>
    /// Gets the HMD model.
    /// </summary>
    public string HmdModel { get; }

    /// <summary>
    /// Gets the eye tracking status.
    /// </summary>
    public string EyeTrackingStatus { get; }

    /// <summary>
    /// Gets the mouth tracking status.
    /// </summary>
    public string MouthTrackingStatus { get; }

    /// <summary>
    /// Gets the number of active eye data points.
    /// </summary>
    public int EyeDataCount { get; }

    /// <summary>
    /// Gets the number of active mouth data points.
    /// </summary>
    public int MouthDataCount { get; }

    /// <summary>
    /// Gets the tracking frame rate.
    /// </summary>
    public int FrameRate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusChangedEventArgs"/> class.
    /// </summary>
    /// <param name="connectionStatus">The connection status.</param>
    /// <param name="hmdModel">The HMD model.</param>
    /// <param name="eyeTrackingStatus">The eye tracking status.</param>
    /// <param name="mouthTrackingStatus">The mouth tracking status.</param>
    /// <param name="eyeDataCount">The number of active eye data points.</param>
    /// <param name="mouthDataCount">The number of active mouth data points.</param>
    /// <param name="frameRate">The tracking frame rate.</param>
    public StatusChangedEventArgs(
        string connectionStatus,
        string hmdModel,
        string eyeTrackingStatus,
        string mouthTrackingStatus,
        int eyeDataCount,
        int mouthDataCount,
        int frameRate
    )
    {
        ConnectionStatus = connectionStatus;
        HmdModel = hmdModel;
        EyeTrackingStatus = eyeTrackingStatus;
        MouthTrackingStatus = mouthTrackingStatus;
        EyeDataCount = eyeDataCount;
        MouthDataCount = mouthDataCount;
        FrameRate = frameRate;
    }
}

/// <summary>
/// Vive Streaming Face Tracking input driver for Resonite.
/// </summary>
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
    private static bool Connected { get; set; }
    private static string? eyeData;
    private static string? lipData;

    // Tracking statistics
    private static int eyeDataCount;
    private static int mouthDataCount;
    private static DateTime lastEyeDataTime = DateTime.MinValue;
    private static DateTime lastMouthDataTime = DateTime.MinValue;
    private static int eyeFrameCount;
    private static int mouthFrameCount;
    private static DateTime lastFrameRateUpdate = DateTime.UtcNow;
    private static DateTime lastStatusUpdate = DateTime.UtcNow;

    private bool _tracking;

    /// <summary>
    /// Event fired when connection status or HMD model changes.
    /// </summary>
    public static event EventHandler<StatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Gets or sets a value indicating whether this driver is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets a value indicating whether the HMD is connected.
    /// </summary>
    public static bool IsConnected => Connected;

    /// <summary>
    /// Gets the HMD model name.
    /// </summary>
    public static string HMDModel => hmdName ?? "Unknown";

    /// <summary>
    /// Gets the connection status as a human-readable string.
    /// </summary>
    public static string ConnectionStatus => Connected ? "Connected" : "Disconnected";

    /// <summary>
    /// Initializes a new instance of the <see cref="ViveStreamingFaceTrackingDriver"/> class.
    /// </summary>
    public ViveStreamingFaceTrackingDriver()
    {
        ResoniteMod.Debug($"Initializing Vive Streaming Face Tracking");

        VS_PC_SDK.VS_SetCallbackFunction(OnStatusUpdate, OnSettingChange, OnDebugLog);

        var init_result = VS_PC_SDK.VS_Init();
        if (init_result != 0)
        {
            throw new InvalidOperationException(
                $"Failed to initialize Vive Streaming Face Tracking SDK: {init_result}"
            );
        }

        ResoniteMod.Msg(
            $"Vive Streaming Face Tracking Initialized. Server: v{VS_PC_SDK.VS_Version()}"
        );
    }

    /// <inheritdoc />
    public int UpdateOrder => 100;

    /// <inheritdoc />
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

    /// <inheritdoc />
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
                UpdateEyeDataStats(value);
                break;

            case "2499":
            case LIP_DATA:
                lipData = value;
                UpdateMouthDataStats(value);
                break;

            case VS_SERVER_VERSION:
                ResoniteMod.Msg($"Vive Streaming Server v{value} connected.");
                break;
            case HMD_NAME:
                hmdName = value;
                ResoniteMod.Msg($"HMD Name: {value}");
                NotifyStatusChanged();
                break;
            case VS_SERVER_STATE:
                HandleServerState(value);
                break;
            default:
                break;
        }
    }

    private static void UpdateEyeDataStats(string data)
    {
        var parts = data.Split(',');
        var validDataCount = 0;

        foreach (var part in parts)
        {
            if (float.TryParse(part, out var value) && !float.IsNaN(value))
            {
                validDataCount++;
            }
        }

        eyeDataCount = validDataCount;
        lastEyeDataTime = DateTime.UtcNow;
        eyeFrameCount++;
    }

    private static void UpdateMouthDataStats(string data)
    {
        var parts = data.Split(',');
        var validDataCount = 0;

        foreach (var part in parts)
        {
            if (float.TryParse(part, out var value) && !float.IsNaN(value))
            {
                validDataCount++;
            }
        }

        mouthDataCount = validDataCount;
        lastMouthDataTime = DateTime.UtcNow;
        mouthFrameCount++;
    }

    private static void HandleServerState(string value)
    {
        if (int.TryParse(value, out var state))
        {
            switch (state)
            {
                case 0:
                    if (!Connected)
                    {
                        ResoniteMod.Msg("HMD connected");
                        Connected = true;
                        NotifyStatusChanged();
                    }
                    break;
                case 2:
                    if (Connected)
                    {
                        ResoniteMod.Msg("HMD disconnected");
                        Connected = false;
                        NotifyStatusChanged();
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

    /// <summary>
    /// Notifies subscribers about status changes.
    /// </summary>
    private static void NotifyStatusChanged()
    {
        var connectionStatus = Connected ? "Connected" : "Disconnected";
        var hmdModel = hmdName ?? "Unknown";
        var eyeTrackingStatus = GetEyeTrackingStatus();
        var mouthTrackingStatus = GetMouthTrackingStatus();
        var frameRate = GetFrameRate();

        StatusChanged?.Invoke(
            null,
            new StatusChangedEventArgs(
                connectionStatus,
                hmdModel,
                eyeTrackingStatus,
                mouthTrackingStatus,
                eyeDataCount,
                mouthDataCount,
                frameRate
            )
        );
    }

    private static string GetEyeTrackingStatus()
    {
        if (!Connected)
        {
            return "Disconnected";
        }
        var timeSinceLastData = DateTime.UtcNow - lastEyeDataTime;
        return timeSinceLastData.TotalSeconds < 1.0 ? "Active" : "Inactive";
    }

    private static string GetMouthTrackingStatus()
    {
        if (!Connected)
        {
            return "Disconnected";
        }
        var timeSinceLastData = DateTime.UtcNow - lastMouthDataTime;
        return timeSinceLastData.TotalSeconds < 1.0 ? "Active" : "Inactive";
    }

    private static int GetFrameRate()
    {
        if (!Connected)
        {
            return -1;
        }

        var now = DateTime.UtcNow;
        var elapsed = now - lastFrameRateUpdate;

        if (elapsed.TotalSeconds >= 1.0)
        {
            var eyeFps = eyeFrameCount / elapsed.TotalSeconds;
            var mouthFps = mouthFrameCount / elapsed.TotalSeconds;

            eyeFrameCount = 0;
            mouthFrameCount = 0;
            lastFrameRateUpdate = now;

            // 目と口の最大フレームレートを返す
            return (int)Math.Max(eyeFps, mouthFps);
        }

        return 0; // 計算中
    }

    /// <inheritdoc />
    public void UpdateInputs(float deltaTime)
    {
        if (!Connected && _tracking)
        {
            VS_PC_SDK.VS_StopFaceTracking();
            _tracking = false;
        }

        if (Connected && !_tracking && IsActive)
        {
            if (!VS_PC_SDK.VS_StartFaceTracking())
            {
                ResoniteMod.Error("Failed to start face tracking");
                throw new InvalidOperationException("Failed to start face tracking");
            }
            _tracking = true;
        }

        if (Connected && _tracking && !IsActive)
        {
            if (!VS_PC_SDK.VS_StopFaceTracking())
            {
                ResoniteMod.Error("Failed to stop face tracking");
                throw new InvalidOperationException("Failed to stop face tracking");
            }
            _tracking = false;
        }

        if (!Connected)
        {
            _tracking = false;
        }

        eyes?.UpdateInputs(Connected, ref eyeData, deltaTime);
        mouth?.UpdateInputs(Connected, ref lipData);

        // 定期的にステータスを更新（1秒間隔）
        var now = DateTime.UtcNow;
        if ((now - lastStatusUpdate).TotalSeconds >= 1.0)
        {
            NotifyStatusChanged();
            lastStatusUpdate = now;
        }
    }

    /// <inheritdoc />
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
