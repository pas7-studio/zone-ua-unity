# Zone-UA — Living Hub: system & lore bible v0.1

Status: **PROPOSED / author review**, not yet canon. Built on `Docs/Narrative` v0.4 in branch `docs/narrative-v04`, 2026-10-08. Existing locked intro and Act 0 are not changed.

## 1. Creative thesis

The hub is an adapted old forestry/logistics base with residents who have work, debts, loyalties, routines and knowledge. They do things for each other and against each other when the player is absent. Stories arise from needs, not from the desire to recite lore.

**World identity:** Ukrainian everyday speech and material culture; generators, delivery notes, pumps, radios, rain, tea, welds, mud, bureaucratic permits, contested routes and the economics of an inhabited exclusion zone. No derivative stalker terminology, iconic campfire legends, copycat faction structure or paranormal explanations for every failure. Mutants/anomalies exist but ordinary greed, neglect, lies and chance remain equally dangerous.

The anomaly system must have internally consistent consequences without explaining every phenomenon. Conflicting testimony is okay; contradictory objective world state is not.

**Narrative level split:**
- CANON: established character roles, arrival and 24.02 anchor, last Kasyr contract, v0.4 speech/presentation.
- WORKING CANON: suggested social history below, subject to review.
- RUMOR: characters can be wrong or bluffing.
- PROPOSED: specific new names, Blood customs, underground ecology and physical mechanics until verified against current game behavior.

Do NOT mention the late-stage legendary artifact by name in early ambient content. Hints should concern route-mark theft, data acquisition and buyer preferences, never explain the main conspiracy.

## 2. Human cast and voice

Established: Касир (networker, terse care), Гугл (competent but fallible guide), Дідо (human/social memory), Семен (stock and logistics, not greedy parody), Шуруп (mechanic), Орест (scientific precision, uncertainty), Кравець (perimeter procedure), Процент (intermittent buyer, NOT permanent kiosk). Мала may appear as a recurring resident/contact once cast is confirmed. Additional bit parts are **proposed**, not retroactive canon: Буряк (freelance porter), Тарас «Термос» (cook/runner), Льоня (newcomer), Лис (route veteran), Руда (independent carrier). Generic NPCs must have a stable role and limited knowledge, not become omniscient extras.

Blood is a **proposed interpretation** of user's brief: a loose alliance of predatory small crews operating through debt, stolen supplies, protection and opportunistic robbery. They are not a mystical order; internal cohesion is poor and their individual members disagree. Do not invent an official uniform, flag, initiation ritual or complete leadership without author approval.

Social rules:
- Dialogue has an immediate subject (missing lid / broken cable / bill), a character agenda, and optionally a subtext.
- Often someone is silent or walks away. 50%+ of all ambient scenes are not jokes.
- Specific nouns beat abstract lore talk. Actions, pauses, interruptions and props carry meaning.
- Never make everyone equally witty, equally profane or equally knowledgeable.
- An NPC can repeat a necessary factual instruction through *different functional lines*, but a distinctive punchline is near-unique.
- Ambient speech = **overhead bubbles**. Player initiates conversation = **cinematic strip**. Distant messages = **КПК chat**. No mumble voice loop.
- Player need not be centered in dialogue. Optional listening can seed a clue but never gates the main quest.

## 3. Spatial semantic anchors, not random wandering

Existing Hub 2.0 regions:
`dido_canopy` (tea/fire/radio 6-8 seats), `central_yard` (transient public event), `semen_warehouse` (receiving/loading), `shurup_garage` (repair), `generator_corner`, `orest_lab` + antenna edge, `kravets_post` + raid trail, `housing`, `well`, `outer_ring` (woodshed, smoking, fence), `road_entry`, `medical_point`.

Activities: `tea`, `eat`, `radio`, `work`, `repair`, `load`, `guard`, `inspect`, `rest`, `smoke`, `argue`, `visit`, `return_from_raid`. Actors reserve a named activity slot and a valid path before starting. Never reroute cinematic intro trail, block shop interactions, spawn at player camera, or obstruct the protected reveal. A seller's service remains usable via proxy/interrupt until scene completes.

Spatial story grammar:
- 1 actor: solitary mutter, object reaction, mechanical activity, silence.
- Exactly 2: confessions, private barter, gossip *that cannot play before a crowd*.
- 3: banter, bargaining with witness, disagreement.
- 4+: competing versions of a story, group joke, contested account, crowd event.
- 6+: rare communal activity, not normal loop. Headcount is **actual available participants within the anchor**, NOT all NPCs in the area.

## 4. Event primitives

**Bark** (1–2 lines, 4–10 seconds): one person reacts to state; 2-6 alternate texts per trigger.
**Micro-exchange** (2–5 lines, 10–25 seconds): no pathing required.
**Scene** (4–9 lines, 25–60 seconds): purposeful positions, props, a small action.
**Microchain** (2–5 beats over separate visits): NPC travels between anchors, an item or attitude changes. Steps can complete offscreen.
**Opportunity** (optional): clue/physical object appears; can lead to procedural world outcomes without quest marker.
**World pulse**: camp collectively responds to temporal events, e.g. 23.02 or 24.02, and overrides default chatter.

The library has stable IDs, strict `min/max` headcounts, casting slots, location, preconditions, exclusion tags, weight, significance, outcome and follow-up. A beat can be COMPLETED_UNSEEN. NPCs do not wait at a spot forever hoping the player sees the show.

## 5. Scheduler: the anti-loop contract

Eligibility requires: Act/world phase; actor alive/available; correct role and location; adequate headcount; props; knowledge gate; player not in locked reveal, cinematic dialogue, battle or another louder bubble event; all required prior beats reached; no conflicting `activity_reservation`. Always prefer **natural silence** over a forced event.

Recommended *starting tuning values* (playtest, not canon):
- At most 1 foreground ambient conversation within player's intelligible radius; max 1 background bark beyond it.
- Global ambient start cooldown: 55–110 sec when player idles in busy hub; 120–240 sec during movement/short visits.
- Any named character: 3–6 min between authored performances.
- Same event ID: not within 4 distinct hub visits and 2 in-game days. Unique or lore-revealing scene: once permanently per save.
- Same joke/punchline tag: at least 8 hub visits and 5 in-game days; prefer once-only.
- After 2 events of same theme in one hub session, suppress theme for rest of session.
- Weighted novelty: unseen ×6, new variant ×3, recently seen ×0.1, fresh story consequence ×4. Avoid strict deterministic cycling.
- Minimum *real* scene density: target 0–2 foreground scenes on a quick vendor visit, 2–4 on a 10-minute rest. **Silence is valid.**
- 60% everyday/material, 15% interpersonal, 10% practical hazard, 10% rumor, 5% rare/ominous (rough target). Ominous events have heavy scarcity.

**Memory:** track `seen_exact_variant`, `played_event_ids`, `topic_exposure`, `npc_knowledge`, `world_facts`, `chain_stage`, `last_scene_by_npc`, `source_version`, `timestamp`. Revisit rules cannot depend solely on scene reload. RNG can be seeded per save/day, not by frame.

**Player arrival/leave:**
- Do not begin conversations just because the player crosses a trigger.
- If the player is passing by, start only when a credible shared activity was already occurring.
- A scene interrupted by player talk pauses or gracefully ends; do not restart first line upon closing UI.
- If player leaves hearing radius, characters can finish silently. Only gameplay-critical state commits through a deterministic end event.
- Combat/alarm/24.02 pulse preempts ordinary scenes and releases reservations.
- Events never interrupt hub services or override 24.02 anchor raid.

## 6. Cross-system consequences

Event facts can affect **visible props, inventory, price rumors, NPC routines or later text**, not just a hidden checkbox. A repaired kettle turns up at Dido's canopy. Missing fuel increases pressure at Shurup's generator. Stolen markers affect Google's route reports. Blood sightings alter which road people use. A body found in a raid can be identified or disputed, not treated as generic mutant loot. Cave gas conversations should reference observation (odor, instrument reading, airflow), not definitive chemistry until the mechanical spec is confirmed.

Avoid guaranteed payoff when procedural world may not spawn required evidence. Use three routes: a) observed consequence, b) offscreen resolution, c) unresolved ambiguity.

**World phase:**
- `act0_regular`: banter, trade, small disputes, investigation seeds.
- `act0_23feb`: worry, radios, families, transport. Lower joking frequency; some scenes become unavailable.
- `act1_24feb`: **authored pulse wins every priority contest**; no generic camp chatter.
- `act1_after`: absence, rationing, changed seating patterns, where people went. Do not repeat pre-war jokes unchanged.

## 7. Sample data contract (YAML-like, semantics authoritative)

```yaml
id: HUB-DID-004
type: microchain_step
chain: kettle_dispute
stage: 2
anchor: shurup_garage
phase: [act0_regular]
cast:
  - role: shurup
  - role: dido
crowd: {min: 2, max: 2, exact: true}
requires: [kettle_dispute.stage1_resolved]
effects: [kettle_dispute.stage2_resolved]
once_per_save: true
interrupt: finish_offscreen
duration_est_s: 28
tags: [humor_dry, repair, everyday]
lines:
  - {who: dido, text: "Ти ручку зробив?"}
  - {who: shurup, text: "Зробив. Нею тепер підняти можна."}
  - {who: dido, text: "А кришку?"}
  - {who: shurup, text: "Я ручку обіцяв."}
```

For multi-role scenes, don't fabricate missing speakers. Author explicit 2/3/4-person variants or forbid the scene. A 4-person scene must have enough seats/path slots. Cast must be bound to stable NPC identity, with fallback roles only for unimportant bit parts. A hidden once-only world clue is **not** replaced by a random line.

## 8. Tools and editorial QA

Content in `HUB_AMBIENT_LIBRARY_V01.md` contains playable scripts (A), multi-beat chains (C), and smaller expansion seeds (S). An engineering import step can convert reviewed entries into per-event YAML/JSON and Unity ScriptableObjects. Before import:
1. Language pass by a Ukrainian native editor; remove bookish phrases, identical voices, and jokes that explain themselves.
2. State consistency: characters cannot know events before hearing them.
3. No hard quest gating from optional ambient conversations.
4. Verify actions exist (tea/kettle/box/carry/body/gas); stage prop-only fallback if not.
5. Automated validation of IDs, cast slots, arity, mutually exclusive event groups, state flags and unreachable chain nodes.
6. Playtest pass: 10 repeated short visits, 3 long stationary visits, player dialogue interruption, NPC missing/dead, 23.02→24.02 transition, rest/save/reload.
7. Human review of sensitive pre-/post-24.02 tone.

References for technique (not narrative material to copy):
- GDC Jason Gregory, *A Context-Aware Character Dialog System* — NPC knowledge + global/environmental context.
- GDC Meg Jayanth, *Writing NPCs with Agency* — people pursue aims that are not about the player.
- GDC Harvey Smith / Matthias Worch, *What Happened Here? Environmental Storytelling* — physical consequences can tell stories.

## 9. First implementation milestone

P0: semantic anchors/reservations, deterministic cancellation, scriptable events, overheard bubbles, save flags, debug inspector.
P1: 8 scenes + 2 chains at dido_canopy, garage, warehouse; 2, 3, 4-person exclusivity tests.
P2: wider library, opportunities and offscreen completion; Blood, gas/caves/body cues only after mechanical verification.
P3: world pulses and 24.02 overrides; always preserve the existing protected intro and anchored crash sequence.

*Editorial status: proposal only. No gameplay code or existing locked narrative has been modified.*
