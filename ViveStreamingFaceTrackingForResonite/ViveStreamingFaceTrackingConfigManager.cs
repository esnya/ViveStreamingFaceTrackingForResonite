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

    private string _connectionStatus = "Disconnected";
    private string _hmdModel = "Unknown";
    private string _eyeTrackingStatus = "Disconnected";
    private string _mouthTrackingStatus = "Disconnected";
    private int _eyeDataCount;
    private int _mouthDataCount;
    private int _frameRate = -1;

    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            if (_connectionStatus != value)
            {
                _connectionStatus = value;
                _config.Set(_connectionStatusKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the HMD model name.
    /// </summary>
    public string HmdModel
    {
        get => _hmdModel;
        set
        {
            if (_hmdModel != value)
            {
                _hmdModel = value;
                _config.Set(_hmdModelKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the eye tracking status.
    /// </summary>
    public string EyeTrackingStatus
    {
        get => _eyeTrackingStatus;
        set
        {
            if (_eyeTrackingStatus != value)
            {
                _eyeTrackingStatus = value;
                _config.Set(_eyeTrackingStatusKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the mouth tracking status.
    /// </summary>
    public string MouthTrackingStatus
    {
        get => _mouthTrackingStatus;
        set
        {
            if (_mouthTrackingStatus != value)
            {
                _mouthTrackingStatus = value;
                _config.Set(_mouthTrackingStatusKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of active eye data points.
    /// </summary>
    public int EyeDataCount
    {
        get => _eyeDataCount;
        set
        {
            if (_eyeDataCount != value)
            {
                _eyeDataCount = value;
                _config.Set(_eyeDataCountKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of active mouth data points.
    /// </summary>
    public int MouthDataCount
    {
        get => _mouthDataCount;
        set
        {
            if (_mouthDataCount != value)
            {
                _mouthDataCount = value;
                _config.Set(_mouthDataCountKey, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the tracking frame rate.
    /// </summary>
    public int FrameRate
    {
        get => _frameRate;
        set
        {
            if (_frameRate != value)
            {
                _frameRate = value;
                _config.Set(_frameRateKey, value);
            }
        }
    }

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
        ModConfigurationKey<int> frameRateKey)
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
        // 初期値を設定（プロパティを使用して自動的にconfigに反映）
        ConnectionStatus = _connectionStatus;
        HmdModel = _hmdModel;
        EyeTrackingStatus = _eyeTrackingStatus;
        MouthTrackingStatus = _mouthTrackingStatus;
        EyeDataCount = _eyeDataCount;
        MouthDataCount = _mouthDataCount;
        FrameRate = _frameRate;
    }
}
