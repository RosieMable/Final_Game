using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Portal : MonoBehaviour
    {
        public string sceneToLoad;
        const string hubScene = "BetaHub";
        public bool isDungeonPortal;
        public bool isHubPortal;

        private void Start()
        {
            if(isHubPortal)
                DungeonCardSystem.Instance.CalculateRewards();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<StateManager>())
            {
                if (isDungeonPortal)
                {
                    //load scene async
                    DungeonCardSystem.Instance.LoadSceneAsync(sceneToLoad);
                    DungeonCardSystem.Instance.asyncDungeonScene.allowSceneActivation = true;
                }
                else if (isHubPortal)
                {
                    DungeonCardSystem.Instance.AddCardRewardToInventory();
                    GameManager.instance.LoadScene(hubScene);
                }
                else
                {
                    GameManager.instance.LoadScene(sceneToLoad);
                }
            }
        }
    }

}
