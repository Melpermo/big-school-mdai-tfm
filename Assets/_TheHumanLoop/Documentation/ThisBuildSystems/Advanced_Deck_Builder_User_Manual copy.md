# 🎴 Advanced Deck Builder (User Manual)

## Description
Allows for the creation and updating of `DeckSO` (deck) assets by filtering the card database using specific criteria defined in the master CSV.

## How to Use
1. **Open the Tool:** Navigate to the top menu: `The Human Loop > Advanced Deck Builder`.
2. **Configure References:**
   - **Reference CSV:** Drag the same CSV used for syncing.
   - **Cards Folder:** The folder where your card assets are stored.
   - **Target Deck SO:** The `DeckSO` object you wish to populate.
3. **Configure Filter Rules:**
   - Click `+ Add Rule`.
   - **CSV Column:** The exact header name from the CSV (e.g., `Phase` or `Category`).
   - **Value:** The value you are looking for (e.g., `Early`, `Mid`, `Technical`).
4. **Execute:** Click the **BUILD FILTERED DECK** button.

## Logic Overview
- **AND Logic:** If you add multiple rules, a card is only added if it meets **all** criteria simultaneously.
- **Case Insensitivity:** The system matches values regardless of whether you type `mid` or `Mid`.
- **Safety First:** This script only manages references; it never creates or deletes card asset files.

## Common Usage Example
To create an **Early Game** deck specifically for **Business** cards:
- Rule 1: `Column: Phase` | `Value: Early`
- Rule 2: `Column: Category` | `Value: Business`