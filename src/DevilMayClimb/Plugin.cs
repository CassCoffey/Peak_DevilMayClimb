using BepInEx;
using BepInEx.Logging;
using DevilMayClimb.Patch;
using DevilMayClimb.Service;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace DevilMayClimb;

// This BepInAutoPlugin attribute comes from Hamunii.BepInEx.AutoPlugin
// For more info, see https://github.com/Hamunii/BepInEx.AutoPlugin
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new(Id);

    internal static ManualLogSource Log { get; private set; } = null!;

    internal static AssetBundle DMCAssets;

    private void Awake()
    {
        Log = Logger;

        Log.LogInfo($"Loading {Name} Version {Version}");

        var dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
        var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "devilmayclimbassets");
        DMCAssets = AssetBundle.LoadFromFile(assetBundleFilePath);

        DMCAssetManager.Init();

        Log.LogInfo($"Applying patches...");
        ApplyPluginPatch();
        Log.LogInfo($"Patches applied");
    }

    private void ApplyPluginPatch()
    {
        _harmony.PatchAll(typeof(PlayerPatch));
    }
}
