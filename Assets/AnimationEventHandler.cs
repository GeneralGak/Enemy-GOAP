using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent onAttack { get; set; } = new UnityEvent();
    public UnityEvent onAttackDone { get; set; } = new UnityEvent();

    public void OnAttack()
    {
        onAttack?.Invoke();
    }

    public void OnAttackDone()
    {
        onAttackDone?.Invoke();
    }
}
