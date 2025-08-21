using System;
using System.Linq;
using System.Reflection;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace ViveStreamingFaceTrackingForResonite;

/// <summary>
/// Resonite mod for Vive Streaming Face Tracking integration.
/// </summary>
public partial class ViveStreamingFaceTrackingMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(ViveStreamingFaceTrackingMod).Assembly;

    /// <inheritdoc />
    public override string Name =>
        ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
        ?? nameof(ViveStreamingFaceTrackingMod);

    /// <inheritdoc />
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";

    /// <inheritdoc />
    public override string Version =>
        ModAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? ModAssembly.GetName().Version?.ToString() ?? "0.0.0";

    /// <inheritdoc />
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(meta => meta.Key == "RepositoryUrl")
            ?.Value
        ?? "";

    private static ModConfiguration? config;
    private static readonly ViveStreamingFaceTrackingDriver driver = new();
    private static ViveStreamingFaceTrackingConfigManager? configManager;

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> enabledkey = new(
        "Enabled",
        "Enable Mod",
        () => true
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<string> connectionStatusKey = new(
        "ConnectionStatus",
        "Connection Status (Read-only)",
        () => "Disconnected"
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<string> hmdModelKey = new(
        "HMDModel",
        "HMD Model (Read-only)",
        () => "Unknown"
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<string> eyeTrackingStatusKey = new(
        "EyeTrackingStatus",
        "Eye Tracking Status (Read-only)",
        () => "Disconnected"
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<string> mouthTrackingStatusKey = new(
        "MouthTrackingStatus",
        "Mouth Tracking Status (Read-only)",
        () => "Disconnected"
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> eyeDataCountKey = new(
        "EyeDataCount",
        "Active Eye Data Points (Read-only)",
        () => 0
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> mouthDataCountKey = new(
        "MouthDataCount",
        "Active Mouth Data Points (Read-only)",
        () => 0
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> frameRateKey = new(
        "FrameRate",
        "Tracking Frame Rate (Read-only)",
        () => -1
    );

    /// <inheritdoc />
    public override void OnEngineInit()
    {
#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif

        Engine.Current.RunPostInit(() =>
        {
#pragma warning disable CA1031
            try
            {
                Init(this);
                Engine.Current.InputInterface.RegisterInputDriver(driver);
            }
            catch (Exception e)
            {
                Error(e);
            }
#pragma warning restore CA1031
        });
    }

    private static void Init(ResoniteMod modInstance)
    {
        config = modInstance?.GetConfiguration();

        driver.IsActive = config?.GetValue(enabledkey) ?? true;

        enabledkey.OnChanged += (_) =>
        {
            driver.IsActive = config?.GetValue(enabledkey) ?? true;
        };

        if (config is not null)
        {
            configManager = new ViveStreamingFaceTrackingConfigManager(
                config,
                connectionStatusKey,
                hmdModelKey,
                eyeTrackingStatusKey,
                mouthTrackingStatusKey,
                eyeDataCountKey,
                mouthDataCountKey,
                frameRateKey
            );

            ViveStreamingFaceTrackingDriver.StatusChanged += OnStatusChanged;
        }
    }

    private static void OnStatusChanged(object? sender, StatusChangedEventArgs e)
    {
        if (configManager != null)
        {
            configManager.ConnectionStatus = e.ConnectionStatus;
            configManager.HmdModel = e.HmdModel;
            configManager.EyeTrackingStatus = e.EyeTrackingStatus;
            configManager.MouthTrackingStatus = e.MouthTrackingStatus;
            configManager.EyeDataCount = e.EyeDataCount;
            configManager.MouthDataCount = e.MouthDataCount;
            configManager.FrameRate = e.FrameRate;
        }
    }

#if DEBUG
    /// <summary>
    /// [HotReloadLib] Called before hot reload to clean up resources.
    /// </summary>
    public static void BeforeHotReload()
    {
        ViveStreamingFaceTrackingDriver.StatusChanged -= OnStatusChanged;
        driver.Dispose();
        configManager = null;
    }

    /// <summary>
    /// [HotReloadLib] Called after hot reload to reinitialize the mod.
    /// </summary>
    /// <param name="modInstance">The mod instance.</param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
        Engine.Current.InputInterface.RegisterInputDriver(driver);
    }
#endif
}
