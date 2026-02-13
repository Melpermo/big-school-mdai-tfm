# Animated UI System with DOTween

A "juicy" UI framework for Unity built on **DOTween**. It features a staggered floating menu, elastic pop-up windows, and dynamic toggle switches with persistent data saving.

## 🚀 Core Components

### 1. FloatingMenu & Manager
- **FloatingMenuManager**: The central hub (Singleton) that orchestrates window transitions and menu states.
- **FloatingMenu**: Handles the corner trigger button and deploys sub-buttons using a staggered animation effect.

### 2. UIWindow
- Manages content panels with symmetrical animations.
- **Entry**: Elastic scaling (`OutBack`) and punch rotation.
- **Exit**: Anticipation pulse and shrinking (`InBack`) for consistency.

### 3. UIToggleSwitch
- Dynamic switch for settings (Music, SFX).
- **Auto-Layout**: Automatically calculates handle limits based on background width.
- **Feedback**: Features a horizontal **Shake** effect to indicate errors or locked states.

## 🛠 Setup Guide

1. **Requirements**: [DOTween](http://dotween.demigiant.com/) must be imported into the project.
2. **Hierarchy Setup**:
    - Attach `FloatingMenuManager` to your persistent "Systems" prefab.
    - UI Panels require the `UIWindow` script and a `CanvasGroup` component.
    - Toggles require `UIToggleSwitch` and a standard Unity `Button` component.
3. **UIFeedback**: Add the `UIFeedback` script to any button to instantly gain "squash and stretch" tactile feedback.

## 📝 Technical Notes
- **Spam Protection**: All methods call `.DOKill()` to ensure animations don't overlap or stutter.
- **Persistence**: Toggle states are automatically saved to `PlayerPrefs` upon interaction.