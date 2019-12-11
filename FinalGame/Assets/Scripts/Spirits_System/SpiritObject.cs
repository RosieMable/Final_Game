using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsThisDarkSouls;

public class SpiritObject : MonoBehaviour
{
    public Spirit_ScriptableObj Spirit;

   
    void AddSpiritToInventory()
    {
        //Add the spirit to the inventory, so we can choose the one we want to equip
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.GetComponent<StateManager>())
        {
            AddSpiritToInventory();

        }
    }

}
