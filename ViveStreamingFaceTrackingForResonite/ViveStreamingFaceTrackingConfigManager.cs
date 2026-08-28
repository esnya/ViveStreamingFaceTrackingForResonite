using ResoniteModLoader;

namespace ViveStreamingFaceTrackingForResonite;

/// <summary>
/// Manages configuration keys for displaying ViveStreaming connection status information.
/// </summary>
public sealed class ViveStreamingFaceTrackingConfigManager
{
    private readonly ModConfiguration _config;
    private readonly ModConfigurationKey<string> _connectionStatusKey;
    private readonly ModConfigurationKey<string> _hmdModelKey;
    private readonly ModConfigurationKey<string> _eyeTrackingStatusKey;
    private readonly ModConfigurationKey<string> _mouthTrackingStatusKey;
    private readonly ModConfigurationKey<int> _eyeDataCountKey;
    private readonly ModConfigurationKey<int> _mouthDataCountKey;
    private readonly ModConfigurationKey<int> _frameRateKey;

    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public string ConnectionStatus
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_connectionStatusKey, value);
            }
        }
    } = "Disconnected";

    /// <summary>
    /// Gets or sets the HMD model name.
    /// </summary>
    public string HmdModel
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_hmdModelKey, value);
            }
        }
    } = "Unknown";

    /// <summary>
    /// Gets or sets the eye tracking status.
    /// </summary>
    public string EyeTrackingStatus
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_eyeTrackingStatusKey, value);
            }
        }
    } = "Disconnected";

    /// <summary>
    /// Gets or sets the mouth tracking status.
    /// </summary>
    public string MouthTrackingStatus
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_mouthTrackingStatusKey, value);
            }
        }
    } = "Disconnected";

    /// <summary>
    /// Gets or sets the number of active eye data points.
    /// </summary>
    public int EyeDataCount
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_eyeDataCountKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of active mouth data points.
    /// </summary>
    public int MouthDataCount
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_mouthDataCountKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the tracking frame rate.
    /// </summary>
    public int FrameRate
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _config.Set(_frameRateKey, value);
            }
        }
    } = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViveStreamingFaceTrackingConfigManager"/> class.
    /// </summary>
    /// <param name="config">The mod configuration instance.</param>
    /// <param name="connectionStatusKey">Configuration key for connection status.</param>
    /// <param name="hmdModelKey">Configuration key for HMD model.</param>
    /// <param name="eyeTrackingStatusKey">Configuration key for eye tracking status.</param>
    /// <param name="mouthTrackingStatusKey">Configuration key for mouth tracking status.</param>
    /// <param name="eyeDataCountKey">Configuration key for eye data count.</param>
    /// <param name="mouthDataCountKey">Configuration key for mouth data count.</param>
    /// <param name="frameRateKey">Configuration key for frame rate.</param>
    public ViveStreamingFaceTrackingConfigManager(
        ModConfiguration config,
        ModConfigurationKey<string> connectionStatusKey,
        ModConfigurationKey<string> hmdModelKey,
        ModConfigurationKey<string> eyeTrackingStatusKey,
        ModConfigurationKey<string> mouthTrackingStatusKey,
        ModConfigurationKey<int> eyeDataCountKey,
        ModConfigurationKey<int> mouthDataCountKey,
        ModConfigurationKey<int> frameRateKey
    )
    {
        _config = config;
        _connectionStatusKey = connectionStatusKey;
        _hmdModelKey = hmdModelKey;
        _eyeTrackingStatusKey = eyeTrackingStatusKey;
        _mouthTrackingStatusKey = mouthTrackingStatusKey;
        _eyeDataCountKey = eyeDataCountKey;
        _mouthDataCountKey = mouthDataCountKey;
        _frameRateKey = frameRateKey;

        InitializeValues();
    }

    private void InitializeValues()
    {
        _config.Set(_connectionStatusKey, ConnectionStatus);
        _config.Set(_hmdModelKey, HmdModel);
        _config.Set(_eyeTrackingStatusKey, EyeTrackingStatus);
        _config.Set(_mouthTrackingStatusKey, MouthTrackingStatus);
        _config.Set(_eyeDataCountKey, EyeDataCount);
        _config.Set(_mouthDataCountKey, MouthDataCount);
        _config.Set(_frameRateKey, FrameRate);
    }
}
