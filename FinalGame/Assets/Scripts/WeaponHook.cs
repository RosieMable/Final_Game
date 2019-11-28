using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsThisDarkSouls
{
    public class WeaponHook : MonoBehaviour
    {
        public GameObject damageCollider;
        
        public void OpenDamageCollider()
        {
            damageCollider.SetActive(true);
        }

        public void CloseDamageCollider()
        {
            damageCollider.SetActive(false);
        }
    }
}
