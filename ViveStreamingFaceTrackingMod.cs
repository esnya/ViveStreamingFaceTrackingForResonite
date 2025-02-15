using System;
using System.Linq;
using System.Reflection;

using Elements.Core;
using ResoniteModLoader;
using FrooxEngine;




#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace ViveStreamingFaceTrackingForResonite
{
    public partial class ViveStreamingFaceTrackingMod : ResoniteMod
    {
        private static Assembly ModAssembly => typeof(ViveStreamingFaceTrackingMod).Assembly;

        public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
        public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

        private static ModConfiguration? config;
        private static readonly ViveStreamingFaceTrackingDriver driver = new();
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enabledkey = new("Enabled", "Enable Mod", () => true);

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
#pragma warning restore CA1031z
            });
        }

        private static void Init(ResoniteMod modInstance)
        {
            config = modInstance?.GetConfiguration();

            driver.Active = config?.GetValue(enabledkey) ?? true;

            enabledkey.OnChanged += (_) =>
            {
                driver.Active = config?.GetValue(enabledkey) ?? true;
            };
        }

#if DEBUG
        public static void BeforeHotReload()
        {
            driver.Dispose();
        }

        public static void OnHotReload(ResoniteMod modInstance)
        {
            Init(modInstance);
            Engine.Current.InputInterface.RegisterInputDriver(driver);
        }
#endif
    }
}
