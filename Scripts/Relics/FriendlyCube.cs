using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace Forge;

[Pool(typeof(EventRelicPool))]
public sealed class FriendlyCube : CustomRelicModel
{

    public override RelicRarity Rarity => RelicRarity.Event;

    private bool _isUsedUp;

    public override bool IsUsedUp => _isUsedUp;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new IHoverTip[]
            {
                HoverTipFactory.FromPower<BufferPower>()
            };
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new DynamicVar[1]
            {
                new DynamicVar("BufferAmount", 1M)
            };
        }
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (!(room is CombatRoom))
            return;

        if (_isUsedUp)
            return;

        Flash();
        await PowerCmd.Apply<BufferPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner.Creature, this.DynamicVars["BufferAmount"].BaseValue, this.Owner.Creature, null);
    }

    public override async Task AfterDamageReceived(
    PlayerChoiceContext choiceContext,
    Creature target,
    DamageResult result,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource)
    {
        if (!CombatManager.Instance.IsInProgress || target != this.Owner.Creature || result.UnblockedDamage <= 0 || props.HasFlag(ValueProp.Unpowered))
            return;

        Flash();
        _isUsedUp = true;
        Status = RelicStatus.Disabled;

        await Task.CompletedTask;
    }
}