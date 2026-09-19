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

public sealed class LavaSlimeSmall : CustomMonsterModel
{
    public const string SPUTTER = "SPUTTER";
    public const string WARMUP = "WARMUP";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 14);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 19, 18);

    private int AfterBurnAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    private int SputterDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);

    private int WarmupVigor => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/LavaSlimeSmall.png");

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

        var sputterState = new MoveState(
            SPUTTER,
            SputterMove,
            new AbstractIntent[] { new SingleAttackIntent(SputterDamage) }
        );

        var warmupState = new MoveState(
            WARMUP,
            WarmupMove,
            new AbstractIntent[] { new BuffIntent() }
        );

        var sputterBranch = new ConditionalBranchState("SPUTTER_BRANCH", SelectNextMove);

        sputterState.FollowUpState = sputterBranch;
        warmupState.FollowUpState = sputterState;

        states.Add(sputterBranch);
        states.Add(sputterState);
        states.Add(warmupState);

        return new MonsterMoveStateMachine(states, sputterState);
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
        await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Creature, WarmupVigor, Creature, null);
    }

    private async Task SputterMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SputterDamage)
            .FromMonster(this)
            .Execute(null);
    }
}