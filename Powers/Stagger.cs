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

public sealed class Stagger : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldScaleInMultiplayer => true;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage <= 0 || !props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
            return;
        Flash();

        int newAmount = Math.Max(0, Amount - result.UnblockedDamage);
        
        SetAmount(newAmount);
        await PowerCmd.Decrement(this);

        if (Amount <= 0 && Owner.Monster is DancingBlade dancingBlade)
        {
            await dancingBlade.OnStagger();
        }
    }
}