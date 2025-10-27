using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JQG_PersistentObject : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
