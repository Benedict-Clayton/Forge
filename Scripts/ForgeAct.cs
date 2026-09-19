using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Unlocks;

namespace Forge;

public sealed class ForgeAct : CustomActModel
{
    public ForgeAct() : base(actNumber: 2) { }

    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
    [
        ModelDb.Encounter<PyrelingWeak>(),
        ModelDb.Encounter<OrbInABoxWeak>(),
        ModelDb.Encounter<ForgeSlimesWeak>(),
        ModelDb.Encounter<DancingBladeWeak>(),

        ModelDb.Encounter<PyrelingNormal>(),
        ModelDb.Encounter<ForgeSlimesNormal>(),
        ModelDb.Encounter<SnekorbNormal>(),
        ModelDb.Encounter<ChompersNormal>(),
        ModelDb.Encounter<SwordBoxNormal>(),

        ModelDb.Encounter<FirekeeperDuoElite>(),
        ModelDb.Encounter<LivingLodestoneElite>(),

        ModelDb.Encounter<OverseerBoss>()
    ];

    public override IEnumerable<EventModel> AllEvents =>
    [
        // ModelDb.Event<ScrapSlime>(),
        ModelDb.Event<Amalgamator>(),

        ModelDb.Event<Bugslayer>(),
        ModelDb.Event<ColorfulPhilosophers>(),
        ModelDb.Event<ColossalFlower>(),
        ModelDb.Event<FieldOfManSizedHoles>(),
        ModelDb.Event<InfestedAutomaton>(),
        ModelDb.Event<LostWisp>(),
        ModelDb.Event<SpiritGrafter>(),
        ModelDb.Event<TheLanternKey>()
    ];

    public override bool IsUnlocked(UnlockState unlockState) => true;

    public override bool Equals(object? obj) => obj is ForgeAct;
    public override int GetHashCode() => typeof(ForgeAct).GetHashCode();

    public override IEnumerable<AncientEventModel> AllAncients
    {
        get
        {
            return Act2Ancients;
        }
    }

    // Colors differ from CustomActModel defaults (which are act 3 themed)
    public override Color MapTraveledColor => new Color("27221C");
    public override Color MapUntraveledColor => new Color("6E7750");
    public override Color MapBgColor => new Color("9B9562");

    // Original had these empty; CustomActModel defaults to act 3 music
    public override string[] BgMusicOptions => ["event:/music/act2_a1_v2", "event:/music/act2_a2_v2"];
    public override string[] MusicBankPaths => ["res://banks/desktop/act2_a1.bank", "res://banks/desktop/act2_a2.bank"];
    public override string AmbientSfx => "event:/sfx/ambience/act2_ambience";

    public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_2_skel_data.tres";

    public override string ChestSpineSkinNameNormal => "act2";
    public override string ChestSpineSkinNameStroke => "act2_stroke";
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act2";

    protected override string CustomMapTopBgPath => ModelDb.Act<Glory>().MapTopBgPath;
    protected override string CustomMapMidBgPath => ModelDb.Act<Glory>().MapMidBgPath;
    protected override string CustomMapBotBgPath => ModelDb.Act<Glory>().MapBotBgPath;
    protected override string CustomRestSiteBackgroundPath => "res://scenes/rest_site/hive_rest_site.tscn";

    protected override int NumberOfWeakEncounters => 2;
    protected override int BaseNumberOfRooms => 14;
}

