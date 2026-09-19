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

public sealed class LivingLodestone : CustomMonsterModel
{
    public const string MAGNETIZE = "MAGNETIZE";
    public const string ATTRACTION = "ATTRACTION";
    public const string ROTATE = "ROTATE";
    public const string EXPULSION = "EXPULSION";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 175, 155);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 175, 155);

    private int AttractionDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 11);
    private const int AttractionHits = 2;

    private int RotateBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 20);

    private int ExpelDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    private int ExpelBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 16, 14);


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/LivingLodestone.png");

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

        var magnetizeState = new MoveState(
            MAGNETIZE,
            MagnetizeMove,
            new AbstractIntent[] { new CardDebuffIntent() }
        );

        var rotateState = new MoveState(
            ROTATE,
            RotateMove,
            new AbstractIntent[] { new DefendIntent() }
        );

        var expelState = new MoveState(
            EXPULSION,
            ExpelMove,
            new AbstractIntent[] { new SingleAttackIntent(ExpelDamage), new DefendIntent() }
        );

        var attractionState = new MoveState(
            ATTRACTION,
            AttractionMove,
            new AbstractIntent[] { new MultiAttackIntent(AttractionDamage, AttractionHits) }
        );

        // Normal move cycle
        magnetizeState.FollowUpState = expelState;
        expelState.FollowUpState = attractionState;
        attractionState.FollowUpState = rotateState;
        rotateState.FollowUpState = expelState;

        states.Add(magnetizeState);
        states.Add(rotateState);
        states.Add(expelState);
        states.Add(attractionState);

        return new MonsterMoveStateMachine(states, magnetizeState);
    }

    private async Task MagnetizeMove(IReadOnlyList<Creature> targets)
    {
        foreach (Creature target in (IEnumerable<Creature>)targets)
        {
            await PowerCmd.Apply<Attract>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), target, 1, this.Creature, null);
        }
    }

    private async Task RotateMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(this.Creature, RotateBlock, ValueProp.Move, null);
    }

    private async Task ExpelMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ExpelDamage)
            .FromMonster(this)
            .Execute(null);

        await CreatureCmd.GainBlock(Creature, ExpelBlock, ValueProp.Move, null);
    }

    private async Task AttractionMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(AttractionDamage)
            .WithHitCount(AttractionHits)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .Execute(null);
    }
}