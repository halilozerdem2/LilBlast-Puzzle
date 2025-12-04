using UnityEngine;
using System.Collections;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;

    [Header("Duraklama Süreleri (Main Menu için)")]
    public float walkDuration = 3f;
    public float idleDuration = 2f;
    public float danceDuration = 4f;

    private Coroutine menuRoutine,gameplayRoutine;
    private bool isPlayState = false;

    private void Start()
    {
        // Başlangıçta Main Menu animasyon rutinini başlat
        SetMenuAnimations();
    }

    public void SetMenuAnimations()
    {
        menuRoutine = StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine()
    {
        animator.applyRootMotion = true;
        // İlk yürüyüş animasyonu sadece 1 kez
        animator.SetTrigger("WalkTrigger");
        yield return new WaitForSeconds(walkDuration);

        // Sonsuz döngü (Idle-Dance1-Dance2)
        while (!isPlayState) // sadece Play State'e geçmeden çalışacak
        {
            animator.SetTrigger("IdleTrigger");
            yield return new WaitForSeconds(idleDuration);

            animator.SetTrigger("Dance1Trigger");
            yield return new WaitForSeconds(danceDuration);

            animator.SetTrigger("Dance2Trigger");
            yield return new WaitForSeconds(danceDuration);
        }
    }

    // GameManager veya State Manager burayı çağırabilir
    public void OnPlayStateEnter()
    {
        isPlayState = true;
        animator.applyRootMotion = false;

        // Menü animasyon döngüsünü durdur
        if (menuRoutine != null)
            StopCoroutine(menuRoutine);


        // Play animasyon lojiklerini başlat
        PlayAnimationLogic();
    }

    private void PlayAnimationLogic()
    {
        gameplayRoutine = StartCoroutine(PlayAnimationLoop());
    }
    
    private IEnumerator PlayAnimationLoop()
    {
        while (isPlayState)
        {
            // 1. animasyon
            animator.SetTrigger("PlayStateIdle1Trigger");
            yield return new WaitForSeconds(GetAnimationLength("PlayState_Idle1"));

            // 2. animasyon
            animator.SetTrigger("PlayStateIdle2Trigger");
            yield return new WaitForSeconds(GetAnimationLength("PlayState_Idle2"));
        }
    }

    private float GetAnimationLength(string animationName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (var clip in ac.animationClips)
        {
            if (clip.name == animationName)
                return clip.length;
        }
        return 1f; // default süre (bulunamazsa)
    }

    public void OnPlayStateExit()
    {
        isPlayState = false;
        if (gameplayRoutine != null)
        {
            StopCoroutine(gameplayRoutine);
            gameplayRoutine = null;
        }
    }
}