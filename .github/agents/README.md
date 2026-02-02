# The Human Loop - GitHub Copilot Agent Documentation

## 📚 Documentation Overview

This folder contains comprehensive documentation to guide GitHub Copilot (and developers) through the development of **The Human Loop**, a card-based decision game about managing an indie game development studio.

### Quick Navigation

| Document | Purpose | When to Read |
|----------|---------|--------------|
| [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) | High-level game concept, design pillars, and scope | Start here for context |
| [GAME_SYSTEMS.md](GAME_SYSTEMS.md) | Detailed system architecture, stats, cards, and game logic | Before implementing any game systems |
| [CODING_CONVENTIONS.md](CODING_CONVENTIONS.md) | Code style, Unity best practices, and architecture patterns | Before writing any code |
| [CARD_DESIGN.md](CARD_DESIGN.md) | Guidelines for creating balanced, engaging cards | When designing card content |
| [UI_UX_DESIGN.md](UI_UX_DESIGN.md) | Visual design, animations, and interaction patterns | When implementing UI components |
| [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md) | Phase-by-phase development plan with milestones | For project planning and tracking |
| [TESTING_QA.md](TESTING_QA.md) | Testing strategies, QA protocols, and benchmarks | Throughout development |

---

## 🎮 What is The Human Loop?

A short-session (3-7 min) card game where players manage an indie game studio by making quick decisions via swipe mechanics. Balance four key stats—Budget, Time, Team Morale, and Technical Quality—to ship your game before everything collapses.

**Core Loop:** Card → Swipe Decision → Consequences → Next Card → Repeat

**Win Condition:** Reach 30 turns without any stat hitting 0  
**Lose Condition:** Any stat collapses to 0

---

## 🎯 For GitHub Copilot

When generating code or providing suggestions, please:

1. **Start with context**: Reference the appropriate documentation file first
2. **Follow conventions**: Adhere to the naming and architecture patterns in [CODING_CONVENTIONS.md](CODING_CONVENTIONS.md)
3. **Understand systems**: The game architecture is defined in [GAME_SYSTEMS.md](GAME_SYSTEMS.md)
4. **Consider the roadmap**: Check [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md) for current development phase
5. **Think about testing**: Reference [TESTING_QA.md](TESTING_QA.md) when suggesting test strategies

### Common Development Scenarios

**Scenario: Creating a new ScriptableObject**
→ Check: `CODING_CONVENTIONS.md` (naming), `GAME_SYSTEMS.md` (data structure)

**Scenario: Implementing swipe mechanic**
→ Check: `UI_UX_DESIGN.md` (interaction details), `CODING_CONVENTIONS.md` (architecture)

**Scenario: Designing a new card**
→ Check: `CARD_DESIGN.md` (guidelines), `GAME_SYSTEMS.md` (card structure)

**Scenario: Adding stat change logic**
→ Check: `GAME_SYSTEMS.md` (stats system), `CODING_CONVENTIONS.md` (events pattern)

**Scenario: Creating UI components**
→ Check: `UI_UX_DESIGN.md` (visual specs), `CODING_CONVENTIONS.md` (MonoBehaviour patterns)

---

## 🏗️ Project Architecture at a Glance

```
The Human Loop
│
├── Data Layer (ScriptableObjects)
│   ├── CardSO - Individual card definitions
│   ├── StatConfigSO - Stat properties and collapse messages
│   └── GameConfigSO - Global game settings
│
├── Logic Layer (Pure C#)
│   ├── GameSession - Runtime game state
│   ├── CardSelector - Weighted card selection
│   ├── ConsequenceResolver - Applies stat changes
│   └── Models - Data structures and enums
│
├── Presentation Layer (MonoBehaviour)
│   ├── CardView - Visual card with swipe
│   ├── StatsHUD - Real-time stat display
│   ├── SwipeController - Input handling
│   └── EndScreen - Victory/defeat screens
│
└── Systems Layer
    └── GameFlowController - Orchestrates game loop
```

**Key Principle:** Separation of concerns. Data, logic, and presentation are decoupled.

---

## 📋 Current MVP Scope

### ✅ Included in MVP
- Swipe-based card mechanics
- 4-stat system (Budget, Time, Morale, Quality)
- 20-30 initial cards across all categories
- Basic weighted card selection (no complex tag system)
- Defeat conditions (4 different endings)
- Victory condition (30 turns)
- Stats HUD with visual feedback
- End screens
- Turn counter

### ⏸️ Post-MVP (Future)
- Advanced card selection with tag system
- Deferred consequences from AI cards
- Meta-progression
- Persistent saves
- Multiple difficulty levels
- Statistics tracking
- Achievements
- Sound and music
- Localization

---

## 🚀 Getting Started (Development)

### For New Developers

1. **Read** [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) - Understand the game concept
2. **Study** [GAME_SYSTEMS.md](GAME_SYSTEMS.md) - Learn the core systems
3. **Review** [CODING_CONVENTIONS.md](CODING_CONVENTIONS.md) - Adopt the code style
4. **Check** [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md) - See current phase
5. **Set up** your Unity project following Phase 0 guidelines

### For GitHub Copilot

When you receive a code request:

1. Determine which system it relates to
2. Reference the appropriate documentation
3. Apply the coding conventions
4. Consider the current development phase
5. Suggest tests if applicable

---

## 🎨 Design Principles (Quick Reference)

### Game Design
- **No perfect choices**: Every decision has trade-offs
- **Realistic tone**: Sober, non-judgmental, slightly ironic
- **Balance, not optimization**: Goal is survival, not maximization
- **Accessible**: Understandable to non-developers
- **Short sessions**: 3-7 minutes per game

### Code Design
- **Separation of concerns**: Data / Logic / Presentation
- **Event-driven**: Decoupled communication between systems
- **ScriptableObjects for data**: Keep content separate from code
- **Testability**: Pure logic classes for unit testing
- **Clarity over cleverness**: Readable code is maintainable code

### UI/UX Design
- **Mobile-first**: Optimized for touch
- **Clear feedback**: Immediate response to all interactions
- **Visual hierarchy**: Important info stands out
- **Performance**: 60 FPS on target devices
- **Accessibility**: Consider color blindness, text size

---

## 🔄 Development Workflow

### Standard Feature Implementation

1. **Design Phase**
   - Review relevant documentation
   - Define data structures (ScriptableObjects if needed)
   - Plan class architecture

2. **Implementation Phase**
   - Create data layer (if needed)
   - Implement logic layer (pure C# classes)
   - Add presentation layer (MonoBehaviour)
   - Connect to game flow

3. **Testing Phase**
   - Write unit tests for logic
   - Manual testing for feel
   - Integration testing
   - Performance check

4. **Polish Phase**
   - Add animations
   - Refine feedback
   - Balance tuning
   - Bug fixing

---

## 📊 Key Metrics & Targets

### Performance
- **Frame Rate:** 60 FPS (mobile), 30 FPS minimum
- **Memory:** < 150MB (mobile), < 200MB (web)
- **Load Time:** < 3 seconds cold start
- **Session Length:** 3-7 minutes average

### Gameplay
- **Victory Rate:** 30-40% of games
- **Turn Count:** 30 turns for victory
- **Stat Balance:** All 4 stats should cause defeats roughly equally
- **Card Variety:** No category appears more than 5 times per game

---

## 🐛 Common Pitfalls to Avoid

Based on documentation across all files:

### Architecture
- ❌ Don't put game logic in MonoBehaviours
- ❌ Don't use Singleton pattern excessively
- ❌ Don't tightly couple UI to game state
- ✅ Do use events for communication
- ✅ Do separate data, logic, and presentation

### Cards
- ❌ Don't create "obviously correct" choices
- ❌ Don't make AI cards "free" (always has cost)
- ❌ Don't use excessive jargon without context
- ✅ Do affect minimum 2 stats per decision
- ✅ Do keep realistic, sober tone

### Performance
- ❌ Don't call GetComponent in Update
- ❌ Don't instantiate GameObjects every turn
- ❌ Don't use string concatenation in loops
- ✅ Do cache component references
- ✅ Do use object pooling for cards
- ✅ Do use StringBuilder for string building

---

## 🔧 Technical Requirements

### Unity Setup
- **Version:** Unity 2022.3 LTS or newer
- **Template:** 2D
- **Packages:** TextMeshPro (required)
- **Target:** Mobile (Android/iOS) and/or WebGL

### Development Tools
- IDE: Visual Studio or JetBrains Rider
- Version Control: Git
- Testing: Unity Test Framework
- Design: Spreadsheet software for card balance

---

## 📈 Project Status Tracking

Reference [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md) for detailed phase-by-phase progress.

### Quick Phase Checklist
- [ ] Phase 0: Project Setup (1-2 days)
- [ ] Phase 1: Data Layer (2-3 days)
- [ ] Phase 2: Core Logic (3-4 days)
- [ ] Phase 3: UI Foundation (2-3 days)
- [ ] Phase 4: Swipe Mechanic (3-4 days)
- [ ] Phase 5: Visual Polish (2-3 days)
- [ ] Phase 6: Card Content (3-5 days)
- [ ] Phase 7: Integration & Polish (2-3 days)
- [ ] Phase 8: Build & Deploy (1 day)

**Estimated Total:** 25-30 days of focused development

---

## 💡 Tips for Efficient Development

### For Copilot-Assisted Coding

1. **Be specific with context**: "Create a CardSO ScriptableObject following GAME_SYSTEMS.md structure"
2. **Reference conventions**: "Implement using event pattern from CODING_CONVENTIONS.md"
3. **Ask for tests**: "Generate unit tests following TESTING_QA.md guidelines"
4. **Request documentation**: "Add XML comments following CODING_CONVENTIONS.md style"

### For Manual Development

1. **Read before coding**: Always check relevant documentation first
2. **Test incrementally**: Don't wait until the end to test
3. **Follow the roadmap**: Stick to current phase scope
4. **Balance regularly**: Playtest cards as you create them

---

## 🆘 When You Get Stuck

### System Design Questions
→ Read [GAME_SYSTEMS.md](GAME_SYSTEMS.md) for architecture details

### Code Structure Questions
→ Check [CODING_CONVENTIONS.md](CODING_CONVENTIONS.md) for patterns

### Balance/Design Questions
→ Consult [CARD_DESIGN.md](CARD_DESIGN.md) for guidelines

### Implementation Order Questions
→ Follow [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md)

### Visual/UX Questions
→ Reference [UI_UX_DESIGN.md](UI_UX_DESIGN.md)

### Testing Approach Questions
→ See [TESTING_QA.md](TESTING_QA.md)

---

## 📝 Documentation Maintenance

These documents should be updated when:

- Core systems change significantly
- New features are added to MVP scope
- Design principles evolve
- Technical architecture shifts
- Performance targets change

**Keep documentation in sync with code!**

---

## 🎓 Learning Path

### For Understanding The Human Loop

**Beginner** (First time seeing the project):
1. PROJECT_OVERVIEW.md
2. IMPLEMENTATION_ROADMAP.md (skim)
3. Start with Phase 0

**Intermediate** (Ready to implement):
1. GAME_SYSTEMS.md (deep read)
2. CODING_CONVENTIONS.md (deep read)
3. UI_UX_DESIGN.md (as needed)

**Advanced** (Creating content or advanced features):
1. CARD_DESIGN.md (for card creation)
2. TESTING_QA.md (for quality assurance)
3. All docs (as reference)

---

## 🤖 AI Assistant Instructions

If you are an AI assistant (like GitHub Copilot) helping with this project:

### Your Primary Goals
1. Help implement The Human Loop efficiently and correctly
2. Ensure code follows established conventions
3. Maintain separation of concerns in architecture
4. Produce testable, maintainable code
5. Respect the MVP scope boundaries

### Your Knowledge Base
You have access to comprehensive documentation covering:
- Game design and mechanics
- System architecture
- Coding standards
- UI/UX specifications
- Implementation roadmap
- Testing strategies

### When Providing Assistance
- Always reference relevant documentation
- Follow naming conventions exactly
- Use appropriate design patterns
- Consider the current development phase
- Suggest tests when applicable
- Point out potential issues

### Example Interactions

**User:** "Create a card ScriptableObject"
**You should:** Reference GAME_SYSTEMS.md structure, follow CODING_CONVENTIONS.md naming (CardSO), include all required fields

**User:** "Implement swipe detection"
**You should:** Reference UI_UX_DESIGN.md for thresholds and feedback, follow CODING_CONVENTIONS.md for MonoBehaviour patterns

**User:** "Design a new card about AI"
**You should:** Reference CARD_DESIGN.md AI card template and guidelines, ensure risk-reward balance

---

## ✅ Final Checklist Before Coding

Before you start implementing any feature:

- [ ] I've read the relevant documentation section
- [ ] I understand the data structures involved
- [ ] I know which design patterns to use
- [ ] I'm clear on the coding conventions
- [ ] I know how this fits in the overall architecture
- [ ] I have a testing plan
- [ ] I'm working on the correct development phase

---

**Good luck with development!** 🚀

For questions, suggestions, or improvements to this documentation, please update the relevant files and commit them to the repository.
