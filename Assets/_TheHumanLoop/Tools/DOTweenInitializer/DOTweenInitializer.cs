using UnityEngine;
using DG.Tweening; // Required

namespace TheHumanLoop.Tools
{
    public class DOTweenInitializer : MonoBehaviour
    {
        void Awake()
        {
            // Set the capacity at startup to avoid warnings and runtime allocation
            // First parameter: Max Tweens (individual animations)
            // Second parameter: Max Sequences (groups of animations)
            DOTween.SetTweensCapacity(1000, 200);
            //DOTween.SetTweensCapacity(800, 200); // Adjust these numbers based on your project's needs

            // Optional: Optimization for UI and Backgrounds
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultEaseType = Ease.OutQuad;
        }
    }
}
