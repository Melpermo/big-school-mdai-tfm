\# 🌍 Localization System \- The Human Loop

\#\# 📋 General Description

Complete localization system for Unity that allows changing the game language at runtime (Spanish/English). Optimized for memory efficiency using \*\*language-separated assets\*\* instead of dynamic lookups.

\*\*Supported Languages:\*\*  
\- 🇪🇸 Spanish (default language)  
\- 🇬🇧 English

\---

\#\# 🏗️ System Architecture

\#\#\# Main Components

LocalizationSystem/  
├── LanguageManager.cs              \# Singleton \- Current language management  
├── DeckLocalizationManager.cs      \# Loading localized decks  
├── EndGameConditionLocalizationManager.cs \# Loading end-game conditions  
├── LanguageSwitcherUI.cs           \# UI for changing language (buttons/toggle)  
├── LocalizationTableSO.cs          \# ScriptableObject with UI translations  
└── LocalizedTextTMP.cs             \# Component for dynamic texts

\#\#\# Data Flow

graph TD  
A\[User changes language\] \--\> B\[LanguageManager.SetLanguage\]  
B \--\> C\[OnLanguageChanged Event\]  
C \--\> D\[DeckLocalizationManager\]  
C \--\> E\[EndGameConditionLocalizationManager\]  
C \--\> F\[LocalizedTextTMP Components\]  
D \--\> G\[Reloads Decks with \_ES/\_EN suffix\]  
E \--\> H\[Reloads Conditions with \_ES/\_EN suffix\]  
F \--\> I\[Updates UI texts\]

\#\# 📁 Required File Structure

\#\#\# 1\. Localized Decks

Assets\_TheHumanLoop\\Core\\Resources\\Decks  
├── CompleteGameDeck\_ES.asset  
├── CompleteGameDeck\_EN.asset  
├── EarlyPhaseDeck\_ES.asset  
├── EarlyPhaseDeck\_EN.asset  
├── MidPhaseDeck\_ES.asset  
├── MidPhaseDeck\_EN.asset  
├── EndPhaseDeck\_ES.asset  
└── EndPhaseDeck\_EN.asset

\#\#\# 2\. Localized End Game Conditions

Assets\_TheHumanLoop\\Core\\Resources\\EndGameConditions  
├── GameOver\_ES.asset / GameOver\_EN.asset  
├── Victory\_ES.asset / Victory\_EN.asset  
├── FailBudget\_ES.asset / FailBudget\_EN.asset  
├── FailMorale\_ES.asset / FailMorale\_EN.asset  
├── FailTime\_ES.asset / FailTime\_EN.asset  
├── FailQuality\_ES.asset / FailQuality\_EN.asset  
├── MetBudget\_ES.asset / MetBudget\_EN.asset  
├── MetMorale\_ES.asset / MetMorale\_EN.asset  
├── MetTime\_ES.asset / MetTime\_EN.asset  
└── MetQuality\_ES.asset / MetQuality\_EN.asset

\#\#\# 3\. Localization Table (Static UI)

Assets\_TheHumanLoop\\Core\\Data\\Localization  
└── MainMenuLocalizationTable.asset

\#\# 🎮 Unity Setup

\#\#\# 1\. Localization GameObject

Created in the main scene:

Hierarchy: LocalizationManagers (GameObject \- DontDestroyOnLoad)  
├── LanguageManager (Component)  
├── DeckLocalizationManager (Component)  
└── EndGameConditionLocalizationManager (Component)

\#\#\# 2\. Manager Configuration

\#\#\#\# LanguageManager

Deck Base Names:  
\- Complete Deck Name: "CompleteGameDeck"  
\- Early Phase Deck Name: "EarlyPhaseDeck"  
\- Mid Phase Deck Name: "MidPhaseDeck"  
\- End Phase Deck Name: "EndPhaseDeck"  
Resources Path: "Decks"  
Show Debug Logs: ✓ (for testing)

\#\#\#\# EndGameConditionLocalizationManager

Condition Base Names:  
\- Game Over Name: "GameOver"  
\- Victory Name: "Victory"  
\- Fail Budget Name: "FailBudget" ... (etc.)  
Resources Path: "EndGameConditions"  
Show Debug Logs: ✓ (for testing)

\#\#\# 3\. Integration with Existing Systems

\#\#\#\# DeckManager  
\`\[SerializeField\] private bool useLocalizedDecks \= true;\`

\#\#\#\# GameStatsManager  
\`\[SerializeField\] private bool useLocalizedConditions \= true;\`  
\`\[SerializeField\] private UI.EndGameUIHandler endGameUIHandler; // Assign\`

\#\# 🔧 System Usage

\#\#\# Changing Language from Code

\`\`\`csharp  
using HumanLoop.LocalizationSystem;

// Change to Spanish  
LanguageManager.Instance.SetLanguage(LanguageManager.Language.Spanish);

// Change to English  
LanguageManager.Instance.SetLanguage(LanguageManager.Language.English);

// Get current language  
LanguageManager.Language current \= LanguageManager.Instance.CurrentLanguage;

// Subscribe to language changes  
LanguageManager.OnLanguageChanged \+= OnLanguageChanged;

void OnLanguageChanged(LanguageManager.Language newLanguage) {  
    Debug.Log($"Language changed to: {newLanguage}");  
}

## **Language Switcher UI**

#### **Option 1: Buttons (Settings)**

1. Create → HumanLoop → Localization → Static Localization Table  
2. **Add entries:**  
   * Key: "menu.play"  
   * Spanish: "Jugar"  
   * English: "Play"  
3. **Apply to TextMeshPro:**  
   * TextMeshProUGUI GameObject: Add Component → LocalizedTextTMP  
   * Static Localization Table: \[Your Static LocalizationTable\]  
   * Translation Key: "menu.play"

## **🧪 System Testing**

## **Basic Tests**

1. **Verify initial load:**  
   * Play Mode → Console: ✅ "\[LanguageManager\] Starting with language: Spanish"  
   * ✅ "\[DeckLocalizationManager\] Loaded decks for language: Spanish"  
   * ✅ "\[EndGameConditionLocalizationManager\] Loaded 10 conditions"  
   * ✅ "\[GameStatsManager\] ✓ Loaded 10 localized end game conditions"  
2. **Change language at runtime:**  
   * Click language button/toggle  
   * Verify Console: ✅ "\[LanguageManager\] Language changed to: English"  
   * ✅ "\[DeckManager\] Language changed to English, reloading deck..."  
   * ✅ "\[GameStatsManager\] ✓ Reloaded 10 conditions in new language"  
3. **Verify localized texts:**  
   * Force Game Over: GameStatsManager → Testing/Force Budget to 0  
   * Verify that the message appears in the correct language

## **Debug Context Menus**

* **LanguageManager:** Debug/Switch to Spanish, Debug/Switch to English, Debug/Log Current Language  
* **DeckLocalizationManager:** Debug/Reload Decks, Debug/Log Cached Decks  
* **EndGameConditionLocalizationManager:** Debug/Reload Conditions, Debug/Log Cached Conditions, Debug/Log Condition Order  
* **GameStatsManager:** Debug/Log EndGameConditionLocalizationManager Status, Debug/Force Reload Localized Conditions

---

## **🐛 Troubleshooting**

## **Problem: "EndGameConditionLocalizationManager.Instance is null"**

**Cause:** GameObject with the component does not exist in the scene.

**Solution:**

1. Verify that "LocalizationManagers" GameObject exists.  
2. Verify it has the EndGameConditionLocalizationManager component.  
3. If it doesn't exist, create the GameObject and add the component.

## **Problem: "Available conditions: 0"**

**Cause:** EndGameConditions are not in Resources or have incorrect names.

**Solution:**

1. Verify path: Assets\_TheHumanLoop\\Core\\Resources\\EndGameConditions\\  
2. Verify names follow the pattern: \[BaseName\]\_ES / \[BaseName\]\_EN  
3. Verify that base names in the Inspector match the assets.

## **Problem: Texts do not update when changing language**

**Cause:** LocalizedTextTMP is not subscribed to the event.

**Solution:**

1. Verify the GameObject with text has the LocalizedTextTMP component.  
2. Verify the LocalizationTable is assigned.  
3. Verify the Translation Key exists in the table.

---

## **📊 System Advantages**

| Aspect | Benefit |
| :---- | :---- |
| **Memory** | Only loads the active language (\~50% less than dynamic systems) |
| **Performance** | No lookups, direct access to assets |
| **Maintenance** | Translations in ScriptableObjects (easy to edit) |
| **Scalability** | Adding a language \= duplicating assets with a new suffix |
| **Testing** | Separated assets \= easy to verify translations |

---

## **🔄 Adding a New Language**

To add French (\_FR):

**Update LanguageManager.cs:**  
C\#  
public enum Language { Spanish, English, French // ← NEW }  
public string GetLanguageSuffix() {  
    return \_currentLanguage switch {  
        Language.Spanish \=\> "\_ES",  
        Language.English \=\> "\_EN",  
        Language.French \=\> "\_FR", // ← NEW  
        \_ \=\> "\_ES"  
    };  
}

1.   
2. **Duplicate assets:**  
   * CompleteGameDeck\_ES → CompleteGameDeck\_FR  
   * GameOver\_ES → GameOver\_FR ... (etc.)  
3. **Translate contents** of the new assets.  
4. **Add button** in LanguageSwitcherUI.

---

## **📝 Naming Conventions**

## **Localized Assets**

Pattern: \[BaseName\]\_\[SUFFIX\].asset

Examples:

* ✅ EarlyPhaseDeck\_ES.asset  
* ✅ Victory\_EN.asset  
* ❌ early\_phase\_deck\_es.asset (lowercase)  
* ❌ Victory-EN.asset (hyphen instead of underscore)

## **Translation Keys (LocalizationTable)**

Examples:

* ✅ menu.play  
* ✅ button.restart  
* ✅ stats.budget  
* ❌ MenuPlay (no separator)  
* ❌ restart\_button (underscore instead of dot)

---

## **🎯 System Summary**

**The localization system allows:**

1. ✅ **Runtime language change** without reloading the scene.  
2. ✅ **Persistence** of the chosen language (PlayerPrefs).  
3. ✅ **Card localization** (language-separated decks).  
4. ✅ **End-game message localization** (separated conditions).  
5. ✅ **Static UI localization** (buttons, menus, etc.).  
6. ✅ **Memory efficiency** (only loads active language).  
7. ✅ **No external dependencies** (fully custom system).  
8. ✅ **Easy to maintain** (translations in ScriptableObjects).

---

## **📚 Code References**

## **Main Files**

Assets\_TheHumanLoop\\ModularSystems\\LocalizationSystem

├── Scripts

│ ├── LanguageManager.cs

│ ├── DeckLocalizationManager.cs

│ ├── EndGameConditionLocalizationManager.cs

│ ├── LanguageSwitcherUI.cs

│ └── LocalizedTextTMP.cs

└── ScriptableObjects

└── LocalizationTableSO.cs

## **Integrated Managers**

Assets\_TheHumanLoop\\Core\\Scripts\\Core\_Scripts

├── DeckManager.cs (useLocalizedDecks \= true)

├── ProgressionManager.cs (useLocalizedDecks \= true)

└── GameStatsManager.cs (useLocalizedConditions \= true)

---

## **🚀 System Version**

**Version:** 1.0

**Date:** March 2026

**Author:** Melpermo

**Project:** The Human Loop \- TFM

**Localization System \- The Human Loop © 2026**

