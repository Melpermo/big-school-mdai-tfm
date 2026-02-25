using HumanLoop.Core;
using UnityEditor;

namespace HumanLoop.Tools
{
#if (UNITY_EDITOR)
    // This class defines a cheat menu in the Unity Editor to force game over or victory conditions for testing purposes.
    public static class CheatMenu
    {
        // Game Over Cheats

        [MenuItem("The Human Loop/Cheats/ForceBudgetGameOver")]
        public static void ForceBudgetGameOver()
        {
            GameStatsManager.Instance.ForceBudgetGameOver();
        }
        
        [MenuItem("The Human Loop/Cheats/ForceTimeGameOver")]
        public static void ForceTimeGameOver()
        {
            GameStatsManager.Instance.ForceTimeGameOver();
        }

        [MenuItem("The Human Loop/Cheats/ForceMoraleGameOver")]
        public static void ForceMoraleGameOver()
        {
            GameStatsManager.Instance.ForceMoraleGameOver();
        }

        [MenuItem("The Human Loop/Cheats/ForceQualityGameOver")]
        public static void ForceQualityGameOver()
        {
            GameStatsManager.Instance.ForceMoraleGameOver();
        }

        // Victory Cheats

        [MenuItem("The Human Loop/Cheats/ForceWin")]
        public static void ForceVictory()
        {
            GameStatsManager.Instance.ForceVictory();
        }

        [MenuItem("The Human Loop/Cheats/ForceBudgetVictory")]
        public static void ForceBudgetVictory()
        {
            GameStatsManager.Instance.ForceBudgetVictory();
        }

        [MenuItem("The Human Loop/Cheats/ForceTimeVictory")]
        public static void ForceTimeVictory()
        {
            GameStatsManager.Instance.ForceTimeVictory();
        }

        [MenuItem("The Human Loop/Cheats/ForceMoraleVictory")]
        public static void ForceMoraleVictory()
        {
            GameStatsManager.Instance.ForceMoraleVictory();
        }

        [MenuItem("The Human Loop/Cheats/ForceQualityVictory")]
        public static void ForceQualityVictory()
        {
            GameStatsManager.Instance.ForceQualityVictory();
        }
    }
#endif
}
