using UnityEngine;

namespace GameSystem    
{
    public class ScrewGameManager:MonoBehaviour
    {
        Screw screw;
        public void Start()
        {
            screw = new Screw();
        }
        public void Update()
        {
            //StartCoroutine(screw.navigatedCards());
        }
    }
}
