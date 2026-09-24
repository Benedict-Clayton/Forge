using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Acts;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Forge;

public sealed class EndangeredCube : CustomEventModel
{
    public override ActModel[] Acts =>
    [
        CustomContentDictionary.CustomActs.First(act => act is ForgeAct)
    ];

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All<Player>((Func<Player, bool>)(p => (Decimal)p.Creature.CurrentHp <= (Decimal)p.Creature.MaxHp * 0.70M));
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new StringVar(
        "FriendlyCube",
        ModelDb.Relic<FriendlyCube>().Title.GetFormattedText()),
        new DynamicVar("HealAmount", 20M)
    };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = new List<EventOption>
        {
            Option(Save, "INITIAL", HoverTipFactory.FromRelic(ModelDb.Relic<FriendlyCube>()).ToArray()),

        Option(Push, "INITIAL", HoverTipFactory.FromRelic(ModelDb.Relic<BurntScar>()).ToArray())
        };

        return options;
    }

    private async Task Save()
    {
        var relic = ModelDb.Relic<FriendlyCube>().ToMutable();
        await RelicCmd.Obtain(relic, Owner!);

        SetEventFinished(PageDescription("SAVE"));
    }

    private async Task Push()
    {
        var relic = ModelDb.Relic<BurntScar>().ToMutable();
        await RelicCmd.Obtain(relic, Owner!);
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars["HealAmount"].BaseValue);

        SetEventFinished(PageDescription("PUSH"));
    }
}