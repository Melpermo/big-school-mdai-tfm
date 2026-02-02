# Testing & QA Guidelines

## Testing Philosophy

- **Test early, test often**: Don't wait until Phase 7
- **Automated where possible**: Unit tests for logic
- **Manual for feel**: Swipe mechanics and balance require human testing
- **Device diversity**: Test on lowest-spec target device

## Unit Testing Strategy

### What to Test

#### Core Logic (High Priority)
```csharp
// GameSession
✅ Stat initialization (all stats start at 50)
✅ Stat modifications within bounds (0-100)
✅ Stat changes trigger events
✅ Collapse detection (stat = 0)
✅ Victory detection (turn = max turns)
✅ Turn increment

// ConsequenceResolver
✅ Stat changes apply correctly
✅ Multiple stat changes in one consequence
✅ Stat clamping (can't go below 0 or above 100)
✅ Events fire after resolution

// CardSelector
✅ Returns card from available pool
✅ Never returns same card twice in a row
✅ Respects card weights
✅ Handles empty pool gracefully
```

### Example Unit Tests

```csharp
[TestFixture]
public class GameSessionTests
{
    private GameSession _session;
    
    [SetUp]
    public void Setup()
    {
        var config = ScriptableObject.CreateInstance<GameConfigSO>();
        config.maxTurns = 30;
        config.startingStatValue = 50;
        _session = new GameSession(config);
    }
    
    [Test]
    public void Initialize_SetsAllStatsTo50()
    {
        // Assert
        Assert.AreEqual(50, _session.GetStat(StatType.Budget));
        Assert.AreEqual(50, _session.GetStat(StatType.Time));
        Assert.AreEqual(50, _session.GetStat(StatType.Morale));
        Assert.AreEqual(50, _session.GetStat(StatType.Quality));
    }
    
    [Test]
    public void ModifyStat_ClampsAtZero()
    {
        // Act
        _session.ModifyStat(StatType.Budget, -60);
        
        // Assert
        Assert.AreEqual(0, _session.GetStat(StatType.Budget));
    }
    
    [Test]
    public void ModifyStat_ClampsAt100()
    {
        // Act
        _session.ModifyStat(StatType.Budget, 60);
        
        // Assert
        Assert.AreEqual(100, _session.GetStat(StatType.Budget));
    }
    
    [Test]
    public void ModifyStat_TriggersStatChangedEvent()
    {
        // Arrange
        StatType capturedStat = StatType.Budget;
        int capturedValue = 0;
        _session.StatChanged += (stat, value) => 
        {
            capturedStat = stat;
            capturedValue = value;
        };
        
        // Act
        _session.ModifyStat(StatType.Time, -10);
        
        // Assert
        Assert.AreEqual(StatType.Time, capturedStat);
        Assert.AreEqual(40, capturedValue);
    }
    
    [Test]
    public void CheckGameEnd_DetectsCollapse_WhenStatIsZero()
    {
        // Arrange
        _session.ModifyStat(StatType.Budget, -50);
        
        // Act
        bool isGameOver = _session.IsGameOver();
        
        // Assert
        Assert.IsTrue(isGameOver);
        Assert.AreEqual(GameEndType.Defeat, _session.GetEndType());
    }
    
    [Test]
    public void CheckGameEnd_DetectsVictory_WhenMaxTurnsReached()
    {
        // Arrange
        for (int i = 0; i < 30; i++)
        {
            _session.IncrementTurn();
        }
        
        // Act
        bool isGameOver = _session.IsGameOver();
        
        // Assert
        Assert.IsTrue(isGameOver);
        Assert.AreEqual(GameEndType.Victory, _session.GetEndType());
    }
    
    [TearDown]
    public void TearDown()
    {
        _session = null;
    }
}
```

```csharp
[TestFixture]
public class CardSelectorTests
{
    private CardSelector _selector;
    private List<CardSO> _testCards;
    
    [SetUp]
    public void Setup()
    {
        _testCards = new List<CardSO>
        {
            CreateTestCard("Card1", CardCategory.Technical),
            CreateTestCard("Card2", CardCategory.Team),
            CreateTestCard("Card3", CardCategory.Business)
        };
        
        _selector = new CardSelector(_testCards);
    }
    
    [Test]
    public void SelectNextCard_NeverSelectsSameCardTwice()
    {
        // Act
        var card1 = _selector.SelectNextCard();
        var card2 = _selector.SelectNextCard();
        
        // Assert
        Assert.AreNotEqual(card1, card2);
    }
    
    [Test]
    public void SelectNextCard_ReturnsNullWhenNoCardsAvailable()
    {
        // Arrange
        _selector = new CardSelector(new List<CardSO>());
        
        // Act
        var card = _selector.SelectNextCard();
        
        // Assert
        Assert.IsNull(card);
    }
    
    private CardSO CreateTestCard(string title, CardCategory category)
    {
        var card = ScriptableObject.CreateInstance<CardSO>();
        card.title = title;
        card.category = category;
        card.weight = 1.0f;
        return card;
    }
}
```

## Integration Testing

### Test Scenarios

#### Full Game Flow
```
Test: Complete game from start to victory
Steps:
1. Start new game
2. Verify stats initialized to 50
3. Make 30 decisions (alternating left/right)
4. Verify turn counter increments
5. Verify victory screen appears
6. Verify final stats displayed
Expected: No errors, smooth progression
```

#### Stat Collapse Detection
```
Test: Game ends when any stat reaches 0
Steps:
1. Start new game
2. Repeatedly choose options that decrease Budget
3. Wait for Budget to reach 0
Expected: Defeat screen with "Insolvency" message
```

#### Card Variety
```
Test: No excessive repetition
Steps:
1. Start new game
2. Track categories of 20 consecutive cards
Expected: No category appears more than 5 times
```

#### Swipe Threshold
```
Test: Invalid swipes return card to center
Steps:
1. Present card
2. Drag card 50px left (below threshold)
3. Release
Expected: Card snaps back to center, no decision made
```

## Manual Playtesting Protocol

### Pre-Test Setup
- [ ] Clear device cache
- [ ] Ensure stable framerate
- [ ] Note device specs (CPU, RAM, screen size)
- [ ] Prepare note-taking method

### Playtesting Session (20-30 minutes)

**Goal:** Play 3-5 complete games, document experience

#### Test 1: First-Time User Experience
- [ ] Start game without instructions
- [ ] Note first confusion point
- [ ] Check if swipe mechanic is discoverable
- [ ] Verify readability of all text
- [ ] Note any UI elements that are unclear

#### Test 2: Gameplay Balance
- [ ] Play to completion (victory or defeat)
- [ ] Track which stats collapsed (if defeat)
- [ ] Note if any cards feel overpowered/underpowered
- [ ] Check for "dominant strategy" emerging
- [ ] Verify variety in cards presented

#### Test 3: Edge Cases
- [ ] Try rapid swiping
- [ ] Try very slow dragging
- [ ] Swipe at extreme angles (diagonals)
- [ ] Minimize/restore app mid-game (mobile)
- [ ] Rotate device (mobile)

#### Test 4: Performance
- [ ] Monitor framerate during animations
- [ ] Check for stuttering on card transitions
- [ ] Verify memory usage over 10+ turns
- [ ] Test on lowest-spec target device

#### Test 5: Fun Factor
- [ ] Rate engagement (1-10)
- [ ] Note most interesting decisions
- [ ] Identify boring or repetitive moments
- [ ] Check if 3-7 minute target is met

### Playtesting Feedback Template

```
Date: ___________
Tester: ___________
Device: ___________
Build Version: ___________

GAME SESSION:
- Result: [Victory / Defeat at Turn ___]
- Duration: ___ minutes
- Final Stats: Budget ___ / Time ___ / Morale ___ / Quality ___

SWIPE MECHANIC:
- Easy to understand? [Yes / No]
- Responsive? [Yes / No]
- Threshold feels right? [Yes / No / Too sensitive / Not sensitive enough]
- Issues noted:

CARD CONTENT:
- Variety adequate? [Yes / No]
- Decisions interesting? [Yes / No]
- Text clear? [Yes / No]
- Most memorable card:
- Most confusing card:

BALANCE:
- Difficulty feels: [Too Easy / Just Right / Too Hard]
- Any dominant strategies found:
- Stats that collapsed most frequently:

UI/UX:
- Readability: [Excellent / Good / Needs Improvement]
- Visual clarity: [Excellent / Good / Needs Improvement]
- Animations smooth? [Yes / No]
- Any UI issues:

OVERALL:
- Fun factor (1-10): ___
- Would play again? [Yes / No]
- Main suggestions:

BUGS FOUND:
1. 
2.
3.
```

## Device Testing Matrix

### Minimum Testing Requirements

| Device Type | OS Version | Screen Size | Memory | Test Priority |
|-------------|------------|-------------|--------|---------------|
| Android Low | Android 8  | 5.5" (720p) | 2GB    | HIGH          |
| Android Mid | Android 10 | 6.0" (1080p)| 4GB    | HIGH          |
| iOS Low     | iOS 13     | iPhone 8    | 2GB    | HIGH          |
| iOS High    | iOS 17     | iPhone 14   | 6GB    | MEDIUM        |
| Web Chrome  | Latest     | 1920x1080   | N/A    | HIGH          |
| Web Firefox | Latest     | 1920x1080   | N/A    | MEDIUM        |
| Web Safari  | Latest     | 1920x1080   | N/A    | LOW           |

## Performance Benchmarks

### Target Performance Metrics

**Mobile:**
- Steady 60 FPS during gameplay
- Card animations at 60 FPS
- Memory usage < 150MB
- Battery drain < 5% per 15 minutes
- Cold start time < 3 seconds

**Web:**
- Steady 60 FPS on desktop
- 30+ FPS on lower-end devices
- Page load < 5 seconds
- Memory usage < 200MB

### Performance Testing Checklist

- [ ] Profile game session in Unity Profiler
- [ ] Check for memory leaks (play 10+ games)
- [ ] Monitor CPU usage during animations
- [ ] Verify garbage collection frequency
- [ ] Test on target low-end device
- [ ] Measure battery usage (mobile)

## Regression Testing

After bug fixes or new features, test these core functions:

- [ ] Start new game
- [ ] Swipe left decision
- [ ] Swipe right decision
- [ ] All 4 stats update correctly
- [ ] Defeat by Budget collapse
- [ ] Defeat by Time collapse
- [ ] Defeat by Morale collapse
- [ ] Defeat by Quality collapse
- [ ] Victory at turn 30
- [ ] Play again from end screen
- [ ] Return to main menu

## Bug Reporting Template

```
Title: [Brief description]
Severity: [Critical / High / Medium / Low]
Priority: [Blocker / High / Medium / Low]

DESCRIPTION:
[Clear description of the bug]

STEPS TO REPRODUCE:
1.
2.
3.

EXPECTED BEHAVIOR:
[What should happen]

ACTUAL BEHAVIOR:
[What actually happens]

ENVIRONMENT:
- Build version:
- Device:
- OS version:
- Screen size:

FREQUENCY:
[Always / Often / Sometimes / Rare]

SCREENSHOT/VIDEO:
[If applicable]

NOTES:
[Additional context]
```

## Common Issues to Watch For

### Swipe Mechanic
- [ ] Cards getting "stuck" mid-drag
- [ ] Accidental swipes from scrolling
- [ ] Threshold too sensitive/insensitive
- [ ] Rotation animation glitching
- [ ] Cards not returning to center properly

### Stats System
- [ ] Stats going below 0 or above 100
- [ ] Stat bars not updating smoothly
- [ ] Color transitions not working
- [ ] Critical warnings not appearing
- [ ] Events not firing correctly

### Card Selection
- [ ] Same card appearing multiple times
- [ ] Card pool exhausted too quickly
- [ ] Empty card displayed
- [ ] Missing card data causing errors
- [ ] Card images not loading

### UI/UX
- [ ] Text overflowing containers
- [ ] Elements overlapping
- [ ] Touch targets too small
- [ ] Buttons not responding
- [ ] Screen orientation issues

### Performance
- [ ] Frame drops during animations
- [ ] Increasing memory usage over time
- [ ] Slow loading times
- [ ] Stuttering card transitions
- [ ] Battery drain on mobile

## Testing Phases

### Alpha Testing (Internal)
- All core functionality implemented
- Playable start to finish
- Known bugs documented
- Balance rough but playable

### Beta Testing (External)
- No critical bugs
- Balance refined
- All animations complete
- Performance acceptable

### Release Candidate
- All features complete
- No high-priority bugs
- Thorough device testing done
- Performance optimized

## Acceptance Criteria for MVP

Before considering MVP complete:

**Functionality:**
- [ ] 20+ unique cards implemented
- [ ] All 4 stats working correctly
- [ ] Swipe mechanic feels responsive
- [ ] Victory and all 4 defeat endings work
- [ ] Can play 5 games in a row without crash

**Polish:**
- [ ] All animations smooth (60fps)
- [ ] No visual glitches
- [ ] Text readable on all target devices
- [ ] Stats update immediately after decisions

**Balance:**
- [ ] Victory achievable in 30-40% of games
- [ ] Average game length 3-7 minutes
- [ ] No obvious dominant strategy
- [ ] All stats relevant (all cause defeats)

**Performance:**
- [ ] Runs at target framerate on min-spec device
- [ ] Memory usage stable over 30 minutes
- [ ] No crashes in 20 consecutive games
- [ ] Cold start < 5 seconds

## Continuous Testing During Development

### Daily Smoke Tests (2 minutes)
- [ ] Game starts without errors
- [ ] Can make at least one decision
- [ ] No console errors

### Weekly Full Tests (15 minutes)
- [ ] Play 3 complete games
- [ ] Test all new features
- [ ] Check for regressions
- [ ] Update known issues list

### Pre-Commit Tests
- [ ] All unit tests pass
- [ ] No new warnings in console
- [ ] Build succeeds for all platforms
