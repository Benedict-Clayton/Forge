using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Forge;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.HasTurnEndInHandEffect), MethodType.Getter)]
public static class DormantHasTurnEndPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance)
    {

        if (__instance.Enchantment is Dormant dormant)
        {
            dormant.WakeUp();
        }
    }
}