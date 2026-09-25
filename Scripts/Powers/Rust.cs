using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Linq;
using System.Threading.Tasks;

namespace Forge;

public sealed class Rust : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new IHoverTip[]
            {
                HoverTipFactory.FromPower<DexterityPower>()
            };
        }
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != this.Owner)
            return;

        if (command.TargetSide == this.Owner.Side)
            return;

        Dictionary<Creature, List<DamageResult>> damageResultsByCreature = new();

        foreach (List<DamageResult> results in command.Results)
        {
            foreach (DamageResult result in results)
            {
                if (!result.Receiver.IsPlayer || !result.WasFullyBlocked)
                    continue;

                if (!damageResultsByCreature.ContainsKey(result.Receiver))
                {
                    damageResultsByCreature.Add(result.Receiver, new List<DamageResult>());
                    this.Flash();
                }  
                damageResultsByCreature[result.Receiver].Add(result);
            }
        }

        foreach (Creature creature in damageResultsByCreature.Keys)
        {
            int blockedHits = damageResultsByCreature[creature].Count;

            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), creature, -this.Amount * blockedHits, this.Owner, null);
        }

        if (damageResultsByCreature.Count > 0)
        {
            this.Flash();
        }
    }
}