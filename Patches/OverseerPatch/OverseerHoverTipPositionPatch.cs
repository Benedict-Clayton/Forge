using Godot;
using HarmonyLib;
using MegaCrit;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.HoverTips;

namespace Forge;

[HarmonyPatch(
	typeof(NHoverTipSet),
	nameof(NHoverTipSet.SetAlignment),
	new[] { typeof(Control), typeof(HoverTipAlignment) }
)]
public static class OverseerHoverTipPositionPatch
{
	[HarmonyPostfix]
	private static void Postfix(
	NHoverTipSet __instance,
	Control node)
	{
		NCreature? creature = node.GetParent() as NCreature;

		if (creature == null)
			return;

		if (creature.Entity.Monster is not Overseer)
			return;

		Vector2 offset = new Vector2(-60f, 400f);

		__instance._textHoverTipContainer.GlobalPosition += offset;
		__instance._cardHoverTipContainer.GlobalPosition += offset;

		__instance.CorrectVerticalOverflow();
		__instance.CorrectHorizontalOverflow();
	}
}