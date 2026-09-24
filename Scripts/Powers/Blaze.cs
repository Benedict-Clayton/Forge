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
using MegaCrit.Sts2.Core.Models.Powers;

namespace Forge;

public sealed class Blaze : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldScaleInMultiplayer => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new IHoverTip[]
            {
                HoverTipFactory.FromPower<StrengthPower>()
            };
        }
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target.Side != this.Owner.Side)
        {
            return;
        }

        if (result.UnblockedDamage <= 0)
        {
            return;
        }

        if (target.CurrentHp > target.MaxHp / 2)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, this.Owner!, 5, this.Owner!, null);
        await PowerCmd.Remove((PowerModel)this);
    }
}