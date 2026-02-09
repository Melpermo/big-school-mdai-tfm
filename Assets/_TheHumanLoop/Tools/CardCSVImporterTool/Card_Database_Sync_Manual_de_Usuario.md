# 🛠 Card Database Sync (Manual de Usuario)

## Descripción
Esta herramienta sincroniza el archivo maestro `human_loop_cards_v1.csv` con la base de datos de assets (`.asset`) de Unity. Crea automáticamente instancias de `SimpleCardData` y actualiza sus valores.

## Cómo usarlo
1. **Abrir la herramienta:** Ve al menú superior `The Human Loop > Card Database Sync`.
2. **Configurar archivos:**
   - **CSV File:** Arrastra el archivo `.csv` desde tu carpeta de Proyecto a este campo.
   - **Assets Target Folder:** Define la ruta donde se guardarán las cartas (ej: `Assets/GameData/Cards`).
3. **Sincronizar:** Haz clic en **SYNC ALL ASSETS**.

## Funcionalidades Clave
- **Regex Parsing:** Soporta comas y comillas dentro de las descripciones narrativas sin romper el formato.
- **Sistema de Dos Pasadas:** 1. Crea los assets y rellena stats y textos.
    2. Vincula las referencias de `Next Card Left/Right` buscando por el ID.
- **Barra de Progreso:** Indica el estado de la importación y vinculación en tiempo real.
- **Preservación de Datos:** Si un asset ya existe, actualiza sus valores sin borrarlo, manteniendo las referencias que tengas en otras partes del proyecto.

> ⚠️ **Nota:** Asegúrate de que la columna `ID` en el CSV sea única para cada carta.