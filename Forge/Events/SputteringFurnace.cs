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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using BaseLib.Patches.Content;

namespace Forge;

// Image NOT drawn by me, Find it here - https://www.artstation.com/artwork/d06PoK?album_id=6280492

public sealed class SputteringFurnace : CustomEventModel
{
    public override ActModel[] Acts =>
    [
        CustomContentDictionary.CustomActs.First(act => act is ForgeAct)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("UseMaxHpLoss", 3M)
    };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            Option(FeedIt),
            Option(UseIt).ThatDecreasesMaxHp(3M)
        };
    }

    private async Task FeedIt()
    {
        await CardPileCmd.RemoveFromDeck(
            (IReadOnlyList<CardModel>)
            (await CardSelectCmd.FromDeckForRemoval(
                Owner!,
                new CardSelectorPrefs(
                    CardSelectorPrefs.RemoveSelectionPrompt,
                    1)))
            .ToList<CardModel>());

        List<CardModel> list = PileType.Deck.GetPile(this.Owner!).Cards.ToList();

        CardModel? randomCard = this.Rng.NextItem<CardModel>((IEnumerable<CardModel>)list);

        if (randomCard != null)
        {
            await CardPileCmd.RemoveFromDeck(randomCard, true);
        }

        SetEventFinished(PageDescription("FEED"));
    }

    private async Task UseIt()
    {
        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            DynamicVars["UseMaxHpLoss"].IntValue,
            false);

        List<CardModel> list = PileType.Deck
            .GetPile(this.Owner!)
            .Cards
            .Where(c => c.IsUpgradable)
            .ToList();

        if (list.Count > 0)
        {
            CardModel? card = this.Rng.NextItem<CardModel>(list);

            if (card != null)
            {
                CardCmd.Upgrade(
                    card,
                    CardPreviewStyle.EventLayout);
            }
        }

        SetEventFinished(PageDescription("USE"));
    }
}