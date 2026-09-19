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

public sealed class Firekeeper : CustomMonsterModel
{
    public const string SEAR1 = "SEAR";
    public const string SEAR2 = "SEAR2";
    public const string SWELTER = "SWELTER";
    public const string INFLAME = "INFLAME";
    public const string INFERNO = "INFERNO";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 80);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 80);

    private int BlazeAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 45, 40);

    private int SearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 6);
    private int SearBurnAmount = 1;

    private int SwelterDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 6);
    private int SwelterWeakenAmount = 1;

    private int InflameStrength => 2;

    private int InfernoDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int InfernoHits => 3;

    private bool _infernoFirst;

    public bool InfernoFirst
    {
        get => _infernoFirst;
        set
        {
            AssertMutable();
            _infernoFirst = value;
        }
    }

    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/Firekeeper.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Blaze>(new ThrowingPlayerChoiceContext(), Creature, BlazeAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var searState = new MoveState(
            SEAR1,
            SearMove,
            new AbstractIntent[]
            {
                new SingleAttackIntent(SearDamage),
                new StatusIntent(SearBurnAmount)
            }
        );

        var swelterState = new MoveState(
            SWELTER,
            SwelterMove,
            new AbstractIntent[]
            {
                new SingleAttackIntent(SwelterDamage),
                new DebuffIntent()
            }
        );

        var sear2State = new MoveState(
            SEAR2,
            SearMove,
            new AbstractIntent[]
            {
                new SingleAttackIntent(SearDamage),
                new StatusIntent(SearBurnAmount)
            }
        );

        var inflameState = new MoveState(
            INFLAME,
            InflameMove,
            new AbstractIntent[]
            {
                new BuffIntent()
            }
        );

        var infernoState = new MoveState(
            INFERNO,
            InfernoMove,
            new AbstractIntent[]
            {
                new MultiAttackIntent(InfernoDamage, InfernoHits)
            }
        );

        searState.FollowUpState = swelterState;
        swelterState.FollowUpState = sear2State;
        sear2State.FollowUpState = inflameState;
        inflameState.FollowUpState = infernoState;
        infernoState.FollowUpState = searState;

        states.Add(searState);
        states.Add(swelterState);
        states.Add(sear2State);
        states.Add(inflameState);
        states.Add(infernoState);

        var initialState = InfernoFirst ? infernoState : searState;

        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task SearMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SearDamage)
            .FromMonster(this)
            .Execute(null);

        await CardPileCmd.AddToCombatAndPreview<Burn>(
            targets,
            PileType.Discard,
            SearBurnAmount,
            null);
    }

    private async Task SwelterMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SwelterDamage)
            .FromMonster(this)
            .Execute(null);

        await PowerCmd.Apply<WeakPower>(
            new ThrowingPlayerChoiceContext(),
            targets,
            SwelterWeakenAmount,
            Creature,
            null);
    }

    private async Task InflameMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            Creature,
            InflameStrength,
            Creature,
            null);
    }

    private async Task InfernoMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(InfernoDamage)
            .WithHitCount(InfernoHits)
            .FromMonster(this)
            .Execute(null);
    }
}