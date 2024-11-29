using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerAnimatorController : MonoBehaviour
{
    Animator anim;

    [SerializeField] private Animator defaultController;
    [SerializeField] private List<AnimatorOverrideController> animControllers = new List<AnimatorOverrideController>();

    [SerializeField] private int index = 0;

    private void Start()
    {
        anim = GetComponent<Animator>();
        
        index = -1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            index++;
            if (index > animControllers.Count - 1) {
                index = 0;
            }
            anim.runtimeAnimatorController = animControllers[index];
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            index--;
            if (index < 0) {
                index = animControllers.Count - 1;
            }
            anim.runtimeAnimatorController = animControllers[index];
        }
    }
}
