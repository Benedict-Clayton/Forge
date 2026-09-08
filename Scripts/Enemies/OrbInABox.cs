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

public sealed class OrbInABox : CustomMonsterModel
{
    public const string CRANK = "CRANK";
    public const string SURPRISE = "SURPRISE";
    private MoveState surpriseState = null!;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 63, 60);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 67, 64);

    private int CrankVigor => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

    private int SurpriseDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private const int SurpriseHits = 3;

    private int _crankCount = 0;
    private const int MaxCranks = 4;


    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/OrbInABox.png");

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

        var crankState = new MoveState(
            CRANK,
            CrankMove,
            new AbstractIntent[] { new BuffIntent() }
        );

        var surpriseState = new MoveState(
            SURPRISE,
            SurpriseMove,
            new AbstractIntent[] { new MultiAttackIntent(SurpriseDamage, SurpriseHits) }
        );

        var crankBranch = new ConditionalBranchState("CRANK_BRANCH", SelectNextMove);

        crankState.FollowUpState = crankBranch;
        surpriseState.FollowUpState = crankState;

        states.Add(crankBranch);
        states.Add(crankState);
        states.Add(surpriseState);

        return new MonsterMoveStateMachine(states, crankState);
    }

    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine)
    {
        float popChance = (float)_crankCount / MaxCranks;

        if (rng.NextFloat() < popChance)
        {
            return SURPRISE;
        }

        return CRANK;
    }

    private async Task CrankMove(IReadOnlyList<Creature> targets)
    {
        _crankCount++;
        await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Creature, CrankVigor, Creature, null);
    }

    private async Task SurpriseMove(IReadOnlyList<Creature> targets)
    {
        _crankCount = 0;

        await DamageCmd.Attack(SurpriseDamage)
            .WithHitCount(SurpriseHits)
            .FromMonster(this)
            .Execute(null);

    }
}