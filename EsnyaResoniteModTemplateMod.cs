using System.Linq;
using System.Reflection;

using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;



#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace ViveStreamingFaceTrackingForResonite
{
    public partial class EsnyaResoniteModTemplateMod : ResoniteMod
    {
        private static Assembly ModAssembly => typeof(EsnyaResoniteModTemplateMod).Assembly;

        public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
        public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

        internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";


        private static ModConfiguration? config;
        private static readonly Harmony harmony = new(HarmonyId);

        public override void OnEngineInit()
        {
            Init(this);

#if DEBUG
            HotReloader.RegisterForHotReload(this);
#endif
        }

        private static void Init(ResoniteMod modInstance)
        {
            harmony.PatchAll();
            config = modInstance?.GetConfiguration();
        }

#if DEBUG
        public static void BeforeHotReload()
        {
            harmony.UnpatchAll(HarmonyId);
        }

        public static void OnHotReload(ResoniteMod modInstance)
        {
            Init(modInstance);
        }
#endif
    }
}
