# 🎴 Advanced Deck Builder (Manual de Usuario)

## Descripción
Permite crear y actualizar `DeckSO` (barajas) filtrando la base de datos de cartas mediante criterios específicos definidos en el CSV.

## Cómo usarlo
1. **Abrir la herramienta:** Ve al menú superior `The Human Loop > Advanced Deck Builder`.
2. **Configurar referencias:**
   - **Reference CSV:** Arrastra el mismo CSV usado en el Sync.
   - **Cards Folder:** Carpeta donde están los assets de las cartas.
   - **Target Deck SO:** El objeto `DeckSO` que deseas rellenar.
3. **Configurar Reglas de Filtrado:**
   - Haz clic en `+ Add Rule`.
   - **Column:** Nombre exacto del encabezado en el CSV (ej: `Phase` o `Category`).
   - **Value:** El valor que quieres buscar (ej: `Early`, `Mid`, `Technical`).
4. **Ejecutar:** Haz clic en **BUILD FILTERED DECK**.

## Lógica de Funcionamiento
- **Lógica AND:** Si añades varias reglas, la carta solo se añadirá si cumple **todas** simultáneamente.
- **Insensibilidad a Mayúsculas:** No importa si escribes `mid` o `Mid`, el sistema lo encontrará.
- **Seguridad:** El script solo añade referencias al mazo; no crea ni borra archivos de cartas.

## Ejemplo de uso común
Para crear un mazo de **Fase Temprana** de tipo **Negocios**:
- Regla 1: `Column: Phase` | `Value: Early`
- Regla 2: `Column: Category` | `Value: Business`