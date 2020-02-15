using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    /// <summary>
    /// Ran on the Player Character currently, calculates attack slots around the players position based on number of slots desired (count) and distance from the player, has methods for AIs to calculate slot position and reserve/clear slots.
    /// </summary>
    public class AttackSlotManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> slots;
        [SerializeField] private int count = 6; // Total number of attack slots
        [SerializeField] private float distance = 3f; // Distance each slot has from the player

        void Awake()
        {
            // Initialise number of slots
            slots = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                slots.Add(null);
            }
        }

        /// <summary>
        /// Retrieve the position of the slot at the desired index.
        /// </summary>
        public Vector3 GetSlotPosition(int index)
        {
            if (index != -1)
            {
                float degreesPerIndex = 360f / count;
                var position = transform.position;
                var offset = new Vector3(0, 0, distance); //+ new Vector3(Random.Range(-1,2), 0, Random.Range(-1, 2));       
                return position + (Quaternion.Euler(new Vector3(0, degreesPerIndex * index, 0)) * offset);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Reserve the slot returned whilst ignoring a specific slot if already occupying a slot with an option to select a slot at random.
        /// </summary>
        public int ReserveSlot(Enemy attacker, int slotToIgnore, bool selectAtRandom)
        {
            var bestPosition = attacker.transform.position;
            var offset = (attacker.transform.position - bestPosition).normalized * distance;
            bestPosition += offset;
            int bestSlot = -1;
            float bestDist = Mathf.Infinity;

            ClearSlot(slotToIgnore);

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    continue;
                }
                var distance = (GetSlotPosition(i) - bestPosition).sqrMagnitude;

                if (distance < bestDist && i != slotToIgnore)
                {
                    bestSlot = i;
                    bestDist = distance;
                }
            }

            if (selectAtRandom)
            {
                while (true)
                {
                    if (bestSlot + 1 < slots.Count && bestSlot + 1 >= 0)
                    {
                        if (slots[bestSlot + 1] == null && bestSlot + 1 != slotToIgnore)
                        {
                            bestSlot = bestSlot + 1;
                            break;
                        }
                    }

                    if (bestSlot - 1 < slots.Count && bestSlot - 1 >= 0)
                    {
                        if (slots[bestSlot - 1] == null && bestSlot - 1 != slotToIgnore)
                        {
                            bestSlot = bestSlot - 1;
                            break;
                        }
                    }

                    if (bestSlot + 2 < slots.Count && bestSlot + 2 >= 0)
                    {
                        if (slots[bestSlot + 2] == null && bestSlot + 2 != slotToIgnore)
                        {
                            bestSlot = bestSlot + 2;
                            break;
                        }
                    }

                    if (bestSlot - 2 < slots.Count && bestSlot - 2 >= 0)
                    {
                        if (slots[bestSlot - 2] == null && bestSlot - 2 != slotToIgnore)
                        {
                            bestSlot = bestSlot - 2;
                            break;
                        }
                    }

                    if (bestSlot + 3 < slots.Count && bestSlot + 3 >= 0)
                    {
                        if (slots[bestSlot + 3] == null && bestSlot + 3 != slotToIgnore)
                        {
                            bestSlot = bestSlot + 3;
                            break;
                        }
                    }

                    if (bestSlot - 3 < slots.Count && bestSlot - 3 >= 0)
                    {
                        if (slots[bestSlot - 3] == null && bestSlot - 3 != slotToIgnore)
                        {
                            bestSlot = bestSlot - 3;
                            break;
                        }
                    }
                    break;
                }
            }

            if (bestSlot != -1)
            {
                slots[bestSlot] = attacker.gameObject;
            }
            return bestSlot;
        }

        /// <summary>
        /// Clear the contents of a slot.
        /// </summary>
        /// <param name="slot"></param>
        public void ClearSlot(int slot)
        {
            if (slot != -1)
            {
                slots[slot] = null;
            }
        }

        /// <summary>
        /// Draws the position of the slots in the scene view for visualisation.
        /// </summary>
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
