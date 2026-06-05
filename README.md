<div align="center">
  <h1>🌌 VRLearningPlatform</h1>
  <p><strong>Next-Generation Virtual Reality Educational Ecosystem</strong></p>

  <!-- Badges -->
  <p>
    <img src="https://img.shields.io/badge/Unity-2022.3%20LTS-black?style=for-the-badge&logo=unity" alt="Unity Version" />
    <img src="https://img.shields.io/badge/C%23-10.0-blue?style=for-the-badge&logo=c-sharp" alt="C# Version" />
    <img src="https://img.shields.io/badge/Platform-Meta%20Quest%202%20%7C%20Pro%20%7C%203-lightgrey?style=for-the-badge&logo=oculus" alt="Platform" />
    <img src="https://img.shields.io/badge/XR%20Interaction%20Toolkit-2.5.2-brightgreen?style=for-the-badge" alt="XR Toolkit" />
    <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" alt="License" />
    <img src="https://img.shields.io/badge/Build-Passing-success?style=for-the-badge&logo=githubactions" alt="Build Status" />
    <img src="https://img.shields.io/badge/Code%20Quality-A%2B-success?style=for-the-badge" alt="Code Quality" />
  </p>
</div>

---

## 📖 Table of Contents

1. [Executive Summary & Vision](#-executive-summary--vision)
2. [Architectural Overview](#-architectural-overview)
3. [Core Technologies & Stack](#-core-technologies--stack)
4. [XR Interaction & Hand Tracking](#-xr-interaction--hand-tracking)
5. [Environment & Asset Pipeline](#-environment--asset-pipeline)
6. [Screenshots & Visuals](#-screenshots--visuals)
7. [Getting Started & Installation](#-getting-started--installation)
8. [Advanced Setup & CI/CD Pipeline](#-advanced-setup--cicd-pipeline)
9. [Performance Optimization & Profiling](#-performance-optimization--profiling)
10. [Design Patterns & Code Standards](#-design-patterns--code-standards)
11. [Roadmap & Future Architecture](#-roadmap--future-architecture)
12. [Contributing Guidelines](#-contributing-guidelines)
13. [License](#-license)

---

## 🚀 Executive Summary & Vision

Welcome to the **VRLearningPlatform**, a state-of-the-art Virtual Reality educational ecosystem developed as a comprehensive **University Capstone Project**. Designed from the ground up to revolutionize digital learning, this project serves as a showcase of advanced spatial computing, software architecture, and immersive UI/UX design. Our primary goal is to leverage VR to create visceral, muscle-memory-forming educational experiences—specifically focusing on interactive physics simulations like our Simple Machines Lab.

Virtual Reality in education is no longer a gimmick; it is a profound pedagogical shift. By utilizing the latest advancements in the Unity Engine, specifically the Universal Render Pipeline (URP) and the XR Interaction Toolkit (XRI), we have crafted a foundation that supports seamless hand-tracking, complex object physics, and multi-user environments. 

This repository serves as the core monolith for our VR client applications. It encapsulates everything from low-level input abstraction layers to high-level networking protocols and rendering pipelines. Whether you are building localized training simulations or globally distributed collaborative classrooms, this architecture provides the necessary scaffolding to deliver consistent 90Hz+ experiences on standalone mobile VR hardware.

---

## 🏗 Architectural Overview

The architecture of the VRLearningPlatform strictly adheres to **SOLID principles**, prioritizing decoupling, dependency injection, and modularity. In the unpredictable landscape of XR hardware, input paradigms shift rapidly. To insulate our core logic from these shifts, we employ a robust abstraction layer.

### The Abstraction Layer Pattern

Instead of tightly coupling our interaction logic to specific vendor SDKs (e.g., Oculus Integration or SteamVR), we exclusively use Unity's **XR Interaction Toolkit (XRI)** and the **OpenXR** standard. This ensures that a single codebase can compile and run flawlessly across Meta Quest, HTC Vive XR Elite, Pico 4, and any future OpenXR-compliant headsets.

### Event-Driven Architecture (EDA)

At the heart of our systems lies a decoupled Event-Driven Architecture. We utilize ScriptableObject-based event channels to facilitate communication between disparate systems (e.g., the Input System communicating with the Audio System) without establishing hard references. This drastically reduces spaghetti code and allows designers to hook into gameplay events purely through the Unity Inspector.

### MVC / MVP Implementation in Unity

For UI and state management, we utilize a modified Model-View-Presenter (MVP) pattern. 
- **Models**: Plain C# objects (POCOs) representing the state (e.g., User Profile, Current Lesson State).
- **Views**: Unity MonoBehaviours containing references to Canvas elements, TextMeshPro fields, and VR interactable canvases.
- **Presenters**: The glue code that subscribes to Model changes and updates the Views, ensuring that Views remain incredibly "dumb" and purely presentation-focused.

---

## 🛠 Core Technologies & Stack

We have selected a technology stack that balances bleeding-edge capabilities with production-ready stability.

- **Unity Engine 2022.3 LTS**: We use the Long Term Support version to guarantee API stability throughout the development lifecycle.
- **Universal Render Pipeline (URP)**: Custom-tailored for mobile VR, our URP settings are heavily optimized to maintain a strict 72-90 FPS target on standalone headsets. We use Single Pass Instanced rendering extensively.
- **OpenXR Plugin**: The undisputed standard for cross-platform XR development.
- **XR Interaction Toolkit 2.5+**: For standardized grabbing, hovering, and locomotion mechanics.
- **XR Hands & Hand Tracking**: For controller-less interaction, utilizing the latest skeletal tracking algorithms.
- **FMOD / Wwise**: While Unity's built-in audio is functional, our architecture supports enterprise-grade spatial audio middleware for true acoustic immersion.
- **Git LFS (Large File Storage)**: Critical for maintaining repository health when dealing with high-fidelity FBX models and 4K PBR textures.

---

## ✋ XR Interaction & Hand Tracking

One of the standout features of this platform is its deeply integrated hand-tracking capabilities. We do not treat hands as mere laser pointers; we treat them as complex, multi-jointed physics objects capable of fine-motor manipulation.

### The Hand Tracking Pipeline

1. **Input Acquisition**: OpenXR reads the skeletal data from the headset's optical sensors.
2. **Filtering & Smoothing**: We apply custom Kalman filters to reduce the inevitable jitter that occurs in low-light environments.
3. **Physics Representation**: Each joint in the hand is represented by a kinematic Rigidbody and a SphereCollider. This allows hands to naturally push, poke, and collide with the virtual environment.
4. **Gesture Recognition**: A custom heuristic engine analyzes the skeletal data in real-time to detect specific gestures (e.g., Pinch, Point, Grasp, Thumbs Up) to trigger context-sensitive actions.

### Interactor and Interactable Architecture

We extend the base `XRBaseInteractor` and `XRBaseInteractable` classes to inject educational specific logic. For instance, our `EducationalGripInteractable` not only handles the physics of being picked up but also logs analytics data (e.g., how long the user held an object, the rotational angles they inspected it at) which is then batched and sent to our learning analytics backend.

---

## 🏙 Environment & Asset Pipeline

The visual fidelity of the VRLearningPlatform is tailored to maximize presence while minimizing draw calls. Our flagship environment, the **VR Classroom**, demonstrates our rigorous asset pipeline.

### The VR Classroom Environment

The classroom serves as the primary hub for learners. It is designed to be familiar yet dynamic.
- **Optimized Geometry**: All architectural models (walls, desks, whiteboards) are heavily decimated, ensuring low vertex counts without sacrificing silhouette quality.
- **PBR Texturing**: We utilize Physically Based Rendering (PBR) workflows. Textures (Albedo, Normal, Metallic, Smoothness) are packed into RGBA channels to save memory bandwidth.
- **Baked Global Illumination**: Real-time lighting is too expensive for standalone VR. We use Unity's Progressive Lightmapper to bake high-quality lightmaps, ambient occlusion, and indirect bounces directly into the textures.
- **Light Probes**: Dynamic objects (like the user's hands or physics props) receive accurate lighting data from a dense network of Light Probes strategically placed throughout the classroom.

### Asset Standards

As a senior team, we enforce strict naming conventions and directory structures:
- Textures must be power-of-two (POT) and compressed using ASTC 6x6 or 8x8 block sizes for Android/Quest targets.
- Meshes must have Read/Write disabled unless dynamically deformed.
- Materials must use the Universal Render Pipeline/Lit or Simple Lit shaders exclusively.

---

## 🎓 Capstone Project Showcase: Simple Machines Lab

Here is a glimpse into the VRLearningPlatform's core physics and interaction modules. These screenshots highlight the **Simple Machines Lab**, where students learn about mechanical advantage through hands-on interaction.

### Module Selection & UI Design
![Lab Selection Menu](./images/capstone-menu.png)
*The sleek, diegetic user interface allowing students to choose between different physics experiments (The Lever, The Pulley, Inclined Plane). The UI utilizes depth and transparency to remain unobtrusive in the virtual environment.*

### Physics Interactions: The Lever
![Lever Interaction](./images/capstone-lever.png)
*Real-time physics interaction. The user physically grabs and pushes down on the lever to lift a 100 kg water crate. The UI dynamically updates to show the applied effort versus mechanical advantage.*

### Real-Time HUD & Feedback
![Lever HUD](./images/capstone-hud.png)
*As the user pulls the lever, a floating Heads-Up Display tracks the exact units of effort required, visually demonstrating how moving the fulcrum alters the required force.*

### Gamified Learning & Analytics
![Completion Screen 1](./images/capstone-completion1.png)
*Upon successfully completing an experiment, the system calculates the student's efficiency and presents a gamified 3-star completion screen.*

### Pedagogical Reinforcement
![Completion Screen 2](./images/capstone-completion2.png)
*The completion screen doesn't just grade the student; it reinforces the core lesson ("Moving the fulcrum closer to the load shortens the load arm..."), bridging the gap between physical interaction and theoretical physics equations.*

---

## ⚙️ Getting Started & Installation

To clone and run this repository locally, you will need a solid understanding of Unity and Git. Since this is a production-scale VR application, the initial setup requires specific attention to detail.

### Prerequisites

1. **Unity Hub**: Installed and updated.
2. **Unity Version**: Precisely `2022.3.x LTS`. Do not attempt to upgrade the project to Unity 6 without consulting the architectural team, as XR packages have specific version dependencies.
3. **Git LFS**: You MUST have Git Large File Storage installed. Failure to install Git LFS before cloning will result in broken 3D models and textures, as you will only download the pointer files.
4. **Android Build Support**: Required for compiling to Meta Quest/Pico devices. Ensure you install the NDK, SDK, and JDK modules via Unity Hub.

### Installation Steps

1. **Clone the Repository**:
   ```bash
   # Ensure Git LFS is initialized
   git lfs install
   
   # Clone the repo
   git clone https://github.com/DUSHIME1212/VRLearningPlatform.git
   ```

2. **Open the Project**:
   - Open Unity Hub.
   - Click `Add` and navigate to the cloned `VRLearningPlatform` directory.
   - Open the project. (Note: The initial import may take up to 20-30 minutes as it processes the ASTC texture compressions and compiles the Shader Graphs).

3. **Configure Build Settings**:
   - Go to `File > Build Settings`.
   - Switch the platform to **Android**.
   - Under `Texture Compression`, select **ASTC**.

4. **XR Configuration Check**:
   - Go to `Edit > Project Settings > XR Plug-in Management`.
   - Ensure the **OpenXR** box is checked under the Android tab.
   - Resolve any red exclamation marks in the OpenXR Project Validation tool.

---

## 🛠 Advanced Setup & CI/CD Pipeline

In a senior development environment, we do not rely on "it works on my machine." We utilize Continuous Integration and Continuous Deployment (CI/CD) to guarantee build integrity.

### GitHub Actions Workflow

Our repository is configured with GitHub Actions. On every pull request to the `main` branch, the following automated pipeline executes:

1. **Linter & Static Analysis**: We run an automated formatting check using `dotnet format` and Roslyn analyzers to ensure all C# code adheres to our strict style guide.
2. **Unit & Integration Tests**: Unity Test Runner executes our suite of PlayMode and EditMode tests. This ensures that refactoring core mechanics (like grabbing or locomotion) doesn't introduce regressions.
3. **Automated Build**: A headless Unity instance compiles the Android `.apk`.
4. **Artifact Generation**: The resulting APK is uploaded as a GitHub Artifact, allowing QA testers to download and side-load the exact build generated by that specific commit.

### Code Review Standards

Before any code is merged, it must pass a mandatory peer review. Reviewers are instructed to look for:
- **Garbage Collection (GC) Allocations**: Code in `Update()`, `FixedUpdate()`, or `LateUpdate()` must NOT allocate memory on the heap. We rigorously enforce object pooling to prevent frame stutter caused by the GC running.
- **Separation of Concerns**: Is UI logic bleeding into gameplay logic? Are physics calculations happening in `Update` instead of `FixedUpdate`?
- **XR Best Practices**: Does the interaction cause motion sickness? Is locomotion properly smoothed or stepped via vignette tunneling?

---

## 📈 Performance Optimization & Profiling

VR development is an exercise in extreme performance optimization. If our application drops below 72 FPS on a Meta Quest 2, users will experience simulator sickness. This is unacceptable.

### Our Optimization Tenets

1. **Draw Call Minimization**:
   - We utilize Static Batching for all non-moving environmental geometry.
   - We utilize GPU Instancing for repetitive props (e.g., desks, chairs, books).
   - We strictly adhere to the SRP Batcher rules. This means ensuring that all materials use URP-compatible shaders and share similar properties to allow the CPU to batch them efficiently.

2. **Fill Rate & Overdraw Management**:
   - Transparency is the enemy of mobile VR. We minimize the use of transparent queues, opting for opaque or alpha-test (cutout) shaders wherever possible.
   - UI elements are rendered using a specialized VR-optimized Canvas system that prevents massive overdraw when UI panels overlap.

3. **CPU Bound Operations**:
   - Physics calculation intervals are decoupled from the render loop.
   - Raycasting for UI interaction is optimized by aggressively culling the physics layers that the XR Ray Interactors can hit. We use layer masks diligently.

4. **Memory Profiling**:
   - We enforce a strict texture budget.
   - Audio files must be set to `Force to Mono` and `Compressed in Memory` to save RAM footprint.

As a developer on this project, you are expected to attach the Unity Profiler to the physical headset over Wi-Fi and verify your changes do not introduce latency spikes.

---

## 🧬 Design Patterns & Code Standards

To maintain a massive codebase, rigid adherence to design patterns is required.

### 1. The Singleton Anti-Pattern
We strictly avoid the global Singleton pattern for managers. Instead, we use a custom **Service Locator** pattern or Dependency Injection (via VContainer/Zenject). This ensures that modules can be tested in isolation and dependencies are explicitly declared in the constructor or `[Inject]` attributes.

### 2. Object Pooling
Instantiating and Destroying objects at runtime in VR causes massive CPU spikes. Every projectile, interaction effect, or UI pop-up MUST use the Unity `ObjectPool<T>` API. 

```csharp
// Example of Senior-level Object Pooling implementation
public class ParticleSystemPool : MonoBehaviour
{
    private IObjectPool<ParticleSystem> _pool;
    [SerializeField] private ParticleSystem _prefab;

    private void Awake()
    {
        _pool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(_prefab),
            actionOnGet: (ps) => ps.gameObject.SetActive(true),
            actionOnRelease: (ps) => ps.gameObject.SetActive(false),
            actionOnDestroy: (ps) => Destroy(ps.gameObject),
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    public ParticleSystem GetParticle() => _pool.Get();
    public void ReleaseParticle(ParticleSystem ps) => _pool.Release(ps);
}
```

### 3. Asynchronous Programming (async/await)
Coroutines are considered legacy in our architecture due to their poor exception handling and allocation footprint. All asynchronous operations (network requests, scene loading, heavy computations) must utilize standard C# `async/await` and the `UniTask` library.

---

## 🔮 Roadmap & Future Architecture

We are building for the future of Spatial Computing. Our immediate architectural roadmap includes:

- **Phase 1: Multi-User Networking (Q3 2026)**
  - Integrating Unity Netcode for GameObjects (NGO) alongside Unity Relay and Lobby services.
  - Implementing Client-Side Prediction and Server Reconciliation to mask network latency for physics objects.
  - Designing a highly optimized spatial audio voice chat system utilizing Vivox.

- **Phase 2: Generative AI NPCs (Q4 2026)**
  - Implementing an on-device Large Language Model (LLM) interface.
  - Connecting LLM outputs to our Inverse Kinematics (IK) animation rigging system to allow educational NPCs to gesture and converse naturally with students.

- **Phase 3: Mixed Reality (MR) Passthrough (Q1 2027)**
  - Transitioning the architecture to fully support AR Foundation.
  - Implementing Room Plan APIs to allow the virtual classroom elements to dynamically map and anchor onto the user's physical walls and furniture.

---

## 🤝 Contributing Guidelines

We welcome contributions from senior XR developers, 3D technical artists, and UX designers. To ensure a smooth integration of your work:

1. **Fork and Branch**: Always branch off `develop`. Use the standard naming convention: `feature/your-feature-name`, `bugfix/issue-description`, or `hotfix/critical-fix`.
2. **Commit Messages**: Follow the Conventional Commits specification (e.g., `feat: added pinch gesture recognition`, `fix: resolved memory leak in object pooler`).
3. **Pull Requests**: Your PR must include a comprehensive description of the architectural changes made. If you altered performance profiles, attach a screenshot of the Unity Profiler proving your changes do not impact the 72 FPS target.
4. **Code Formatting**: Ensure you have run your code through our `.editorconfig` ruleset.

---

## 📄 License

This project is licensed under the **MIT License**. This permissive license allows for the free use, modification, and distribution of this software for both academic and commercial purposes. 

As developers, we believe that educational technology should be open and accessible. By keeping our core platform open-source, we hope to accelerate the adoption of spatial computing in classrooms worldwide. 

---

<div align="center">
  <p><i>"Education is the passport to the future, for tomorrow belongs to those who prepare for it today."</i></p>
  <p><b>— Engineered with precision by the VRLearningPlatform Architecture Team.</b></p>
</div>
