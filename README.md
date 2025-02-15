# ViveStreamingFaceTrackingForResonite

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) to track eyes and lips with ViveStreaming.
Adding facial tracking to Resonite for HMD using Vive Hub such as Vive Focus 3, Vive XR Elite, and Vive Focus Vision.


## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place the [ViveStreamingFaceTracking.dll](https://github.com/esnya/EsnyaResoniteModTemplate/releases/latest/download/ViveStreamingFaceTracking.dll) into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed, it will create this folder for you.
3. Place 3 dlls from [ViveStreamingFaceTrackingModule/Libs](./ViveStreamingFaceTrackingModule/Libs) into your Resonite folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite` for a standard installation.
4. Launch the game. If you want to check that the mod is working, you can check your Resonite logs.


## Development Requirements

For development, you will need the [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) to be able to hot reload your mod with DEBUG build.


## Acknowledgements

This project includes components from [ViveStreamingFaceTrackingModule](https://github.com/ViveSoftware/ViveStreamingFaceTrackingModule) licensed under Apache License 2.0.
