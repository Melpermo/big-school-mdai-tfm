# Card Design Guidelines

## Core Design Principles

### 1. No Perfect Choices
Every decision must:
- Solve one problem
- Create or worsen another problem
- Affect **minimum 2 stats**
- Have clear trade-offs

**Example:**
```
Situation: "Critical bug found before release"

Left: "Delay release to fix properly"
└─ Time: -15, Quality: +10

Right: "Ship with workaround, fix later"
└─ Quality: -10, Morale: -5 (team knows it's wrong)
```

### 2. Realistic Scenarios
Cards should reflect actual game development situations:
- ✅ "Team member requests training course" 
- ✅ "Publisher asks for additional platform port"
- ✅ "Legacy code causing integration issues"
- ❌ "Alien invasion threatens office"
- ❌ "Win the lottery"

### 3. Neutral Tone
Avoid moralizing or caricaturing:
- ✅ "AI tool offers automated testing. Fast but requires review"
- ❌ "Evil AI wants to steal your job"
- ✅ "Publisher contract includes marketing support but demands exclusivity"
- ❌ "Greedy publisher tries to exploit you"

### 4. Accessibility
Cards must be understandable by non-developers:
- Use plain language
- Explain technical concepts briefly
- Avoid excessive jargon
- Context in the situation text

## Card Anatomy

### Title (5-8 words)
- Clear and specific
- Indicates the type of situation
- **Examples:**
  - "Critical Bug Before Launch"
  - "Team Member Burnout"
  - "Publisher Demands Feature"

### Situation Text (20-50 words)
- Sets the context
- Presents the dilemma
- No hidden information
- **Structure:** Problem → Context → Stakes

**Example:**
```
"Your lead programmer found a critical memory leak in the multiplayer 
system. It only happens under specific conditions, but it could cause 
crashes at launch. You're 2 weeks from release."
```

### Option Text (3-10 words)
- Action-oriented
- Clear consequence implication
- **Examples:**
  - "Delay to fix properly"
  - "Ship now, patch later"
  - "Hire contractor to help"
  - "Cut the feature entirely"

### Consequences
- **Minimum 2 stats** affected per option
- Magnitude: -20 to +20 (balanced), -30 to +30 (extreme)
- Optional: Tag changes, deferred effects

## Card Categories Deep Dive

### ⚙️ Technical Cards

**Focus:** Code quality, architecture, technical debt

**Common Patterns:**
- Bug vs. Feature trade-offs
- Quick fix vs. Proper solution
- Legacy code issues
- Architecture decisions
- Tool and framework choices

**Example Card:**
```yaml
Title: "Refactor or Move On?"
Situation: "Your AI programmer wants to spend 2 weeks refactoring the 
pathfinding system. It works, but it's messy and hard to modify."

Left: "Let them refactor"
Consequences:
  - Time: -20
  - Quality: +15
  - Morale: +5

Right: "Keep moving forward"
Consequences:
  - Quality: -10
  - Time: +5
  
Tags: [technical, debt]
```

### 👥 Team Cards

**Focus:** People, motivation, conflicts, organization

**Common Patterns:**
- Burnout and crunch
- Team conflicts
- Training and growth requests
- Work-life balance
- Recognition and feedback

**Example Card:**
```yaml
Title: "Team Member Wants Remote Work"
Situation: "Your artist is struggling with the commute and asks to work 
remotely 3 days a week. They're productive but team cohesion matters."

Left: "Allow remote work"
Consequences:
  - Morale: +10
  - Time: -5 (coordination overhead)

Right: "Require office presence"
Consequences:
  - Morale: -10
  - Time: +5
  
Tags: [team, flexibility]
```

### 💼 Business Cards

**Focus:** Money, contracts, publishing, monetization

**Common Patterns:**
- Publisher offers and demands
- Funding opportunities
- Monetization decisions
- Contract negotiations
- Platform exclusivity

**Example Card:**
```yaml
Title: "Publisher Advances Deadline"
Situation: "Your publisher wants to move the release forward by one month 
to hit a better launch window. They'll increase marketing budget by 30%."

Left: "Accept new deadline"
Consequences:
  - Time: -20
  - Budget: +15
  - Morale: -10 (crunch)

Right: "Negotiate for original date"
Consequences:
  - Budget: -10 (lower marketing)
  - Morale: +5 (team relieved)
  
Tags: [business, deadline]
```

### ⏰ Time/Scope Cards

**Focus:** Deadlines, feature creep, cuts

**Common Patterns:**
- Scope creep from stakeholders
- Feature cut decisions
- Deadline pressure
- MVP vs. Full vision

**Example Card:**
```yaml
Title: "Marketing Wants New Feature"
Situation: "Marketing believes a social sharing feature would boost 
virality. It wasn't in the original scope and would take 2-3 weeks."

Left: "Add the feature"
Consequences:
  - Time: -20
  - Quality: -5 (rushing integration)

Right: "Stick to current scope"
Consequences:
  - Budget: -10 (potentially lower sales)
  
Tags: [scope, marketing]
```

### 🤖 AI Cards

**Focus:** AI as development tool (NEVER free)

**Design Rules:**
- Always has risk-reward balance
- Often includes deferred consequences
- Affects multiple stats
- May activate "ai_used" tag for future consequences

**Common Patterns:**
- Code generation tools
- Asset creation AI
- Automated testing
- AI-assisted design
- Quality vs. Speed trade-offs

**Example Card:**
```yaml
Title: "AI Code Generation Tool"
Situation: "An AI coding assistant could generate boilerplate and common 
patterns quickly, but the team needs to review and understand all output."

Left: "Adopt the AI tool"
Consequences:
  - Time: +10 (faster initial dev)
  - Quality: -15 (potential hidden bugs)
  - Morale: -5 (some devs uncomfortable)
Tags Added: [ai_used]
Deferred: Chance of bug card in 3-5 turns

Right: "Code manually"
Consequences:
  - Time: -10
  - Morale: +5 (team trusts process)
  
Tags: [ai, tools]
```

### 🌱 Opportunity Cards

**Focus:** Positive events with hidden costs

**Common Patterns:**
- Festival invitations
- Grant applications
- Press opportunities
- Community events
- Award nominations

**Example Card:**
```yaml
Title: "Indie Festival Invitation"
Situation: "A prestigious indie festival invites you to showcase your 
game. Great visibility, but requires a playable demo in 3 weeks."

Left: "Accept invitation"
Consequences:
  - Time: -15 (demo prep)
  - Budget: +10 (potential publisher interest)
  - Morale: +5 (team excited)

Right: "Decline politely"
Consequences:
  - Time: +5
  - Budget: -5 (missed opportunity)
  
Tags: [opportunity, marketing]
```

## Consequence Magnitude Guide

### Small Impact (-5 to +5)
- Minor adjustments
- Side effects
- Long-term cumulative changes

### Medium Impact (-10 to +15)
- Standard decision consequences
- Most cards should use this range
- Noticeable but not devastating

### Large Impact (-20 to +20)
- Major decisions
- Crisis situations
- Late game pressure cards

### Extreme Impact (-30 to +30)
- Rare, critical moments
- Should be uncommon
- Often paired with tags or special conditions

## Tag System Usage

### Common Tags
- `technical_debt` - Affects future technical cards
- `crunch_active` - Team is overworked
- `ai_used` - AI tools in use (affects future outcomes)
- `publisher_relations` - Affects business negotiations
- `community_engaged` - Affects marketing opportunities

### Tag Patterns

**Progressive Tags:**
```
crunch_active_minor → crunch_active → crunch_active_severe
```

**Exclusive Tags:**
```
monetization_premium XOR monetization_freemium
```

**Conditional Cards:**
```
Card: "AI-Generated Bug Report"
Required Tags: [ai_used]
```

## Deferred Effects

Used primarily in AI cards and complex business deals.

**Example:**
```yaml
Card: "Quick AI Asset Generation"
Immediate:
  - Time: +15
  - Quality: -5

Deferred (in 4-6 turns):
  - Trigger: "Asset Consistency Issues" card
  - OR: Quality: -10 if not addressed
```

## Writing Style Guide

### Do:
- Use active voice
- Be specific and concrete
- Show stakes clearly
- Keep it concise
- Use realistic terminology

### Don't:
- Judge or moralize
- Use excessive jargon without context
- Create strawman scenarios
- Include hidden "gotchas"
- Make one choice obviously better

## Balance Testing Checklist

Before finalizing a card:

- [ ] Both choices affect at least 2 stats
- [ ] No choice is strictly better
- [ ] Consequences are proportional to the situation
- [ ] Text is under 50 words
- [ ] Options are clear and actionable
- [ ] Accessible to non-developers
- [ ] Fits the realistic, sober tone
- [ ] Category and tags are appropriate
- [ ] Image concept is defined (if applicable)

## Card Templates

### Standard Decision Template
```yaml
Title: "[Situation Type]"
Situation: "[Problem] [Context] [Stakes]"
Category: [technical/team/business/time/ai/opportunity]

Left: "[Action A]"
Consequences:
  - [Stat]: [value]
  - [Stat]: [value]
Tags: [relevant, tags]

Right: "[Action B]"
Consequences:
  - [Stat]: [value]
  - [Stat]: [value]
Tags: [relevant, tags]

Weight: 1.0
Cooldown: 0
MinTurn: 0
MaxTurn: -1
```

### AI Card Template
```yaml
Title: "[AI Tool/Feature]"
Situation: "[Tool description] [Benefits] [Risks]"
Category: ai

Left: "[Adopt/Use AI]"
Consequences:
  - Time: [usually positive]
  - Quality: [usually negative]
  - Morale: [context dependent]
Tags Added: [ai_used]
Deferred: [Future consequence description]

Right: "[Traditional approach]"
Consequences:
  - Time: [usually negative]
  - Quality: [usually positive]
  - Morale: [context dependent]

Weight: 0.8
Cooldown: 5
```

### Crisis Card Template
```yaml
Title: "[CRISIS: Type]"
Situation: "[Urgent problem] [Immediate stakes] [Time pressure]"
Category: [appropriate]

Left: "[Extreme measure]"
Consequences:
  - [Stat]: [-20 to -30]
  - [Stat]: [+10 to +20]
Tags: [crisis, relevant]

Right: "[Conservative approach]"
Consequences:
  - [Stat]: [-15]
  - [Stat]: [-15]

Weight: 0.5
RequiredTags: [crisis_trigger or stat < 20]
Cooldown: 10
```

## Example Card Set (Starter Pack)

### Early Game (Turns 1-10)
1. "First Team Conflict" - Team category, soft consequences
2. "Publisher Interest" - Business category, introduces contract concept
3. "Minor Bug Found" - Technical category, introduces quality vs. time
4. "Scope Clarification" - Time category, manageable stakes
5. "AI Tool Suggestion" - AI category, gentle introduction

### Mid Game (Turns 11-20)
6. "Growing Technical Debt" - Technical category, medium stakes
7. "Team Burnout Signs" - Team category, morale pressure
8. "Budget Constraints" - Business category, tough choice
9. "Feature Creep" - Time category, scope management
10. "Community Feedback" - Opportunity category, mixed blessing

### Late Game (Turns 21-30)
11. "Critical Launch Bug" - Technical category, high stakes
12. "Team Member Leaving" - Team category, crisis
13. "Publisher Ultimatum" - Business category, major decision
14. "Deadline Crunch" - Time category, extreme pressure
15. "AI-Generated Issue" - AI category (requires ai_used tag), consequence of earlier decision

## Iteration Guidelines

After playtesting:
- Track which cards cause the most collapse
- Monitor average stat values per turn
- Identify "dominant strategies"
- Balance overpowered/underpowered cards
- Adjust consequences by ±5 as needed
