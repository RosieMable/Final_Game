using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class WeaponHook : MonoBehaviour
    {
        public GameObject damageCollider;
        
        /// <summary>
        /// Enable damage attached collider.
        /// </summary>
        public void OpenDamageCollider()
        {
            damageCollider.SetActive(true);
        }

        /// <summary>
        /// Disable damage attached collider.
        /// </summary>
        public void CloseDamageCollider()
        {
            damageCollider.SetActive(false);
        }
    }
}
