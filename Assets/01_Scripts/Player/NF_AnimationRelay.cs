using UnityEngine;

public class NF_AnimationRelay : MonoBehaviour
{
    public void ResetCombo()
    {
        var controller = GetComponentInParent<CA_PlayerController>();
        if (controller != null)
            controller.ResetCombo();
    }
}
