using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class WeaponHook : MonoBehaviour
    {
        public DamageCollider damageCollider;

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
    }
}
