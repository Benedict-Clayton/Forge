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

namespace Forge;

public sealed class ScrapSlime : CustomEventModel
{
    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All(p => p.Creature.CurrentHp >= 12);
    }

    /*
    public override ActModel[] Acts => new[]
    {
        ModelDb.Act<ForgeAct>()
    };
    */

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DamageVar(11M, ValueProp.Unblockable | ValueProp.Unpowered)
    };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            Option(ReachIn).ThatDoesDamage(DynamicVars.Damage.BaseValue),
            Option(StudyIt)
        };
    }

    private async Task ReachIn()
    {
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);

        var relic = RelicFactory.PullNextRelicFromFront(Owner).ToMutable();
        await RelicCmd.Obtain(relic, Owner);

        SetEventFinished(PageDescription("REACH"));
    }

    private async Task StudyIt()
    {
        PotionModel? potionModel = Owner!.PlayerRng.Rewards.NextItem<PotionModel>(
            Owner.Character!
                .PotionPool
                .GetUnlockedPotions(Owner.UnlockState)
                .Concat(
                    ModelDb.PotionPool<SharedPotionPool>()
                        .GetUnlockedPotions(Owner.UnlockState))
                .Where(p => p.Rarity == PotionRarity.Uncommon));

        if (potionModel != null)
        {
            await RewardsCmd.OfferCustom(
                Owner,
                new List<Reward>
                {
                    new PotionReward(
                        potionModel.ToMutable(),
                        Owner)
                });
        }

        SetEventFinished(PageDescription("STUDY"));
    }
}