# Game Systems Architecture

## Core Stats System (4 Variables)

### 💰 Budget (Presupuesto)
- **Range:** 0-100
- **Represents:** Available money for salaries, tools, licenses, external services
- **Collapse at 0:** Studio cannot continue (insolvency)
- **Affected by:** Business decisions, hiring, tools, outsourcing

### ⏳ Time (Tiempo)
- **Range:** 0-100
- **Represents:** Time pressure, deadlines, delays, project margin
- **NOT real-time:** Represents maneuver room
- **Collapse at 0:** Project cancelled due to delays
- **Affected by:** Scope changes, technical debt, team efficiency

### 🧠 Team Morale (Moral del equipo)
- **Range:** 0-100
- **Represents:** Motivation, cohesion, mental health, trust
- **Collapse at 0:** Team abandons or breaks apart
- **Affected by:** Crunch, ethical decisions, automation, conflicts, recognition

### 🧩 Technical Quality (Calidad técnica)
- **Range:** 0-100
- **Represents:** Code health, architecture, technical debt
- **Collapse at 0:** Project becomes unmaintainable
- **Affected by:** Quick patches, refactors, technical decisions, AI usage

## Stats Interactions

### Key Tensions
- **Time ↔ Quality**: Fast solutions create technical debt
- **Budget ↔ Morale**: Cost-cutting affects team wellbeing
- **Morale ↔ Time**: Team pressure affects deadlines
- **Quality ↔ Budget**: Good practices require investment

### Balance Philosophy
No stat should be maximized. The goal is to keep all stats above 0 until project completion.

## Card System Architecture

### Card Structure
```
Card
├── Title (string)
├── Situation Text (string)
├── Left Option (string)
├── Right Option (string)
├── Left Consequences (StatChanges, TagChanges, DeferredEffects)
├── Right Consequences (StatChanges, TagChanges, DeferredEffects)
├── Category (enum)
├── Image (Sprite)
├── Weight (float)
├── Cooldown (int turns)
├── MinTurn / MaxTurn (int)
├── RequiredTags (List<string>)
└── BlockedTags (List<string>)
```

### Consequence Types
1. **Immediate Stat Changes**: Applied instantly (minimum 2 stats per decision)
2. **Tag Activation/Removal**: Changes context for future cards
3. **Deferred Effects**: Consequences that trigger later (especially AI cards)

## Card Categories (Tags)

### ⚙️ Technical
- Focus: Bugs, refactors, technical debt, architecture
- Tension: **Time ↔ Quality**
- Examples: "Critical bug found", "Refactor proposal", "Legacy code issue"

### 👥 Team
- Focus: Conflicts, burnout, motivation, organization
- Tension: **Morale ↔ Time/Budget**
- Examples: "Team member burnout", "Conflict between developers", "Request for training"

### 💼 Business
- Focus: Publishers, contracts, funding, monetization
- Tension: **Budget ↔ Morale/Quality**
- Examples: "Publisher offer", "Funding opportunity", "Monetization decision"

### ⏰ Time/Scope
- Focus: Deadlines, scope creep, cuts, delays
- Tension: **Time pressure**
- Examples: "Feature request from marketing", "Deadline moved forward", "Scope reduction needed"

### 🤖 AI
- Focus: AI as development tool
- **Never free**: Risk-reward with deferred consequences
- Tension: **Multiple stats with delayed effects**
- Examples: "AI code generation offer", "Automated testing tool", "AI-assisted asset creation"

### 🌱 Opportunity
- Focus: Festivals, grants, visibility, community
- **Relief with hidden costs**
- Examples: "Festival invitation", "Grant application", "Community event"

## Deck Selection System

### Selection Rules
1. **No consecutive cards** from same category
2. **Weighted probability** based on current game state
3. **Urgent cards** triggered when stats are critical (< 20)
4. **Progressive difficulty**:
   - Early game (turns 1-10): Soft introductory cards
   - Mid game (turns 11-20): Balanced challenges
   - Late game (turns 21+): High pressure situations

### Card Availability Logic
```
Card can appear IF:
├── CurrentTurn >= MinTurn
├── CurrentTurn <= MaxTurn (if defined)
├── Card not in cooldown
├── All RequiredTags are active
├── No BlockedTags are active
└── Category not same as previous card
```

### Critical State Cards
When any stat < 20:
- Increase probability of cards that can restore that stat
- Add "critical situation" cards specific to that stat
- Reduce probability of cards that would further damage that stat

## Game Session Flow

### Initialization
1. Load card pool
2. Initialize stats at 50 (balanced start)
3. Clear all tags
4. Set turn counter to 0
5. Select first card

### Turn Execution
1. Present card
2. Wait for player decision (swipe)
3. Apply consequences
4. Update tags
5. Check collapse conditions
6. Increment turn counter
7. Check victory condition
8. Select next card

### End Conditions
- **Collapse**: Any stat = 0 → Immediate defeat with specific ending
- **Victory**: Turn count = 30 (or configured value) → Victory with quality-based ending

## Technical Implementation Guidelines

### Data Layer (ScriptableObjects)
- `CardSO`: Individual card data
- `StatConfigSO`: Stat definitions and collapse messages
- `GameConfigSO`: Game parameters (max turns, starting values, etc.)

### Logic Layer (Systems)
- `GameSession`: Runtime state manager
- `CardSelector`: Weighted card selection logic
- `ConsequenceResolver`: Applies stat changes and effects
- `GameFlowController`: Turn progression and end conditions

### Presentation Layer (UI)
- `CardView`: Visual card with swipe interaction
- `StatsHUD`: Real-time stat display
- `EndScreen`: Defeat/victory messages
- `SwipeController`: Input handling and validation

## Performance Considerations
- Pool card objects (avoid instantiation per turn)
- Cache sprite references
- Use events for stat changes (decoupled UI updates)
- Lazy load card images if memory constrained
