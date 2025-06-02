using UnityEngine;
using System.Collections;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;

    [Header("Duraklama Süreleri")]
    public float walkDuration = 3f;
    public float idleDuration = 2f;
    public float danceDuration = 4f;

    private void Start()
    {
        StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine()
    {
        // İlk yürüyüş animasyonu sadece 1 kez
        animator.SetTrigger("WalkTrigger");
        yield return new WaitForSeconds(walkDuration);

        // Sonsuz döngü (Idle-Dance1-Dance2)
        while (true)
        {
            animator.SetTrigger("IdleTrigger");
            yield return new WaitForSeconds(idleDuration);

            animator.SetTrigger("Dance1Trigger");
            yield return new WaitForSeconds(danceDuration);

            animator.SetTrigger("Dance2Trigger");
            yield return new WaitForSeconds(danceDuration);
        }
    }
}