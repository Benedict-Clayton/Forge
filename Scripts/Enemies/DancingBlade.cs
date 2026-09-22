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

public sealed class DancingBlade : CustomMonsterModel
{
    public const string DANCE = "SWORD_DANCE";
    public const string RIPOSTE = "RIPOSTE";
    public const string CUT = "CUT";
    public const string EXECUTE = "EXECUTE";
    private const string STUNNED = "STUNNED";
    private MoveState _stunnedState = null!;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 85, 80);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 85, 80);

    private int DanceStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);
    private int DanceBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 8);

    private int RiposteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);
    private int RiposteBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 10);

    private int CutDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private const int CutHits = 2;

    private int ExecuteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private const int ExecuteHits = 4;

    private int StaggerAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 40);

    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/DancingBlade.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    // Put statuses here.
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Stagger>(new ThrowingPlayerChoiceContext(), Creature, StaggerAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var danceState = new MoveState(
            DANCE,
            DanceMove,
            new AbstractIntent[] { new DefendIntent(), new BuffIntent() }
        );

        var riposteState = new MoveState(
            RIPOSTE,
            RiposteMove,
            new AbstractIntent[] { new SingleAttackIntent(RiposteDamage), new DefendIntent() }
        );

        var cutState = new MoveState(
            CUT,
            CutMove,
            new AbstractIntent[] { new MultiAttackIntent(CutDamage, CutHits) }
        );

        var executeState = new MoveState(
            EXECUTE,
            ExecuteMove,
            new AbstractIntent[] { new MultiAttackIntent(ExecuteDamage, ExecuteHits) }
        );

        _stunnedState = new MoveState(
            STUNNED,
            Stunned,
            new AbstractIntent[] { new StunIntent() }
        );

        danceState.FollowUpState = riposteState;
        riposteState.FollowUpState = cutState;
        cutState.FollowUpState = executeState;
        executeState.FollowUpState = danceState;

        _stunnedState.FollowUpState = danceState;

        states.Add(danceState);
        states.Add(riposteState);
        states.Add(cutState);
        states.Add(executeState);
        states.Add(_stunnedState);

        return new MonsterMoveStateMachine(states, danceState);
    }

    private async Task DanceMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, DanceStrength, Creature, null);
        await CreatureCmd.GainBlock(Creature, DanceBlock, ValueProp.Move, null);
    }

    private async Task RiposteMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(RiposteDamage)
            .FromMonster(this)
            .Execute(null);

        await CreatureCmd.GainBlock(Creature, RiposteBlock, ValueProp.Move, null);
    }

    private async Task CutMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(CutDamage)
            .WithHitCount(CutHits)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .Execute(null);
    }

    private async Task ExecuteMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ExecuteDamage)
            .WithHitCount(ExecuteHits)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .Execute(null);
    }

    private async Task Stunned(IReadOnlyList<Creature> targets)
    {
        // Stunned — does nothing, next move is Fell
        await Cmd.Wait(0.5f);
    }

    // Called when Stagger
    public async Task OnStagger()
    {
        await Cmd.Wait(0.3f);

        SetMoveImmediate(_stunnedState, true);
    }
}