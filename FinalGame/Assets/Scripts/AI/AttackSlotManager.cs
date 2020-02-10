using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class AttackSlotManager : MonoBehaviour
    {
        private List<GameObject> slots;
        [SerializeField] private int count = 6;
        [SerializeField] private float distance = 3f;

        void Start()
        {
            slots = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                slots.Add(null);
            }
        }

        public Vector3 GetSlotPosition(int index)
        {
            float degreesPerIndex = 360f / count;
            var position = transform.position;
            var offset = new Vector3(0, 0, distance);
            return position + (Quaternion.Euler(new Vector3(0, degreesPerIndex * index, 0)) * offset);
        }

        public int ReserveSlot(Enemy attacker)
        {
            var bestPosition = attacker.transform.position;
            var offset = (attacker.transform.position - bestPosition).normalized * distance;
            bestPosition += offset;
            int bestSlot = -1;
            float bestDist = Mathf.Infinity;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    continue;
                }

                var distance = (GetSlotPosition(i) - bestPosition).sqrMagnitude;

                if (distance < bestDist)
                {
                    bestSlot = i;
                    bestDist = distance;
                }
            }

            if (bestSlot != -1)
            {
                slots[bestSlot] = attacker.gameObject;
            }
            return bestSlot;
        }

        public void ClearSlot(int slot)
        {
            slots[slot] = null;
        }

        void OnDrawGizmosSelected()
        {
            for (int index = 0; index < count; ++index)
            {
                if (slots == null || slots.Count <= index || slots[index] == null)
                    Gizmos.color = Color.black;
                else
                    Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(GetSlotPosition(index), 0.5f);
            }
        }
    }
}
