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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Linq;
using System.Threading.Tasks;

namespace Forge;

public sealed class Afterburn : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get => HoverTipFactory.FromCardWithCardHoverTips<Burn>();
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented || creature != this.Owner)
        {
            return;
        }

        ICombatState combatState;
        combatState = this.CombatState;
        IReadOnlyList<Creature> targets  = combatState.PlayerCreatures;

        await CardPileCmd.AddToCombatAndPreview<Burn>(
            targets,
            PileType.Discard,
            Amount,
            null);
    }
}