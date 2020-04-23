using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class WeaponHook : MonoBehaviour
    {
        public DamageCollider damageCollider;
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] attackClips;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Enable damage attached collider.
        /// </summary>
        public void OpenDamageCollider()
        {
            damageCollider.gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable damage attached collider.
        /// </summary>
        public void CloseDamageCollider()
        {
            damageCollider.gameObject.SetActive(false);
        }

        public void PlayAttackSound()
        {
            int clipToPlay = Random.Range(0, attackClips.Length);
            audioSource.clip = attackClips[clipToPlay];
            audioSource.Play();
        }
    }
}
