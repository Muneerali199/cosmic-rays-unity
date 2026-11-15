# COSMIC RAYS: Areeba's Promise

## Product Requirements Document

| Field | Value |
| --- | --- |
| Version | 0.2 |
| Status | Pre-production / vertical slice |
| Updated | 14 November 2026 |
| Working title | COSMIC RAYS: Areeba's Promise |
| Genre | Narrative science-fiction adventure |
| Initial platform | Desktop and mobile web |
| Target rating | Everyone 10+ / PEGI 7 |

## 1. Product Summary

### Logline

After her scientist mother disappears during the launch of an experimental solar-particle probe, a curious young girl named Areeba follows a trail of hidden research that leads to Enceladus, Saturn's frozen moon.

### Player fantasy

The player becomes a young space investigator: exploring meaningful places, understanding real science, operating experimental technology, and solving the mystery of Alice's disappearance.

### Product promise

COSMIC RAYS combines an emotional mother-daughter mystery with science that the player learns by doing rather than by reading long lessons.

## 2. Product Goals

1. Deliver a memorable family mystery with a strong reason to continue playing.
2. Turn space science into interactive puzzles and decisions.
3. Clearly separate established science from fictional technology.
4. Produce a polished 10-15 minute vertical slice before expanding scope.
5. Keep the prototype playable on laptops and phones without installation.

## 3. Non-Goals For The Prototype

- A complete six-month Enceladus campaign.
- Photorealistic 3D graphics.
- Combat, weapons, or enemies.
- Multiplayer or online accounts.
- Full voice acting.
- Proving that SEP-MPS-269 is physically achievable.
- Building final production systems before testing the core experience.

## 4. Audience

### Primary audience

- Players aged 10-18 who enjoy mystery, space, and story-driven games.
- Students who engage better with interactive science than textbooks.
- Casual players comfortable with short exploration and puzzle sessions.

### Secondary audience

- Parents, teachers, science clubs, and game-jam or hackathon judges.
- Fans of compact narrative adventures and environmental storytelling.

## 5. Design Pillars

### Mystery before exposition

Every scientific fact should help answer a story question. The player discovers information because it matters to Areeba, not because the game wants to deliver a lecture.

### Science you can touch

Players route magnetic fields, classify particles, repair systems, and interpret data. Reading supports interaction but does not replace it.

### Human stakes in a huge universe

The central motivation is finding Alice and understanding why she left. Enceladus and the propulsion research increase the scale without replacing the personal story.

### Honest speculation

Real facts, current engineering, and fictional inventions use visibly different labels. The game may imagine impossible technology, but it must not teach speculation as fact.

## 6. Narrative Canon

### Main characters

**Areeba**

- Playable protagonist.
- Curious, observant, and emotionally close to her mother.
- Begins as a child in the prologue and becomes a trained mission specialist after the time jump.

**Dr. Alice Rahman**

- Areeba's mother and lead researcher for SPA-077.
- Warm at home, guarded about her research, and under pressure before launch.
- Disappears on the day SPA-077 launches.

### Canon timeline

| Date | Event |
| --- | --- |
| 4 October 2028 | Dinner conversation and Areeba's first secret visit to Alice's lab. |
| 7 December 2028 | Areeba's birthday, SPA-077 launches, and Alice disappears. |
| 2028-2038 | SPA-077 studies solar energetic particles for ten years. |
| 7 December 2038 | A signal linked to Alice is detected near Enceladus. The COSMIC RAYS mission begins. |

The 2026 date from the initial presentation is superseded by this timeline so the ten-year mission ends at the stated 2038 launch.

### Prologue sequence

1. Alice introduces Areeba to high-energy particles over dinner.
2. Alice avoids explaining the purpose of her research.
3. Three hours later, the player explores Areeba's room.
4. The player enters Alice's lab and opens the SPA-077 mission file.
5. A hidden note warns Areeba to watch the sky on 7 December.
6. SPA-077 launches and Alice disappears.
7. Ten years later, a signal points toward Enceladus.

### Main-game premise

Adult Areeba joins the COSMIC RAYS mission as its particle-systems specialist. During the six-month Enceladus assignment, she helps stabilize a research habitat while investigating signals that suggest Alice's disappearance and the SEP-MPS-269 experiment are connected.

## 7. Science Rules

### Established science

- Cosmic rays are high-energy particles that mostly originate beyond the Solar System.
- Solar energetic particles, or SEPs, are accelerated by solar flares, coronal mass ejections, and related solar events.
- Some charged particles travel close to the speed of light.
- Enceladus is an icy moon of Saturn with a subsurface ocean and water-rich plumes.
- Enceladus has an extremely thin exosphere, not a breathable atmosphere.

### Speculative technology

- SPA-077 can attempt limited SEP capture and confinement for research.
- SEP-MPS-269 is an experimental propulsion hypothesis, not established engineering.
- Its theoretical `0.9c` target is a mystery and long-term aspiration, not a demonstrated capability.
- Magnets direct charged particles; they do not create free energy. The fictional system still requires an external energy source.
- Any quoted Earth-to-Saturn travel time must identify that it ignores acceleration, deceleration, shielding, and route constraints.

### Content rule

Every science entry displayed to players must be tagged as one of:

- **Observed:** supported by established evidence.
- **Engineering:** possible or demonstrated technology.
- **Experimental:** plausible research without a working mission system.
- **Fictional:** invented for the story.

## 8. Core Gameplay Loop

1. **Explore** a room, facility, or Enceladus location.
2. **Investigate** terminals, objects, signals, and conversations.
3. **Understand** the relevant scientific relationship.
4. **Solve** a spatial, logic, classification, or systems puzzle.
5. **Decide** how Areeba responds or which risk she accepts.
6. **Unlock** a clue, relationship change, or mission capability.

## 9. Gameplay Systems

### Exploration

- Top-down movement in compact, readable spaces.
- Keyboard, pointer, and touch controls.
- Context-sensitive interaction prompts.
- No pixel hunting; important objects have consistent visual language.

### Dialogue

- Short exchanges with player responses.
- Choices reveal personality or affect trust, clues, and later options.
- Knowledge checks provide useful feedback instead of punishment.
- Dialogue remains skippable and can be reviewed in a log in the full game.

### Investigation

- Mission files contain optional detail and one required clue.
- Environmental clues connect the family story to the scientific mystery.
- Players can distinguish observed data from Alice's private hypotheses.

### Science puzzles

Planned puzzle families:

- Route a magnetic field without overloading containment coils.
- Identify solar particles from energy and charge readings.
- Reconstruct a corrupted transmission using timing data.
- Balance habitat heat, power, oxygen, and water systems.
- Compare plume samples while avoiding contamination.

### Mission decisions

- Decisions trade safety, scientific discovery, time, and trust.
- Choices should change later dialogue or puzzle conditions.
- The final game should not have a single obviously correct moral answer.

## 10. Vertical Slice

### Purpose

Prove the emotional hook, interaction model, visual identity, and science presentation before committing to a larger engine or campaign.

### Target playtime

10-15 minutes for version 0.2.

### Included content

- Title and mission briefing screen.
- Dinner scene with Alice and Areeba.
- One player dialogue choice with responsive feedback.
- Areeba's room as the movement tutorial.
- Alice's lab as an exploration scene.
- Magnetic-containment routing puzzle that unlocks the terminal.
- Interactive SPA-077 mission file.
- Hidden note and ten-year cliffhanger.
- Keyboard and touch controls.
- Restart capability.

### Explicitly excluded

- Enceladus gameplay.
- Save slots and cloud sync.
- Inventory management.
- Voice acting and original soundtrack.
- User accounts, analytics, and network services.

## 11. Functional Requirements

| ID | Requirement | Priority |
| --- | --- | --- |
| FR-01 | The player can start and restart the prologue. | Must |
| FR-02 | Dialogue advances with a visible button and keyboard input. | Must |
| FR-03 | The player can make at least one dialogue choice. | Must |
| FR-04 | Areeba can move using WASD, arrow keys, and touch controls. | Must |
| FR-05 | The interface communicates the current objective. | Must |
| FR-06 | Nearby interactive objects show a clear prompt. | Must |
| FR-07 | The player can enter the lab and read the SPA-077 file. | Must |
| FR-08 | Reading the mission file unlocks the hidden note. | Must |
| FR-09 | Reading the note completes the slice and shows the cliffhanger. | Must |
| FR-10 | The prototype works at widths down to 320 CSS pixels. | Must |
| FR-11 | All controls can be operated without a mouse. | Must |
| FR-12 | Important state changes are announced to assistive technology. | Should |
| FR-13 | The SPA-077 file unlocks only after a valid magnetic route is tested. | Must |

## 12. Controls

| Action | Keyboard | Touch / pointer |
| --- | --- | --- |
| Move | WASD or arrow keys | Direction pad |
| Interact | E or Enter | Interact button |
| Advance dialogue | Enter, Space, or visible button | Continue button |
| Choose response | Tab and Enter, or click | Tap response |
| Close file | Escape or close button | Close button |
| Restart | Restart button | Restart button |

## 13. User Experience Requirements

- Text remains readable over every background.
- Controls are at least 24 by 24 CSS pixels and at least 44 by 44 on coarse pointers.
- Focus indicators are always visible for keyboard users.
- Dialogue and objectives use semantic HTML, not image-only text.
- Layout accounts for phone safe areas.
- Spatial animation is optional and respects reduced-motion settings.
- No rapid flashing or unavoidable timed input.
- The prototype contains no autoplaying audio.

## 14. Visual Direction

### Style

Stylized retro-future mission graphics rather than photorealism. Domestic scenes feel warm and amber; research spaces use cold cyan instrumentation; Enceladus uses white, blue, and near-black.

### Visual language

- Amber indicates story clues and human connection.
- Cyan indicates observed scientific data.
- Magenta indicates experimental or unstable technology.
- Dashed frames and index numbers evoke classified mission documents.
- Character silhouettes remain simple until final character art is approved.

## 15. Audio Direction

Planned production direction:

- Warm household ambience during the prologue.
- Sparse electronics and distant mechanical tones in the lab.
- A small recurring musical motif shared by Alice and Areeba.
- Captions and transcripts for all important speech and sounds.

No audio is required for the first browser prototype.

## 16. Technical Plan

### Prototype stack

- Semantic HTML for game UI and dialogue.
- CSS for environments, responsive layout, and art direction.
- Vanilla JavaScript for the state machine, movement, and interactions.
- Node.js built-in modules for the local development server.
- No runtime dependencies.

### Why web first

- It is playable immediately on the available machine.
- It validates story and interaction before an engine decision.
- It supports desktop and touch testing from one build.
- It avoids committing early to a large 3D production pipeline.

### Engine gate

Evaluate Godot, Unity, or continued web development after the vertical slice. Move only when 3D environments, larger content pipelines, console targets, or advanced audio make the migration worthwhile.

## 17. Non-Functional Requirements

- Initial load under 2 MB before audio is added.
- No required network requests after loading.
- Stable movement at a target of 60 frames per second on a typical laptop.
- No uncaught console errors during a complete playthrough.
- Source code remains modular enough to replace placeholder art and story data.
- Chrome, Safari, Firefox, and Edge current versions are the initial browser target.

## 18. Success Metrics

The version 0.2 test succeeds when:

- 80% of first-time testers complete the slice without verbal instructions.
- 80% can explain the difference between cosmic rays and SEPs afterward.
- 70% want to know what happened to Alice.
- Median completion time is between 5 and 10 minutes.
- No tester becomes blocked by movement or interaction controls.

No telemetry is included in the prototype. These metrics are collected through supervised playtests and a short feedback form.

## 19. Risks And Mitigations

| Risk | Impact | Mitigation |
| --- | --- | --- |
| The game becomes a slideshow | Low engagement | Require an interaction or decision for every major information reveal. |
| Scientific claims undermine trust | Educational harm | Tag speculation and conduct a science review before release. |
| Full game scope grows too quickly | Project stalls | Finish and test one vertical slice before adding Enceladus systems. |
| The mystery lacks a clear payoff | Story disappointment | Define Alice's fate and the ending before full production. |
| Touch controls obscure the scene | Poor mobile experience | Use compact controls, safe areas, and responsive layouts. |
| Engine migration causes rework | Lost development time | Keep story data and game rules independent from rendering code. |

## 20. Milestones

### M0: Foundation

- Approve this PRD.
- Build project shell and title screen.
- Implement the prologue state machine.

### M1: Playable Prologue

- Complete dinner dialogue and choice.
- Add bedroom and lab exploration.
- Add mission file, note, and ending.
- Verify keyboard and touch controls.

### M2: Science Puzzle

- Add a magnetic-field routing puzzle. Completed in prototype 0.2.
- Add observed/experimental/fictional labels.
- Conduct the first five-player playtest.

### M3: Vertical Slice Polish

- Replace key placeholder art.
- Add original sound and captions.
- Improve pacing based on playtest results.
- Package a shareable release build.

### M4: Production Decision

- Confirm target platforms and final engine.
- Lock the complete chapter outline.
- Estimate art, audio, writing, and engineering work.

## 21. Version 0.2 Acceptance Criteria

- A new player can progress from the title screen to the ending without developer tools.
- The opening choice responds to either answer and never dead-ends.
- Movement stays inside each playable room.
- Interaction activates only near the correct object.
- The mission file must be read before the note appears.
- The mission file remains locked until the magnetic-containment puzzle is solved.
- The experience is usable with keyboard only.
- Touch direction controls support press-and-hold movement.
- Content fits a 320-pixel-wide screen without horizontal scrolling.
- Reduced-motion users do not receive spatial entrance animations.
- Automated JavaScript checks and model tests pass.

## 22. Prioritized Backlog

### Next

- Magnetic containment puzzle.
- Dialogue history.
- Settings for text size and sound.
- Save and resume within the current chapter.
- Character portraits and environment concept art.

### Later

- Adult Areeba and the 2038 mission briefing.
- Enceladus habitat hub.
- Oxygen, heat, water, and power management puzzles.
- Plume research expedition.
- Branching trust relationships with the mission crew.
- Multiple endings tied to Alice and SEP-MPS-269.

## 23. Open Product Decisions

- Alice's exact fate and whether she is physically near Enceladus.
- Areeba's ages in 2028 and 2038.
- Whether the full game remains 2D or moves to 3D.
- Final mission travel method and scientifically defensible duration.
- Single-language release or English/Hindi localization at launch.
- Commercial release, educational distribution, or both.

These decisions are intentionally left open until the playable prologue is tested.
