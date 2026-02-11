# Sistema de Audio Modular (HumanLoop AudioSystem)

Este sistema utiliza **ScriptableObjects** para desacoplar los datos del sonido de la lógica de programación, permitiendo una gestión eficiente mediante **Object Pooling** y control dinámico a través de **AudioMixers**.

## 🛠 Estructura Principal

1. **AudioConfiguration (SO)**: Define *cómo* suena (Volumen, Pitch, Mixer Group).
2. **SoundEvent (SO)**: Define *qué* suena (Lista de clips + Configuración).
3. **AudioManager**: Singleton que gestiona el pool de fuentes y el Cross-fade de música. 
	-- AVISO --En caso de usar Bootstrap comentar o eliminar las líneas de código que hacen persistente el Singleton entre escenas:
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
4. **AudioToggleLink**: Conecta la UI (Toggles) con el Mixer y guarda las preferencias.

## 🚀 Configuración Paso a Paso

### 1. El AudioMixer
* Crea un Mixer con la jerarquía: `Master > Music, SFX, Ambient`.
* **Importante**: Haz clic derecho en el volumen de cada grupo y selecciona "Expose to script".
* En la pestaña *Exposed Parameters*, renómbralos exactamente como: `MasterVol`, `MusicVol`, `SFXVol`, `AmbientVol`.

### 2. Creación de Sonidos
1. Crea un asset de **AudioConfiguration** (ej. `Config_SFX`). Asigna el grupo del Mixer correspondiente.
2. Crea un asset de **SoundEvent** (ej. `Snd_Salto`).
3. Arrastra los clips de audio al SoundEvent y asigna la configuración creada en el paso 1.

### 3. Escena y UI
1. Coloca el prefacio `AudioManager` en tu escena inicial.
2. Para los ajustes, añade el componente `AudioToggleLink` a cada Toggle de tu menú.
3. Escribe el nombre del parámetro expuesto (ej. `MusicVol`) en el campo del script.

## 💡 Uso en Código
Para reproducir un sonido desde cualquier script:
```csharp
using HumanLoop.AudioSystem;

public SoundEvent miSonido;
// Efecto de sonido
AudioManager.Instance.PlaySound(miSonido); 
// Música con transición
AudioManager.Instance.PlayMusic(miSonido, 2.0f);