using UnityEngine;

namespace GameSystem    
{
    public class ScrewGameManager:MonoBehaviour
    {
        Screw screw;
        public void Start()
        {
            Debug.Log("hello");
            screw = new Screw();
        }
        public void Update()
        {
            Debug.Log("hello");
            StartCoroutine(screw.navigatedCards());
        }
    }
}
