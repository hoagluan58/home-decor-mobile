using JSAM;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class Car : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (UIManager.Instance.IsPointerOverUIObject()) return;
            if (AudioManager.IsSoundPlaying(ESound.CarHorn)) return;
            AudioManager.PlaySound(ESound.CarHorn);
        }
    }
}