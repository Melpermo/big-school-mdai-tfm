# 📊 Sistema de Game Stats y End Game Conditions

**Versión:** 1.0  
**Proyecto:** The Human Loop  
**Última actualización:** Febrero 2026

---

## 📑 Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Componentes Principales](#componentes-principales)
4. [Flujo de Ejecución](#flujo-de-ejecución)
5. [Configuración Paso a Paso](#configuración-paso-a-paso)
6. [Cómo Extender el Sistema](#cómo-extender-el-sistema)
7. [Testing y Debug](#testing-y-debug)
8. [Troubleshooting](#troubleshooting)
9. [Ejemplos Prácticos](#ejemplos-prácticos)

---

## 🎯 Visión General

Este sistema gestiona las **4 estadísticas principales del juego** (Budget, Time, Morale, Quality) y detecta automáticamente **condiciones de fin de juego** (Victoria, Game Over, etc.).

### Características Principales

✅ **Gestión centralizada de stats** mediante Singleton  
✅ **Condiciones configurables** sin tocar código (ScriptableObjects)  
✅ **Sistema de eventos desacoplado** para comunicación entre sistemas  
✅ **Extensible y mantenible** - Fácil añadir nuevas condiciones  
✅ **Herramientas de debug integradas** - ContextMenu para testing

### Las 4 Estadísticas

| Stat | Rango | Representa | Colapso en 0 |
|------|-------|------------|--------------|
| 💰 **Budget** | 0-100 | Dinero para salarios, herramientas | Insolvencia |
| ⏳ **Time** | 0-100 | Plazos, margen del proyecto | Proyecto cancelado |
| 🧠 **Morale** | 0-100 | Motivación, salud mental | Equipo abandona |
| 🧩 **Quality** | 0-100 | Salud del código, deuda técnica | Código inmantenible |

---

## 🏗️ Arquitectura del Sistema

El sistema está compuesto por **3 capas principales**:

### Separación de Responsabilidades

| Componente | Responsabilidad | Tipo |
|------------|----------------|------|
| **GameStatsManager** | Gestionar stats y detectar condiciones | MonoBehaviour (Logic) |
| **EndGameConditionSO** | Definir condición y evento asociado | ScriptableObject (Data) |
| **GameEventSO** | Canal de comunicación entre sistemas | ScriptableObject (Events) |
| **EndGameUIHandler** | Mostrar UI cuando se cumple condición | MonoBehaviour (Presentation) |

---

## 🧩 Componentes Principales

### 1. GameStatsManager.cs

**Ubicación:** `Assets\_TheHumanLoop\Core\Scripts\Core_Scripts\GameStatsManager.cs`

**Responsabilidades:**
- Mantener valores actuales de los 4 stats
- Aplicar cambios (deltas) y clampearlos a 0-100
- Disparar evento `OnStatsChanged` tras cada actualización
- Verificar todas las `EndGameConditionSO` configuradas
- Delegar la decisión de disparar eventos a cada condición

**API Pública:**
```csharp
	// Ejemplo de API pública
	public void UpdateBudget(float delta);
	public void UpdateTime(float delta);
	public void UpdateMorale(float delta);
	public void UpdateQuality(float delta);
```

### 2. EndGameConditionSO.cs

**Ubicación:** `Assets\_TheHumanLoop\Core\Scripts\ScriptableObjects\CardDataSO\EndGameConditionSO.cs`

**Responsabilidades:**
- Definir **qué stat(s)** vigilar y con qué **threshold**
- Implementar lógica de `CheckCondition()` para determinar si se cumple
- Almacenar referencia al **GameEventSO** que debe dispararse
- Ejecutar `RaiseEvent()` cuando se cumple la condición

**Campos Configurables:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `conditionName` | string | Nombre descriptivo (ej: "Sin Presupuesto") |
| `conditionType` | enum | Victory, GameOver, SpecialMet, SpecialFailed |
| `statToCheck` | enum | All, Budget, Time, Morale, Quality |
| `threshold` | float | Valor de comparación (ej: 0 para Game Over) |
| `endGameConditionEvent` | GameEventSO | Evento a disparar cuando se cumpla |
| `endGameMessage` | string | Mensaje para mostrar en UI |
| `endGameBG_image` | Sprite | Imagen de fondo para UI |

**Lógica de Verificación:**
```csharp
	// Ejemplo de lógica en CheckCondition
	if(statToCheck == StatType.Budget && currentBudget <= threshold) {
		// Lógica para Game Over por bajo presupuesto
	}
```

---

### 3. GameEventSO.cs

**Ubicación:** `Assets\_TheHumanLoop\Core\Scripts\ScriptableObjects\Events\GameEventSO.cs`

**Responsabilidades:**
- Actuar como canal de comunicación desacoplado
- Permitir que múltiples sistemas se suscriban al mismo evento
- No contener lógica de negocio

**Patrón Observer:**

---

## 🔄 Flujo de Ejecución

### Secuencia Completa: "Jugador pierde presupuesto y llega a 0"

### Paso a Paso Detallado

1. **Carta aplica efectos:** `CardDataSO` llama a `GameStatsManager.UpdateStats()`
2. **Stats se actualizan:** Budget pasa de 50 a 0 (clamped)
3. **Evento de cambio:** Se dispara `OnStatsChanged` → UI se actualiza
4. **Verificación de condiciones:** `CheckEndGameConditions()` itera la lista
5. **Evaluación individual:** Cada `EndGameConditionSO` ejecuta `CheckCondition()`
6. **Primera condición cumplida:** "Budget GameOver" detecta budget <= 0
7. **Disparo de evento específico:** `endGameConditionEvent.Raise()` (ej: `OnGameOverEvent`)
8. **UI reacciona:** `EndGameUIHandler` escucha el evento y muestra pantalla
9. **Detención:** `return` evita que se evalúen más condiciones (first match wins)

---

## ⚙️ Configuración Paso a Paso

### 1. Crear Eventos (GameEventSO)

**Ubicación recomendada:** `Assets\_TheHumanLoop\Core\Data\Events\`

1. Clic derecho → **Create → HumanLoop → Events → Game Event**
2. Nombrar: `OnGameOverEvent`, `OnVictoryEvent`, etc.
3. Repetir para cada tipo de fin de juego

### 2. Crear Condiciones (EndGameConditionSO)

**Ubicación recomendada:** `Assets\_TheHumanLoop\Core\Data\EndGameConditions\`

#### Ejemplo: Condición "Sin Presupuesto"

1. Clic derecho → **Create → The Human Loop → EndGameCondition → EndGameConditionSO**
2. Configurar campos:
| Campo | Valor |
|-------|-------|
| `conditionName` | "Sin Presupuesto" |
| `conditionType` | GameOver |
| `statToCheck` | Budget |
| `threshold` | 0 |
| `endGameMessage` | "Te has quedado sin presupuesto." |
| `endGameBG_image` | (Seleccionar imagen) |

3. Asignar evento correspondiente (`OnGameOverEvent`)
4. Repetir para otras condiciones como "Tiempo Agotado" o "Mala Calidad"

### 3. Configurar GameStatsManager en la Escena

1. Seleccionar el GameObject que contiene `GameStatsManager`
2. En el Inspector, configurar:

````````
**⚠️ Orden Importante:** Las condiciones se evalúan en orden. Pon las más específicas primero si hay solapamiento.

### 4. Configurar UI Handler

1. En `EndGameUIHandler`, asignar:
   - Eventos específicos (`OnGameOverEvent`, `OnVictoryEvent`, etc.)
   - Suscribirse en `OnEnable()` / Desuscribirse en `OnDisable()`

---

## 🔧 Cómo Extender el Sistema

### Añadir un Nuevo Tipo de Condición

1. **Extender el enum en EndGameConditionSO:**
   - Añadir nuevo valor (ej: `CustomCondition`)
2. **Implementar lógica en CheckCondition():**
   - Definir criterio para la nueva condición
3. **Crear el GameEventSO asociado:**
   - `OnCustomConditionEvent`
4. **Crear las instancias de EndGameConditionSO:**
   - Configurar con el nuevo tipo

### Añadir un Nuevo Stat

1. **En GameStatsManager, añadir campo:**
   - Nuevo campo para la estadística (ej: `private float currentSatisfaction;`)
2. **Clampeo y eventos:**
   - Incluir en `ClampStats()` y `UpdateStats()`
3. **Exponer en API pública:**
   - agregar métodos como `public void UpdateSatisfaction(float delta);`

---

## 🧪 Testing y Debug

- **Debug de Stats:** Ventana en tiempo real de valores de stats
- **Forzado de condiciones:** Opción para forzar condiciones de fin de juego
- **Logs detallados:** Registro en consola de cambios y condiciones cumplidas

### Herramientas Integradas (ContextMenu)

Haz clic derecho en `GameStatsManager` en el Inspector para acceder a:

#### Debug
- **Log Current Stats** - Muestra valores actuales
- **Log All Conditions** - Lista todas las condiciones y su estado

#### Testing
- **Reset to Initial Values** - Vuelve a valores de inicio
- **Test Positive Change (+10 All)** - Incrementa todo en 10
- **Test Negative Change (-10 All)** - Decrementa todo en 10
- **Force Budget to 0** - Dispara Game Over por presupuesto
- **Force Time to 0** - Dispara Game Over por tiempo
- **Force Morale to 0** - Dispara Game Over por moral
- **Force Quality to 0** - Dispara Game Over por calidad
- **Force All Stats to 100** - Dispara Victoria

### Logs de Verificación

Cuando se actualizan stats, verás logs como:

---

## 🔍 Validar Configuración

En `EndGameConditionSO`, clic derecho → **Validate Configuration**

Verifica:
- ✅ Nombre no vacío
- ✅ Evento asignado
- ✅ Mensaje configurado
- ⚠️ Advierte sobre configuraciones ambiguas

---

## ❗ Troubleshooting

### Problema: "Se dispara la condición incorrecta"

**Síntoma:** Budget está bien pero se dispara "Sin Presupuesto"

**Causas:**
1. ❌ Varias condiciones GameOver con `statToCheck = All`
2. ❌ Thresholds mal configurados
3. ❌ Orden incorrecto en la lista

**Solución:**

### Problema: "No se dispara ningún evento"

**Checklist:**
- [ ] `GameStatsManager.Instance` no es null
- [ ] Lista `endGameConditionsList` tiene elementos
- [ ] Cada `EndGameConditionSO` tiene `endGameConditionEvent` asignado
- [ ] El `GameEventSO` tiene listeners suscritos (`OnEventRaised`)

**Debug:**
````````

### Problema: "Stats no se actualizan en UI"

**Causa:** `_onStatsChangedEvent` no está asignado o no tiene listeners

**Solución:**
1. Verificar que `_onStatsChangedEvent` tiene un ScriptableObject asignado
2. En el script de UI, suscribirse correctamente:
````````

### Problema: "Memory leaks con eventos"

**Causa:** No desuscribirse en `OnDisable()`

**Solución:**
````````

---

## 📚 Ejemplos Prácticos

### Ejemplo 1: Carta que afecta stats

```csharp
public class CartaEjemplo : MonoBehaviour
{
    public void AplicarEfecto()
    {
        GameStatsManager.Instance.UpdateBudget(-10);
        GameStatsManager.Instance.UpdateTime(-5);
        GameStatsManager.Instance.UpdateMorale(-15);
        GameStatsManager.Instance.UpdateQuality(10);
    }
}
```

#### Resultado Esperado

- Budget baja de 50 a 40
- Time baja de 75 a 70
- Morale baja de 60 a 45
- Quality sube de 20 a 30

---

### Ejemplo 2: Configuración de Condición

#### Condición "Tiempo Agotado"

| Campo | Valor |
|-------|-------|
| `conditionName` | "Tiempo Agotado" |
| `conditionType` | GameOver |
| `statToCheck` | Time |
| `threshold` | 0 |
| `endGameMessage` | "El tiempo se ha agotado." |
| `endGameBG_image` | (Seleccionar imagen) |

---

### Ejemplo 3: Victoria por excelencia

| Campo | Valor |
|-------|-------|
| `conditionName` | "Victoria por excelencia" |
| `conditionType` | Victory |
| `statToCheck` | Quality |
| `threshold` | 100 |
| `endGameMessage` | "¡Has logrado una calidad de código excepcional!" |
| `endGameBG_image` | (Seleccionar imagen) |

---

### Ejemplo 4: Script de UI

```csharp
public class EndGameUIHandler : MonoBehaviour
{
    public Text mensajeFinJuego;
    public Image bgImage;

    private void OnEnable()
    {
        GameEventManager.StartListening("OnGameOverEvent", MostrarGameOver);
        GameEventManager.StartListening("OnVictoryEvent", MostrarVictoria);
    }

    private void OnDisable()
    {
        GameEventManager.StopListening("OnGameOverEvent", MostrarGameOver);
        GameEventManager.StopListening("OnVictoryEvent", MostrarVictoria);
    }

    public void MostrarGameOver(string mensaje)
    {
        mensajeFinJuego.text = mensaje;
        bgImage.sprite = /* Sprite de Game Over */;
        gameObject.SetActive(true);
    }

    public void MostrarVictoria(string mensaje)
    {
        mensajeFinJuego.text = mensaje;
        bgImage.sprite = /* Sprite de Victoria */;
        gameObject.SetActive(true);
    }
}
```

**Lógica especial (si necesitas condiciones complejas):**
````````

---

## 📝 Convenciones y Buenas Prácticas

### Nomenclatura

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Condiciones SO | Descripción clara | `BudgetGameOver`, `ProjectCompleted` |
| Eventos SO | On + Acción + Event | `OnGameOverEvent`, `OnVictoryEvent` |
| Nombres en Inspector | Español descriptivo | "Sin Presupuesto", "Proyecto Completado" |

### Organización de Assets

```
````````

### Performance

✅ **Hacer:**
- Mantener la lista de condiciones pequeña (< 10 condiciones)
- Usar `return` tras la primera coincidencia
- Cachear referencias a ScriptableObjects

❌ **Evitar:**
- Condiciones con lógica compleja en `CheckCondition()`
- Múltiples evaluaciones de la misma condición
- Crear ScriptableObjects en runtime

---

## 🎓 Conceptos Clave

### Patrón Singleton
`GameStatsManager.Instance` permite acceso global sin dependencias explícitas.

### ScriptableObjects como Data
Configuración visual sin recompilar, compartible entre escenas.

### Event-Driven Architecture
Comunicación desacoplada: emisores no conocen receptores.

### First Match Wins
Primera condición que se cumple gana (previene múltiples fin de juego).

---

## 🔗 Referencias

- **Documentación del Proyecto:** `.github/copilot-instructions.md`
- **Convenciones de Código:** `.github/agents/CODING_CONVENTIONS.md`
- **Arquitectura del Juego:** `.github/agents/GAME_SYSTEMS.md`

---

## 📊 Diagrama de Arquitectura

---

## 📄 Licencia y Créditos

**Proyecto:** The Human Loop  
**Autor:** Melpermo  
**Institución:** Big School - Máster en Desarrollo de IA  
**Fecha:** Febrero 2026  
**Repositorio:** https://github.com/Melpermo/big-school-mdai-tfm

---

**¿Necesitas ayuda?** Revisa la sección de [Troubleshooting](#troubleshooting) o usa las herramientas de [Testing](#testing-y-debug) integradas.