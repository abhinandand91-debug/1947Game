# 1947 Game

A 2D top-down space shooter built with Unity (2018.3).

- **Author:** Abhinandan Dharmadhikari
- **Created:** 2026-10-12

## Gameplay
Pilot your ship, dodge meteors, shoot down enemy ships and survive as long as you can.

## Opening the project
Open the project folder with Unity 2018.3.5f1 (or newer) and load `Assets/Scenes/MainScene.unity`.

## Features
- Screen shake, hit flash, particle explosions and floating score popups
- Synthesised sound effects (`python3 Tools/GenerateSfx.py` regenerates them)
- Saved best score and a game over screen with Retry
- Difficulty waves: Rookies (easy), Fighters (medium), Aces (hard), then a Boss, repeating tougher each round
- Difficulty selector (Easy / Normal / Hard) on the main menu
- Pickups dropped by enemies: heart, shield and screen-clearing bomb
- Boss health bar with a second "enraged" phase, and a bonus heart for beating a boss
- Enemy variety: strafers, kamikaze chasers and V / line formations
- Combo multiplier for chained kills
- Pause menu, vibration and optional manual fire button on mobile
- The player's gun upgrades as enemies are destroyed: single, fast, double, triple spread, five-way, and one level is lost whenever you are hit

## Controls
| Platform | Move | Shoot |
|----------|------|-------|
| Desktop  | Arrow keys / WASD | Space |
| Mobile   | Touch and drag anywhere, the ship follows your finger | Auto-fires while touching |

The game runs in landscape on Android and iOS.
