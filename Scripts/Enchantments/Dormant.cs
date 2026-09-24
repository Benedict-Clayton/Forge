using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forge;

public sealed class Dormant : CustomEnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new IHoverTip[]
            {
                HoverTipFactory.FromKeyword(CardKeyword.Unplayable)
            };
        }
    }

    public void WakeUp()
    {
        if (Status != EnchantmentStatus.Normal)
            return;

        Card.RemoveKeyword(CardKeyword.Unplayable);
        Status = EnchantmentStatus.Disabled;
    }

    protected override void OnEnchant()
    {
        this.Card.AddKeyword(CardKeyword.Unplayable);
        this.Card.EnergyCost.UpgradeBy(-1);
    }
}