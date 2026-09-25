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

public sealed class Pyreling : CustomMonsterModel
{
    public const string BURN = "BURN";
    public const string GLOWER = "GLOWER";

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 26, 23);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 27, 24);

    private int GlowerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int BurnAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);

    private int FlareAmount = AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

    private bool _burnFirst;

    public bool BurnFirst
    {
        get => _burnFirst;
        set
        {
            AssertMutable();
            _burnFirst = value;
        }
    }

    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://images/monsters/Pyreling.png");

        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    // Put statuses here.
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Flare>(new ThrowingPlayerChoiceContext(), Creature, FlareAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        var burnState = new MoveState(
            BURN,
            BurnMove,
            new AbstractIntent[] { new StatusIntent(BurnAmount) }
        );

        var glowerState = new MoveState(
            GLOWER,
            GlowerMove,
            new AbstractIntent[] { new SingleAttackIntent(GlowerDamage) }
        );

        burnState.FollowUpState = glowerState;
        glowerState.FollowUpState = burnState;

        states.Add(glowerState);
        states.Add(burnState);

        var initialState = BurnFirst ? burnState : glowerState;

        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task BurnMove(IReadOnlyList<Creature> targets)
    {
        /*
        var spazAnim = Rng.Chaotic.NextInt(3) switch
        {
            0 => "spaz1",
            1 => "spaz2",
            _ => "spaz3"
        };
        await CreatureCmd.TriggerAnim(Creature, spazAnim, 0.0f);
        */

        // AFTPModAudio.Play("general", "thunderclap");

        // var sentryPos = Sts1VfxHelper.GetCreatureCenter(Creature);
        // ShockWaveEffect.PlayRoyal(sentryPos);

        // NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
        // await Cmd.Wait(0.5f);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Hand, BurnAmount, null);
    }

    private async Task GlowerMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Attack", 0.0f);

        // BorderFlashEffect.PlaySky();
        // AFTPModAudio.Play("general", "magic_beam_short");

        /*
        var playerCreature = targets.FirstOrDefault(c => c.Player != null);
        var playerPos = playerCreature != null
            ? Sts1VfxHelper.GetCreatureCenter(playerCreature)
            : Vector2.Zero;
        var sentryPos = Sts1VfxHelper.GetCreatureCenter(Creature);

        var laser = SmallLaserEffect.Create(sentryPos, playerPos);
        Sts1VfxHelper.Play(laser);

        await Cmd.Wait(0.3f);
        */

        await DamageCmd.Attack(GlowerDamage)
            .FromMonster(this)
            .Execute(null);
    }

    /*
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("idle", true);
        var attack = new AnimState("attack");
        var spaz1 = new AnimState("spaz1");
        var spaz2 = new AnimState("spaz2");
        var spaz3 = new AnimState("spaz3");
        var hit = new AnimState("hit");
        attack.NextState = idle;
        spaz1.NextState = idle;
        spaz2.NextState = idle;
        spaz3.NextState = idle;
        hit.NextState = idle;
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("spaz1", spaz1);
        animator.AddAnyState("spaz2", spaz2);
        animator.AddAnyState("spaz3", spaz3);
        animator.AddAnyState("Hit", hit);

        var animState = controller.GetAnimationState();
        animState.SetTimeScale(2.0f);
        var current = animState.GetCurrent(0);
        current.SetTrackTime(Rng.Chaotic.NextFloat(current.GetAnimationEnd()));
        animState.Update(0.0f);
        animState.Apply(controller.GetSkeleton());

        return animator;
    }
    */
}