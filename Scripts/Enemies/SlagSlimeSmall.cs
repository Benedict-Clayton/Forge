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

public sealed class SlagSlimeSmall : CustomMonsterModel
{
    public const string SPIKE = "SPIKE";
    public const string CORRODE = "CORRODE";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 14);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 19, 18);

    private int SlagAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    private int SpikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);
    private int SpikeHits => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);

    private int CorrodeWeakenAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);
    private int CorrodeSlimeAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/SlagSlimeSmall.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    // Put statuses here.
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Slag>(new ThrowingPlayerChoiceContext(), Creature, SlagAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var spikeState = new MoveState(
            SPIKE,
            SpikeMove,
            new AbstractIntent[]
            {
                new MultiAttackIntent(SpikeDamage, SpikeHits)
            }
        );

        var corrodeState = new MoveState(
            CORRODE,
            CorrodeMove,
            new AbstractIntent[]
            {
                new DebuffIntent(), new StatusIntent(CorrodeSlimeAmount)
            }
        );

        spikeState.FollowUpState = corrodeState;
        corrodeState.FollowUpState = spikeState;

        states.Add(spikeState);
        states.Add(corrodeState);

        return new MonsterMoveStateMachine(states, corrodeState);
    }

    private async Task SpikeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SpikeDamage)
            .WithHitCount(SpikeHits)
            .FromMonster(this)
            .Execute(null);
    }

    private async Task CorrodeMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, CorrodeWeakenAmount, Creature, null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, CorrodeSlimeAmount, null);
    }
}