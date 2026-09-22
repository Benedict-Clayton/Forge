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

public sealed class GhostCouncil : CustomEventModel
{
    public override ActModel[] Acts =>
    [
        CustomContentDictionary.CustomActs.First(act => act is ForgeAct)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new StringVar(
        "Dormant",
        ModelDb.Enchantment<Dormant>().Title.GetFormattedText()),
        new DynamicVar("UseMaxHpLoss", 5M),
        new DynamicVar("UseMaxHpGain", 14M)
    };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            Option(SchemeIt, HoverTipFactory.FromEnchantment<Dormant>()),
            Option(PlayIt, HoverTipFactory.FromCardWithCardHoverTips<Writhe>()),
            Option(CaptureIt, "INITIAL", HoverTipFactory.FromPotion((PotionModel) ModelDb.Potion<GhostInAJar>())).ThatDecreasesMaxHp(5M)
        };
    }

    private async Task SchemeIt()
    {
        EnchantmentModel dormant = ModelDb.Enchantment<Dormant>();

        List<CardModel> list = PileType.Deck
            .GetPile(this.Owner!)
            .Cards
            .Where(c => dormant.CanEnchant(c))
            .ToList();

        CardSelectorPrefs prefs = new CardSelectorPrefs(
            CardSelectorPrefs.EnchantSelectionPrompt,
            1);

        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(
            (IReadOnlyList<CardModel>)list,
            dormant,
            1,
            prefs)).FirstOrDefault();

        if (card != null)
        {
            CardCmd.Enchant<Dormant>(card, 1M);
        }

        SetEventFinished(PageDescription("SCHEME"));
    }

    private async Task PlayIt()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, 14M);
        await CardPileCmd.AddCurseToDeck<Writhe>(this.Owner);

        SetEventFinished(PageDescription("PLAY"));
    }

    private async Task CaptureIt()
    {
        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            6M,
            false);

        await RewardsCmd.OfferCustom(
            Owner,
            new List<Reward>
            {
            new PotionReward(
                ModelDb.Potion<GhostInAJar>().ToMutable(),
                Owner)
            });

        SetEventFinished(PageDescription("CAPTURE"));
    }
}