\# 🌍 Sistema de Localización \- The Human Loop

\#\# 📋 Descripción General

Sistema de localización completo para Unity que permite cambiar el idioma del juego en runtime (Español/Inglés). Optimizado para eficiencia de memoria usando \*\*assets separados por idioma\*\* en lugar de lookups dinámicos.

\*\*Idiomas Soportados:\*\*  
\- 🇪🇸 Español (idioma por defecto)  
\- 🇬🇧 Inglés

\---

\#\# 🏗️ Arquitectura del Sistema

\#\#\# Componentes Principales

LocalizationSystem/   
├── LanguageManager.cs              \# Singleton \- Gestión de idioma actual   
├── DeckLocalizationManager.cs      \# Carga de decks localizados   
├── EndGameConditionLocalizationManager.cs \# Carga de condiciones de fin de juego ├── LanguageSwitcherUI.cs           \# UI para cambiar idioma (botones/toggle)   
├── LocalizationTableSO.cs          \# ScriptableObject con traducciones de UI   
└── LocalizedTextTMP.cs             \# Componente para textos dinámicos

\#\#\# Flujo de Datos

graph TD A\[Usuario cambia idioma\] \--\> B\[LanguageManager.SetLanguage\] B \--\> C\[OnLanguageChanged Event\] C \--\> D\[DeckLocalizationManager\] C \--\> E\[EndGameConditionLocalizationManager\] C \--\> F\[LocalizedTextTMP Components\] D \--\> G\[Recarga Decks con sufijo \_ES/\_EN\] E \--\> H\[Recarga Conditions con sufijo \_ES/\_EN\] F \--\> I\[Actualiza textos UI\]

\#\# 📁 Estructura de Archivos Requerida

\#\#\# 1\. Decks Localizados

Assets\_TheHumanLoop\\Core\\Resources\\Decks  
├── CompleteGameDeck\_ES.asset   
├── CompleteGameDeck\_EN.asset   
├── EarlyPhaseDeck\_ES.asset   
├── EarlyPhaseDeck\_EN.asset   
├── MidPhaseDeck\_ES.asset   
├── MidPhaseDeck\_EN.asset   
├── EndPhaseDeck\_ES.asset   
└── EndPhaseDeck\_EN.asset

\#\#\# 2\. End Game Conditions Localizadas

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

\#\#\# 3\. Localization Table (UI Estática)

Assets\_TheHumanLoop\\Core\\Data\\Localization  
└── MainMenuLocalizationTable.asset

\#\# 🎮 Setup en Unity

\#\#\# 1\. GameObject de Localización

Creado en la escena principal:

Hierarchy: LocalizationManagers (GameObject \- DontDestroyOnLoad)   
├── LanguageManager (Component)   
├── DeckLocalizationManager (Component)   
└── EndGameConditionLocalizationManager (Component)

\#\#\# 2\. Configuración de Managers

\#\#\#\# LanguageManager

Deck Base Names: Complete Deck Name: "CompleteGameDeck"   
Early Phase Deck Name: "EarlyPhaseDeck"   
Mid Phase Deck Name: "MidPhaseDeck"   
End Phase Deck Name: "EndPhaseDeck"  
Resources Path: "Decks" Show Debug Logs: ✓ (para testing)

\#\#\#\# EndGameConditionLocalizationManager

Condition Base Names:   
Game Over Name: "GameOver"   
Victory Name: "Victory"   
Fail Budget Name: "FailBudget" ... (etc.)  
Resources Path: "EndGameConditions" Show Debug Logs: ✓ (para testing)

\#\#\# 3\. Integración con Sistemas Existentes

\#\#\#\# DeckManager

\[SerializeField\] private bool useLocalizedDecks \= true;

\#\#\#\# GameStatsManager

\[SerializeField\] private bool useLocalizedConditions \= true; \[SerializeField\] private UI.EndGameUIHandler endGameUIHandler; // Asignar

\#\# 🔧 Uso del Sistema

\#\#\# Cambiar Idioma desde Código

using HumanLoop.LocalizationSystem;  
// Cambiar a español LanguageManager.Instance.SetLanguage(LanguageManager.Language.Spanish);  
// Cambiar a inglés LanguageManager.Instance.SetLanguage(LanguageManager.Language.English);  
// Obtener idioma actual LanguageManager.Language current \= LanguageManager.Instance.CurrentLanguage;  
// Subscribirse a cambios de idioma LanguageManager.OnLanguageChanged \+= OnLanguageChanged;  
void OnLanguageChanged(LanguageManager.Language newLanguage) { Debug.Log($"Idioma cambiado a: {newLanguage}"); }

\#\#\# UI de Cambio de Idioma

\#\#\#\# Opción 1: Botones (Settings)

Create → HumanLoop → Localization → Static Localization Table

2\. \*\*Añadir entradas:\*\*

Key: "menu.play" Spanish: "Jugar" English: "Play"

3\. \*\*Aplicar a TextMeshPro:\*\*

TextMeshProUGUI GameObject: Add Component → LocalizedTextTMP \- Static Localization Table: \[Tu Static LocalizationTable\] \- Translation Key: "menu.play"

\#\# 🧪 Testing del Sistema

\#\#\# Tests Básicos

1\. \*\*Verificar carga inicial:\*\*

* Play Mode → Console: ✅ "\[LanguageManager\]   
* Starting with language: Spanish" ✅   
* "\[DeckLocalizationManager\] Loaded decks for language: Spanish" ✅ "\[EndGameConditionLocalizationManager\] Loaded 10 conditions" ✅   
* "\[GameStatsManager\] ✓ Loaded 10 localized end game conditions"

2\. \*\*Cambiar idioma en runtime:\*\*

* Clic en botón/toggle de idioma

Verificar Console: ✅ 

* "\[LanguageManager\] Language changed to: English" ✅   
* "\[DeckManager\] Language changed to English, reloading deck..." ✅   
* "\[GameStatsManager\] ✓ Reloaded 10 conditions in new language"

3\. \*\*Verificar textos localizados:\*\*

* Forzar Game Over: GameStatsManager → Testing/Force Budget to 0  
* Verificar que mensaje aparece en idioma correcto

\#\#\# Context Menus de Debug

// LanguageManager Debug/Switch to Spanish Debug/Switch to English Debug/Log Current Language  
// DeckLocalizationManager Debug/Reload Decks Debug/Log Cached Decks  
// EndGameConditionLocalizationManager Debug/Reload Conditions Debug/Log Cached Conditions Debug/Log Condition Order  
// GameStatsManager Debug/Log EndGameConditionLocalizationManager Status Debug/Force Reload Localized Conditions

\---

\#\# 🐛 Troubleshooting

\#\#\# Problema: "EndGameConditionLocalizationManager.Instance is null"

\*\*Causa:\*\* GameObject con el componente no existe en la escena.

\*\*Solución:\*\*

1\.	Verificar que existe GameObject "LocalizationManagers"  
2\.	Verificar que tiene componente EndGameConditionLocalizationManager  
3\.	Si no existe, crear GameObject y añadir componente

\#\#\# Problema: "Available conditions: 0"

\*\*Causa:\*\* EndGameConditions no están en Resources o nombres incorrectos.

\*\*Solución:\*\*

1\.	Verificar ruta: Assets\_TheHumanLoop\\Core\\Resources\\EndGameConditions\\  
2\.	Verificar nombres siguen patrón: \[BaseName\]\_ES / \[BaseName\]\_EN  
3\.	Verificar que base names en Inspector coinciden con assets

\#\#\# Problema: Textos no se actualizan al cambiar idioma

\*\*Causa:\*\* LocalizedTextTMP no está subscrito al evento.

\*\*Solución:\*\*

1\.	Verificar que GameObject con texto tiene componente LocalizedTextTMP  
2\.	Verificar que LocalizationTable está asignado  
3\.	Verificar que Translation Key existe en la tabla

\---

\#\# 📊 Ventajas del Sistema

| Aspecto | Beneficio |  
|---------|-----------|  
| \*\*Memoria\*\* | Solo carga idioma activo (\~50% menos que sistema dinámico) |  
| \*\*Performance\*\* | Sin lookups, acceso directo a assets |  
| \*\*Mantenimiento\*\* | Traducciones en ScriptableObjects (fácil de editar) |  
| \*\*Escalabilidad\*\* | Añadir idioma \= duplicar assets con nuevo sufijo |  
| \*\*Testing\*\* | Assets separados \= fácil verificar traducciones |

\---

\#\# 🔄 Añadir Nuevo Idioma

Para añadir francés (\_FR):

1\. \*\*Actualizar LanguageManager.cs:\*\*  
public enum Language { Spanish, English, French  // ← NUEVO }  
public string GetLanguageSuffix() {   
return \_currentLanguage switch { Language.Spanish \=\> "\_ES", Language.English \=\> "\_EN", Language.French \=\> "\_FR",  // ← NUEVO \_ \=\> "\_ES" };   
}

2\. \*\*Duplicar assets:\*\*

CompleteGameDeck\_ES → CompleteGameDeck\_FR GameOver\_ES → GameOver\_FR ... (etc.)

3\. \*\*Traducir contenidos\*\* de los nuevos assets.

4\. \*\*Añadir botón\*\* en LanguageSwitcherUI.

\---

\#\# 📝 Convenciones de Nombres

\#\#\# Assets Localizados

Patrón: \[BaseName\]\_\[SUFFIX\].asset  
Ejemplos:   
✅ EarlyPhaseDeck\_ES.asset  
 ✅ Victory\_EN.asset   
❌ early\_phase\_deck\_es.asset (minúsculas)   
❌ Victory-EN.asset (guión en lugar de underscore)

\#\#\# Translation Keys (LocalizationTable)

Ejemplos:   
✅ menu.play   
✅ button.restart   
✅ stats.budget   
❌ MenuPlay (sin separador)   
❌ restart\_button (underscore en lugar de punto)

\---

\#\# 🎯 Resumen del Sistema

\*\*El sistema de localización permite:\*\*

1\. ✅ \*\*Cambio de idioma en runtime\*\* sin recargar escena  
2\. ✅ \*\*Persistencia\*\* del idioma elegido (PlayerPrefs)  
3\. ✅ \*\*Localización de cartas\*\* (decks separados por idioma)  
4\. ✅ \*\*Localización de mensajes de fin de juego\*\* (conditions separadas)  
5\. ✅ \*\*Localización de UI estática\*\* (botones, menús, etc.)  
6\. ✅ \*\*Eficiencia de memoria\*\* (solo carga idioma activo)  
7\. ✅ \*\*Sin dependencies externas\*\* (sistema completamente custom)  
8\. ✅ \*\*Fácil de mantener\*\* (traducciones en ScriptableObjects)

\---

\#\# 📚 Referencias de Código

\#\#\# Archivos Principales

Assets\_TheHumanLoop\\ModularSystems\\LocalizationSystem  
├── Scripts  
├── LanguageManager.cs   
├── DeckLocalizationManager.cs   
├── EndGameConditionLocalizationManager.cs   
├── LanguageSwitcherUI.cs   
├── LocalizedTextTMP.cs   
└── ScriptableObjects  
└── LocalizationTableSO.cs

\#\#\# Managers Integrados

Assets\_TheHumanLoop\\Core\\Scripts\\Core\_Scripts  
├── DeckManager.cs (useLocalizedDecks \= true)   
├── ProgressionManager.cs (useLocalizedDecks \= true)   
└── GameStatsManager.cs (useLocalizedConditions \= true)

\---

\#\# 🚀 Versión del Sistema

\*\*Versión:\*\* 1.0    
\*\*Fecha:\*\* Marzo 2026    
\*\*Autor:\*\* Melpermo    
\*\*Proyecto:\*\* The Human Loop \- TFM  

\*\*Sistema de Localización \- The Human Loop © 2026\*\*  
