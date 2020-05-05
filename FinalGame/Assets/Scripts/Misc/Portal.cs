using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Portal : MonoBehaviour
    {
        public string sceneToLoad;
        public bool isDungeonPortal;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<StateManager>())
            {
                if (isDungeonPortal)
                {
                    //load scene async
                    DungeonCardSystem.Instance.LoadSceneAsync(sceneToLoad);
                }
                else
                {
                    GameManager.instance.LoadScene(sceneToLoad);
                }
            }
        }
    }

}
