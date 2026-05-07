
# Endless Puzzle Prototype

A Unity 3D endless puzzle game built for portrait/mobile play.

Players answer runtime-generated prompts by tapping the falling object with the correct real color and shape. The twist is that object labels and prompt text styling can be misleading, so the player must trust the actual 3D object, not the displayed words.

## Demo

https://github.com/user-attachments/assets/8363e275-d44d-4997-876b-0cbf52708a65

```md
https://github.com/user-attachments/assets/8363e275-d44d-4997-876b-0cbf52708a65
```

## Features

- 3D falling shape selection gameplay
- Runtime-generated color and shape questions
- Misleading object labels and prompt color styling
- Score, timer, correct tap rewards, wrong tap penalties, and miss penalties
- JSON-driven colors, shapes, question templates, difficulty, and spawn settings
- Gradual difficulty scaling by score
- Object pooling for better runtime performance
- Portrait-friendly Unity UI with menu, HUD, restart, and game over flow

## Project Structure

```text
Assets/
  Scenes/SampleScene.unity
  Game/
    Resources/JSON/
    Scripts/
      Bootstrap/
      Data/
      Gameplay/
      Managers/
      Pooling/
      UI/
    Docs/PROJECT_GUIDE.md
```

## How To Run

1. Open the project in Unity.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.
4. Use Start from the menu to begin the run.

## Notes

Detailed architecture, tuning values, scene hierarchy, and test checklist are documented in `Assets/Game/Docs/PROJECT_GUIDE.md`.

## GitHub About

**Description:** Unity 3D endless puzzle prototype with runtime-generated questions, misleading labels, JSON-driven difficulty, and mobile portrait gameplay.

**Topics:** `unity`, `unity3d`, `csharp`, `game-development`, `mobile-game`, `android-game`, `puzzle-game`, `endless-game`, `3d-game`, `json-driven`, `object-pooling`
