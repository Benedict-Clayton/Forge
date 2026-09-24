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

public sealed class Bury : CustomEnchantmentModel
{
    public override bool ShouldStartAtBottomOfDrawPile => true;

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != this.Card.Owner!)
            return;

        if (player.PlayerCombatState!.TurnNumber > 1)
            return;

        await CardCmd.Discard(choiceContext, this.Card);
    }
}