# Endless Puzzle Prototype

## 1. Overview

This is a 3D endless puzzle prototype built in Unity for Android-style portrait play.

Core loop:

- a question is generated at runtime
- multiple 3D shapes fall through the play area
- each object has a real shape, a real color, and a text label
- the player taps the correct object based on the real color and shape
- correct taps add score and time
- wrong taps and missed objects reduce score and time
- the run ends when the timer reaches zero

Example prompts:

- `Select the green object`
- `Tap the blue sphere`
- `Select the red cube`

The important gameplay rule is:

- real material color and real shape are the truth
- the text shown above the object can be misleading
- the prompt color word can also be displayed in a misleading text color

## 2. Final Gameplay Rules

### Truth Model

Each falling target has:

- actual color
- actual shape
- label text above it

Example:

- a blue sphere may display the word `red`
- the correct answer logic still treats it as a blue sphere

### Scoring and Timer

- correct tap:
  - `+1` score
  - adds base time reward
  - some shapes add extra bonus time
- wrong tap:
  - score penalty
  - time penalty
- missed object crossing the lower boundary:
  - score penalty
  - time penalty
- timer reaches zero:
  - game over

### Difficulty

Difficulty scales by score through JSON data.

It currently ramps by:

- increasing fall speed gradually
- increasing active object count gradually
- increasing misleading label chance gradually
- reducing spawn interval gradually

The curve was tuned to be smoother in the early game so pressure grows in steps instead of jumping too sharply.

## 3. Scene Structure

The final flow uses one gameplay scene:

- `Assets/Scenes/SampleScene.unity`

This scene contains:

- authored menu UI for Start / Exit
- the runtime gameplay bootstrap

Behavior:

- pressing Play shows the scene menu first
- runtime gameplay waits
- pressing Start hides the menu
- after a short delay, runtime gameplay starts

## 4. Folder Structure

```text
Assets/
  Scenes/
    SampleScene.unity
  Game/
    Docs/
      PROJECT_GUIDE.md
      UNITY_SCENE_SETUP_FROM_SCRATCH.md
    Materials/
      Generated/
    Resources/
      JSON/
        Colors.json
        Shapes.json
        QuestionTemplates.json
        Difficulty.json
        SpawnSettings.json
      Presentation/
        ColorMaterialPalette.asset
        PrototypePresentationSettings.asset
    Scripts/
      Bootstrap/
        RuntimeBootstrapper.cs
        PrototypeSceneBuilder.cs
      Data/
        ColorMaterialPalette.cs
        GameConfigDatabase.cs
        GameConfigModels.cs
        JsonDataLoader.cs
        MaterialLibrary.cs
        PrimitiveMeshLibrary.cs
        PrototypePresentationSettings.cs
      Gameplay/
        TargetObject.cs
        TargetObjectFactory.cs
      Managers/
        BoundaryDetector.cs
        DifficultyManager.cs
        GameManager.cs
        InputHandler.cs
        PoolManager.cs
        QuestionManager.cs
        ScoreManager.cs
        SpawnManager.cs
        UIManager.cs
      Pooling/
        ComponentPool.cs
      UI/
        MenuSceneController.cs
        SafeAreaFitter.cs
```

## 5. Runtime Hierarchy

During Play mode, the runtime creates the gameplay stack under `EndlessPuzzleRuntime`.

Expected runtime hierarchy:

```text
SampleScene
  Main Camera
  Directional Light
  EventSystem
  SceneAuthoredMenuUI
  EndlessPuzzleRuntime
    GameplayRoot
      ActiveTargets
      SpawnArea
    Managers
      ScoreManager
      DifficultyManager
      QuestionManager
      PoolManager
      SpawnManager
      InputHandler
      BoundaryDetector
      UIManager
      GameManager
    UIRoot
      RuntimeCanvas
        SafeArea
          HudRoot
            TopPanel
              ScoreText
              TimerText
              BestText
            QuestionCard
              QuestionText
            GameOverPanel
              TitleText
              ReasonText
              ScoreSummaryText
              RestartButton
```

## 6. Data-Driven Design

All game content is loaded from `Assets/Game/Resources/JSON`.

### Colors

`Colors.json`

- logical color ids
- display names
- hex colors used for materials and rich text

### Shapes

`Shapes.json`

- logical shape ids
- primitive mapping
- scale tuning
- optional time bonus per shape

### Question Templates

`QuestionTemplates.json`

- format strings using `{color}` and `{shape}`
- required attribute mask
- min score unlock
- weight

### Difficulty

`Difficulty.json`

- score threshold per tier
- active object count
- fall speed
- misleading chance
- spawn interval

### Spawn Settings

`SpawnSettings.json`

- spawn height
- lower miss boundary
- spacing
- prewarm count
- target scale
- timer values
- score/time penalties

## 7. Current JSON Tuning

### Difficulty

Current tiers:

```json
{
  "tiers": [
    { "scoreThreshold": 0, "maxActiveObjects": 3, "fallSpeed": 1.8, "misleadingChance": 0.06, "spawnInterval": 1.02 },
    { "scoreThreshold": 5, "maxActiveObjects": 3, "fallSpeed": 2.0, "misleadingChance": 0.10, "spawnInterval": 0.96 },
    { "scoreThreshold": 11, "maxActiveObjects": 4, "fallSpeed": 2.2, "misleadingChance": 0.15, "spawnInterval": 0.90 },
    { "scoreThreshold": 18, "maxActiveObjects": 4, "fallSpeed": 2.4, "misleadingChance": 0.22, "spawnInterval": 0.84 },
    { "scoreThreshold": 26, "maxActiveObjects": 5, "fallSpeed": 2.65, "misleadingChance": 0.30, "spawnInterval": 0.78 },
    { "scoreThreshold": 35, "maxActiveObjects": 6, "fallSpeed": 2.95, "misleadingChance": 0.40, "spawnInterval": 0.71 },
    { "scoreThreshold": 45, "maxActiveObjects": 7, "fallSpeed": 3.25, "misleadingChance": 0.50, "spawnInterval": 0.64 }
  ]
}
```

### Spawn Settings

Current values:

```json
{
  "spawnY": 7.4,
  "failY": -9.2,
  "horizontalMin": -3.45,
  "horizontalMax": 3.45,
  "depthMin": -0.35,
  "depthMax": 0.35,
  "minSpacingX": 1.55,
  "minSpacingZ": 0.55,
  "spawnStackSpacingY": 1.45,
  "safeQuestionDistanceFromFailY": 2.2,
  "maxSpawnAttempts": 22,
  "poolPrewarmCount": 24,
  "absoluteMaxActiveObjects": 8,
  "targetScale": 0.8,
  "startTimeSeconds": 28.0,
  "maxTimeSeconds": 40.0,
  "correctTimeRewardSeconds": 0.9,
  "wrongTapTimePenaltySeconds": 0.65,
  "missTimePenaltySeconds": 0.5,
  "wrongTapScorePenalty": 1,
  "missScorePenalty": 1,
  "labelColorCycleSeconds": 0.5
}
```

The lower miss boundary was moved down so targets now leave visible space before being counted as missed.

## 8. Script Responsibilities

### Bootstrap

`RuntimeBootstrapper`

- owns runtime entry
- waits for the menu start signal
- builds gameplay only when the run should begin

`PrototypeSceneBuilder`

- creates runtime camera/light support if needed
- creates manager objects
- creates gameplay containers
- builds the in-game HUD and game over UI

### Data

`JsonDataLoader`

- loads JSON resources

`GameConfigDatabase`

- exposes parsed config lookups

`MaterialLibrary`

- creates or resolves shared materials

`PrimitiveMeshLibrary`

- caches primitive meshes

`PrototypePresentationSettings`

- optional font/material presentation asset

`ColorMaterialPalette`

- shared material asset mapping by color id

### Gameplay

`TargetObject`

- pooled falling target
- stores gameplay identity
- configures visuals and label placement

`TargetObjectFactory`

- builds the pooled target prototype

### Managers

`GameManager`

- owns the run state
- starts/reset the run
- applies score/time changes
- handles game over

`ScoreManager`

- session score and best score

`DifficultyManager`

- resolves current runtime difficulty tier from score

`QuestionManager`

- chooses valid questions
- ensures exactly one correct answer
- builds the display prompt

`SpawnManager`

- spawns targets
- updates falling movement
- applies readable spacing
- tracks active combos

`PoolManager`

- owns target pooling

`InputHandler`

- touch/click raycast selection

`BoundaryDetector`

- lower miss boundary check

`UIManager`

- updates score, timer, question, and game over UI

### Utility

`ComponentPool<T>`

- reusable component pool

`MenuSceneController`

- Start / Exit menu button behavior

`SafeAreaFitter`

- applies device safe area

## 9. UI State

### In-Game Header

Current HUD improvements:

- dedicated stat strip for score / timer / best
- dedicated question card below stats
- better spacing and clearer visual hierarchy
- smoother font sizing for portrait screens

### World Labels

Current world-label improvements:

- per-shape height offsets
- cleaner alignment above the mesh
- reduced object tilt for readability
- optional separate world font support through `PrototypePresentationSettings`

## 10. Fonts

Current font setup:

- screen UI uses TMP font from `PrototypePresentationSettings`
- world labels use `TextMesh`

Recommended polish:

- assign a cleaner TMP font for the HUD
- assign a readable standard `Font` for world labels in `PrototypePresentationSettings`

The code already supports that final font swap.

## 11. Performance Notes

Performance-oriented choices kept in the final build:

- pooled falling targets
- `Physics.RaycastNonAlloc` for input
- simple scripted falling
- primitive meshes
- shared materials
- no Rigidbody-based gameplay
- no realtime target shadows
- no post-processing
- lightweight overlay UI

## 12. Cleanup For Interview Build

Removed from the project:

- editor-only Android configurator script
- editor-only presentation asset generator script
- unused pulse UI script

Kept intentionally:

- `MenuSceneController`
  - used by the scene Start / Exit buttons
- `PrototypePresentationSettings`
  - used by runtime presentation setup
- `ColorMaterialPalette`
  - used by runtime material mapping

This keeps the project leaner without removing scripts that are still part of the actual shipped flow.

## 13. Manual Unity Setup

Before test build:

1. Open `Assets/Scenes/SampleScene.unity`
2. Import TMP essentials if Unity asks
3. Make sure your scene menu Start button calls:
   - `MenuSceneController.StartGame()`
4. Make sure your Exit button calls:
   - `MenuSceneController.ExitGame()`
5. Assign menu references in `MenuSceneController`:
   - `Menu Canvas`
   - `GraphicRaycaster`
   - `MenuRoot` if used
6. Optionally assign a better TMP font and world label font in `PrototypePresentationSettings.asset`

## 14. Test Checklist

- menu appears first on Play
- pressing Start hides the menu
- gameplay starts after the delay
- shapes visibly fall before counting as missed
- labels stay aligned above shapes
- question prompt is readable
- header spacing looks correct on portrait aspect ratios
- difficulty ramps gradually
- restart works
- wrong taps and misses reduce time/score correctly
- stable frame pacing on target device

## 15. Final Requirement Check

- 3D endless puzzle gameplay: yes
- runtime-generated questions: yes
- JSON-driven content: yes
- misleading labels: yes
- misleading prompt color styling: yes
- correct tap score increase: yes
- wrong tap penalty: yes
- miss penalty: yes
- timer-based game over: yes
- gradual difficulty scaling: yes
- object pooling: yes
- responsive mobile UI: yes
- Android-focused optimization: yes
- modular architecture: yes
