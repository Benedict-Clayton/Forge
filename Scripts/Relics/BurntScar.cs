using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Forge;

[Pool(typeof(EventRelicPool))]
public sealed class BurntScar : CustomRelicModel
{
    public const string _combatsKey = "Combats";
    public const string _burnCountKey = "BurnCount";
    public int _combatsLeft = 1;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsUsedUp => CombatsLeft <= 0;

    public override bool ShowCounter => false;

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new DynamicVar[2]
            {
                new DynamicVar("Combats", (Decimal)CombatsLeft),
                new DynamicVar("BurnCount", 1M)
            };
        }
    }

    [SavedProperty]
    public int CombatsLeft
    {
        get => _combatsLeft;
        set
        {
            AssertMutable();
            _combatsLeft = value;
            DynamicVars["Combats"].BaseValue = (Decimal)_combatsLeft;
            InvokeDisplayAmountChanged();

            if (!IsUsedUp)
                return;

            Status = RelicStatus.Disabled;
        }
    }

    public override async Task BeforeCombatStart()
    {
        if (CombatsLeft <= 0)
            return;

        await CardPileCmd.AddToCombatAndPreview<Burn>(
            Owner.Creature,
            PileType.Draw,
            DynamicVars["BurnCount"].IntValue,
            Owner,
            CardPilePosition.Random);

        CombatsLeft--;
        Flash();
    }
}