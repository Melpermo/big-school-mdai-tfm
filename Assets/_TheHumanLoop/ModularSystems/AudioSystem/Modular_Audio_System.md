# Modular Audio System (HumanLoop AudioSystem)

A simple audio architecture for Unity that decouples audio data from logic using **ScriptableObjects**, featuring **Object Pooling**, **Cross-fading**, and **Persistent Settings**.

## 🛠 Core Components

1. **AudioConfiguration (SO)**: Defines the *how* (Volume, Pitch, Mixer Group output).
2. **SoundEvent (SO)**: Defines the *what* (Audio clips array + Configuration reference).
3. **AudioManager**: A Singleton handling the AudioSource pool and music transitions.
	-- NOTICE --If using Bootstrap, comment out or remove the lines of code that make the Singleton persistent between scenes:
	```csharp
	// Simple Singleton Pattern
	if (Instance == null)
	{
		Instance = this;
		//DontDestroyOnLoad(gameObject);
		InitializePool();
	}
	else
	{
		//Destroy(gameObject);
	}
	```csharp
4. **AudioToggleLink**: Connects UI Toggles to the Mixer with automatic `PlayerPrefs` saving.

## 🚀 Setup Guide

### 1. AudioMixer Setup
* Create a Mixer hierarchy: `Master > Music, SFX, Ambient`.
* **Critical**: Right-click the "Volume" label of each group and select "Expose to script".
* In the *Exposed Parameters* tab, rename them to: `MasterVol`, `MusicVol`, `SFXVol`, and `AmbientVol`.

### 2. Creating Audio Assets
1. Create an **AudioConfiguration** asset (e.g., `SFX_Config`). Assign its Mixer Group.
2. Create a **SoundEvent** asset (e.g., `Player_Jump_Snd`).
3. Add your clips to the SoundEvent and link the Configuration from step 1.

### 3. Scene Integration
1. Place the `AudioManager` in your bootstrap scene.
2. Attach `AudioToggleLink` to your settings menu Toggles.
3. Type the exact exposed parameter name (e.g., `SFXVol`) in the script field.

## 💡 Code Implementation
To trigger sounds from any gameplay script:
```csharp
using HumanLoop.AudioSystem;

public SoundEvent jumpSFX;

// Play a one-shot SFX
AudioManager.Instance.PlaySound(jumpSFX);

// Play music with a 2-second cross-fade
AudioManager.Instance.PlayMusic(backgroundMusic, 2.0f);