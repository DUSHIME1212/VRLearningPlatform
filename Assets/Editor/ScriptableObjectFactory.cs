using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRLearning;
using VRLearning.Core;
using VRLearning.Simulation;
using VRLearning.Modules.CodeWorld;

namespace VRLearning.Editor
{
    /// <summary>
    /// Creates every ScriptableObject the project needs in one click.
    /// Menu: Tools > VRLearning > Create All ScriptableObjects
    /// Safe to run multiple times — existing assets are updated, not duplicated.
    /// </summary>
    public static class ScriptableObjectFactory
    {
        // ── Paths ────────────────────────────────────────────────
        const string ResDir       = "Assets/Resources";
        const string CharDir      = "Assets/ScriptableObjects/Characters";
        const string CourseDir    = "Assets/ScriptableObjects/Courses";
        const string ModuleDir    = "Assets/ScriptableObjects/Modules";
        const string PuzzleRoot   = "Assets/ScriptableObjects/Puzzles";
        const string PuzzleSM     = "Assets/ScriptableObjects/Puzzles/SimpleMachines";
        const string PuzzleBio    = "Assets/ScriptableObjects/Puzzles/Biology";
        const string PuzzleCW     = "Assets/ScriptableObjects/Puzzles/CodeWorld";
        const string ScenarioDir  = "Assets/ScriptableObjects/Scenarios";

        // ── Entry point ──────────────────────────────────────────
        [MenuItem("Tools/VRLearning/Create All ScriptableObjects")]
        public static void CreateAll()
        {
            EnsureDirectories();

            // ① Global settings
            var settings = MakeAppSettings();

            // ② Character
            var guide = MakeCharacter("guide", "char_guide_name");

            // ③ SimulationData — one per experiment scene
            var simLever    = MakeSimData("SimDef_Lever",        "sm_lever",          "The Lever",          "Inkoni",             "Use the lever and weights to explore mechanical advantage.", "Koresha inkoni n'ibiremba kugira ngo usuzume inyungu za mekanika.", 1, "SimpleMachines_Lever");
            var simPulley   = MakeSimData("SimDef_Pulley",       "sm_pulley",         "The Pulley",         "Iguruka",            "Spin a wheel to lift heavy loads with less effort.",           "Vugurura umuzunguruko kuzamura umutwaro munini wishize ingufu.", 2, "SimpleMachines_Pulley");
            var simIncline  = MakeSimData("SimDef_InclinedPlane","sm_inclined_plane", "Inclined Plane",     "Inzira Yagenewe",    "Control friction on a ramp to slide a block to the bottom.",   "Genzura gutsindagira kuri ramp kugira ngo ukorere igice hasi.", 3, "SimpleMachines_InclinedPlane");
            var simHeart    = MakeSimData("SimDef_HeartPump",    "blood_heart_pump",  "The Heart Pump",     "Inzira y'Umutima",   "Explore how the heart's four chambers pump blood around the body.", "Suzuma uburyo inzira enye z'umutima zisukura amaraso mu mubiri.", 1, "BloodCirc_HeartPump");
            var simArterial = MakeSimData("SimDef_ArterialFlow", "blood_arterial",    "Arterial Blood Flow","Inzira y'Amaraso",   "Watch blood cells travel through arteries under pressure.",    "Reba ingero z'amaraso zinyura mu miruka munsi y'umuvuduko.", 2, "BloodCirc_ArterialFlow");
            var simCap      = MakeSimData("SimDef_Capillary",    "blood_capillary",   "Capillary Exchange", "Gusana kwa Capillary","Zoom into capillaries and see oxygen crossing into cells.",    "Reba imbere mu miruka mike no kubona oxygen yinjira mu ngero.", 3, "BloodCirc_Capillary");
            var simDiaph    = MakeSimData("SimDef_Diaphragm",    "breath_diaphragm",  "The Diaphragm",      "Ingingo y'Umwuka",   "Move the diaphragm to control breathing in real time.",        "Yungura ingingo y'umwuka kugenzura guhumeka.", 1, "Breathing_Diaphragm");
            var simGas      = MakeSimData("SimDef_GasExchange",  "breath_gas",        "Gas Exchange",       "Gusana kwa Gaze",    "See how oxygen enters blood and CO2 leaves at the alveoli.",   "Reba uburyo oxygen yinjira mu maraso naho CO2 ivishe mu mashumba.", 2, "Breathing_GasExchange");
            var simLung     = MakeSimData("SimDef_LungExpansion","breath_lungs",      "Lung Expansion",     "Kwaguka kw'Ibihaha", "Expand the chest to draw air into the lungs.",                 "Gura isena kuzana umwuka mu bihaha.", 3, "Breathing_LungExpansion");

            // ④ PuzzleDefinitions — Simple Machines (update existing)
            var pdLever   = MakePuzzle(PuzzleSM, "PuzzleDef_Lever",   "sm_lever",   "simple_machines", "exp_lever_title",   DifficultyLevel.Easy,   45f, new[]{"hint_lever_1","hint_lever_2","hint_lever_3"}, new InstructionType[0]);
            var pdPulley  = MakePuzzle(PuzzleSM, "PuzzleDef_Pulley",  "sm_pulley",  "simple_machines", "exp_pulley_title",  DifficultyLevel.Easy,   60f, new[]{"hint_pulley_1","hint_pulley_2"}, new InstructionType[0]);
            var pdIncline = MakePuzzle(PuzzleSM, "PuzzleDef_Incline", "sm_incline", "simple_machines", "exp_incline_title", DifficultyLevel.Medium, 90f, new[]{"hint_incline_1","hint_incline_2"}, new InstructionType[0]);

            // ④ PuzzleDefinitions — Biology
            var pdHeart   = MakePuzzle(PuzzleBio, "PuzzleDef_HeartPump",    "bio_heart",   "blood_circulation", "exp_heart_title",   DifficultyLevel.Easy,   60f,  new[]{"hint_heart_1","hint_heart_2"},         new InstructionType[0]);
            var pdArter   = MakePuzzle(PuzzleBio, "PuzzleDef_ArterialFlow", "bio_arterial","blood_circulation", "exp_arterial_title",DifficultyLevel.Easy,   60f,  new[]{"hint_arterial_1","hint_arterial_2"},   new InstructionType[0]);
            var pdCap     = MakePuzzle(PuzzleBio, "PuzzleDef_Capillary",    "bio_cap",     "blood_circulation", "exp_cap_title",     DifficultyLevel.Medium, 90f,  new[]{"hint_cap_1","hint_cap_2"},             new InstructionType[0]);
            var pdDiaph   = MakePuzzle(PuzzleBio, "PuzzleDef_Diaphragm",    "bio_diaph",   "breathing",         "exp_diaph_title",   DifficultyLevel.Easy,   45f,  new[]{"hint_diaph_1","hint_diaph_2"},         new InstructionType[0]);
            var pdGas     = MakePuzzle(PuzzleBio, "PuzzleDef_GasExchange",  "bio_gas",     "breathing",         "exp_gas_title",     DifficultyLevel.Medium, 90f,  new[]{"hint_gas_1","hint_gas_2"},             new InstructionType[0]);
            var pdLungE   = MakePuzzle(PuzzleBio, "PuzzleDef_LungExpansion","bio_lung",    "breathing",         "exp_lung_title",    DifficultyLevel.Easy,   60f,  new[]{"hint_lung_1","hint_lung_2"},           new InstructionType[0]);

            // ④ PuzzleDefinitions — Code World (with CorrectSequence)
            var pdSeq1  = MakePuzzle(PuzzleCW, "PuzzleDef_CW_Seq1",  "cw_seq_1",  "code_world", "cw_seq1_title",  DifficultyLevel.Easy,   45f, new[]{"hint_cw_seq1_1","hint_cw_seq1_2"},
                new[]{InstructionType.MoveForward, InstructionType.MoveForward, InstructionType.TurnRight, InstructionType.MoveForward});

            var pdSeq2  = MakePuzzle(PuzzleCW, "PuzzleDef_CW_Seq2",  "cw_seq_2",  "code_world", "cw_seq2_title",  DifficultyLevel.Easy,   60f, new[]{"hint_cw_seq2_1","hint_cw_seq2_2"},
                new[]{InstructionType.MoveForward, InstructionType.TurnLeft, InstructionType.MoveForward, InstructionType.MoveForward, InstructionType.TurnRight, InstructionType.PickUp});

            var pdLoop1 = MakePuzzle(PuzzleCW, "PuzzleDef_CW_Loop1", "cw_loop_1", "code_world", "cw_loop1_title", DifficultyLevel.Medium, 60f, new[]{"hint_cw_loop1_1","hint_cw_loop1_2"},
                new[]{InstructionType.MoveForward});

            var pdLoop2 = MakePuzzle(PuzzleCW, "PuzzleDef_CW_Loop2", "cw_loop_2", "code_world", "cw_loop2_title", DifficultyLevel.Medium, 90f, new[]{"hint_cw_loop2_1"},
                new[]{InstructionType.TurnRight, InstructionType.MoveForward});

            var pdCond1 = MakePuzzle(PuzzleCW, "PuzzleDef_CW_Cond1", "cw_cond_1", "code_world", "cw_cond1_title", DifficultyLevel.Hard,   90f, new[]{"hint_cw_cond_1","hint_cw_cond_2"},
                new InstructionType[0]);

            // ⑤ Condition Scenarios
            MakeScenario("Scenario_LockedDoor", "locked_door", "scenario_door_desc", true,  "scenario_door_condition", ActionType.PickUpKey,   ActionType.WalkThrough);
            MakeScenario("Scenario_RainyDay",   "rainy_day",   "scenario_rain_desc", false, "scenario_rain_condition", ActionType.Wait,         ActionType.WalkThrough);
            MakeScenario("Scenario_OpenGate",   "open_gate",   "scenario_gate_desc", true,  "scenario_gate_condition", ActionType.OpenDoor,    ActionType.WalkThrough);

            // ⑥ Module Definitions
            MakeModule("ModuleDef_CodeWorld",
                "code_world", "module_codeworld_title", "module_codeworld_desc",
                "Rwanda ICT P3–P4 – Sequencing, Loops, Conditions",
                AgeGroup.Group6to9, 0.6f,
                new[]{"obj_cw_sequence","obj_cw_loop","obj_cw_condition"},
                new[]{ pdSeq1, pdSeq2, pdLoop1, pdLoop2, pdCond1 });

            MakeModule("ModuleDef_SimpleMachines",
                "simple_machines", "module_simplemachines_title", "module_simplemachines_desc",
                "Rwanda Science P5–P6 – Force and Motion",
                AgeGroup.Group10to12, 0.6f,
                new[]{"obj_sm_lever","obj_sm_pulley","obj_sm_incline"},
                new[]{ pdLever, pdPulley, pdIncline });

            MakeModule("ModuleDef_BloodCirculation",
                "blood_circulation", "module_bloodcirc_title", "module_bloodcirc_desc",
                "Rwanda Biology P4–P5 – Human Body Systems",
                AgeGroup.Group10to12, 0.6f,
                new[]{"obj_bc_heart","obj_bc_arteries","obj_bc_capillaries"},
                new[]{ pdHeart, pdArter, pdCap });

            MakeModule("ModuleDef_Breathing",
                "breathing", "module_breathing_title", "module_breathing_desc",
                "Rwanda Biology P4–P5 – Respiratory System",
                AgeGroup.Group10to12, 0.6f,
                new[]{"obj_br_diaphragm","obj_br_gasexchange","obj_br_lungs"},
                new[]{ pdDiaph, pdGas, pdLungE });

            // ⑦ Course Data
            MakeCourse("CourseDef_CodeWorld",
                "code_world", "Code World", "Isi ya Code",
                "Primary 3–4 · ICT",
                "Walk through a VR town and solve programming puzzles!",
                "Spacer mu mugi wa VR ukemure ibibazo by'porogaramu!",
                new Color(0.29f, 0.56f, 0.89f), 1,
                new SimulationData[0]);   // CodeWorld uses ConceptBuildings, not SimDef

            MakeCourse("CourseDef_SimpleMachines",
                "simple_machines", "Simple Machines", "Ibikoresho Byoroheje",
                "Primary 5–6 · Science",
                "Explore levers, pulleys and inclined planes in a hands-on VR lab.",
                "Suzuma inkoni, iguruka n'inzira yagenewe mu laboratoire ya VR.",
                new Color(0.91f, 0.63f, 0.13f), 2,
                new[]{ simLever, simPulley, simIncline });

            MakeCourse("CourseDef_BloodCirculation",
                "blood_circulation", "Blood Circulation", "Inzira y'Amaraso",
                "Primary 4–5 · Biology",
                "Explore how the heart pumps blood through arteries and capillaries.",
                "Suzuma uburyo umutima usukura amaraso mu miruka n'imiruka mike.",
                new Color(0.75f, 0.22f, 0.17f), 3,
                new[]{ simHeart, simArterial, simCap });

            MakeCourse("CourseDef_BreathingMechanics",
                "breathing_mechanics", "Breathing Mechanics", "Imiterere y'Guhumeka",
                "Primary 4–5 · Biology",
                "Discover how lungs and the diaphragm bring oxygen into your body.",
                "Menya uburyo ibihaha n'ingingo y'umwuka bizana oxygen mu mubiri.",
                new Color(0.15f, 0.68f, 0.38f), 4,
                new[]{ simDiaph, simGas, simLung });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ScriptableObjectFactory] All ScriptableObjects created/updated.");
        }

        // ── Builders ─────────────────────────────────────────────

        static AppSettings MakeAppSettings()
        {
            EnsureDir(ResDir);
            string path = $"{ResDir}/AppSettings.asset";
            var so = Fetch<AppSettings>(path);
            so.MaxVRSessionMinutes   = 25f;
            so.WarningAtMinutesLeft  = 5f;
            so.DifficultyWindowSize  = 5;
            so.UpThreshold           = 0.80f;
            so.DownThreshold         = 0.40f;
            so.DefaultLanguage       = Language.Kinyarwanda;
            so.TargetFrameRate       = 72;
            so.ReduceTexturesOnLowRam = true;
            so.EnableAnalytics       = true;
            so.EnableCloudSync       = true;
            EditorUtility.SetDirty(so);
            return so;
        }

        static CharacterDefinition MakeCharacter(string id, string nameKey)
        {
            EnsureDir(CharDir);
            var so = Fetch<CharacterDefinition>($"{CharDir}/CharDef_{id}.asset");
            so.CharacterId = id;
            so.NameKey     = nameKey;
            EditorUtility.SetDirty(so);
            return so;
        }

        static SimulationData MakeSimData(string assetName, string id,
            string titleEN, string titleRW,
            string descEN,  string descRW,
            int badgeNum, string sceneName)
        {
            var so = Fetch<SimulationData>($"{ModuleDir}/{assetName}.asset");
            so.SimulationId  = id;
            so.TitleEN       = titleEN;
            so.TitleRW       = titleRW;
            so.DescriptionEN = descEN;
            so.DescriptionRW = descRW;
            so.BadgeNumber   = badgeNum;
            so.SceneName     = sceneName;
            so.IsLocked      = false;
            EditorUtility.SetDirty(so);
            return so;
        }

        static PuzzleDefinition MakePuzzle(string dir, string assetName, string puzzleId, string moduleId,
            string titleKey, DifficultyLevel diff, float threeStarTime,
            string[] hints, InstructionType[] sequence)
        {
            EnsureDir(dir);
            var asset = Fetch<PuzzleDefinition>($"{dir}/{assetName}.asset");
            asset.PuzzleId          = puzzleId;
            asset.ModuleId          = moduleId;
            asset.TitleKey          = titleKey;
            asset.Difficulty        = diff;
            asset.ThreeStarTimeLimit = threeStarTime;
            asset.Hints             = new List<string>(hints);
            asset.CorrectSequence   = new List<InstructionType>(sequence);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static ConditionScenario MakeScenario(string assetName, string id,
            string descKey, bool conditionIsTrue, string conditionDescKey,
            ActionType correctIf, ActionType correctElse)
        {
            EnsureDir(ScenarioDir);
            var so = Fetch<ConditionScenario>($"{ScenarioDir}/{assetName}.asset");
            so.ScenarioId             = id;
            so.DescriptionKey         = descKey;
            so.ConditionIsTrue        = conditionIsTrue;
            so.ConditionDescriptionKey = conditionDescKey;
            so.CorrectIfAction        = correctIf;
            so.CorrectElseAction      = correctElse;
            EditorUtility.SetDirty(so);
            return so;
        }

        static ModuleDefinition MakeModule(string assetName,
            string moduleId, string titleKey, string descKey,
            string curriculum, AgeGroup minAge, float passThreshold,
            string[] objectives, PuzzleDefinition[] puzzles)
        {
            var so = Fetch<ModuleDefinition>($"{ModuleDir}/{assetName}.asset");
            so.ModuleId              = moduleId;
            so.TitleKey              = titleKey;
            so.DescriptionKey        = descKey;
            so.CurriculumAlignment   = curriculum;
            so.MinAgeGroup           = minAge;
            so.PassThreshold         = passThreshold;
            so.LearningObjectiveKeys = new List<string>(objectives);
            so.Puzzles               = new List<PuzzleDefinition>(puzzles);
            EditorUtility.SetDirty(so);
            return so;
        }

        static CourseData MakeCourse(string assetName, string courseId,
            string titleEN, string titleRW, string unitLabel,
            string descEN, string descRW,
            Color cardColor, int badgeNum, SimulationData[] sims)
        {
            EnsureDir(CourseDir);
            var so = Fetch<CourseData>($"{CourseDir}/{assetName}.asset");
            so.CourseId      = courseId;
            so.TitleEN       = titleEN;
            so.TitleRW       = titleRW;
            so.UnitLabel     = unitLabel;
            so.DescriptionEN = descEN;
            so.DescriptionRW = descRW;
            so.CardColor     = cardColor;
            so.BadgeNumber   = badgeNum;
            so.Simulations   = new List<SimulationData>(sims);
            EditorUtility.SetDirty(so);
            return so;
        }

        // ── Utilities ────────────────────────────────────────────

        /// Load existing or create new ScriptableObject at path.
        static T Fetch<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var inst = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(inst, path);
            return inst;
        }

        static void EnsureDirectories()
        {
            EnsureDir(ResDir);
            EnsureDir(CharDir);
            EnsureDir(CourseDir);
            EnsureDir(ModuleDir);
            EnsureDir(PuzzleRoot);
            EnsureDir(PuzzleSM);
            EnsureDir(PuzzleBio);
            EnsureDir(PuzzleCW);
            EnsureDir(ScenarioDir);
        }

        static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string[] parts = path.Split('/');
            string   cur   = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
