using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SophieBoomAnamaliaHelper : MonoBehaviour
{
    public SophieBoomAnamalia sophieBoomAnamalia;
    private bool isOnly = false;

    void OnEnable()
    {
        isOnly = false;
    }

    void OnDisable()
    {
        isOnly = false;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnly)
        {
            isOnly = true;
            sophieBoomAnamalia.WakeUpSophie();
        }
    }
}
