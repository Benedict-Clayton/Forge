using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace Junkyard.Scripts;

// Required attribute for registering the Mod.
// The string must match the name of the initialization function.
[ModInitializer(nameof(Init))]
public class Entry
{
    // Initialization function
    public static void Init()
    {
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
        var harmony = new Harmony("sts2.reme.Junkyard");
        harmony.PatchAll();

        // Allows .tscn files to load custom scripts.
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

        Log.Info("Mod initialized!");
    }
}