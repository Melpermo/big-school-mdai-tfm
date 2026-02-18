# Sistema de UI Animada con DOTween

Este sistema proporciona una interfaz de usuario fluida, "jugosa" (juicy) utilizando **DOTween** en Unity. Incluye un menú flotante en cascada, ventanas pop-up elásticas y switches de opciones con guardado persistente.

## 🚀 Componentes Principales

### 1. FloatingMenu & Manager
- **FloatingMenuManager**: El cerebro central (Singleton) que coordina la apertura de ventanas y el cierre del menú.
- **FloatingMenu**: Gestiona el botón de la esquina y despliega la lista de sub-botones con un efecto de retraso escalonado (*Stagger*).

### 2. UIWindow
- Controla los paneles de contenido con animaciones simétricas.
- **Entrada**: Escala elástica (`OutBack`) y rotación tipo "punch".
- **Salida**: Anticipación y encogimiento (`InBack`).

### 3. UIToggleSwitch
- Interruptor dinámico para opciones (Música, SFX).
- **Auto-Layout**: Calcula automáticamente los límites de movimiento según el tamaño del fondo.
- **Feedback**: Incluye un efecto de vibración (*Shake*) en caso de error o bloqueo.

## 🛠 Instalación y Configuración

1. **Requisitos**: Tener instalado [DOTween](http://dotween.demigiant.com/).
2. **Jerarquía**:
    - Coloca el `FloatingMenuManager` en tu objeto persistente de sistemas.
    - Las ventanas deben tener el script `UIWindow` y un `CanvasGroup`.
    - Los switches deben tener `UIToggleSwitch` y el componente `Button`.
3. **Uso de UIFeedback**: Añade el script `UIFeedback` a cualquier botón para obtener un efecto de presión automático al hacer clic.

## 📝 Notas de Uso
- Todas las animaciones son seguras; usan `.DOKill()` para evitar conflictos si el usuario hace "spam" de clics.
- Los ajustes de los switches se guardan automáticamente en `PlayerPrefs`.