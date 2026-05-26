# 3D Assets Specification
## VR Learning Simulation Platform — Blender Production Guide

All assets target **Android VR (3 GB RAM minimum)**:
- Poly budget per character: ≤ 2,500 tris
- Poly budget per building: ≤ 3,000 tris
- Poly budget per prop: ≤ 500 tris
- Texture resolution: 512×512 (props), 1024×1024 (characters/buildings)
- Format: FBX → Unity (apply all transforms, Y-up, 0.01 scale)
- UV-unwrapped, no overlapping UVs
- Vertex-coloured OR single-atlas-texture (no multi-material for perf)
- Rigged characters: Humanoid rig compatible with Unity Mecanim

---

## CHARACTERS

### CHR_Robot_Main
- Description: The learner's robot companion in Code World. Friendly, round, non-threatening.
  Inspired by classic toy robots. Primary blue body, yellow visor, orange accent joints.
- Poly budget: 2,200 tris
- Rig: Humanoid — spine, shoulders, arms (2 bones each), head
- Animations (separate FBX clips):
  - Idle_Loop (stand, gentle bounce, 60 frames)
  - Walk_Forward (8-frame cycle)
  - TurnLeft_90 / TurnRight_90 (20 frames each)
  - PickUp (30 frames, reach + grab)
  - Jump (25 frames)
  - Celebrate (40 frames, arms raise)
  - Error_Shake (20 frames, head shake)
- Materials: 1 atlas texture (512×512), cel-shaded style (flat colour + dark outline baked)

### CHR_Guide_NPC
- Description: Friendly town guide. Gender-neutral, African-inspired clothing (orange + green kanga),
  warm smile, oversized eyes for expressiveness (child-friendly).
- Poly budget: 2,500 tris
- Rig: Humanoid
- Animations:
  - Idle_Loop (60 frames, breathing)
  - Wave (30 frames)
  - Point_Right / Point_Left (20 frames each)
  - Celebrate (40 frames)
  - Talk_Loop (20 frames, subtle head nod for VO playback)
- Materials: 1 atlas texture (1024×1024)

### CHR_ChildAvatar_AgeGroup1 (ages 6–9)
- Description: Simple third-person avatar for menus only. Cartoon proportions, school uniform.
- Poly budget: 1,500 tris
- No rig needed (static pose for profile screen)

### CHR_ChildAvatar_AgeGroup2 (ages 10–12)
- Description: Slightly older proportions. Same school uniform.
- Poly budget: 1,500 tris

---

## ENVIRONMENT — CODE WORLD TOWN

### ENV_Town_Ground_Plane
- A 20×20m flat ground with subtle grid texture (like graph paper, softened)
- Poly budget: 4 tris (single quad)
- Texture: 512×512, tileable

### ENV_Building_SequenceHQ
- A blocky, colourful "Sequence Headquarters" building
- Design: blue walls, numbered door (1-2-3), arrow decals on sides indicating order
- Poly budget: 2,800 tris
- Has: door frame (separate mesh for door_open animation), sign board, steps
- Includes LOD0 and LOD1 (50% reduction)

### ENV_Building_LoopFactory
- A circular factory building with spinning gear on the roof (looping motion)
- Design: orange walls, repeating diamond pattern, smoke stack
- Poly budget: 2,800 tris
- Animated: roof gear (continuous rotation, Animator)

### ENV_Building_ConditionCastle
- A small castle with a drawbridge. The drawbridge is up or down based on puzzle state.
- Design: green/grey stone, flag on tower, visible locked/unlocked door
- Poly budget: 3,000 tris
- Animated: drawbridge (30-frame open/close, driven by ConditionPuzzle)

### ENV_Town_Road_Straight / ENV_Town_Road_Corner
- Cartoon cobblestone road segments (2m × 4m tiles)
- Poly budget: 8 tris each (quads with normal map)

### ENV_Town_Tree_A / ENV_Town_Tree_B
- Low-poly trees (lollipop silhouette, flat-colour green canopy)
- Poly budget: 120 tris each

### ENV_Town_Fence_Segment
- Simple wooden fence (1m wide tile, repeating)
- Poly budget: 80 tris

### ENV_Town_Lamppost
- Street lamp with warm glow
- Poly budget: 200 tris
- Has Point Light child object (range 5m, intensity 1.5)

### ENV_Town_Signpost
- Directional sign post pointing to each building
- Poly budget: 150 tris
- Text panels use TextMeshPro world-space canvases (localised at runtime)

---

## PROPS — INTERACTABLES

### PROP_InstructionBlock_Base
- The grabbable programming instruction blocks
- Shape: rounded cube, 25cm × 25cm × 8cm
- Each variant uses a colour + icon to identify instruction type:
  - PROP_Block_MoveForward  — green,   arrow-forward icon
  - PROP_Block_TurnLeft     — blue,    arrow-left icon
  - PROP_Block_TurnRight    — blue,    arrow-right icon
  - PROP_Block_PickUp       — orange,  hand icon
  - PROP_Block_Jump         — yellow,  jump icon
  - PROP_Block_Wait         — grey,    clock icon
- Poly budget: 80 tris per block (shared mesh, different material colour)
- Has: XRGrabInteractable component, Rigidbody (kinematic), BoxCollider
- Glow outline when hovered (shader-based, no extra geometry)

### PROP_InstructionSlot
- A flat square socket on the program board where blocks snap into
- Dimensions: 28cm × 28cm × 2cm
- Glows teal when a block is near, white when occupied
- Poly budget: 12 tris

### PROP_RepeatCounter_Dial
- A large rotary dial (like a cartoon safe dial) for selecting loop count 1–5
- Shows the selected number in the centre
- Poly budget: 350 tris
- Animated: rotation (driven by RepeatCountSelector)

### PROP_ConditionDoor
- A door with padlock. Padlock visible/hidden based on condition state.
- Poly budget: 400 tris
- Animated: door_open (20 frames)

### PROP_Key
- A large cartoon key (golden) that the robot picks up
- Poly budget: 180 tris
- Animated: idle_float (gentle bob, 90 frames, loop)

### PROP_ProgramBoard_Horizontal
- The flat board on which instruction blocks are placed for Simulation Builder
- Dimensions: 1.5m × 0.8m × 0.05m
- Poly budget: 12 tris
- Has 8 slot positions marked with subtle indentations

### PROP_StarPickup
- A 3D star that spins and floats above completed buildings
- Poly budget: 60 tris
- Animated: idle_spin (continuous, 120 frames)

---

## UI 3D ASSETS

### UI3D_SpeechBubble
- World-space speech bubble mesh (rounded rectangle with tail)
- Poly budget: 80 tris
- Scales with LookAt camera, driven by NPCSpeechBubble.cs
- Background texture: white with blue border (1 material)

### UI3D_ProgressRing
- A circular progress indicator rendered in world space above buildings
- Poly budget: 120 tris (segmented ring)
- Shader: unlit, segments fill green as puzzles complete

### UI3D_HintPanel
- Flat world-space panel (30cm × 20cm) that appears near the learner
- Has: TextMeshPro text, question-mark icon mesh, soft blue background
- Poly budget: 8 tris (quad)

---

## MATERIALS & SHADERS

### MAT_CelShade_Character
- Custom Unity shader: flat colour + Fresnel dark outline + 2-step diffuse ramp
- Parameters: BaseColor, OutlineThickness (0.02), RampTexture
- Used on all characters

### MAT_Building_Base
- Standard Unity URP/Lit with emission for window glow
- Parameters: BaseMap (atlas), EmissionMap, EmissionColor

### MAT_Block_Interactive
- Unlit base + outline rim glow when _HoverActive = 1
- Parameters: BaseColor, HoverColor (#00FFBB), _HoverActive (0/1), _CompletedBlend (0/1)

### MAT_Ground_GridTile
- Tileable cobblestone/grid
- Parameters: BaseMap, NormalMap, Tiling (5,5)

### MAT_UI_WorldSpace
- Unlit, always-on-top (ZTest Always)
- Used for speech bubbles and floating labels

---

## BLENDER PRODUCTION CHECKLIST (per asset)

- [ ] Origin set to base of mesh (not world origin)
- [ ] All transforms applied (Ctrl+A → All Transforms)
- [ ] Normals facing outward (Overlay → Face Orientation, all blue)
- [ ] UV unwrapped, no overlaps, 5% margin between islands
- [ ] Named correctly: CHR_ / ENV_ / PROP_ / UI3D_ prefix
- [ ] Exported as FBX with: Apply Transform ✓, Y Forward, Z Up → Unity corrects to Y-up
- [ ] Rigged meshes: armature included in same FBX, rest pose is T-pose
- [ ] Animation clips exported as separate FBX files in Animations/ subfolder
- [ ] Tested in Unity: no import errors, correct scale, pivot correct

