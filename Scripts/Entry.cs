using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Patches.Content;

namespace Forge.Scripts;

// Required attribute for registering the Mod.
// The string must match the name of the initialization function.
[ModInitializer(nameof(Init))]
public class Entry
{
    // Initialization function
    public static void Init()
    {
        var forgeAct = new ForgeAct();

        Log.Info($"Custom Acts count: {CustomContentDictionary.CustomActs.Count}");

        foreach (var act in CustomContentDictionary.CustomActs)
        {
            Log.Info($"CUSTOM ACT: {act.GetType().FullName}");
        }

        var sts2Assembly = typeof(ActModel).Assembly;

        foreach (var type in sts2Assembly.GetTypes())
        {
            if (typeof(ActModel).IsAssignableFrom(type))
            {
                Log.Info($"ACT TYPE: {type.FullName}");
            }
        }

        // Used for applying patches (i.e. modifying game code).
        // The parameter can be anything, as long as it doesn't conflict with someone else's.
        var harmony = new Harmony("sts2.reme.Forge");
        harmony.PatchAll();

        // Allows .tscn files to load custom scripts.
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

        Log.Info("Mod initialized!");
    }
}