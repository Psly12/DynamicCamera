using SharpPluginLoader.Core;

namespace DynamicCamera
{
    internal class StageConditionals
    {
        public static readonly Stage[] nonCombatStages =
        [
           Stage.Astera,
           Stage.AsteraHub,
           Stage.PrivateSuite,
           Stage.ChamberOfFive,
           Stage.LivingQuarters,
           Stage.ResearchBase,
           Stage.Seliana,
           Stage.SelianaHub,
           Stage.SelianaRoom,
           Stage.PrivateQuarters
        ];

        public static readonly Dictionary<Stage, float> FOVRanges = new Dictionary<Stage, float>
        {
            { Stage.ResearchBase, 60f},
            { Stage.Astera, 60f},
            { Stage.AsteraHub, 60f},
            { Stage.ChamberOfFive, 60f},
            { Stage.Everstream, 57f},
            { Stage.CastleSchrade, 57f },
            { Stage.OriginIsleSharaIshvalda, 57f },
            { Stage.GuidingLands, 53f },
            { Stage.SelianaSupplyCache, 53f },
            { Stage.AlatreonStage, 53f },
            { Stage.OriginIsleNergigante, 53f },
            { Stage.HoarfrostReach, 53f},
            { Stage.SelianaHub, 53f},
            { Stage.Seliana, 53f},
            { Stage.CoralHighlands, 53f},
            { Stage.RottenVale, 53f},
            { Stage.WildspireWaste, 53f},
            { Stage.ElderRecess, 53f},
            { Stage.AncientForest, 53f},
            { Stage.TrainingCamp, 53f},
            { Stage.SpecialArena, 53f},
            { Stage.ConfluenceOfFates, 53f},
            { Stage.ChallengeArena, 53f},
            { Stage.ElDorado, 53f},
            { Stage.SelianaRoom, 51f},
            { Stage.PrivateSuite, 51f},
            { Stage.PrivateQuarters, 49f},
            { Stage.LivingQuarters, 48f}
        };
    }
}
