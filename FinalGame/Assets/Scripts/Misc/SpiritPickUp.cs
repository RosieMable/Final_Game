using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class SpiritPickUp : MonoBehaviour
    {
        public BaseSpirit spiritPickUp;
        [SerializeField] private float rotationSpeed = 180f;
        private bool pickedUp;
        [SerializeField] private bool respawnAfterDelay;

        void Update()
        {
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<StateManager>() && !pickedUp)
            {
                pickedUp = true;
                print("Pickup");

                CardInventory.instance.AddSpiritCardToInventory(spiritPickUp);
                other.GetComponent<SpiritSystem>().spiritEquipped = spiritPickUp;
                other.GetComponent<SpiritSystem>().OnEquipSpirit(spiritPickUp);
                GetComponent<Collider>().enabled = false;
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Light>().enabled = false;

                if (respawnAfterDelay)
                {
                    StartCoroutine(RespawnAfterDelay(5));
                }
            }
        }

        IEnumerator RespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GetComponent<Collider>().enabled = true;
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<Light>().enabled = true;
            pickedUp = false;
        }
    }
}
