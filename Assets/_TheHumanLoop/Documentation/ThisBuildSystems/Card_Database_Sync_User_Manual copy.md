# 🛠 Card Database Sync (User Manual)

## Description
This tool synchronizes the master `human_loop_cards_v1.csv` file with Unity's asset database (`.asset`). It automatically creates instances of `SimpleCardData` and updates their values based on the CSV data.

## How to Use
1. **Open the Tool:** Navigate to the top menu: `The Human Loop > Card Database Sync`.
2. **Configure Files:**
   - **CSV File:** Drag your `.csv` file from the Project window into this field.
   - **Assets Target Folder:** Define the path where cards will be saved (e.g., `Assets/GameData/Cards`).
3. **Synchronize:** Click the **SYNC ALL ASSETS** button.

## Key Features
- **Regex Parsing:** Supports commas and quotes within narrative descriptions without breaking the format.
- **Two-Pass System:** 1. First, it creates assets and populates stats and text fields.
    2. Second, it links `Next Card Left/Right` references by searching for the corresponding IDs.
- **Progress Bar:** Provides real-time visual feedback during the import and linking process.
- **Data Preservation:** If an asset already exists, it updates values without deleting the file, preserving existing references within your project.

> ⚠️ **Note:** Ensure the `ID` column in the CSV is unique for every card.