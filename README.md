# The Pill

![Gameplay Screenshot/GIF](https://github.com/phucvuongaman/ThePill/blob/main/Assets/Main/Images/h%C3%ACnh%20%E1%BA%A3nh_2026-03-27_153718403.png?raw=true)

A short 3D first-person anomaly detection horror game developed in Unity. This repository contains the source code, specifically published for portfolio and code review purposes.

🔗 **Links:** [Playable Build (Itch.io)](https://phucvuong147.itch.io/the-pill) | [Gameplay Demo (YouTube)](https://youtu.be/f2e8AzceDfo)

## Credits & Roles
- **Programming & System Design:** Vương
- **3D Art & Environment Assets:** Sourced from standard asset packs and community creators (Special thanks to JashiPSX, Aredius (Daniel Arias), McPato, CursedLake, Vinrax, Kkryy, Elbolilloduro, CemCkrc, FrodoUndead).

## Overview
Inspired by games like "The Exit 8" and "I'm on Observation Duty", players are trapped in a looping environment. They must carefully observe their surroundings, detect anomalies, and make correct decisions to survive the night. 

## Technical Details

- **Engine:** Unity 2022.3 LTS
- **Language:** C#
- **Version Control Note:** The game was developed daily using **Unity Version Control (Plastic SCM)**. This GitHub repo is a consolidated snapshot for review.

### Core Architecture & Highlights
- **Event System (Observer Pattern):** Used a centralized `EventManager` to decouple systems. Managers and UI components communicate via events (e.g., `OnDayStart`, `OnPlayerCaught`) without hard dependencies.
- **Finite State Machine (FSM):** Implemented custom generic state machines to handle isolated logic:
  - `PlayerStateMachine`: Manages movement and interactions.
  - `SanityStateMachine`: Controls sanity degradation and triggers visual/audio effects.
  - `AnomalyEnemyStateMachine`: Drives NavMesh-based AI behaviors.
- **Data-Driven Design (Scriptable Objects):** Utilized for data containers and event pooling (`PhasePoolSO`, `AnomalyEventSO`) to easily configure, balance, and randomize daily anomalies without hardcoding.
- **Modular Interaction System:** A raycast-based system using interfaces (`IInteractable`, `IPressInteractable`) to standardize how the player interacts with the environment, making it scalable for new mechanics.

## How to Play
To play the game, please download the standalone `.exe` build from the [Itch.io page](https://phucvuong147.itch.io/the-pill) rather than building from source. A quick walkthrough is also available in the [Demo Video](https://youtu.be/f2e8AzceDfo).
