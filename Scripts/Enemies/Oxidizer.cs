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

public sealed class Oxidizer : CustomMonsterModel
{
    public const string FEELER = "FEELER";
    public const string OXIDIZE = "OXIDIZE";
    public const string SLURP = "SLURP";
    public const string LUNGE = "LUNGE";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 129, 123);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 129, 123);

    private const int RustAmount = 2;

    private int FeelerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    private  int FeelerWeak = AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 2, 1);

    private int OxidizeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 8);
    private  int InfectionAmount = AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    private int SlurpDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int SlurpHits => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

    private int LungeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 23);

    public override NCreatureVisuals CreateCustomVisuals()
	{
		Texture2D texture = GD.Load<Texture2D>("res://images/monsters/Oxidizer.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
	}

	// Put statuses here.
	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<Rust>(new ThrowingPlayerChoiceContext(), Creature, RustAmount, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
        var states = new List<MonsterState>();

        var feelerState = new MoveState(
            FEELER,
            FeelerMove,
            new AbstractIntent[]
            {
                new SingleAttackIntent(FeelerDamage),
                new DebuffIntent()
            }
        );

        var oxidizeState = new MoveState(
            OXIDIZE,
            OxidizeMove,
            new AbstractIntent[]
            {
                new SingleAttackIntent(OxidizeDamage),
                new StatusIntent(InfectionAmount)
            }
        );

        var slurpState = new MoveState(
            SLURP,
            SlurpMove,
            new AbstractIntent[]
            {
                new MultiAttackIntent(SlurpDamage, SlurpHits)
            }
        );

        var lungeState = new MoveState(
            LUNGE,
            LungeMove,
            new AbstractIntent[]
            {
                new SingleAttackIntent(LungeDamage)
            }
        );


        feelerState.FollowUpState = oxidizeState;
        oxidizeState.FollowUpState = slurpState;
        slurpState.FollowUpState = lungeState;
        lungeState.FollowUpState = feelerState;

        states.Add(feelerState);
        states.Add(oxidizeState);
        states.Add(slurpState);
        states.Add(lungeState);

        return new MonsterMoveStateMachine(states, feelerState);
    }

    private async Task FeelerMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(FeelerDamage)
            .FromMonster(this)
            .Execute(null);

        await PowerCmd.Apply<WeakPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)targets, FeelerWeak, this.Creature, null);
	}

    private async Task OxidizeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(OxidizeDamage)
            .FromMonster(this)
            .Execute(null);

        await CardPileCmd.AddToCombatAndPreview<Infection>(targets, PileType.Discard, InfectionAmount, null);
    }

    private async Task SlurpMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SlurpDamage)
            .WithHitCount(SlurpHits)
            .FromMonster(this)
            .Execute(null);
    }

    private async Task LungeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(LungeDamage)
            .FromMonster(this)
            .Execute(null);
    }
}