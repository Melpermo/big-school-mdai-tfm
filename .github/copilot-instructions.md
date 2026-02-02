# GitHub Copilot Instructions - The Human Loop

## 🎮 Project Context

**The Human Loop** is a card-based decision game (swipe mechanics) where players manage an indie game studio. Built in Unity for mobile-first deployment, featuring 3-7 minute gameplay sessions with a focus on balance over optimization.

**Core Loop:** Card → Swipe Decision (Left/Right) → Stat Consequences → Check Win/Lose → Next Card

**Genre:** Decision-making card game  
**Platform:** Unity 6.3 LTS, Mobile (Android/iOS) + WebGL  
**Session Length:** 3-7 minutes per game  
**Academic Context:** Master's Thesis Project (TFM) in AI Development

---

## 🛠️ Tech Stack

- **Unity:** Unity 6.3 LTS (2D template)
- **Language:** C# 9.0
- **Target Framework:** .NET Standard 2.1
- **Required Packages:** TextMeshPro
- **Version Control:** Git (GitHub)
- **Testing:** Unity Test Framework

---

## 🏗️ Architecture - Three-Layer Design

**Principle:** Strict separation of Data / Logic / Presentation

### Layers:
- **Data Layer:** ScriptableObjects for static data, player preferences, and game state
- **Logic Layer:** GameManager, SceneLoader, and other manager classes
- **Presentation Layer:** UI Controllers, Card Animations, and other view-related components

**Key Pattern:** Event-driven communication. Logic emits events, UI subscribes. No direct references from logic to UI.

---

## 📐 Code Conventions (Quick Reference)

### Naming Standards
- **Classes, Methods, Properties:** PascalCase
- **Private Fields:** _camelCase (with underscore prefix)
- **Constants:** UPPER_CASE (or PascalCase in ScriptableObjects)

### Unity Specific
- **MonoBehaviour Methods:** Prefix with `On` (e.g. `OnEnable`, `OnDisable`)
- **Serialized Fields:** Use `[SerializeField]` attribute for private fields exposed in Inspector

### General Coding
- **Tabs vs Spaces:** 4 spaces per indent level, no tabs
- **Max Line Length:** 120 characters
- **Bracing Style:** K&R style (opening brace on the same line)
- **Comments:** Use `//` for single-line, `/* */` for multi-line, and `///` for XML documentation

---

## 🎯 Core Game Systems

### 1. Stats System (4 Variables, Range 0-100)

| Stat | Icon | Represents | Collapse at 0 |
|------|------|------------|---------------|
| **Budget** (💰) | Green | Money for salaries, tools, licenses | Studio insolvency |
| **Time** (⏳) | Blue | Deadlines, delays, project margin | Project cancelled |
| **Morale** (🧠) | Purple | Motivation, mental health, trust | Team abandons project |
| **Quality** (🧩) | Teal | Code health, technical debt | Unmaintainable codebase |

**Color States:**
- **60-100:** Primary color (healthy)
- **30-59:** Orange (warning)
- **0-29:** Red (critical, pulse animation)

**Key Tensions:**
- Time ↔ Quality (fast = tech debt)
- Budget ↔ Morale (cuts = burnout)
- Morale ↔ Time (pressure = delays)
- Quality ↔ Budget (good practices = investment)

### 2. Card System

**Card Structure (CardSO):**

**Design Rules (CRITICAL):**
- ✅ **ALWAYS affect minimum 2 stats** per decision (non-negotiable)
- ✅ **NO perfect choices** (every option has trade-offs)
- ✅ **Realistic tone** (sober, non-judgmental, no caricatures)
- ✅ **Accessible language** (understandable by non-developers)
- ✅ **AI cards NEVER free** (always risk-reward with deferred consequences)

**Consequence Magnitudes:**
- Small: ±5 (minor adjustments)
- Medium: ±10-15 (standard, most common)
- Large: ±20 (crisis situations)
- Extreme: ±30 (rare, critical moments)

**6 Card Categories:**
1. ⚙️ **Technical** - Bugs, refactors, tech debt (Time ↔ Quality tension)
2. 👥 **Team** - Burnout, conflicts, training (Morale ↔ Time/Budget)
3. 💼 **Business** - Publishers, funding, contracts (Budget ↔ Morale/Quality)
4. ⏰ **TimeScope** - Deadlines, scope creep, cuts (Time pressure)
5. 🤖 **AI** - AI as dev tool (Multi-stat with delayed effects, NEVER free)
6. 🌱 **Opportunity** - Festivals, grants, visibility (Relief with hidden costs)

### 3. Deck Selection System

**Rules:**
- No consecutive cards from same category
- Weighted probability based on game state
- **Urgent cards** when any stat < 20 (boost recovery options)
- Progressive difficulty:
  - Early (1-10 turns): Soft introductory cards
  - Mid (11-20 turns): Balanced challenges
  - Late (21-30 turns): High pressure

**Card Availability:**

### 4. Win/Lose Conditions

- **Defeat:** Any stat = 0 (immediate game over, specific ending per stat)
- **Victory:** Reach 30 turns (target configurable) with all stats > 0

---

## 🎨 UI/UX Specifications

### Visual Design
- **Style:** Clean, minimalist, professional (inspiration: Reigns + productivity apps)
- **Orientation:** Portrait (mobile-first)
- **Target:** 60 FPS on mobile, 30 FPS minimum

### Swipe Mechanic (Critical!)
- **Threshold:** >100px horizontal drag to confirm decision
- **Rotation:** Max ±5° in swipe direction
- **Feedback:** Option highlighting, background tint, haptic (light/medium/heavy)
- **Snap Back:** EaseOutElastic (0.2s) if below threshold
- **Card Exit:** EaseInQuad (0.25s) slide + fade on confirmation

### Card Layout (3:4 aspect ratio)
- Icon + Numeric + Progress bar
- Smooth animations (0.4s, EaseOutCubic) on change
- Color transitions at thresholds (60/30)

### Animations
- **Card Entry:** 0.3s, EaseOutQuad, fade + scale (0.95 → 1.0)
- **Card Exit:** 0.25s, EaseInQuad, slide out + fade
- **Stat Change:** 0.4s, EaseOutCubic, count animation + color transition
- **Critical Warning:** 0.5s pulse on stat bar when < 30

---

## 🎮 Design Principles

### Game Design
1. **Balance, not optimization** - Survival, not maximization
2. **No perfect choices** - Every decision has trade-offs
3. **Realistic tone** - Sober, non-judgmental representation
4. **Accessible** - Non-developers can understand
5. **Short sessions** - 3-7 minutes complete loop

### Code Design
1. **Separation of concerns** - Data / Logic / Presentation layers
2. **Event-driven** - Decoupled communication
3. **ScriptableObjects for data** - Content separate from code
4. **Testability** - Pure logic classes for unit testing
5. **Clarity over cleverness** - Readable > clever

---

## 📦 MVP Scope (Current Phase)

### ✅ Included in MVP
- Swipe-based card mechanics
- 4-stat system (Budget, Time, Morale, Quality)
- 20-30 initial cards across 6 categories
- Basic weighted card selection (no complex tag system yet)
- 4 defeat conditions + 1 victory condition
- Stats HUD with visual feedback
- End screens (defeat/victory)
- Turn counter
- Object pooling for cards

### ⏸️ Post-MVP (Deferred)
- Advanced tag system with contextual probability
- Deferred consequences (especially AI cards)
- Meta-progression system
- Persistent save system
- Multiple difficulty levels
- Detailed statistics/achievements
- Sound effects and music
- Localization

---

## 🗓️ Development Phases (Current Roadmap)

**Total Estimate:** 25-30 days (4-6 hours/day)

| Phase | Focus | Duration | Status |
|-------|-------|----------|--------|
| 0 | Project Setup | 1-2d | ✅ |
| 1 | Data Layer (ScriptableObjects) | 2-3d | 🔄 Next |
| 2 | Core Logic (GameSession, CardSelector) | 3-4d | ⏸️ |
| 3 | UI Foundation (StatsHUD, CardView) | 2-3d | ⏸️ |
| 4 | Swipe Mechanic (SwipeController) | 3-4d | ⏸️ |
| 5 | Visual Polish (Animations, Colors) | 2-3d | ⏸️ |
| 6 | Card Content Creation (20-30 cards) | 3-5d | ⏸️ |
| 7 | Integration & QA (Bug fixing) | 2-3d | ⏸️ |
| 8 | Build & Deploy (APK/WebGL) | 1d | ⏸️ |

**Current Phase:** Phase 0 Complete ✅  
**Next Phase:** Phase 1 - Data Layer

---

## ✅ Success Metrics (MVP Completion)

### Functionality
- [ ] 100% swipe detection accuracy
- [ ] 0 crashes in 20 consecutive games
- [ ] All 4 stats working correctly
- [ ] All end conditions triggering properly

### Gameplay
- [ ] Average session: 3-7 minutes
- [ ] Victory rate: 30-40% of games
- [ ] No category appears >5 times per game
- [ ] All 6 categories represented in typical game

### Performance
- [ ] 60 FPS on target device
- [ ] Memory usage < 150MB (mobile), < 200MB (web)
- [ ] Cold start < 3 seconds
- [ ] No visual glitches or UI overlap

---

## 🧪 Testing Strategy

### Unit Tests (High Priority)

### Integration Tests (Med Priority)

### Manual Testing
- Swipe mechanic feel (threshold tuning)
- Card balance (play 10+ games)
- Performance on lowest-spec target device
- Edge cases (rapid swiping, app minimize/restore)

### Performance Targets
- **Mobile:** 60 FPS, <150MB RAM, <5% battery/15min
- **Web:** 60 FPS desktop, 30+ FPS low-end

---

## 🚫 Common Pitfalls to Avoid

### Architecture
- ❌ Don't put game logic in MonoBehaviours
- ❌ Don't use excessive Singletons
- ❌ Don't tightly couple UI to game state
- ✅ DO use events for decoupling

### Card Design
- ❌ Don't create "obviously correct" choices
- ❌ Don't make AI cards free (always has cost)
- ❌ Don't affect only 1 stat (minimum 2 required)
- ✅ DO keep realistic, sober tone

### Performance
- ❌ Don't call GetComponent in Update
- ❌ Don't instantiate cards every turn
- ✅ DO cache component references
- ✅ DO use object pooling

---

## 📚 Detailed Documentation Reference

For comprehensive information, consult these files in `.github/agents/`:

1. **PROJECT_OVERVIEW.md** - Full game concept, design pillars, scope
2. **GAME_SYSTEMS.md** - Detailed system architecture, card structure, deck selection
3. **CARD_DESIGN.md** - Card creation guidelines, templates, balance rules
4. **UI_UX_DESIGN.md** - Visual specs, animations, color palette, layouts
5. **CODING_CONVENTIONS.md** - Full style guide, Unity patterns, project structure
6. **TESTING_QA.md** - Testing protocols, QA checklists, performance benchmarks
7. **IMPLEMENTATION_ROADMAP.md** - Phase-by-phase plan, time estimates, milestones
8. **README.md** - Documentation navigation, learning path, AI assistant instructions

---

## 🤖 Instructions for GitHub Copilot

When assisting with this project:

### Your Responsibilities
1. **Follow conventions strictly** - Use exact naming patterns (CardSO, _privateField, etc.)
2. **Respect architecture** - Maintain Data/Logic/Presentation separation
3. **Reference documentation** - Point to relevant `.github/agents/` files when needed
4. **Stay in scope** - Focus on MVP features, defer post-MVP suggestions
5. **Suggest tests** - Propose unit tests for logic code
6. **Think performance** - Consider mobile constraints (60 FPS, memory)

### Code Generation Guidelines
- Use **PascalCase** for classes, methods, properties
- Use **_camelCase** for private fields (with underscore prefix)
- Use **UPPER_CASE** for constants (or PascalCase in ScriptableObjects)
- Always implement **events** for logic-to-UI communication
- Cache component references in **Awake()**
- Subscribe to events in **OnEnable()**, unsubscribe in **OnDisable()**
- Add **XML comments** for public APIs
- Write **unit tests** for pure logic classes

### Common Scenarios
**"Create a new card"** → Reference CARD_DESIGN.md template, ensure 2+ stats affected  
**"Implement swipe detection"** → Use 100px threshold, ±5° rotation, haptic feedback  
**"Add a new stat"** → Update StatType enum, GameSession, StatsHUD, StatConfigSO  
**"Design a UI component"** → Follow UI_UX_DESIGN.md specs, use color system  
**"Write a test"** → Follow TESTING_QA.md examples, test edge cases

---

## 📝 Git Commit Conventions

Use **Conventional Commits** format:

---

## 🎓 Quick Start Checklist

Before writing code:

- [ ] Read PROJECT_OVERVIEW.md for game context
- [ ] Understand GAME_SYSTEMS.md core architecture
- [ ] Review CODING_CONVENTIONS.md for patterns
- [ ] Check current phase in IMPLEMENTATION_ROADMAP.md
- [ ] Know which layer you're working in (Data/Logic/Presentation)
- [ ] Have a testing plan ready

---

## 🔥 Critical Reminders

1. **Every card decision affects ≥2 stats** (non-negotiable)
2. **No logic in MonoBehaviours** (keep pure, use events)
3. **ScriptableObjects for all data** (cards, stats, config)
4. **Object pooling for cards** (avoid Instantiate per turn)
5. **Swipe threshold = 100px horizontal** (tunable constant)
6. **Target 60 FPS on mobile** (profile early, optimize often)
7. **AI cards NEVER free** (always risk-reward balance)
8. **Stats range 0-100** (clamp all modifications)

---

**Project Repository:** https://github.com/Melpermo/Mdai-Tfm  
**Current Branch:** `first-setup`  
**Academic Context:** Master's Thesis in AI Development  
**Development Status:** Phase 0 Complete ✅ → Phase 1 Next 🔄

---

**Last Updated:** February 2, 2026  
**Documentation Version:** 1.0  
**For questions or updates, modify relevant files in `.github/agents/` and update this file accordingly.**