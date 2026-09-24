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
using BaseLib.Patches.Content;
namespace Forge;

public sealed class GremlinContractor : CustomEventModel
{
    public override ActModel[] Acts =>
    [
        CustomContentDictionary.CustomActs.First(act => act is ForgeAct)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
	{
		new StringVar("Bury", ModelDb.Enchantment<Bury>().Title.GetFormattedText()),
		new DynamicVar("HealAmount", 5M),
		new DynamicVar("StandardCost", 55M),
		new DynamicVar("DeluxeCost", 99M)
	};

	public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.All<Player>((Func<Player, bool>)(p => p.Gold >= 55));
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> options = new List<EventOption>
		{
			Option(Trial),
			
			Option(Standard, HoverTipFactory.FromEnchantment<Bury>())
		};

		if (Owner!.Gold >= DynamicVars["DeluxeCost"].IntValue)
		{
			options.Add(Option(Deluxe, HoverTipFactory.FromEnchantment<Bury>()));
		}
		else
		{
			options.Add(new EventOption(this, null, "FORGE-GREMLIN_CONTRACTOR.pages.INITIAL.options.DELUXE_LOCKED", Array.Empty<IHoverTip>()));
		}

		return options;
	}

	private async Task Trial()
	{
		await CreatureCmd.Heal(Owner!.Creature, DynamicVars["HealAmount"].BaseValue);

		SetEventFinished(PageDescription("TRIAL"));
	}

	private async Task Standard()
	{
		await PlayerCmd.LoseGold(DynamicVars["StandardCost"].BaseValue, this.Owner!, GoldLossType.Spent);

		await EnchantCards(1);

		SetEventFinished(PageDescription("STANDARD"));
	}

	private async Task Deluxe()
	{
		await PlayerCmd.LoseGold(DynamicVars["DeluxeCost"].BaseValue, this.Owner!, GoldLossType.Spent);

		await EnchantCards(2);

		SetEventFinished(PageDescription("DELUXE"));
	}

	private async Task EnchantCards(int amount)
	{
		EnchantmentModel bury = ModelDb.Enchantment<Bury>();

		List<CardModel> list = PileType.Deck
			.GetPile(Owner!)
			.Cards
			.Where(c => bury.CanEnchant(c))
			.ToList();

		if (list.Count == 0)
			return;

		CardSelectorPrefs prefs = new CardSelectorPrefs(
			CardSelectorPrefs.EnchantSelectionPrompt,
			amount);

		IEnumerable<CardModel> cards =
			await CardSelectCmd.FromDeckForEnchantment(
				(IReadOnlyList<CardModel>)list,
				bury,
				amount,
				prefs);

		foreach (CardModel card in cards)
		{
			CardCmd.Enchant<Bury>(card, 1M);
		}
	}
}