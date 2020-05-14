using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace ZaldensGambit
{
    public class Portal : MonoBehaviour
    {
        const string hubScene = "BetaHub";
        const string defaultSpawnName = "PlayerSpawnPoint";

        [SerializeField] private bool isHubPortal;

        AsyncOperation asyncLoad;

        private bool m_isAwaitingLoad;
        private GameObject m_PlayerToMove;
        private string m_spawnPointName;
        private string m_sceneToLoad;

        public bool m_additiveLoad = false;

        private void Awake()
        {
            if(isHubPortal)
            { 
                DungeonCardSystem.Instance.CalculateRewards();
                LoadAsync();
            }
        }

        public void LoadAsync(string sceneName = null, string spawnPointName = defaultSpawnName)
        {
            if (asyncLoad != null)
            {
                Debug.LogError("Already loading scene.");
                return;
            }
            
            if(isHubPortal)
            {
                m_sceneToLoad = hubScene;
            }
            else if(!string.IsNullOrEmpty(sceneName))
            {
                m_sceneToLoad = sceneName;
            }
            else
            {
                Debug.LogError("No scene name passed.");
                return;
            }

            m_spawnPointName = spawnPointName;

            asyncLoad = SceneManager.LoadSceneAsync(m_sceneToLoad, m_additiveLoad ? LoadSceneMode.Additive : LoadSceneMode.Single);
            asyncLoad.allowSceneActivation = false;
            asyncLoad.completed += OnLoaded;
        }

        private void OnLoaded(AsyncOperation op)
        {
            if(m_isAwaitingLoad)
            {
                m_isAwaitingLoad = false;
                asyncLoad = null;
                TeleportToNextScene();
            }
        }

        private void TeleportToNextScene()
        {
            if(m_additiveLoad)
            {
                var spawnPoint = SceneManager.GetSceneByName(m_sceneToLoad).GetRootGameObjects().Where(x => x.name == m_spawnPointName).First();
                m_PlayerToMove.transform.position = spawnPoint.transform.position;
            }
        }

        public void ActivatePortal()
        {
            this.enabled = true;
            for(int i = 0; i < this.transform.childCount; ++i)
            {
                this.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        public void DeactivatePortal()
        {
            this.enabled = false;
            for (int i = 0; i < this.transform.childCount; ++i)
            {
                this.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (this.enabled && other.gameObject.GetComponent<StateManager>())
            {
                m_PlayerToMove = other.gameObject;
                if(asyncLoad.isDone)
                {
                    TeleportToNextScene();
                }
                else
                {
                    if (!asyncLoad.allowSceneActivation)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                    m_isAwaitingLoad = true;
                }
                
            }
        }
    }

}
