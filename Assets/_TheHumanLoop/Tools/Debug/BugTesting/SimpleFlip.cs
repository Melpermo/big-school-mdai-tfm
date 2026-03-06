using DG.Tweening;
using UnityEngine;

namespace TheHumanLoop.Debugging
{
    public class SimpleFlip : MonoBehaviour
    {
        private bool isFlipped = false;
        private float countDown = 1f;

        private void Update()
        {
                     

        }

        [ContextMenu("Flip")]
        public void Flip()
        {
            isFlipped = !isFlipped;
            //transform.localScale = new Vector3(isFlipped ? -1 : 1, 1, 1);
            //transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
            transform.DORotate(new(0, isFlipped ? 0f : 180f, 0), 0.25f);
        }
    }
}
