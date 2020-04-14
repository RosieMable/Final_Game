using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Portal : MonoBehaviour
    {


        public string sceneToLoad;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<StateManager>())
            {
                //load scene async
                DungeonCardSystem.Instance.LoadSceneAsync(sceneToLoad);
            }
        }
    }

}
