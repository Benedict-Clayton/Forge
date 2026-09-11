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

public sealed class Attract : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldFlush(Player player) => player != this.Owner.Player;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        foreach (var card in Owner.Player!.PlayerCombatState!.AllCards)
        {
            card.GiveSingleTurnRetain();
        }

        Flash();
        await PowerCmd.Apply<Repel>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner, 1, this.Owner, null);
        await PowerCmd.Remove(this);
    }
}