using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using Unity.VRTemplate;
using VRLearning.Simulation;
using VRLearning.Simulation.SimpleMachines;
using VRLearning.UI;

namespace VRLearning.Editor
{
    public static class SimpleMachinesSetup
    {
        const string LeverScene    = "Assets/Scenes/Experiments/SimpleMachines_Lever.unity";
        const string PulleyScene   = "Assets/Scenes/Experiments/SimpleMachines_Pulley.unity";
        const string InclineScene  = "Assets/Scenes/Experiments/SimpleMachines_InclinedPlane.unity";
        const string SODir         = "Assets/ScriptableObjects/Puzzles/SimpleMachines";
        const string PhysicsDir    = "Assets/Physics";
        const string StarPrefabPath = "Assets/Prefabs/UI/StarDisplay.prefab";
        const string WhiteboardPrefabPath = "Assets/Prefabs/UI/FormulaWhiteboard.prefab";
        const string WireRopePrefabPath   = "Assets/Prefabs/Simulation/WireRope.prefab";

        // ─────────────────────────────────────────────────────────
        // MENU ITEMS
        // ─────────────────────────────────────────────────────────

        [MenuItem("Tools/Simple Machines/Setup All Scenes")]
        public static void SetupAll()
        {
            EnsureDirectories();
            CreatePhysicsMaterials();
            CreatePuzzleDefinitions();
            SetupLever();
            SetupPulley();
            SetupInclinedPlane();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SimpleMachinesSetup] All three scenes wired up.");
        }

        [MenuItem("Tools/Simple Machines/1 - Lever Scene Only")]
        public static void SetupLever()
        {
            EnsureDirectories();
            CreatePhysicsMaterials();
            var wbMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>($"{PhysicsDir}/WeightBlock.physicMaterial");

            var scene = EditorSceneManager.OpenScene(LeverScene, OpenSceneMode.Single);

            var plank = GameObject.Find("LeverPlank");
            if (plank == null) { Debug.LogError("[Setup] 'LeverPlank' not found in lever scene."); return; }

            // Rigidbody
            var rb = GetOrAdd<Rigidbody>(plank);
            rb.mass            = 0.5f;
            rb.linearDamping   = 2f;
            rb.angularDamping  = 3f;
            rb.useGravity      = true;
            // No RigidbodyConstraints here: the HingeJoint below (fixed world anchor, no
            // connectedBody) already fully constrains position and 2 of the 3 rotation axes by
            // itself. Adding redundant RigidbodyConstraints on top of that over-constrains PhysX's
            // solver and was found to fully lock even the joint's supposedly-free rotation axis —
            // the plank would never move no matter how much torque was applied.
            rb.constraints     = RigidbodyConstraints.None;

            // HingeJoint — pivot at plank's world position
            var hinge = GetOrAdd<HingeJoint>(plank);
            hinge.anchor = Vector3.zero;
            hinge.axis   = Vector3.forward;
            hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = plank.transform.position; // world pivot
            hinge.useLimits = true;
            hinge.limits    = new JointLimits { min = -45f, max = 45f };

            var leverCtrl = GetOrAdd<LeverController>(plank);
            var leverPuzz = GetOrAdd<LeverPuzzleController>(plank);

            // Snap notches along both arms — the child changes the moment arm (distance) by
            // choosing a notch, and the balance + formula update live. Clean up ANY previous
            // notch/zone layout first (names and arm values have changed across setup revisions).
            DestroyChildrenByPrefix(plank, "SnapZone_");

            // The plank is a thin board (localScale.y is small, e.g. 0.07), and a snap zone's
            // localPosition.y is expressed in the PLANK's local space — Unity multiplies it by
            // the plank's scale to get the world offset. A flat "0.05" local offset therefore
            // barely lifts anything (0.05 * 0.07 ≈ 0.0035 world units), which is why blocks used
            // to sit embedded in the beam instead of resting on top of it. Compute the local
            // offset that actually places a WeightBlock's bottom face on the plank's top surface.
            const float PlankLocalHalfExtent    = 0.5f;  // unit-cube mesh half-size before scale
            const float BlockWorldHalfHeight    = 0.1f;  // matches CreateWeightBlock's 0.2 cube / 2
            const float BlockWorldHalfWidth     = 0.1f;  // matches CreateWeightBlock's 0.2 cube / 2
            float plankScaleY = Mathf.Max(0.0001f, plank.transform.localScale.y);
            float plankScaleX = Mathf.Max(0.0001f, plank.transform.localScale.x);
            float notchLocalY = PlankLocalHalfExtent + (BlockWorldHalfHeight / plankScaleY);

            // The beam's visible mesh only spans local X in [-0.5, 0.5] (a unit cube scaled by
            // plankScaleX) — a snap notch placed past that, in the SAME local-space convention a
            // child's localPosition uses, floats off the end of the beam instead of resting on
            // it. Keep every notch (plus half the block's own width) within that visible extent.
            float maxSafeArmLocalX = PlankLocalHalfExtent - (BlockWorldHalfWidth / plankScaleX);

            var leverZones = new List<WeightSnapZone>();
            float[] notchArms = { maxSafeArmLocalX * 0.33f, maxSafeArmLocalX * 0.67f, maxSafeArmLocalX };
            foreach (float arm in notchArms)
            {
                foreach (int sign in new[] { -1, 1 })
                {
                    float x = arm * sign;
                    string zoneName = $"SnapZone_{(sign < 0 ? "L" : "R")}{Mathf.RoundToInt(arm * 100)}";
                    var zGo = GetOrCreateChild(plank, zoneName);
                    zGo.transform.localPosition = new Vector3(x, notchLocalY, 0f);
                    zGo.transform.localRotation = Quaternion.identity;
                    var sz = GetOrAdd<WeightSnapZone>(zGo);
                    var sc = GetOrAdd<SphereCollider>(zGo);
                    sc.isTrigger = true;
                    sc.radius    = 0.12f;
                    leverZones.Add(sz);
                }
            }

            // Force arrows
            var arrowL = CreateForceArrow(plank, "ForceArrow_Left",  new Vector3(-0.7f, 0.15f, 0f));
            var arrowR = CreateForceArrow(plank, "ForceArrow_Right", new Vector3( 0.7f, 0.15f, 0f));

            // Weight blocks (world space, start above snap zones)
            var wbLight = CreateWeightBlock("WeightBlock_Light",
                new Vector3(-0.85f, 0.95f, 2f), 0.5f, 1f, new Color(0.3f, 0.6f, 1f), wbMat);
            var wbHeavy = CreateWeightBlock("WeightBlock_Heavy",
                new Vector3( 0.85f, 0.95f, 2f), 1.5f, 3f, new Color(0.9f, 0.3f, 0.2f), wbMat);

            // Bilingual labels
            CreateMachineLabel("Label_Effort",  new Vector3(-1.0f, 1.15f, 2f), "lbl_effort",  "lbl_effort");
            CreateMachineLabel("Label_Load",    new Vector3( 1.0f, 1.15f, 2f), "lbl_load",    "lbl_load");
            CreateMachineLabel("Label_Fulcrum", new Vector3( 0.0f, 1.05f, 2f), "lbl_fulcrum", "lbl_fulcrum");

            // Success particles
            var psGo = GetOrCreateWorld("SuccessParticles", new Vector3(0f, 1.5f, 2f));
            var ps   = GetOrAdd<ParticleSystem>(psGo);
            ConfigureSuccessParticles(ps);

            // Star display
            SpawnStarDisplay(new Vector3(0f, 2.3f, 2f));

            // DifficultyAdapter
            GetOrAdd<DifficultyAdapter>(GetOrCreateWorld("DifficultyAdapter", new Vector3(10f, 0f, 0f)));

            // Wire LeverController
            using (var so = new SerializedObjectScope(leverCtrl))
            {
                var zonesProp = so.Prop("zones");
                zonesProp.arraySize = leverZones.Count;
                for (int i = 0; i < leverZones.Count; i++)
                    zonesProp.GetArrayElementAtIndex(i).objectReferenceValue = leverZones[i];

                so.Prop("leverRenderer").objectReferenceValue     = plank.GetComponent<MeshRenderer>();
                so.Prop("successParticles").objectReferenceValue  = ps;
                so.Prop("leftArrow").objectReferenceValue         = arrowL;
                so.Prop("rightArrow").objectReferenceValue        = arrowR;
            }

            // Colour-matched formula whiteboard, hovering above the lever.
            var leverBoard = CreateFormulaWhiteboard("FormulaBoard_Lever",
                new Vector3(0f, 2.6f, 2f), out var lbTitle, out var lbFormula, out var lbResult);
            var leverHud = GetOrAdd<LeverFormulaHUD>(leverBoard);
            using (var so = new SerializedObjectScope(leverHud))
            {
                so.Prop("lever").objectReferenceValue        = leverCtrl;
                so.Prop("titleLabel").objectReferenceValue   = lbTitle;
                so.Prop("formulaLabel").objectReferenceValue = lbFormula;
                so.Prop("resultLabel").objectReferenceValue  = lbResult;
            }

            // Wire LeverPuzzleController
            var leverDef = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>($"{SODir}/PuzzleDef_Lever.asset");
            using (var so = new SerializedObjectScope(leverPuzz))
            {
                so.Prop("leverController").objectReferenceValue = leverCtrl;
                so.Prop("puzzleData").objectReferenceValue      = leverDef;
                // requiredTippedSide enum int: 0=Left (default) — left tilts when heavy block on right
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SimpleMachinesSetup] Lever scene done.");
        }

        [MenuItem("Tools/Simple Machines/2 - Pulley Scene Only")]
        public static void SetupPulley()
        {
            EnsureDirectories();
            CreatePhysicsMaterials();

            var scene = EditorSceneManager.OpenScene(PulleyScene, OpenSceneMode.Single);

            var wheel = GameObject.Find("Wheel");
            if (wheel == null) { Debug.LogError("[Setup] 'Wheel' not found in pulley scene."); return; }

            // Wheel Rigidbody — spins on local X, position frozen
            var wheelRb = GetOrAdd<Rigidbody>(wheel);
            wheelRb.mass           = 0.3f;
            wheelRb.linearDamping  = 0.5f;
            wheelRb.angularDamping = 2f;
            wheelRb.useGravity     = false;
            // No RigidbodyConstraints: the HingeJoint below (fixed world anchor, no connectedBody)
            // already fully constrains position and 2 of 3 rotation axes. Stacking redundant
            // RigidbodyConstraints on top of that was found (on the lever) to over-constrain
            // PhysX's solver and fully lock even the joint's supposedly-free rotation axis.
            wheelRb.constraints    = RigidbodyConstraints.None;

            // HingeJoint on wheel — rotates around X axis
            var hinge = GetOrAdd<HingeJoint>(wheel);
            hinge.anchor = Vector3.zero;
            hinge.axis   = Vector3.right;
            hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = wheel.transform.position;

            var pulleyCtrl = GetOrAdd<PulleyController>(wheel);
            var pulleyPuzz = GetOrAdd<PulleyPuzzleController>(wheel);

            // Pulley weight — constrained to vertical rail via Rigidbody constraints
            var pwGo = GetOrCreateWorld("PulleyWeight", new Vector3(0.3f, 0.5f, 2f));
            SetupPrimitiveMesh(pwGo, PrimitiveType.Cube, new Vector3(0.25f, 0.25f, 0.25f),
                new Color(0.6f, 0.4f, 0.1f));
            var pwRb = GetOrAdd<Rigidbody>(pwGo);
            pwRb.mass        = 1f;
            pwRb.useGravity  = true;
            pwRb.constraints = RigidbodyConstraints.FreezePositionX
                             | RigidbodyConstraints.FreezePositionZ
                             | RigidbodyConstraints.FreezeRotationX
                             | RigidbodyConstraints.FreezeRotationY
                             | RigidbodyConstraints.FreezeRotationZ;

            // Rope handle — grabbed by player to pull rope
            var rhGo = GetOrCreateWorld("RopeHandle", new Vector3(0.3f, 1.8f, 2f));
            SetupPrimitiveMesh(rhGo, PrimitiveType.Cylinder, new Vector3(0.05f, 0.08f, 0.05f), Color.gray);
            var rhRb = GetOrAdd<Rigidbody>(rhGo);
            rhRb.isKinematic = true;
            GetOrAdd<XRGrabInteractable>(rhGo);
            var ropeHandle = GetOrAdd<RopeHandle>(rhGo);

            // LineRenderer as child of wheel
            var lrGo = GetOrCreateChild(wheel, "RopeRenderer");
            lrGo.transform.localPosition = Vector3.zero;
            var lr = GetOrAdd<LineRenderer>(lrGo);
            lr.positionCount = 3;
            lr.startWidth    = 0.02f;
            lr.endWidth      = 0.02f;
            lr.useWorldSpace = true;
            // Default material is fine; positions set at runtime

            // Target zone — visual disc at height determined by difficulty at runtime
            var tzGo = GetOrCreateWorld("TargetZone", new Vector3(0.3f, 1.0f, 2f));
            SetupPrimitiveMesh(tzGo, PrimitiveType.Cylinder, new Vector3(0.5f, 0.01f, 0.5f),
                new Color(0.1f, 0.9f, 0.2f, 0.6f));

            // Labels
            CreateMachineLabel("Label_Pull", new Vector3(0.7f, 1.5f, 2f), "lbl_pull", "lbl_pull");

            // Distance-to-target label as child of weight
            var distLabelGo = GetOrCreateChild(pwGo, "Label_DistToTarget");
            distLabelGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            distLabelGo.transform.localRotation = Quaternion.identity;
            var distTmp = GetOrAdd<TextMeshPro>(distLabelGo);
            distTmp.text      = "0.00 m";
            distTmp.fontSize  = 0.3f;
            distTmp.alignment = TextAlignmentOptions.Center;
            var distLabel = GetOrAdd<MachineLabel>(distLabelGo);

            SpawnStarDisplay(new Vector3(0f, 2.5f, 2f));
            GetOrAdd<DifficultyAdapter>(GetOrCreateWorld("DifficultyAdapter", new Vector3(10f, 0f, 0f)));

            // Wire PulleyController
            using (var so = new SerializedObjectScope(pulleyCtrl))
            {
                so.Prop("wheelTransform").objectReferenceValue = wheel.transform;
                so.Prop("weightBlock").objectReferenceValue    = pwGo.transform;
                so.Prop("ropeHandle").objectReferenceValue     = rhGo.transform;
                so.Prop("ropeRenderer").objectReferenceValue   = lr;
                so.Prop("distanceLabel").objectReferenceValue  = distLabel;
            }

            // Wire RopeHandle
            using (var so = new SerializedObjectScope(ropeHandle))
            {
                so.Prop("wheelTransform").objectReferenceValue = wheel.transform;
            }

            // Wire PulleyPuzzleController
            var pulleyDef = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>($"{SODir}/PuzzleDef_Pulley.asset");
            using (var so = new SerializedObjectScope(pulleyPuzz))
            {
                so.Prop("pulleyController").objectReferenceValue = pulleyCtrl;
                so.Prop("targetZone").objectReferenceValue       = tzGo.transform;
                so.Prop("puzzleData").objectReferenceValue       = pulleyDef;
            }

            // ── Real physics rope (Wire.cs) replacing the cosmetic LineRenderer ──────────────
            // Start = the RopeHandle itself, so the rope visibly starts at the point the player
            // grabs and follows it as they pull it up/down its track; End = an anchor that rides
            // on the load (a child empty with no Rigidbody, so the wire follows the load without
            // physically fighting PulleyController's height drive). Generous slack keeps it stable.
            var strayAnchor = GameObject.Find("WireStartAnchor");
            if (strayAnchor != null) Object.DestroyImmediate(strayAnchor); // leftover from an earlier revision

            var wireEnd = GetOrCreateChild(pwGo, "WireEndAnchor");
            wireEnd.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            float ropeGap = Mathf.Max(0.6f, rhGo.transform.position.y - wireEnd.transform.position.y);
            var wireRope = InstantiateWireRope("WireRope", rhGo.transform, wireEnd.transform, ropeGap * 1.15f);
            // Hide the old cosmetic rope — the Wire draws the rope now.
            lr.enabled = false;

            // Pulley formula whiteboard (live lift height).
            var pulleyBoard = CreateFormulaWhiteboard("FormulaBoard_Pulley",
                new Vector3(-1.0f, 2.2f, 2f), out var pbTitle, out var pbFormula, out var pbResult);
            var pulleyHud = GetOrAdd<PulleyFormulaHUD>(pulleyBoard);
            using (var so = new SerializedObjectScope(pulleyHud))
            {
                so.Prop("pulley").objectReferenceValue       = pulleyCtrl;
                so.Prop("titleLabel").objectReferenceValue   = pbTitle;
                so.Prop("formulaLabel").objectReferenceValue = pbFormula;
                so.Prop("resultLabel").objectReferenceValue  = pbResult;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SimpleMachinesSetup] Pulley scene done.");
        }

        [MenuItem("Tools/Simple Machines/3 - Inclined Plane Scene Only")]
        public static void SetupInclinedPlane()
        {
            EnsureDirectories();
            CreatePhysicsMaterials();
            var rampMat  = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>($"{PhysicsDir}/RampSurface.physicMaterial");
            var blockMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>($"{PhysicsDir}/BlockDefault.physicMaterial");

            var scene = EditorSceneManager.OpenScene(InclineScene, OpenSceneMode.Single);

            var ramp = GameObject.Find("Ramp");
            if (ramp == null) { Debug.LogError("[Setup] 'Ramp' not found in inclined plane scene."); return; }

            // Apply physics material to ramp collider
            var rampColl = ramp.GetComponent<Collider>() ?? GetOrAdd<BoxCollider>(ramp);
            rampColl.material = rampMat;

            var planeCtrl = GetOrAdd<InclinedPlaneController>(ramp);
            var planePuzz = GetOrAdd<InclinedPlanePuzzleController>(ramp);

            // Sliding block
            var sbGo = GetOrCreateWorld("SlidingBlock", new Vector3(0f, 0.8f, 1.0f));
            SetupPrimitiveMesh(sbGo, PrimitiveType.Cube, new Vector3(0.2f, 0.2f, 0.2f),
                new Color(0.2f, 0.5f, 0.9f));
            var sbColl = sbGo.GetComponent<Collider>() ?? GetOrAdd<BoxCollider>(sbGo);
            sbColl.material = blockMat;
            var sbRb = GetOrAdd<Rigidbody>(sbGo);
            sbRb.mass                   = 1f;
            sbRb.useGravity             = true;
            sbRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            GetOrAdd<XRGrabInteractable>(sbGo);

            // Spawn point child for resetting block position
            var spawnGo = GetOrCreateChild(sbGo, "RampTopSpawnPoint");
            spawnGo.transform.localPosition = Vector3.zero;
            spawnGo.transform.localRotation = Quaternion.identity;

            // Friction dial (XRKnob + FrictionDial)
            var dialGo = GetOrCreateWorld("FrictionDial", new Vector3(0.9f, 1.2f, 2f));
            SetupPrimitiveMesh(dialGo, PrimitiveType.Cylinder,
                new Vector3(0.12f, 0.06f, 0.12f), new Color(0.25f, 0.25f, 0.25f));
            var knob = GetOrAdd<XRKnob>(dialGo);
            using (var so = new SerializedObjectScope(knob))
            {
                so.Prop("m_Value").floatValue        = 0.5f;
                so.Prop("m_ClampedMotion").boolValue = true;
                so.Prop("m_MinAngle").floatValue     = -135f;
                so.Prop("m_MaxAngle").floatValue     =  135f;
            }
            var frictionDial = GetOrAdd<FrictionDial>(dialGo);

            // Friction labels
            CreateMachineLabel("Label_Friction", new Vector3(1.2f, 1.5f, 2f), "lbl_friction", "lbl_friction");
            var fvLabelGo = GetOrCreateWorld("Label_FrictionValue", new Vector3(0.9f, 0.95f, 2f));
            var fvTmp = GetOrAdd<TextMeshPro>(fvLabelGo);
            fvTmp.text      = "μ = 0.50";
            fvTmp.fontSize  = 0.35f;
            fvTmp.alignment = TextAlignmentOptions.Center;
            var fvLabel = GetOrAdd<MachineLabel>(fvLabelGo);

            // Ground collider physics material
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                var gc = ground.GetComponent<Collider>();
                if (gc != null) gc.material = blockMat;
            }

            SpawnStarDisplay(new Vector3(0f, 2.5f, 2f));
            GetOrAdd<DifficultyAdapter>(GetOrCreateWorld("DifficultyAdapter", new Vector3(10f, 0f, 0f)));

            // Wire InclinedPlaneController
            using (var so = new SerializedObjectScope(planeCtrl))
            {
                so.Prop("slidingBlock").objectReferenceValue       = sbRb;
                so.Prop("rampMaterial").objectReferenceValue       = rampMat;
                so.Prop("blockMaterial").objectReferenceValue      = blockMat;
                so.Prop("rampTopSpawnPoint").objectReferenceValue  = spawnGo.transform;
                so.Prop("frictionValueLabel").objectReferenceValue = fvLabel;
            }

            // Wire FrictionDial
            using (var so = new SerializedObjectScope(frictionDial))
            {
                so.Prop("planeController").objectReferenceValue = planeCtrl;
            }

            // Wire InclinedPlanePuzzleController
            var inclineDef = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>($"{SODir}/PuzzleDef_Incline.asset");
            using (var so = new SerializedObjectScope(planePuzz))
            {
                so.Prop("planeController").objectReferenceValue = planeCtrl;
                so.Prop("puzzleData").objectReferenceValue      = inclineDef;
            }

            // Inclined-plane formula whiteboard (MA constant + live friction μ).
            var inclineBoard = CreateFormulaWhiteboard("FormulaBoard_Incline",
                new Vector3(-1.2f, 2.2f, 2f), out var ibTitle, out var ibFormula, out var ibResult);
            var inclineHud = GetOrAdd<InclinedPlaneFormulaHUD>(inclineBoard);
            using (var so = new SerializedObjectScope(inclineHud))
            {
                so.Prop("plane").objectReferenceValue        = planeCtrl;
                so.Prop("titleLabel").objectReferenceValue   = ibTitle;
                so.Prop("formulaLabel").objectReferenceValue = ibFormula;
                so.Prop("resultLabel").objectReferenceValue  = ibResult;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SimpleMachinesSetup] Inclined Plane scene done.");
        }

        // ─────────────────────────────────────────────────────────
        // ASSET CREATION
        // ─────────────────────────────────────────────────────────

        static void EnsureDirectories()
        {
            EnsureDir("Assets/ScriptableObjects");
            EnsureDir("Assets/ScriptableObjects/Puzzles");
            EnsureDir(SODir);
            EnsureDir(PhysicsDir);
        }

        static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts   = path.Split('/');
            string cur  = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        static void CreatePhysicsMaterials()
        {
            MakePhysicsMat($"{PhysicsDir}/RampSurface.physicMaterial",  0.5f, 0f, PhysicsMaterialCombine.Minimum);
            MakePhysicsMat($"{PhysicsDir}/BlockDefault.physicMaterial", 0.5f, 0f, PhysicsMaterialCombine.Minimum);
            MakePhysicsMat($"{PhysicsDir}/WeightBlock.physicMaterial",  0.6f, 0f, PhysicsMaterialCombine.Average);
            AssetDatabase.SaveAssets();
        }

        static PhysicsMaterial MakePhysicsMat(string path, float friction, float bounce,
            PhysicsMaterialCombine combine)
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (mat == null)
            {
                mat = new PhysicsMaterial(Path.GetFileNameWithoutExtension(path));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.dynamicFriction = friction;
            mat.staticFriction  = friction;
            mat.bounciness      = bounce;
            mat.frictionCombine = combine;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void CreatePuzzleDefinitions()
        {
            MakePuzzleDef($"{SODir}/PuzzleDef_Lever.asset",  "sm_lever",  "simple_machines",
                new[] { "hint_lever_1", "hint_lever_2", "hint_lever_3" });
            MakePuzzleDef($"{SODir}/PuzzleDef_Pulley.asset", "sm_pulley", "simple_machines",
                new[] { "hint_pulley_1", "hint_pulley_2" });
            MakePuzzleDef($"{SODir}/PuzzleDef_Incline.asset","sm_incline","simple_machines",
                new[] { "hint_incline_1", "hint_incline_2" });
            AssetDatabase.SaveAssets();
        }

        static void MakePuzzleDef(string path, string id, string moduleId, string[] hints)
        {
            var def = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<PuzzleDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }
            def.PuzzleId = id;
            def.ModuleId = moduleId;
            def.Hints    = new List<string>(hints);
            EditorUtility.SetDirty(def);
        }

        // ─────────────────────────────────────────────────────────
        // SCENE OBJECT HELPERS
        // ─────────────────────────────────────────────────────────

        static ForceArrow CreateForceArrow(GameObject parent, string name, Vector3 localPos)
        {
            var go = GetOrCreateChild(parent, name);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;

            var shaft = GetOrCreateChild(go, "Shaft");
            SetupPrimitiveMesh(shaft, PrimitiveType.Cube, new Vector3(0.04f, 0.3f, 0.04f), Color.yellow);

            var head = GetOrCreateChild(go, "Head");
            SetupPrimitiveMesh(head, PrimitiveType.Cube, new Vector3(0.08f, 0.08f, 0.08f), Color.yellow);
            head.transform.localPosition = new Vector3(0f, 0.19f, 0f);

            var arrow = GetOrAdd<ForceArrow>(go);
            var grad  = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.red, 1f) },
                new[] { new GradientAlphaKey(1f, 0f),         new GradientAlphaKey(1f, 1f) });

            using (var so = new SerializedObjectScope(arrow))
            {
                so.Prop("arrowShaft").objectReferenceValue    = shaft.transform;
                so.Prop("arrowHead").objectReferenceValue     = head.transform;
                so.Prop("colorGradient").gradientValue        = grad;
            }
            return arrow;
        }

        static WeightBlock CreateWeightBlock(string name, Vector3 worldPos,
            float mass, float weightValue, Color color, PhysicsMaterial mat)
        {
            var go = GetOrCreateWorld(name, worldPos);
            SetupPrimitiveMesh(go, PrimitiveType.Cube, new Vector3(0.2f, 0.2f, 0.2f), color);

            var coll = go.GetComponent<Collider>() ?? GetOrAdd<BoxCollider>(go);
            if (mat != null) coll.material = mat;

            var rb  = GetOrAdd<Rigidbody>(go);
            rb.mass = mass;

            GetOrAdd<XRGrabInteractable>(go);
            var wb = GetOrAdd<WeightBlock>(go);

            using (var so = new SerializedObjectScope(wb))
            {
                so.Prop("weightValue").floatValue = weightValue;
                so.Prop("DisplayColor").colorValue = color;   // so the whiteboard can colour-match
            }

            return wb;
        }

        static MachineLabel CreateMachineLabel(string name, Vector3 worldPos, string keyEN, string keyRW)
        {
            var go  = GetOrCreateWorld(name, worldPos);
            var tmp = GetOrAdd<TextMeshPro>(go);
            tmp.text      = keyEN;
            tmp.fontSize  = 0.4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localRotation = Quaternion.identity;

            var ml = GetOrAdd<MachineLabel>(go);
            using (var so = new SerializedObjectScope(ml))
            {
                so.Prop("keyEN").stringValue = keyEN;
                so.Prop("keyRW").stringValue = keyRW;
            }
            return ml;
        }

        static void ConfigureSuccessParticles(ParticleSystem ps)
        {
            var main          = ps.main;
            main.duration     = 1f;
            main.loop         = false;
            main.startLifetime = 1f;
            main.startSpeed   = 3f;
            main.startSize    = 0.08f;
            main.startColor   = new ParticleSystem.MinMaxGradient(Color.yellow, Color.green);

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });
            ps.Stop();
        }

        static StarDisplay SpawnStarDisplay(Vector3 worldPos)
        {
            var existing = GameObject.Find("StarDisplay");
            if (existing != null) return existing.GetComponent<StarDisplay>();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StarPrefabPath);
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.name = "StarDisplay";
                inst.transform.position = worldPos;
                return inst.GetComponent<StarDisplay>();
            }

            // Fallback: bare GO with script
            var go = new GameObject("StarDisplay");
            go.transform.position = worldPos;
            return GetOrAdd<StarDisplay>(go);
        }

        static void SetupPrimitiveMesh(GameObject go, PrimitiveType type, Vector3 scale, Color color)
        {
            if (go.GetComponent<MeshFilter>() != null)
            {
                go.transform.localScale = scale;
                return;
            }

            var temp = GameObject.CreatePrimitive(type);
            go.AddComponent<MeshFilter>().sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;

            var mr  = go.AddComponent<MeshRenderer>();
            var sh  = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(sh);
            mat.color = color;
            mr.sharedMaterial = mat;

            // Copy collider type without settings (defaults are correct for primitives)
            if (go.GetComponent<Collider>() == null)
            {
                if      (temp.GetComponent<BoxCollider>()     != null) go.AddComponent<BoxCollider>();
                else if (temp.GetComponent<CapsuleCollider>() != null) go.AddComponent<CapsuleCollider>();
                else if (temp.GetComponent<SphereCollider>()  != null) go.AddComponent<SphereCollider>();
            }

            Object.DestroyImmediate(temp);
            go.transform.localScale = scale;
        }

        // ─────────────────────────────────────────────────────────
        // FORMULA WHITEBOARD  (reusable world-space Canvas prefab)
        // ─────────────────────────────────────────────────────────

        // Instantiate the blank whiteboard prefab at worldPos and hand back its three text slots.
        // The caller adds the machine-specific FormulaHUD subclass and wires these in.
        static GameObject CreateFormulaWhiteboard(string name, Vector3 worldPos,
            out TMP_Text title, out TMP_Text formula, out TMP_Text result)
        {
            var prefab = GetOrCreateWhiteboardPrefab();
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = name;
            }
            go.transform.position = worldPos;

            title   = go.transform.Find("Title").GetComponent<TMP_Text>();
            formula = go.transform.Find("Formula").GetComponent<TMP_Text>();
            result  = go.transform.Find("Result").GetComponent<TMP_Text>();
            return go;
        }

        static GameObject GetOrCreateWhiteboardPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(WhiteboardPrefabPath);
            if (existing != null) return existing;

            EnsureDir("Assets/Prefabs");
            EnsureDir("Assets/Prefabs/UI");

            var temp   = BuildWhiteboardObject("FormulaWhiteboard");
            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, WhiteboardPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        // Builds a child-friendly world-space whiteboard: cream panel + Title / Formula / Result text.
        static GameObject BuildWhiteboardObject(string name)
        {
            var go     = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440, 320);
            go.transform.localScale = Vector3.one * (0.8f / 440f);   // ~0.8 m wide board

            var panel = new GameObject("Panel");
            panel.transform.SetParent(go.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0.99f, 0.98f, 0.92f, 0.97f);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            CreateWhiteboardText(go, "Title",   new Vector2(0f, 120f),  new Vector2(420, 64),  42f,
                new Color(0.13f, 0.13f, 0.2f),  FontStyles.Bold);
            CreateWhiteboardText(go, "Formula", new Vector2(0f, 10f),   new Vector2(420, 150), 30f,
                new Color(0.10f, 0.10f, 0.12f), FontStyles.Normal);
            CreateWhiteboardText(go, "Result",  new Vector2(0f, -120f), new Vector2(420, 64),  34f,
                new Color(0.10f, 0.10f, 0.12f), FontStyles.Bold);
            return go;
        }

        static TMP_Text CreateWhiteboardText(GameObject canvasGo, string name, Vector2 anchoredPos,
            Vector2 size, float fontSize, Color color, FontStyles style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGo.transform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = style;
            tmp.richText  = true;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return tmp;
        }

        // ─────────────────────────────────────────────────────────
        // WIRE ROPE  (reusable physics-rope prefab, built on Wire.cs)
        // ─────────────────────────────────────────────────────────

        static GameObject InstantiateWireRope(string name, Transform start, Transform end, float totalLength)
        {
            var prefab = GetOrCreateWireRopePrefab();
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = name;
            }
            go.transform.position = start.position;

            var wire = go.GetComponent<Wire>();
            using (var so = new SerializedObjectScope(wire))
            {
                so.Prop("start").objectReferenceValue = start;
                so.Prop("end").objectReferenceValue   = end;
                so.Prop("totalLength").floatValue      = totalLength;
            }

            var wlr = go.GetComponent<WireLineRenderer>();
            using (var so = new SerializedObjectScope(wlr))
            {
                so.Prop("start").objectReferenceValue = start;
                so.Prop("end").objectReferenceValue   = end;
            }
            return go;
        }

        static GameObject GetOrCreateWireRopePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(WireRopePrefabPath);
            if (existing != null) return existing;

            EnsureDir("Assets/Prefabs");
            EnsureDir("Assets/Prefabs/Simulation");

            var root = new GameObject("WireRope");

            var line = root.AddComponent<LineRenderer>();
            line.startWidth        = 0.02f;
            line.endWidth          = 0.02f;
            line.numCapVertices    = 2;
            line.numCornerVertices = 2;
            line.useWorldSpace     = true;
            var ropeShader = Shader.Find("Universal Render Pipeline/Unlit")
                             ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
            line.sharedMaterial = new Material(ropeShader) { color = new Color(0.25f, 0.18f, 0.12f) };

            var segments = new GameObject("Segments");
            segments.transform.SetParent(root.transform, false);

            var wire = root.AddComponent<Wire>();
            using (var so = new SerializedObjectScope(wire))
            {
                so.Prop("segmentsParent").objectReferenceValue = segments.transform;
                so.Prop("segmentCount").intValue        = 12;
                so.Prop("totalLength").floatValue        = 1.6f;
                so.Prop("totalWeight").floatValue        = 0.4f;
                so.Prop("usePhysics").boolValue          = true;
                so.Prop("radius").floatValue             = 0.02f;
                so.Prop("drag").floatValue               = 0.6f;
                so.Prop("angularDrag").floatValue        = 1.2f;
                so.Prop("swingLimitAngle").floatValue    = 10f;
                so.Prop("colliderShape").enumValueIndex  = 1; // Capsule
            }

            var wlr = root.AddComponent<WireLineRenderer>();
            using (var so = new SerializedObjectScope(wlr))
                so.Prop("segmentsParent").objectReferenceValue = segments.transform;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, WireRopePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ─────────────────────────────────────────────────────────
        // GENERIC UTILITIES
        // ─────────────────────────────────────────────────────────

        static void DestroyChildIfExists(GameObject parent, string childName)
        {
            var t = parent.transform.Find(childName);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        static void DestroyChildrenByPrefix(GameObject parent, string namePrefix)
        {
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                var child = parent.transform.GetChild(i);
                if (child.name.StartsWith(namePrefix)) Object.DestroyImmediate(child.gameObject);
            }
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        static GameObject GetOrCreateChild(GameObject parent, string name)
        {
            var t = parent.transform.Find(name);
            if (t != null) return t.gameObject;
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        static GameObject GetOrCreateWorld(string name, Vector3 worldPos)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;
            var go = new GameObject(name);
            go.transform.position = worldPos;
            return go;
        }

        // Disposable RAII wrapper so we never forget ApplyModifiedProperties
        class SerializedObjectScope : System.IDisposable
        {
            readonly SerializedObject _so;
            public SerializedObjectScope(Object target) { _so = new SerializedObject(target); }
            public SerializedProperty Prop(string name) => _so.FindProperty(name);
            public void Dispose() => _so.ApplyModifiedProperties();
        }
    }
}
