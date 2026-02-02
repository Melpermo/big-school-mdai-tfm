# Implementation Roadmap

## MVP Scope Definition

### Core Features (Must Have)
- ✅ Swipe mechanic for card decisions
- ✅ 4 stats system (Budget, Time, Morale, Quality)
- ✅ 20-30 initial cards covering all categories
- ✅ Basic card selection (weighted, no repetition)
- ✅ Defeat conditions (any stat = 0)
- ✅ Victory condition (30 turns)
- ✅ Stats HUD with visual feedback
- ✅ End screens (defeat/victory)
- ✅ Turn counter

### Deferred Features (Post-MVP)
- ⏸️ Advanced card selection (tag system, contextual probability)
- ⏸️ Deferred consequences from AI cards
- ⏸️ Meta-progression system
- ⏸️ Persistent save system
- ⏸️ Multiple difficulty levels
- ⏸️ Detailed statistics screen
- ⏸️ Achievement system
- ⏸️ Card encyclopedia/collection
- ⏸️ Sound effects and music
- ⏸️ Localization

## Development Phases

### Phase 0: Project Setup (1-2 days)
**Goal:** Get Unity project ready with proper structure

**Tasks:**
- [ ] Create new Unity project (2D template)
- [ ] Set up folder structure (Scripts, Data, Sprites, Scenes)
- [ ] Configure version control (.gitignore for Unity)
- [ ] Install required packages (TextMeshPro)
- [ ] Create main scene
- [ ] Set up build settings (mobile/web target)

**Deliverable:** Clean project structure ready for development

---

### Phase 1: Data Layer (2-3 days)
**Goal:** Create all ScriptableObjects and data structures

**Tasks:**
- [ ] Create `CardSO` ScriptableObject
  - Title, situation text, options
  - Consequences structure
  - Category enum
  - Image reference
- [ ] Create `StatConfigSO` ScriptableObject
  - Stat definitions
  - Starting values
  - Collapse messages
- [ ] Create `GameConfigSO` ScriptableObject
  - Max turns
  - Victory/defeat settings
- [ ] Create `ConsequenceData` struct
  - Stat changes
  - (Optional: tag changes for later)
- [ ] Create enum definitions
  - `StatType` (Budget, Time, Morale, Quality)
  - `CardCategory` (Technical, Team, Business, etc.)

**Deliverable:** Data structures ready to be populated

**Test:** Create 2-3 test cards in Unity editor

---

### Phase 2: Core Logic (3-4 days)
**Goal:** Implement game logic without UI

**Tasks:**
- [ ] Create `GameSession` class
  - Stats dictionary (StatType → int)
  - Current turn counter
  - Active tags set (for future)
  - Stat modification methods
  - Events: StatChanged, GameEnded
- [ ] Create `ConsequenceResolver` class
  - Apply stat changes
  - Validate stat bounds (0-100)
  - Trigger events
- [ ] Create `CardSelector` class (simplified MVP version)
  - Load all cards
  - Simple weighted random selection
  - Prevent same card twice in a row
  - No complex tag system yet
- [ ] Create `GameFlowController` class
  - Initialize game
  - Present card
  - Process decision
  - Check win/lose conditions
  - Manage turn progression

**Deliverable:** Fully functional game logic (console-testable)

**Test:** Run game logic in Unity editor console
- Start game with test cards
- Make decisions, verify stat changes
- Verify defeat when stat reaches 0
- Verify victory at turn 30

---

### Phase 3: UI Foundation (2-3 days)
**Goal:** Create basic UI without polish

**Tasks:**
- [ ] Create `StatsHUD` MonoBehaviour
  - 4 stat displays (icon + value + bar)
  - Subscribe to GameSession.StatChanged
  - Update displays on change
- [ ] Create `CardView` MonoBehaviour
  - Display title, situation, options
  - (Image placeholder for now)
  - Basic layout
- [ ] Create `EndScreen` MonoBehaviour
  - Victory message
  - Defeat message with collapsed stat
  - Play Again button
- [ ] Create `TurnCounter` MonoBehaviour
  - Display current turn / max turns

**Deliverable:** Functional UI that displays game state

**Test:** Play through game with button-based decisions

---

### Phase 4: Swipe Mechanic (3-4 days)
**Goal:** Implement core swipe interaction

**Tasks:**
- [ ] Create `SwipeController` MonoBehaviour
  - Detect drag input (mouse/touch)
  - Calculate drag distance and direction
  - Emit events: SwipeStarted, SwipeCancelled, SwipeConfirmed
- [ ] Implement card drag behavior in `CardView`
  - Move card with pointer
  - Rotate card based on drag direction
  - Visual feedback for left/right options
  - Snap back if threshold not met
  - Animate out on valid swipe
- [ ] Define swipe threshold constant
  - Horizontal distance required for decision
  - Tune based on device/screen size
- [ ] Connect swipe to game logic
  - SwipeController → CardView → GameFlowController
  - Execute decision on confirmed swipe

**Deliverable:** Working swipe mechanic for card decisions

**Test:** 
- Drag card left/right
- Verify rotation and feedback
- Verify card returns if drag too short
- Verify decision executes on valid swipe

---

### Phase 5: Visual Polish (2-3 days)
**Goal:** Make the game look presentable

**Tasks:**
- [ ] Implement stat color system
  - Green/primary (60-100)
  - Orange/warning (30-59)
  - Red/critical (0-29)
- [ ] Add stat change animations
  - Number count-up/down
  - Progress bar smooth transition
  - Color transitions
- [ ] Add card animations
  - Card entry (fade + slight scale)
  - Card exit (slide + fade)
  - Card snap back (elastic ease)
- [ ] Polish swipe feedback
  - Option highlighting
  - Background tints
  - Smooth transitions
- [ ] Add end screen animations
  - Screen dim
  - End panel slide up
- [ ] Create card background/template
  - Clean design
  - Category color accent

**Deliverable:** Polished, visually coherent game

**Test:** Play through full game, verify all animations work

---

### Phase 6: Card Content Creation (3-5 days)
**Goal:** Create 20-30 balanced cards

**Tasks:**
- [ ] Design 4-5 Technical cards
- [ ] Design 4-5 Team cards
- [ ] Design 4-5 Business cards
- [ ] Design 3-4 Time/Scope cards
- [ ] Design 2-3 AI cards (simplified, no deferred effects yet)
- [ ] Design 2-3 Opportunity cards
- [ ] Create ScriptableObject instances for all cards
- [ ] Balance test: Play multiple times
- [ ] Adjust consequence values based on testing
- [ ] Ensure variety in decision types

**Deliverable:** Complete, balanced card set

**Test:** 
- Play 10+ full games
- Verify variety in cards
- Check if any stats collapse too frequently
- Verify victory is achievable but challenging

---

### Phase 7: Integration & Polish (2-3 days)
**Goal:** Final integration and bug fixing

**Tasks:**
- [ ] Create main menu (basic)
  - Start Game button
  - (Optional: Settings placeholder)
- [ ] Connect all screens
  - Menu → Game → End → Menu
- [ ] Implement Play Again functionality
- [ ] Add turn counter to UI
- [ ] Final balance pass on cards
- [ ] Bug fixing
  - Stat overflow/underflow
  - Card selection edge cases
  - UI glitches
  - Swipe detection issues
- [ ] Performance testing
  - Mobile device testing
  - Memory profiling
  - Frame rate verification

**Deliverable:** Fully playable MVP game

**Test:** Complete QA pass
- Play 20+ games on target device
- Test all edge cases
- Verify all end conditions work
- Check performance

---

### Phase 8: Build & Deploy (1 day)
**Goal:** Create distributable builds

**Tasks:**
- [ ] Configure build settings
  - Platform-specific settings
  - Optimization passes
  - Icon and splash screen
- [ ] Create builds
  - Android APK (if mobile)
  - WebGL (if web)
  - Windows standalone (if desktop)
- [ ] Test builds on target devices
- [ ] Prepare documentation
  - How to play
  - Known issues
- [ ] (Optional) Deploy to itch.io or similar

**Deliverable:** Playable game ready for testing/distribution

---

## Time Estimates

**Optimistic:** 18-24 days  
**Realistic:** 25-30 days  
**Conservative:** 35-40 days

*(Assuming 4-6 hours of focused work per day)*

## Critical Path

The critical dependencies:
```
Phase 1 (Data) → Phase 2 (Logic) → Phase 4 (Swipe)
                                  ↓
Phase 3 (UI) → Phase 5 (Polish) → Phase 7 (Integration)
              ↓
Phase 6 (Cards) → Phase 7 (Integration) → Phase 8 (Build)
```

**Parallel work possible:**
- Phase 3 (UI) can start while Phase 2 (Logic) is finishing
- Phase 6 (Cards) can start as soon as Phase 1 (Data) is done
- Phase 5 (Polish) can happen alongside card creation

## Risk Mitigation

### High Risk Areas
1. **Swipe mechanic feel** (Phase 4)
   - Mitigation: Prototype early, iterate on thresholds
   - Fallback: Button-based decisions if swipe doesn't work

2. **Card balance** (Phase 6)
   - Mitigation: Spreadsheet for stat tracking during design
   - Fallback: Start with wider consequence ranges, narrow later

3. **Performance on mobile** (Phase 7)
   - Mitigation: Profile early, use object pooling
   - Fallback: Reduce animations, simplify visuals

### Medium Risk Areas
1. **Card selection feeling repetitive**
   - Mitigation: Ensure good category distribution
   - Improvement path: Add tag system in post-MVP

2. **Game too short/too long**
   - Mitigation: Make max turns configurable
   - Easy adjustment: Just change constant

## Success Metrics (MVP)

### Core Functionality
- [ ] 100% of swipes register correctly
- [ ] 0 crashes in 20 consecutive games
- [ ] All 4 stats working correctly
- [ ] All end conditions triggering properly

### Gameplay Quality
- [ ] Average game length: 3-7 minutes
- [ ] Players reach turn 30 in ~30-40% of attempts
- [ ] No single card appears more than twice in one game
- [ ] All 6 card categories represented in typical game

### Polish Level
- [ ] All animations smooth (60fps on target device)
- [ ] No visual glitches or UI overlap
- [ ] Text readable at all supported screen sizes
- [ ] Stats update immediately after decisions

## Post-MVP Roadmap (Future)

### Version 1.1: Tag System
- Implement full tag system
- Add card prerequisites and blocks
- Add deferred consequences
- Create advanced AI cards

### Version 1.2: Meta-Progression
- Unlock system for cards
- Player stats tracking
- Multiple difficulty modes
- Achievements

### Version 1.3: Polish & Content
- Sound effects and music
- More cards (expand to 50-60)
- Card images (unique art per card)
- Multiple game endings

### Version 2.0: Advanced Features
- Persistent save system
- Daily challenges
- Leaderboards
- Localization
- Accessibility options

## Development Tools Checklist

- [ ] Unity 2022.3 LTS or newer
- [ ] TextMeshPro package
- [ ] Git for version control
- [ ] Visual Studio or Rider for IDE
- [ ] Unity UI Toolkit (optional, can use uGUI)
- [ ] Spreadsheet software for card design
- [ ] Device for mobile testing (if targeting mobile)

## Documentation Checklist

- [ ] GDD (already created)
- [ ] Code documentation (inline comments)
- [ ] API documentation for public classes
- [ ] Setup instructions (README.md)
- [ ] Build instructions
- [ ] Playtesting notes
- [ ] Known issues log
- [ ] Future feature backlog

## Quality Gates

Before moving to next phase:

**After Phase 1:** 
- All data structures compile without errors
- Can create cards in Unity editor

**After Phase 2:** 
- Game logic can run 30 turns in editor
- All stat changes work correctly

**After Phase 4:** 
- Swipe mechanic feels responsive
- Threshold tuning complete

**After Phase 6:** 
- Can win and lose the game
- Card variety confirmed

**Before Phase 8:** 
- No critical bugs
- Performance acceptable on target device
- All features from MVP scope working
