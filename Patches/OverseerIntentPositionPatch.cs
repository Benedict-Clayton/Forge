using Godot;
using HarmonyLib;
using MegaCrit;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Forge;
[HarmonyPatch(
    typeof(NCreature),
    nameof(NCreature.UpdateBounds),
    new[] { typeof(Node) }
)]
public static class OverseerIntentPositionPatch
{
    [HarmonyPrefix]
    private static void Prefix(NCreature __instance, Node boundsContainer)
    {
        if (__instance.Entity.Monster is not Overseer)
            return;

        Marker2D intentPos =
            boundsContainer.GetNode<Marker2D>("IntentPos");

        intentPos.Position += new Vector2(-180, 470);
    }
}