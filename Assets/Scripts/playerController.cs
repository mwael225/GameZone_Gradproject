using Unity.Netcode;
using UnityEngine;

public class playerController : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(IsOwner)
        {
            transform.GetChild(1).gameObject.SetActive(true);
            Debug.Log(transform.GetChild(1).gameObject.name+"is active");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
