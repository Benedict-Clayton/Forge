using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Forge;

public sealed class Attract : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
    "Attract",
    "Your hand has Retain. At the end of your turn, swap to Repel.",
    "Your hand has Retain. At the end of your turn, swap to Repel.");

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        foreach (var card in Owner.Player.PlayerCombatState.AllCards)
        {
            card.GiveSingleTurnRetain();
            await CardCmd.Afflict<Retain>(card, 1m);
        }

        Flash();
        await PowerCmd.Apply<Repel>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Remove(this);
    }
}