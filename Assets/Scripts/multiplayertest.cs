using System;
using Unity.Services.Core;
using UnityEngine;

public class Multiplayertest : MonoBehaviour
{
     void Start()
    {
        Debug.Log("Cloud Project ID: " + Application.cloudProjectId);
    }
}