using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;

namespace Forge;

public sealed class LavaSlimeMedium : CustomMonsterModel
{
    public const string BURNING_SPEW = "BURNING_SPEW";
    public const string SPUTTER = "SPUTTER";
    public const string WARMUP = "WARMUP";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 41, 40);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 43);

    private int AfterBurnAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    private int SputterDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 11);

    private int WarmupVigor => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 6);
    private int WarmupBurnAmount = 2;
    // private int BurningSpewSlimeAmount = 1;


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/LavaSlimeMedium.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    // Put statuses here.
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Afterburn>(new ThrowingPlayerChoiceContext(), Creature, AfterBurnAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        /*
        var burningSpewState = new MoveState(
            BURNING_SPEW,
            BurningSpewMove,
            new AbstractIntent[]
            {
                new StatusIntent(BurningSpewBurnAmount + BurningSpewSlimeAmount)
            }
        );
        */

        var sputterState = new MoveState(
            SPUTTER,
            SputterMove,
            new AbstractIntent[] { new SingleAttackIntent(SputterDamage) }
        );

        var warmupState = new MoveState(
            WARMUP,
            WarmupMove,
            new AbstractIntent[] { new StatusIntent(WarmupBurnAmount) , new BuffIntent() }
        );

        var sputterBranch = new ConditionalBranchState("SPUTTER_BRANCH", SelectNextMove);

        sputterState.FollowUpState = sputterBranch;
        warmupState.FollowUpState = sputterState;

        states.Add(sputterBranch);
        states.Add(sputterState);
        states.Add(warmupState);

        return new MonsterMoveStateMachine(states, warmupState);
    }

    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine)
    {
        if (rng.NextFloat() < 0.5f)
        {
            return SPUTTER;
        }

        return WARMUP;
    }

    private async Task WarmupMove(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Burn>(
            targets,
            PileType.Hand,
            WarmupBurnAmount,
            null);

        await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Creature, WarmupVigor, Creature, null);
    }

    private async Task SputterMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SputterDamage)
            .FromMonster(this)
            .Execute(null);
    }

    /*
    private async Task BurningSpewMove(IReadOnlyList<Creature> targets)
    {
        

        await CardPileCmd.AddToCombatAndPreview<Slimed>(
            targets,
            PileType.Discard,
            BurningSpewSlimeAmount,
            null);
    }
    */
}