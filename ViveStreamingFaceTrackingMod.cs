using System;
using System.Linq;
using System.Reflection;
using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Patching;





namespace ViveStreamingFaceTrackingForResonite
{
    public partial class ViveStreamingFaceTrackingMod : Monkey<ViveStreamingFaceTrackingMod>
    {
        private static Assembly ModAssembly => typeof(ViveStreamingFaceTrackingMod).Assembly;

        public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
        public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

        private static readonly ViveStreamingFaceTrackingDriver driver = new();

        public override void OnEngineInit()
        {

            Engine.Current.RunPostInit(() =>
            {
#pragma warning disable CA1031
                try
                {
                    Init();
                    Engine.Current.InputInterface.RegisterInputDriver(driver);
                }
                catch (Exception e)
                {
                    Error(e);
                }
#pragma warning restore CA1031
            });
        }

        private static void Init()
        {
            driver.active = true; // TODO: add configuration support when available
        }

    }
}
