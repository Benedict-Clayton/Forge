using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forge;

public sealed class Simplify : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        foreach (CardModel card in this.Owner.Player!.PlayerCombatState!.AllCards)
        {
            await CardCmd.Afflict<Simplified>(card, Amount);
        }
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner != this.Owner.Player || card.EnergyCost.Canonical < 0)
            return Task.CompletedTask;
        int cost = this.Amount;
        card.EnergyCost.SetThisCombat(Amount);
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        foreach (CardModel card in oldOwner.Player!.PlayerCombatState!.AllCards.Where<CardModel>((Func<CardModel, bool>)(c => c.Affliction is Simplified)))
            CardCmd.ClearAffliction(card);
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(
      CardModel card,
      Decimal originalCost,
      out Decimal modifiedCost)
    {
        if (!(card.Affliction is Simplified) || card.Owner != this.Owner.Player)
        {
            modifiedCost = originalCost;
            return false;
        }
        modifiedCost = (Decimal)this.Amount;
        return true;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        await PowerCmd.Remove(this);
    }
}