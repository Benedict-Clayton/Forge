using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Afflictions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forge;

public sealed class Repel : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        Flash();
        await PowerCmd.Apply<Attract>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner, 1, this.Owner, null);
        await PowerCmd.Remove(this);
    }


    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        return card.Owner == this.Owner.Player && card.Affliction is Hexed && keywords.Add(CardKeyword.Ethereal);
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        foreach (CardModel allCard in this.Owner.Player!.PlayerCombatState!.AllCards)
            await this.Afflict(allCard);
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner != this.Owner.Player)
            return;
        await this.Afflict(card);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        foreach (CardModel allCard in this.Owner.Player!.PlayerCombatState!.AllCards)
        {
            if (allCard.Affliction is Hexed)
                CardCmd.ClearAffliction(allCard);
        }
        return Task.CompletedTask;
    }

    public async Task Afflict(CardModel card)
    {
        if (card.Affliction != null)
            return;
        await CardCmd.Afflict<Hexed>(card, (Decimal)this.Amount);
    }
}