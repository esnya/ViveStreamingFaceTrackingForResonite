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
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

    /// <inheritdoc />
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    /// <inheritdoc />
    public override string Version =>
        ModAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

    /// <inheritdoc />
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(meta => meta.Key == "RepositoryUrl")
            .Value;

    private static ModConfiguration? config;
    private static readonly ViveStreamingFaceTrackingDriver driver = new();

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> enabledkey = new(
        "Enabled",
        "Enable Mod",
        () => true
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
    }

#if DEBUG
    /// <summary>
    /// [HotReloadLib] Called before hot reload to clean up resources.
    /// </summary>
    public static void BeforeHotReload()
    {
        driver.Dispose();
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
