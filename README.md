# MTCodex

A Unity project with WASD movement controls.

## Features

- **WASD Movement**: Use W, A, S, D keys to move the player character
- **Mouse Look**: Look around with mouse movement (3D mode)
- **Jump**: Press Space to jump
- **Ground Detection**: Player can only jump when touching the ground

## Scripts

### PlayerController3D.cs
3D movement controller with:
- WASD movement in 3D space
- Mouse look with cursor lock
- Physics-based movement using Rigidbody
- Ground detection for jumping
- Press Escape to unlock/lock cursor

### PlayerController.cs
2D movement controller with:
- WASD movement in 2D space
- Physics-based movement using Rigidbody2D
- Ground detection for jumping
- W key can also be used for jumping

## Scene Setup

The SampleScene includes:
- Player GameObject with movement script attached
- Ground plane for the player to walk on
- Main Camera for viewing
- Directional Light for scene lighting

## Controls

- **W**: Move forward / Jump (2D mode)
- **A**: Move left
- **S**: Move backward
- **D**: Move right
- **Space**: Jump
- **Mouse**: Look around (3D mode)
- **Escape**: Toggle cursor lock (3D mode)

## Getting Started

1. Open the project in Unity 2022.3 or later
2. Load the SampleScene from Assets/Scenes/
3. Press Play to test the movement
4. Use WASD keys to move around