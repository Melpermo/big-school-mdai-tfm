# The Human Loop - Project Overview

## Project Identity
**Game Title:** The Human Loop  
**Genre:** Decision-making card game (swipe mechanics)  
**Platform:** Unity  
**Target:** Mobile-first, web-capable  
**Session Length:** 3-7 minutes per game

## Core Concept
A card-based decision game where players manage a small indie game development studio. Players face technical, human, economic, and creative challenges during development, making quick decisions by swiping cards left or right.

## Key Design Pillars
1. **Balance, not optimization**: The goal is to maintain equilibrium, not maximize stats
2. **Realistic tone**: Sober, slightly ironic, non-judgmental representation of game development
3. **No perfect choices**: Every decision solves one problem while creating another
4. **Accessible**: Understandable for non-developers, with deeper reading for developers
5. **Short sessions**: Complete gameplay loop in under 10 minutes

## Core Gameplay Loop
1. Card presented with a situation
2. Player reads and swipes card (left or right)
3. Decision executed
4. Consequences applied to studio stats
5. Check for defeat or victory
6. Next card appears

## Win/Lose Conditions
- **Defeat**: Any stat reaches 0 (immediate game over with specific ending)
- **Victory**: Reach target number of cards (e.g., 30 turns) without collapsing

## Project Scope (MVP)
- Complete swipe mechanics
- 4 core stats system
- Initial pool of 20-30 cards
- Basic defeat/victory endings
- Card images (design phase)
- No AI gameplay integration yet
- No meta-progression or persistent saves

## Technical Philosophy
- Clear separation: Data / Logic / Presentation
- ScriptableObjects for data
- Runtime GameSession for state
- Modular card selection system
- Event-driven consequences
