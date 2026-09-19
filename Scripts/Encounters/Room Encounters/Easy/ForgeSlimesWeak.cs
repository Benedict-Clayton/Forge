using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class ForgeSlimesWeak : CustomEncounterModel
{
    public ForgeSlimesWeak() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override bool IsWeak => true;

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<LavaSlimeSmall>(),
        ModelDb.Monster<SlagSlimeSmall>(),
        ModelDb.Monster<LavaSlimeMedium>(),
        ModelDb.Monster<SlagSlimeMedium>()
    ];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var slime0 = (LavaSlimeSmall)ModelDb.Monster<LavaSlimeSmall>().ToMutable();
        var slime1 = (SlagSlimeSmall)ModelDb.Monster<SlagSlimeSmall>().ToMutable();

        var slime2 = Rng.NextItem<MonsterModel>([
        ModelDb.Monster<LavaSlimeMedium>(),
        ModelDb.Monster<SlagSlimeMedium>()
        ])!.ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (slime0, null),
            (slime1, null),
            (slime2, null)
        };
    }
}