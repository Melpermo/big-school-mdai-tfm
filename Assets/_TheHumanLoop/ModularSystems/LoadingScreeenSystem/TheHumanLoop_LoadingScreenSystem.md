# 🔄 Loading Screen System

**Version:** 1.0  
**Project:** The Human Loop  
**Namespace:** `TheHumanLoop.LoadingScreenSystem`

---

## 📑 Table of Contents

1. [General Description](#general-description)
2. [Key Features](#key-features)
3. [Components](#components)
4. [Configuration](#configuration)
5. [Usage](#usage)
6. [Animations](#animations)
7. [Technical Details](#technical-details)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 General Description

A modular system designed to manage asynchronous scene loading with an animated loading screen. It provides a smooth user experience during transitions between scenes.

### ✨ Key Features

✅ **Asynchronous Loading** - The game doesn't freeze during the loading process.  
✅ **Smoothed Progress** - Blends real progress with a guaranteed minimum time.  
✅ **Visual Animations** - Rotating icons, pulsing images, and animated text.  
✅ **Random Tips** - Displays useful advice during the load time.  
✅ **Smooth Transitions** - Fade in/out using CanvasGroup alpha.  
✅ **Configurable** - All timings and speeds are adjustable from the Inspector.

---

## 🧩 Components

### SceneLoader.cs

**Location:** `Assets\_TheHumanLoop\ModularSystems\LoadingScreeenSystem\Scripts\SceneLoader.cs`

**Responsibilities:**
- Manage asynchronous scene loading (by index or name).
- Control blended real and "fake" progress.
- Animate visual elements (rotation, pulsing, dots).
- Display random tips.
- Handle fade in/out transitions.

---

## ⚙️ Configuration

### Step 1: Prepare the Loading Scene

1. **Create Canvas:**
   - Hierarchy → UI → Canvas
   - Name: `LoadingScreenCanvas`
   - Render Mode: **Screen Space - Overlay**
   - Add component: **Canvas Group**

2. **Recommended UI Structure:**

LoadingScreenCanvas (Canvas + CanvasGroup) 
├─ BackgroundScreen (Image - dark background) 
├─ LoadingScreen (Panel) 
│   ├─ LoadingIcon (Image - rotating icon) 
│   ├─ ProgressBar (Slider) 
│   │   ├─ Background 
│   │   ├─ Fill Area 
│   │   │   └─ Fill (Image) 
│   │   └─ Handle Slide Area (optional) 
│   ├─ ProgressText (TextMeshProUGUI - "0%") 
│   ├─ LoadingText (TextMeshProUGUI - "Loading...") 
│   ├─ PulsingImages[] (Images - visual effects) 
│   └─ TipsContainer (Panel) 
│       ├─ Tip1 (TextMeshProUGUI) 
│       ├─ Tip2 (TextMeshProUGUI) 
│       └─ Tip3 (TextMeshProUGUI)


### Step 2: Configure the SceneLoader

1. **Add Script:**
   - Select `LoadingScreenCanvas` → Add Component → **Scene Loader**

2. **Assign Inspector References:**

**UI Elements:**
- `Background Screen`: Background GameObject.
- `Game Objects To Deact`: Array of objects to deactivate (e.g., Main Menu).
- `Loading Screen`: Main loading panel.
- `Loading Slider`: Progress slider.
- `Progress Text`: Percentage text.
- `Loading Canvas Group`: CanvasGroup of the main canvas.

**Animation Elements:**
- `Loading Icon`: RectTransform of the rotating icon.
- `Pulsing Images`: Array of images with pulsing effects.
- `Loading Text`: "Loading..." text (animates dots).

**Loading Tips:**
- `Tip Text TMP List`: List of TextMeshProUGUI components for tips.

3. **Configure Values (Optional):**
- `Fake Load Time`: 2.0s (minimum "fake" time).
- `Minimum Load Time`: 1.0s (minimum visible time).
- `Fade In Duration`: 0.5s.
- `Fade Out Duration`: 0.5s.
- `Icon Rotation Speed`: 180°/s.
- `Pulse Speed`: 1.0.
- `Dots Animation Speed`: 0.5s.

### Step 3: Add Scenes to Build
- **File → Build Settings → Scenes In Build**

---

## 🎮 Usage

### From UI (Buttons)

**Load by Index:**
- Button Component → OnClick()
- Drag GameObject with SceneLoader
- Function: `SceneLoader.LoadScenes(int)`
- Parameter: 1 (Scene Index)

**Load by Name:**
- Button Component → OnClick()
- Drag GameObject with SceneLoader
- Function: `SceneLoader.LoadSceneByName(string)`
- Parameter: "GameLevel1" (Scene Name)

### From Code

```csharp
using TheHumanLoop.LoadingScreenSystem;

// Get reference
SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();

// Load by index
sceneLoader.LoadScenes(1);

// Load by name
sceneLoader.LoadSceneByName("GameLevel1");

Animations
1. Icon Rotation
Behavior: Continuous counter-clockwise rotation.

Setup: Assign a RectTransform with a circular image and adjust Icon Rotation Speed.

2. Pulsing Images
Behavior: Alpha oscillates between 0.3 and 1.0 using a sine wave for smooth transitions.

Setup: Add images to the Pulsing Images array and adjust Pulse Speed.

3. Animated Dots
Behavior: Cycles through "", ".", "..", "..." to show the process is active.

Setup: Assign the Loading Text component and adjust Dots Animation Speed.

🛠️ Technical Details & Best Practices
Performance
✅ Do:

Use TextMeshProUGUI instead of legacy UI Text.

Keep the number of pulsing images under 5.

Use Sprite Atlases for UI elements.

Deactivate unnecessary elements during loading.

❌ Avoid:

Complex animations in Update().

Too many elements with simultaneous fades.

Heavy calculations inside animation coroutines.

UX
✅ Do:

Minimum 1s visible screen time.

Varied and useful tips (5-10 different ones).

Smoothed progress (avoid sudden jumps).

Constant visual feedback (rotation/pulsing).

❌ Avoid:

Instant loads (confuses the user).

Extremely long tips (max 2 lines).

Progress bar that moves backward.

Static screens without any animation.

🚀 Future Extensions
[ ] All-in-one Prefab - Ready-to-use drag-and-drop prefab.

[ ] Dynamic Loading Tips - Load tips from JSON/ScriptableObject.

[ ] Multiple Themes - Different styles of loading screens.

[ ] Phased Progress - Show "Loading assets...", "Initializing...", etc.



