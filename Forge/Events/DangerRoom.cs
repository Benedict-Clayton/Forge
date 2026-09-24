using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Forge;

public sealed class DangerRoom : CustomEventModel
{
    private ActModel _normalAct;
    private ActModel _eliteAct;

    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState)
    {
        return runState.TotalFloor >= 23 && runState.CurrentActIndex == 1; // So 4 rooms after the Ancient so as to not surprise a player with a forced normal in the first 5 rooms.
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new StringVar("NormalAct", ""),
        new StringVar("EliteAct", "")
    };

    public override void OnRoomEnter()
    {
        var currentAct = Owner!.RunState.Act;

        var otherActs = ModelDb.ActsByIndex[1].Where(act => act != currentAct).ToList(); // [1] because thats an Act 2.

        _normalAct = Rng.NextItem(otherActs)!;
        _eliteAct = Rng.NextItem(otherActs)!;

        var NormalActName = (StringVar)DynamicVars["NormalAct"];
        var eliteActName = (StringVar)DynamicVars["EliteAct"];

        NormalActName.StringValue = _normalAct.Title.GetFormattedText();
        eliteActName.StringValue = _eliteAct.Title.GetFormattedText();
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            Option(FightNormal),
            Option(FightElite)
        };
    }

    private Task FightNormal()
    {
        var encounters = _normalAct.GenerateAllEncounters().Where(encounter => encounter.RoomType == RoomType.Monster && !encounter.IsWeak).ToList();
        var encounter = Rng.NextItem(encounters);

        EnterCombatWithoutExitingEvent(encounter, Array.Empty<Reward>(), false);

        return Task.CompletedTask;
    }

    private Task FightElite()
    {
        var encounters = _eliteAct.GenerateAllEncounters().Where(encounter => encounter.RoomType == RoomType.Elite).ToList();
        var encounter = Rng.NextItem(encounters);

        EnterCombatWithoutExitingEvent(encounter, Array.Empty<Reward>(), false);

        return Task.CompletedTask;
    }
}