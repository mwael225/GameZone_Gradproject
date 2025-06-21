using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

namespace GameSystem
{
    public class CameraControl : NetworkBehaviour
    {
        private Transform playercamera;

        void Start()
        {
            playercamera = transform.GetChild(1);
            if (IsOwner)
            {
                playercamera.gameObject.SetActive(true);
            }
            else
            {
                playercamera.gameObject.SetActive(false);
            }
        }
        void switchtoMaincamera()
        {
            
        }
    }

}
