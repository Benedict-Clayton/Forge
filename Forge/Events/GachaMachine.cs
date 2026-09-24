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
namespace Forge;

public sealed class GachaMachine : CustomEventModel
{
    public override ActModel[] Acts =>
    [
        CustomContentDictionary.CustomActs.First(act => act is ForgeAct)
    ];

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All<Player>((Func<Player, bool>)(p => p.Gold >= 80));
    }
    private bool _smashed;
    private int _wishCount = 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("WishCost", 80M)
    };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = new List<EventOption>();

        if (_smashed)
        {
            options.Add(Option(Leave));
            return options;
        }

        if (Owner!.Gold >= DynamicVars["WishCost"].IntValue)
        {
            options.Add(Option(Wish));

            if (_wishCount == 0)
            {
                options.Add(Option(Smash));
            }
            else
            {
                options.Add(Option(Leave));
            }
        }
        else
        {
            options.Add(new EventOption(
                this,
                null,
                "FORGE-GACHA_MACHINE.pages.INITIAL.options.WISH_LOCKED",
                Array.Empty<IHoverTip>()));

            options.Add(Option(Leave));
        }

        return options;
    }

    private async Task Wish()
    {
        await PlayerCmd.LoseGold(DynamicVars["WishCost"].BaseValue, Owner!);

        var relic = RelicFactory.PullNextRelicFromFront(Owner!).ToMutable();
        await RelicCmd.Obtain(relic, Owner!);

        _wishCount++;
        DynamicVars["WishCost"].BaseValue *= 2;

        SetEventState(PageDescription("WISH"), GenerateInitialOptions());
    }

    private async Task Smash()
    {
        await PlayerCmd.GainGold(Rng.NextInt(40, 51), Owner!);

        _smashed = true;

        SetEventFinished(PageDescription("SMASH"));
    }

    private Task Leave()
    {
        SetEventFinished(PageDescription("LEAVE"));
        return Task.CompletedTask;
    }
}