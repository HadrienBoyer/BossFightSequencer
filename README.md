# BossFightSequencer ⚔️🎛️

A modular, no-code/low-code Boss Fight & Wave Sequencer for **Unity**, built with **UI Toolkit** and powered by **Odin Inspector**. 

Design, choreograph, and balance complex boss encounters, attack patterns, minion waves, and arena hazards with a visual timeline inspired by digital audio workstations (DAWs like Ableton Live).

---

## ✨ Key Features

- **DAW-Style Timeline Editor (`UI Toolkit`)**:
  - Horizontal multi-track sequencing (Boss actions, Minion waves, Arena hazards).
  - Ableton-like interactive workflow: double-click to add clips, drag to reposition, pull right handles to resize duration, and right-click to delete.
  - Time ruler markings with automatic layout and zoom/scroll capabilities.
- **Deep Odin Inspector Integration**:
  - Click on any clip on the canvas to inspect and configure its parameters immediately via Odin drawers.
  - Rich conditional rendering (`[ShowIf]`, `[TableList]`, `[MinMaxSlider]`, `[PreviewField]`).
- **Complete ScriptableObject Architecture**:
  - **Boss Profile & Stats**: Max HP, armor, poise/stagger meter, move/sprint speeds, enrage multiplier.
  - **Animation Sets**: Locomotion triggers (`Idle`, `Move`, `Enrage`, `Stagger`, `Death`) and custom pattern animations.
  - **Projectile Patterns**: Bullet hell & combat patterns supporting `Single`, `RadialCircle`, `ConeArc`, `Spiral`, and `HomingBurst`, organized in sequential wave bursts.
  - **Juice & Feedbacks**:
    - **AudioFeedbacks**: Multi-clip pools with pitch variations (`MinMaxSlider`), volumes, delays, and playback modes (`RandomOneShot`, `Sequential`, `Simultaneous`).
    - **VisualFeedbacks**: Prefabs ready for Runtime Object Pooling, auto-recycling, and built-in **Tweens** (`PunchScale`, `ShakePosition`, `FadeAlpha`, `FlashColor` with `AnimationCurve`) + Camera Shake integration.
- **FSM & Runtime Sequencer Player**:
  - Hierarchical State Machine (HFSM) decoupling Macro-Phases (Boss combat vs. Interludes/Adds) from Micro-Tactics (Telegraph $\rightarrow$ Execution $\rightarrow$ Recovery).
  - Runtime Player (`BossSequencerPlayer`) that plays timeline sequences frame-by-frame and coordinates boss animations, spawners, and arena traps.

---

## 📁 Repository Structure

```text
BossFightSequencer/
├── BossSequencer/
│   ├── Documentation/
│   │   └── README.md
│   └── Scripts/
│       ├── Editor/
│       │   └── BossSequencerWindow.cs            # EditorWindow (UI Toolkit + Odin)
│       └── Runtime/
│           ├── BossController.cs                 # Main boss orchestrator & blackboard
│           ├── BossFightPhaseState.cs            # FSM state: Active boss combat
│           ├── BossWaveInterludeState.cs         # FSM state: Add wave / safety interlude
│           ├── BossPatternData.cs                # ScriptableObject: Single attack pattern
│           ├── BossTimelineSequence.cs           # ScriptableObject: Multi-track sequence
│           ├── BossSequencerPlayer.cs            # Runtime playback runner
│           ├── IBossState.cs                     # State machine interface
│           ├── Data/
│           │   ├── BossCharacterProfile.cs       # Complete boss identity sheet
│           │   ├── BossStatsData.cs              # Vitality & mobility stats
│           │   └── BossAnimationData.cs          # Animator trigger mappings
│           ├── Feedbacks/
│           │   ├── AudioFeedbackDefinition.cs    # SFX lists & variations
│           │   └── VisualFeedbackDefinition.cs   # VFX, pooling keys & tweens
│           └── Projectiles/
│               └── ProjectilePatternDefinition.cs# Bullet hell shapes & wave steps
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites
- **Unity 2021.3 LTS / 2022.3 LTS / Unity 6+**
- **Odin Inspector & Serializer** (by Sirenix)

### Installation
1. Clone or download this repository:
   ```bash
   git clone https://github.com/HadrienBoyer/BossFightSequencer.git
   ```
2. Copy the `BossSequencer/` folder into your Unity project's `Assets/` directory.

### Quick Workflow
1. **Open the Tool**: In Unity's top bar, navigate to:  
   `Tools > Boss Sequencer (DAW Timeline)`
2. **Create a Sequence**:
   - Right-click in the Project Window $\rightarrow$ `Create > Boss > Timeline Sequence`.
   - Set the `Total Duration` (e.g., 60 seconds).
   - Drag and drop the asset into the `Target Sequence` field of the Sequencer window.
3. **Choreograph your Wave**:
   - Click `+ Ajouter Piste` to add tracks (Boss, Minions, Hazards).
   - **Double-click** on any track to create a clip.
   - **Drag** to set the trigger timestamp.
   - **Pull the right handle** to adjust its duration.
   - **Select the clip** to configure its pattern data, prefabs, and feedbacks directly in the Odin inspector drawer above the canvas.
4. **Playback in Runtime**:
   - Attach `BossSequencerPlayer` to a GameObject in your scene.
   - Assign the `BossController` and your `BossTimelineSequence`.
   - Call `player.PlayWave(sequence);` from your game manager or trigger zones!

---

## 📄 License

This project is open-source and released under the [MIT License](LICENSE).
