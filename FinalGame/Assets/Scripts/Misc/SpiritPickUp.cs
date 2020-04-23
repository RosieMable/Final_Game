using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class SpiritPickUp : MonoBehaviour
    {
        [SerializeField] private BaseSpirit spiritPickUp;
        [SerializeField] private float rotationSpeed = 180f;
        private bool pickedUp;

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
                Destroy(gameObject);
            }
        }
    }
}
