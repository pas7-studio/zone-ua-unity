# Zone UA testing

The project uses two layers of automated verification:

- EditMode tests cover deterministic world generation, input state, health/damage, faction policy, NPC brain transitions, weapon fire/reload state, persistence, and performance budgets.
- PlayMode tests load isolated scenes and verify runtime wiring: world generation, player movement, player/NPC combat composition, and NPC-to-NPC targeting.

## Isolated scenes

Run `Zone UA/Tests/Build Isolated Test Scenes` to rebuild these scenes:

- `WorldGenerationTestScene` — deterministic generator and generated terrain.
- `PlayerMovementTestScene` — player controller and movement command.
- `CombatTestScene` — player, weapon stack, and NPC damage target.
- `NpcCombatTestScene` — two NPC actors and explicit target acquisition.

## Runbook

1. Run EditMode tests from Unity Test Runner.
2. Run PlayMode tests from Unity Test Runner; they load the isolated scenes without changing the production scene.
3. For a manual smoke check, open one of the four test scenes and press Play.
4. A failure should identify the smallest layer: pure state, runtime component wiring, or scene composition.
