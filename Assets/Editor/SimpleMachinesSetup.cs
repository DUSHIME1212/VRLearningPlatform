using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            rb.constraints     = RigidbodyConstraints.FreezePositionX
                               | RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezePositionZ
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationY;

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

            // Snap zones as children of LeverPlank
            var snapLGo = GetOrCreateChild(plank, "SnapZone_Left");
            snapLGo.transform.localPosition = new Vector3(-0.9f, 0.05f, 0f);
            snapLGo.transform.localRotation = Quaternion.identity;
            var szL = GetOrAdd<WeightSnapZone>(snapLGo);
            var scL = GetOrAdd<SphereCollider>(snapLGo);
            scL.isTrigger = true;
            scL.radius    = 0.25f;

            var snapRGo = GetOrCreateChild(plank, "SnapZone_Right");
            snapRGo.transform.localPosition = new Vector3(0.9f, 0.05f, 0f);
            snapRGo.transform.localRotation = Quaternion.identity;
            var szR = GetOrAdd<WeightSnapZone>(snapRGo);
            var scR = GetOrAdd<SphereCollider>(snapRGo);
            scR.isTrigger = true;
            scR.radius    = 0.25f;

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
                so.Prop("leftZone").objectReferenceValue          = szL;
                so.Prop("rightZone").objectReferenceValue         = szR;
                so.Prop("leverRenderer").objectReferenceValue     = plank.GetComponent<MeshRenderer>();
                so.Prop("successParticles").objectReferenceValue  = ps;
                so.Prop("leftArrow").objectReferenceValue         = arrowL;
                so.Prop("rightArrow").objectReferenceValue        = arrowR;
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
            wheelRb.constraints    = RigidbodyConstraints.FreezePositionX
                                   | RigidbodyConstraints.FreezePositionY
                                   | RigidbodyConstraints.FreezePositionZ
                                   | RigidbodyConstraints.FreezeRotationY
                                   | RigidbodyConstraints.FreezeRotationZ;

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
                so.Prop("weightValue").floatValue = weightValue;

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
        // GENERIC UTILITIES
        // ─────────────────────────────────────────────────────────

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
