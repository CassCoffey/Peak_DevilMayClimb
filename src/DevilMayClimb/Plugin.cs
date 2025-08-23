using BepInEx;
using BepInEx.Logging;
using DevilMayClimb.Patch;
using HarmonyLib;
using System.Reflection;

namespace DevilMayClimb;

// This BepInAutoPlugin attribute comes from Hamunii.BepInEx.AutoPlugin
// For more info, see https://github.com/Hamunii/BepInEx.AutoPlugin
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new(Id);

    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        Log.LogInfo($"Loading {Name} Version {Version}");

        Log.LogInfo($"Applying patches...");
        ApplyPluginPatch();
        Log.LogInfo($"Patches applied");
    }

    private void ApplyPluginPatch()
    {
        _harmony.PatchAll(typeof(PlayerPatch));
    }
}
