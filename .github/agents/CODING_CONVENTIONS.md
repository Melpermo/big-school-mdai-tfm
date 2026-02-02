# Coding Conventions & Unity Best Practices

## Naming Conventions

### Files and Classes
- **ScriptableObjects**: End with `SO` (e.g., `CardSO`, `GameConfigSO`)
- **MonoBehaviours**: Descriptive names (e.g., `CardView`, `SwipeController`)
- **Managers/Controllers**: End with `Manager`, `Controller`, or `System` (e.g., `GameFlowController`)
- **Interfaces**: Prefix with `I` (e.g., `ICard`, `IConsequence`)
- **Enums**: Singular form (e.g., `StatType`, `CardCategory`)

### Variables and Methods
```csharp
// Private fields: camelCase with underscore prefix
private int _currentTurn;
private float _swipeThreshold;

// Public properties: PascalCase
public int CurrentTurn { get; private set; }
public float SwipeThreshold => _swipeThreshold;

// Methods: PascalCase with verb
public void ApplyConsequences(Card card, bool isLeftChoice)
private void ValidateSwipe()
protected void OnCardSelected()

// Events: PascalCase, past tense
public event Action<Card> CardSelected;
public event Action<StatType, int> StatChanged;
public event Action GameEnded;

// Constants: UPPER_CASE
private const int MAX_TURNS = 30;
private const float SWIPE_THRESHOLD = 100f;
```

## Project Structure

```
Assets/
├── Scripts/
│   ├── Data/               # ScriptableObjects definitions
│   │   ├── CardSO.cs
│   │   ├── StatConfigSO.cs
│   │   └── GameConfigSO.cs
│   ├── Runtime/            # Game logic (no MonoBehaviour)
│   │   ├── GameSession.cs
│   │   ├── CardSelector.cs
│   │   ├── ConsequenceResolver.cs
│   │   └── Models/         # Data structures
│   ├── UI/                 # MonoBehaviour UI components
│   │   ├── CardView.cs
│   │   ├── StatsHUD.cs
│   │   ├── SwipeController.cs
│   │   └── EndScreen.cs
│   ├── Systems/            # Core game systems
│   │   └── GameFlowController.cs
│   └── Utilities/          # Helper classes
│       ├── Extensions.cs
│       └── Constants.cs
├── Data/                   # ScriptableObject assets
│   ├── Cards/
│   ├── Stats/
│   └── Config/
├── Sprites/
│   └── Cards/
└── Scenes/
    ├── Game.unity
    └── MainMenu.unity
```

## Architecture Principles

### 1. Separation of Concerns
```csharp
// ✅ GOOD: Logic separated from presentation
public class GameSession
{
    public void ApplyStatChange(StatType stat, int amount)
    {
        // Pure logic, no UI references
        stats[stat] += amount;
        OnStatChanged?.Invoke(stat, stats[stat]);
    }
}

public class StatsHUD : MonoBehaviour
{
    private void OnEnable()
    {
        GameSession.OnStatChanged += UpdateStatDisplay;
    }
}

// ❌ BAD: Logic coupled with UI
public class GameSession : MonoBehaviour
{
    public Text budgetText;
    
    public void ApplyStatChange(StatType stat, int amount)
    {
        stats[stat] += amount;
        budgetText.text = stats[stat].ToString(); // UI in logic!
    }
}
```

### 2. ScriptableObjects for Data
```csharp
// ✅ GOOD: Data in ScriptableObjects
[CreateAssetMenu(fileName = "New Card", menuName = "The Human Loop/Card")]
public class CardSO : ScriptableObject
{
    public string title;
    public string situationText;
    public CardChoice leftChoice;
    public CardChoice rightChoice;
    public CardCategory category;
    public Sprite cardImage;
}

// ❌ BAD: Hardcoded data in code
public class Card : MonoBehaviour
{
    void Start()
    {
        if (cardId == 1)
        {
            title = "Bug crítico";
            description = "...";
        }
    }
}
```

### 3. Events for Decoupling
```csharp
// ✅ GOOD: Event-driven communication
public class GameSession
{
    public event Action<StatType, int> StatChanged;
    public event Action<string> TagAdded;
    
    public void ApplyChange(StatType stat, int value)
    {
        stats[stat] += value;
        StatChanged?.Invoke(stat, stats[stat]);
    }
}

// ❌ BAD: Direct references
public class GameSession
{
    public StatsHUD statsHUD;
    
    public void ApplyChange(StatType stat, int value)
    {
        stats[stat] += value;
        statsHUD.UpdateStat(stat, stats[stat]); // Tight coupling
    }
}
```

## Unity-Specific Best Practices

### MonoBehaviour Lifecycle
```csharp
public class CardView : MonoBehaviour
{
    // Cache references in Awake
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    
    // Subscribe to events in OnEnable
    private void OnEnable()
    {
        SwipeController.OnSwipeStarted += HandleSwipeStart;
    }
    
    // Unsubscribe in OnDisable
    private void OnDisable()
    {
        SwipeController.OnSwipeStarted -= HandleSwipeStart;
    }
    
    // Initialize with external data in Start or custom method
    private void Start()
    {
        // Scene references ready here
    }
    
    public void Initialize(CardSO cardData)
    {
        // Explicit initialization
    }
}
```

### Memory Management
```csharp
// ✅ GOOD: Object pooling for cards
public class CardPool
{
    private Queue<CardView> _pool = new Queue<CardView>();
    
    public CardView GetCard()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();
        return Instantiate(_cardPrefab);
    }
    
    public void ReturnCard(CardView card)
    {
        card.gameObject.SetActive(false);
        _pool.Enqueue(card);
    }
}

// ❌ BAD: Creating new GameObjects every turn
public void ShowCard(CardSO data)
{
    var card = Instantiate(cardPrefab); // Memory leak!
    card.Setup(data);
}
```

### Coroutines vs Async
```csharp
// ✅ Use Coroutines for simple Unity animations
private IEnumerator AnimateCardSwipe(Vector3 targetPosition)
{
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        transform.position = Vector3.Lerp(start, targetPosition, elapsed / duration);
        yield return null;
    }
}

// ✅ Use async/await for logic without Unity dependencies
public async Task<List<CardSO>> LoadCardsAsync()
{
    var cards = await CardDatabase.LoadAllAsync();
    return cards.Where(c => c.IsValid()).ToList();
}
```

## Code Quality Guidelines

### Comments
```csharp
// ✅ GOOD: Explain WHY, not WHAT
// We delay card selection by one frame to ensure all stat change events 
// have been processed before evaluating the next card's availability
yield return null;
SelectNextCard();

// ❌ BAD: Stating the obvious
// Increment the counter
_turnCounter++;
```

### Null Checking
```csharp
// ✅ GOOD: Defensive programming
public void ApplyCard(CardSO card)
{
    if (card == null)
    {
        Debug.LogError("Attempted to apply null card");
        return;
    }
    
    // ... implementation
}

// ✅ Use null-conditional operator
_onCardApplied?.Invoke(card);

// ✅ Unity null check for assets
if (_cardSprite == null)
{
    Debug.LogWarning($"Missing sprite for card: {card.title}");
    _cardSprite = _defaultSprite;
}
```

### Error Handling
```csharp
// ✅ GOOD: Graceful degradation
public CardSO SelectNextCard()
{
    var availableCards = GetAvailableCards();
    
    if (availableCards.Count == 0)
    {
        Debug.LogWarning("No available cards, using fallback");
        return _fallbackCard;
    }
    
    return WeightedSelection(availableCards);
}

// ❌ BAD: Silent failure
public CardSO SelectNextCard()
{
    var cards = GetAvailableCards();
    return cards[Random.Range(0, cards.Count)]; // IndexOutOfRangeException!
}
```

## Performance Considerations

### Avoid in Update/FixedUpdate
```csharp
// ❌ BAD: GetComponent in Update
void Update()
{
    GetComponent<Rigidbody>().velocity = Vector3.forward;
}

// ✅ GOOD: Cache reference
private Rigidbody _rb;

void Awake()
{
    _rb = GetComponent<Rigidbody>();
}

void Update()
{
    _rb.velocity = Vector3.forward;
}
```

### String Handling
```csharp
// ❌ BAD: String concatenation in loops
string result = "";
foreach (var stat in stats)
{
    result += stat.ToString() + "\n"; // Creates new string each iteration
}

// ✅ GOOD: StringBuilder
var sb = new StringBuilder();
foreach (var stat in stats)
{
    sb.AppendLine(stat.ToString());
}
string result = sb.ToString();
```

### LINQ Usage
```csharp
// ✅ GOOD: LINQ for readability in non-critical paths
var availableCards = allCards
    .Where(c => c.MinTurn <= currentTurn)
    .Where(c => !c.RequiredTags.Except(activeTags).Any())
    .ToList();

// ⚠️ CAUTION: Avoid LINQ in Update if performance critical
// Consider caching results or using for loops
```

## Testing Approach

### Unit Testing Structure
```csharp
[TestFixture]
public class CardSelectorTests
{
    private CardSelector _selector;
    private GameSession _session;
    
    [SetUp]
    public void Setup()
    {
        _session = new GameSession();
        _selector = new CardSelector(_session);
    }
    
    [Test]
    public void SelectCard_NoAvailableCards_ReturnsFallback()
    {
        // Arrange
        _session.CurrentTurn = 100; // All cards past MaxTurn
        
        // Act
        var card = _selector.SelectNextCard();
        
        // Assert
        Assert.IsNotNull(card);
        Assert.AreEqual(_selector.FallbackCard, card);
    }
    
    [TearDown]
    public void TearDown()
    {
        _selector = null;
        _session = null;
    }
}
```

## Common Pitfalls to Avoid

### 1. Singleton Abuse
```csharp
// ❌ AVOID: Global singleton for everything
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameSession session;
    public CardSelector selector;
    public ConsequenceResolver resolver;
    // ... everything in one place
}

// ✅ PREFER: Dependency injection
public class GameFlowController : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    private GameSession _session;
    private CardSelector _selector;
    
    private void Awake()
    {
        _session = new GameSession(_config);
        _selector = new CardSelector(_session, _config.cards);
    }
}
```

### 2. Magic Numbers
```csharp
// ❌ BAD: Magic numbers
if (stats[StatType.Budget] < 20)
{
    ShowWarning();
}

// ✅ GOOD: Named constants
private const int CRITICAL_THRESHOLD = 20;

if (stats[StatType.Budget] < CRITICAL_THRESHOLD)
{
    ShowWarning();
}
```

### 3. Over-Engineering
```csharp
// ❌ BAD: Unnecessary abstraction for simple case
public interface IStatModifier { }
public interface IStatModifierFactory { }
public class CompositeStatModifierFactory : IStatModifierFactory { }

// ✅ GOOD: Simple and clear
public void ModifyStat(StatType stat, int amount)
{
    stats[stat] += amount;
}
```

## Git Commit Messages

Follow conventional commits:
```
feat: add swipe animation to card view
fix: prevent negative stat values after consequence application
refactor: extract card selection logic into separate class
docs: update game systems architecture
test: add unit tests for consequence resolver
perf: implement card object pooling
style: apply consistent naming conventions to stat properties
```

## When in Doubt

1. **Favor readability over cleverness**
2. **Keep methods under 30 lines**
3. **One class, one responsibility**
4. **Test edge cases** (negative values, null references, empty lists)
5. **Document public APIs**
6. **Use Debug.Log** strategically (not excessively)
