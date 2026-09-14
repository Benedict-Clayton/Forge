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

public sealed class ServoB : CustomMonsterModel
{
    public const string ROLL = "ROLL";
    public const string STEAM = "STEAM";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 36, 32);

    private int RollDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int RollHits => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int RollBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 6);

    private int SteamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);

    // private int DebuffAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/ServoB.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    // Put statuses here.
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        await PowerCmd.Apply<HighVoltagePower>(
            new ThrowingPlayerChoiceContext(),
            Creature,
            1,
            Creature,
            null!);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var rollState = new MoveState(
            ROLL,
            RollMove,
            new AbstractIntent[] { new MultiAttackIntent(RollDamage, RollHits), new DefendIntent() }
        );

        var steamState = new MoveState(
            STEAM,
            SteamMove,
            new AbstractIntent[] { new SingleAttackIntent(SteamDamage) }
        );

        rollState.FollowUpState = steamState;
        steamState.FollowUpState = rollState;

        states.Add(rollState);
        states.Add(steamState);

        var initialState = Creature.SlotName is "servo1" or "servo3"
        ? rollState
        : steamState;

        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task RollMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(RollDamage)
            .WithHitCount(RollHits)
            .FromMonster(this)
            .Execute(null);

        await CreatureCmd.GainBlock(Creature, RollBlock, ValueProp.Move, null);
    }

    private async Task SteamMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SteamDamage)
            .FromMonster(this)
            .Execute(null);
    }
}