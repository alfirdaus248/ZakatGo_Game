# Zakat GO!

## Overview

Zakat GO! is a third-person educational simulation game developed using Unity. The game is designed to teach players about the concept of zakat distribution in an interactive and engaging way.

Players take on the role of an Amil (zakat collector) who must collect zakat from Muzaki (donors) and distribute it to Mustahik (recipients), while managing limited resources and avoiding interference from thieves.

## Play the Game

The playable version of the game is available on Itch.io:

[https://alfirdaus248.itch.io/zakat-go](https://alfirdaus248.itch.io/zakat-go)

## Features

* Third-person character movement and camera system
* Resource collection and distribution mechanics
* Dynamic zakat economy balancing system
* AI-based enemy (Maling) with chase, steal, and escape behavior
* Quick Time Event (QTE) interaction during theft attempts
* Limited inventory (bag capacity) system
* Timer-based win/lose condition
* Randomized NPC spawning for replayability
* Progress tracking UI (Muzaki collected and Mustahik fulfilled)
* Pause menu and tutorial system
* Game result system (win/lose screen)

## Gameplay

### Objective

Collect and distribute all zakat before the timer runs out.

### Core Loop

1. Find Muzaki and collect zakat
2. Manage inventory capacity
3. Find Mustahik and distribute zakat
4. Avoid or defend against Maling (thieves)
5. Complete all distributions to win

### Win Condition

* All Mustahik have received their required zakat

### Lose Conditions

* Time runs out
* Zakat becomes insufficient to fulfill all Mustahik

## Controls

WASD - Move
Mouse - Camera control
F - Interact
Shift - Run
ESC - Pause

## Technical Details

* Engine: Unity (C#)
* Architecture: Component-based system with event-driven updates
* AI: Finite State Machine (FSM) for enemy (Maling) behavior
* Systems Implemented:

  * Inventory system
  * NPC interaction system
  * Economy balancing system
  * Timer system
  * UI update system

## Project Structure

* Assets/Scripts/Player → player movement and interaction
* Assets/Scripts/NPC → Muzaki, Mustahik, Maling logic
* Assets/Scripts/System → inventory, economy, tracker
* Assets/Scripts/UI → UI and game state controllers
* Assets/Scripts/Spawner → NPC spawning system

## Notes

* This project was developed as an educational serious game
* Gameplay is designed to reflect real-world zakat distribution concepts
* The economy system ensures that total zakat supply is sufficient to meet demand
* Some systems use singleton patterns for global access

## Author

M. Hisyam Al Firdaus