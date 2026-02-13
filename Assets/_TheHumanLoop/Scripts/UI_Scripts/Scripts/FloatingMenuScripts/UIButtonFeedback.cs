using DG.Tweening;
using UnityEngine;

namespace TheHumanLoop.UI
{
    public class UIButtonFeedback : MonoBehaviour
    {
        public void OnButtonClick()
        {
            // Efecto de "latido" rápido
            transform.DOScale(0.95f, 0.1f).OnComplete(() => {
                transform.DOScale(1f, 0.1f);
            });
        }
    }
}
