using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class AnimationDisplay : MonoBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int JabTrigger = Animator.StringToHash("Hit");
        private static readonly int SweepTrigger = Animator.StringToHash("HeavySwipe");

        [Header("References")] [SerializeField]
        private Animator animator;

        [Header("Animation Settings")] [SerializeField]
        private float delayBetweenLoops = 2f;

        [SerializeField] private bool playRun = false;
        [FormerlySerializedAs("playJab")] [SerializeField] private bool playHit = false;
        [FormerlySerializedAs("playSweep")] [SerializeField] private bool playHeavySwipe = false;
        [SerializeField] private bool playClean = false;

        [Header("Cleaning Settings (only used if cleaning")] [SerializeField]
        private ParticleSystem foamParticles;
        [SerializeField] private float particleYOffset = 0.5f;

        void Start()
        {
            if (playRun)
            {
                animator.SetBool(IsRunning, true);
            }
            else if (playHit)
            {
                StartCoroutine(LoopAttackAnimation(JabTrigger, "Hit"));
            }
            else if (playHeavySwipe)
            {
                StartCoroutine(LoopAttackAnimation(SweepTrigger, "HeavySwipe"));
            } else if (playClean)
            {
                StartCoroutine(LoopCleanParticleEmission());
            }
        }

        private IEnumerator LoopAttackAnimation(int triggerHash, string animationName)
        {
            float attackDuration = GetAnimationLength(animationName);

            while (true)
            {
                // Trigger attack animation
                animator.Play(triggerHash);

                // Wait for attack animation to complete
                yield return new WaitForSeconds(attackDuration);

                // Idle plays automatically, wait for delay period
                yield return new WaitForSeconds(delayBetweenLoops);
            }
        }

        private IEnumerator LoopCleanParticleEmission()
        {
            while (true)
            {
                EmitFoamParticle();
                yield return new WaitForSeconds(0.1f);
            }
        }

        private float GetAnimationLength(string animationName)
        {
            if (animator.runtimeAnimatorController == null)
                return 1f;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    return clip.length;
                }
            }

            Debug.LogWarning($"Animation '{animationName}' not found in Animator Controller!");
            return 1f; // Default fallback
        }

        private void EmitFoamParticle()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 1f;
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

            Vector2 targetPos = new Vector2(spawnPosition.x, this.transform.position.y + particleYOffset);

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = targetPos + Vector2.up * 0.1f,
                velocity = new Vector3(UnityEngine.Random.insideUnitCircle.x * 0.5f, 0, 0),
                startSize = UnityEngine.Random.Range(0.2f, 0.5f),
                rotation = UnityEngine.Random.Range(0f, 360f),
                startLifetime = UnityEngine.Random.Range(1.5f, 2.5f)
            };
            foamParticles.Emit(emitParams, 1);
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}