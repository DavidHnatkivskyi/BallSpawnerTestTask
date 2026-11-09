using DG.Tweening;
using UnityEngine;

namespace BallGame
{
    internal sealed class Door : MonoBehaviour
    {
        private Transform _leftLeaf;
        private Transform _rightLeaf;

        public bool IsOpen { get; private set; }

        public void Initialize(Transform leftLeaf, Transform rightLeaf)
        {
            _leftLeaf = leftLeaf;
            _rightLeaf = rightLeaf;
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;
            _leftLeaf.DOLocalMoveX(_leftLeaf.localPosition.x - 1.4f, 0.35f).SetEase(Ease.OutBack);
            _rightLeaf.DOLocalMoveX(_rightLeaf.localPosition.x + 1.4f, 0.35f).SetEase(Ease.OutBack);
        }
    }
}
