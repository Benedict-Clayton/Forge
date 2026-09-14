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

public sealed class Overseer : CustomMonsterModel
{
    public const string CALL = "CALL";
    public const string FOCUS = "FOCUS";
    public const string EVOKE = "EVOKE";
    public const string REINFORCE = "REINFORCE";
    public const string HOTFIX = "HOTFIX";

    private string? previousMove;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 275, 255);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 275, 255);

    private int FocusDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);
    private int FocusVigorAmount = 1;

    private int EvokeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private const int EvokeHits = 2;
    private int EvokeVigorAmount = 1;

    private int ReinforceBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
    private int ReinforceVigorAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    private int HotfixVigorAmount = 4;

    private const int ArtifactAmount = 2;

    private int servoSpawnCount = 0;

    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>(
            "res://images/monsters/Overseer.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        await PowerCmd.Apply<ArtifactPower>(
            new ThrowingPlayerChoiceContext(),
            Creature,
            ArtifactAmount,
            Creature,
            null!);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var callState = new MoveState(
            CALL,
            CallMove,
            new AbstractIntent[] { new SummonIntent() }
        );

        var focusState = new MoveState(
            FOCUS,
            FocusMove,
            new AbstractIntent[] { new SingleAttackIntent(FocusDamage), new BuffIntent(), new SummonIntent() }
        );

        var evokeState = new MoveState(
            EVOKE,
            EvokeMove,
            new AbstractIntent[] { new MultiAttackIntent(EvokeDamage, EvokeHits), new BuffIntent(), new SummonIntent() }
        );

        var reinforceState = new MoveState(
            REINFORCE,
            ReinforceMove,
            new AbstractIntent[] { new DefendIntent(), new BuffIntent(), new SummonIntent() }
        );

        var hotfixState = new MoveState(
            HOTFIX,
            HotfixMove,
            new AbstractIntent[] { new BuffIntent() }
        );

        var pickState = new ConditionalBranchState(
            "PICK_MOVE",
            SelectNextMove
        );

        callState.FollowUpState = pickState;
        focusState.FollowUpState = pickState;
        evokeState.FollowUpState = pickState;
        reinforceState.FollowUpState = pickState;
        hotfixState.FollowUpState = pickState;

        states.Add(pickState);
        states.Add(callState);
        states.Add(focusState);
        states.Add(evokeState);
        states.Add(reinforceState);
        states.Add(hotfixState);

        return new MonsterMoveStateMachine(states, callState);
    }

    // Helper Method for spawning Servos.
    private async Task SpawnEnemy<T>() where T : MonsterModel
    {
        EncounterModel encounter = this.CombatState.Encounter!;

        if (this.CombatState.Enemies.Count >= encounter.Slots.Count)
        {
            return;
        }

        string? slotName = encounter.Slots.LastOrDefault(
            s => this.CombatState.Enemies.All(
                c => c.SlotName != s));

        if (slotName != null)
        {
            var summoned = await CreatureCmd.Add<T>(
                this.CombatState,
                slotName);

            await PowerCmd.Apply<MinionPower>(
                new ThrowingPlayerChoiceContext(),
                summoned,
                1,
                Creature,
                null!);
        }

        await Cmd.Wait(0.3f);
    }

    // Spawns the requested number of ServoAs.
    // After 8 ServoAs have spawned, pairs can be replaced by ServoBs.
    private async Task SpawnServos(int amount)
    {
        while (amount >= 2)
        {
            if (servoSpawnCount >= 6 && GD.Randf() < 0.75f)
            {
                await SpawnEnemy<ServoB>();
                servoSpawnCount += 2;
                amount -= 2;
            }
            else
            {
                await SpawnEnemy<ServoA>();
                servoSpawnCount++;
                amount--;
            }
        }

        if (amount == 1)
        {
            await SpawnEnemy<ServoA>();
            servoSpawnCount++;
        }
    }

    private int NumAliveMinions()
    {
        return CombatState.GetTeammatesOf(Creature)
            .Count(t => t != Creature && t.IsAlive);
    }

    private string SelectNextMove(
    Creature owner,
    Rng rng,
    MonsterMoveStateMachine stateMachine)
    {
        int numMinions = NumAliveMinions();

        string nextMove;

        switch (numMinions)
        {
            case 4:
                nextMove = previousMove == HOTFIX ? FOCUS : HOTFIX;
                break;

            case 3:
                nextMove = previousMove == FOCUS ? HOTFIX : FOCUS;
                break;

            case 2:
                nextMove = previousMove == EVOKE ? REINFORCE : EVOKE;
                break;

            case 1:
                nextMove = previousMove == REINFORCE ? EVOKE : REINFORCE;
                break;

            default:
                nextMove = CALL;
                break;
        }

        previousMove = nextMove;
        return nextMove;
    }


    public async Task CallMove(IReadOnlyList<Creature> targets)
    {
        await SpawnServos(4);
    }

    private async Task FocusMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(FocusDamage)
            .FromMonster(this)
            .Execute(null!);

        await PowerCmd.Apply<VigorPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(),
            (IEnumerable<Creature>)this.CombatState.GetTeammatesOf(this.Creature), FocusVigorAmount, this.Creature, (CardModel)null!);

        await SpawnServos(1);
    }

    private async Task EvokeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(EvokeDamage)
            .FromMonster(this)
            .WithHitCount(EvokeHits)
            .Execute(null!);

        await PowerCmd.Apply<VigorPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(),
            (IEnumerable<Creature>)this.CombatState.GetTeammatesOf(this.Creature), EvokeVigorAmount, this.Creature, (CardModel)null!);

        await SpawnServos(2);
    }

    private async Task ReinforceMove(IReadOnlyList<Creature> targets)
    {
        // await CreatureCmd.GainBlock(Creature, ReinforceBlock, ValueProp.Move, null);

        foreach (var teammate in CombatState.GetTeammatesOf(Creature))
        {
            await CreatureCmd.GainBlock(teammate, ReinforceBlock, ValueProp.Move, null);
        }

        await PowerCmd.Apply<VigorPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(),
            (IEnumerable<Creature>)this.CombatState.GetTeammatesOf(this.Creature), ReinforceVigorAmount, this.Creature, (CardModel)null!);

        await SpawnServos(3);

    }

    private async Task HotfixMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VigorPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), 
            (IEnumerable<Creature>)this.CombatState.GetTeammatesOf(this.Creature), HotfixVigorAmount, this.Creature, (CardModel)null!);
    }
}