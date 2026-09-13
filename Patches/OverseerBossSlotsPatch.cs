using Godot;

using HarmonyLib;
using MegaCrit;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
namespace Forge;

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.CreateEnemyNodes))]
public static class OverseerBossSlotsPatch
{
	[HarmonyPrefix]
	private static void Prefix(NCombatRoom __instance)
	{
		if (__instance._visuals.Encounter is not OverseerBoss)
			return;

		if (__instance.EncounterSlots != null)
			return;

		var slots = new Control
		{
			Name = "OverseerBossSlots"
		};

		AddSlot(slots, "servo1", new Vector2(850, 638));
		AddSlot(slots, "servo2", new Vector2(1000, 688));
		AddSlot(slots, "servo3", new Vector2(1150, 638));
		AddSlot(slots, "servo4", new Vector2(1300, 688));
		AddSlot(slots, "overseer", new Vector2(1700, 600));

		__instance.EncounterSlots = slots;
	}

	private static void AddSlot(
		Control parent,
		string name,
		Vector2 position)
	{
		var marker = new Marker2D
		{
			Name = name,
			Position = position
		};

		parent.AddChild(marker);
	}
}