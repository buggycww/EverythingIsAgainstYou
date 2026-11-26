# EverythingIsAgainstYou

A simple 2D game where everything is out to get you. Be warned.

## Overview

A 2D survival game made in Unity for Brackeys Game Jam 2026.2. The jam theme was "Trust No One." Accordingly, everything in the game poses a threat to you, the player, an adventurer who arrived in what appears to be an abandoned ruin. Unexpected hazards come from the environment, objects, and enemies intending to block your path or kill you.

This was my first 2D game project. It was built during the jam timeframe, so some systems are rough and a few known issues remain.

Try it out for free on [itch.io](https://buggycww.itch.io/everything-is-against-you).

## Controls

| Action | Input |
| --- | --- |
| Move | WASD |
| Show interact option | Left click on the interaction prompt |
| Interact | E |

## Built with

- Unity
- Universal Render Pipeline with the 2D Renderer
- C#

## Running the project

1. Clone the repository:

   ```bash
   git clone https://github.com/buggycww/EverythingIsAgainstYou.git
   ```

2. Open Unity Hub and add the cloned folder as a project.

3. Open the project with the Unity version listed in `ProjectSettings/ProjectVersion.txt`.

4. If Unity reports missing packages, let the Package Manager resolve the dependencies listed in `Packages/manifest.json`.

5. Open the main scene from `Assets/Scenes/` and press Play in the Unity Editor.

## Known issues

- Sorting-layer issues around pivot-based sorting, especially with rocks and pits. Some fixes are in place, but edge cases may remain.
- General rough edges from the project being built during a game jam and being my first 2D game.
