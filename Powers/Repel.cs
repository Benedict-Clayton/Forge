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

public sealed class Repel : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
    "Repel",
    "Your hand has Ethereal. At the end of your turn, swap to Attract.",
    "Your hand has Ethereal. At the end of your turn, swap to Attract.");

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        foreach (var card in Owner.Player.PlayerCombatState.AllCards)
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
        }

        Flash();
        await PowerCmd.Apply<Attract>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Remove(this);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        
    }
}