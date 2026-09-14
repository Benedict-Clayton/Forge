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

public sealed class ServoA : CustomMonsterModel
{
    public const string BOOT = "BOOT";
    public const string FLUTTER = "FLUTTER";
    public const string SCRAMBLE = "SCRAMBLE";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 14);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 16, 15);

    private int FlutterDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int FlutterHits => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int DebuffAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/ServoA.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    // Put statuses here.
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var bootState = new MoveState(
            BOOT,
            BootMove,
            new AbstractIntent[] { new SleepIntent() }
        );

        var flutterState = new MoveState(
            FLUTTER,
            FlutterMove,
            new AbstractIntent[] { new MultiAttackIntent(FlutterDamage, FlutterHits), new StatusIntent(DebuffAmount) }
        );

        var scrambleState = new MoveState(
            SCRAMBLE,
            ScrambleMove,
            new AbstractIntent[] { new MultiAttackIntent(FlutterDamage, FlutterHits)}
        );

        bootState.FollowUpState = flutterState;
        flutterState.FollowUpState = scrambleState;
        scrambleState.FollowUpState = flutterState;

        states.Add(bootState);
        states.Add(flutterState);
        states.Add(scrambleState);

        var initialState = Creature.SlotName is "servo1" or "servo3"
        ? flutterState
        : scrambleState;

        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task BootMove(IReadOnlyList<Creature> targets)
    {
        await Cmd.Wait(0.3f);
        return;
    }

    private async Task FlutterMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(FlutterDamage)
            .WithHitCount(FlutterHits)
            .FromMonster(this)
            .Execute(null);

        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, DebuffAmount, null);
    }

    private async Task ScrambleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(FlutterDamage)
            .WithHitCount(FlutterHits)
            .FromMonster(this)
            .Execute(null);
    }
}