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
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Forge;

public sealed class Potionomical : CustomEventModel
{
    private PotionModel? _potionOption;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new StringVar("PotionName", "")
    };

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All<Player>(
            player => player.Potions.Any<PotionModel>());
    }

    public override void CalculateVars()
    {
        _potionOption = Owner!.Potions.Any()
            ? Rng.NextItem(Owner.Potions)
            : null;

        if (_potionOption != null)
            ((StringVar)DynamicVars["PotionName"]).StringValue =
                _potionOption.Title.GetFormattedText();
    }

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        Owner!.CanUseOrRemovePotions = false;
        return Task.CompletedTask;
    }

    protected override void OnEventFinished()
    {
        Owner!.CanUseOrRemovePotions = true;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        if (_potionOption != null)
        {
            options.Add(new EventOption(
                this,
                async () => await Experiment(_potionOption),
                $"{Id.Entry}.pages.INITIAL.options.EXPERIMENT",
                _potionOption.HoverTips
            ).ThatHasDynamicTitle());
        }

        options.Add(new EventOption(
            this,
            async () => await BrewIt(),
            $"{Id.Entry}.pages.INITIAL.options.BREW_IT",
            Array.Empty<IHoverTip>()
        ));

        return options;
    }

    private async Task Experiment(PotionModel potion)
    {
        await PotionCmd.Discard(potion);

        var rewards = new List<Reward>();

        rewards.Add(new CardReward(
                CardCreationOptions.ForNonCombatWithDefaultOdds(
                    new[]
                    {
                        (CardPoolModel)ModelDb.CardPool<ColorlessCardPool>()
                    }),
                3,
                Owner!));

        await RewardsCmd.OfferCustom(Owner, rewards);

        SetEventFinished(PageDescription("EXPERIMENT"));
    }

    private async Task BrewIt()
    {
        await RewardsCmd.OfferCustom(
            Owner!,
            new List<Reward>
            {
                new PotionReward(
                    ModelDb.Potion<FoulPotion>().ToMutable(),
                    Owner!)
            });

        SetEventFinished(PageDescription("BREW_IT"));
    }
}