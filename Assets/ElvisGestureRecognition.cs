using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElvisGestureRecognition : MonoBehaviour
{
    public string gestureName = "";

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }


    public void SetElvisAnimationByGesture(string gestureName)
    {
        Debug.Log(gestureName);

        string triggerId = "";

        switch (gestureName)
        {
            case "Direita":
                triggerId = "CHICKEN";
                break;
            case "180 Esq":
                triggerId = "TWIST";
                break;
            case "360":
                triggerId = "SAMBA";
                break;
            default:
                triggerId = "NO";
                break;
        }

        animator.SetTrigger(triggerId);
    }
}
