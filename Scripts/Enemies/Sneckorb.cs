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

public sealed class Sneckorb : CustomMonsterModel
{
	public const string SLAM = "SLAM";
	public const string SIMPLIFY = "SIMPLIFY";
	public const string LASER = "LASER";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 115, 110);
	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 115, 110);

	private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	private int SimplifyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 8);
	private int NormalizeAmount = 1;
	private int currentNormalizeAmount = 0;

	private int LaserDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
	private const int LaserHits = 2;

	private const int ArtifactAmount = 2;


	public override NCreatureVisuals CreateCustomVisuals()
	{
		Texture2D texture = GD.Load<Texture2D>("res://images/monsters/Sneckorb.png");

		return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
	}

	// Put statuses here.
	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, ArtifactAmount, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var states = new List<MonsterState>();

		var slamState = new MoveState(
			SLAM,
			SlamMove,
			new AbstractIntent[] { new SingleAttackIntent(SlamDamage) }
		);

		var simplifyState = new MoveState(
			SIMPLIFY,
			SimplifyMove,
			new AbstractIntent[] { new SingleAttackIntent(SimplifyDamage), new CardDebuffIntent() }
		);

		var laserState = new MoveState(
			LASER,
			LaserMove,
			new AbstractIntent[] { new MultiAttackIntent(LaserDamage, LaserHits) }
		);


		slamState.FollowUpState = simplifyState;
		simplifyState.FollowUpState = laserState;
		laserState.FollowUpState = slamState;

		states.Add(slamState);
		states.Add(simplifyState);
		states.Add(laserState);

		return new MonsterMoveStateMachine(states, slamState);
	}

	private async Task SlamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SlamDamage)
			.FromMonster(this)
			.Execute(null);
	}

	private async Task SimplifyMove(IReadOnlyList<Creature> targets)
	{
		currentNormalizeAmount++;

		await DamageCmd.Attack(SimplifyDamage)
			.FromMonster(this)
			.Execute(null);

		await PowerCmd.Apply<Simplify>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)targets, currentNormalizeAmount, this.Creature, (CardModel)null);
	}

	private async Task LaserMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LaserDamage)
			.WithHitCount(LaserHits)
			.FromMonster(this)
			.Execute(null);
	}
}