using GameManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject visualPanel;
    
        [Header("Input")]
        [SerializeField] private InputActionReference pauseActionRef;
        [SerializeField] private InputActionReference cancelActionRef;

        [Header("UI Root")]
        //Get the components where Gameobjects are
        [SerializeField] Transform uiComponentRoot;
        [SerializeField] GameObject audioManager;
        [SerializeField] GameObject closeButton;

        private InputAction pauseAction;
        private InputAction cancelAction;

        private bool haveClickedOnAudio = false;


        private void Awake()
        {
            if (pauseActionRef != null)
            {
                pauseAction = pauseActionRef.action;
            }
            else
            {
                Debug.LogError("Pause Action Reference not assigned!");
            }
        
            if (cancelActionRef != null)
            {
                cancelAction = cancelActionRef.action;
            }
            else
            {
                Debug.LogError("Cancel Action Reference not assigned!");
            }
        }

        private void OnEnable()
        {

            if (haveClickedOnAudio)
            {
                RenablePauseComponents();
            }

            if (pauseAction != null)
            {
                pauseAction.performed += OnPauseTriggered;
                pauseAction.Enable();
            }
        
            if (cancelAction != null)
            {
                cancelAction.performed += OnCancelTriggered;
                cancelAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPauseTriggered;
            }
        
            if (cancelAction != null)
            {
                cancelAction.performed -= OnCancelTriggered;
            }
        }

        private void Start()
        {
            if (visualPanel != null)
            {
                visualPanel.SetActive(false);
            }
        
            Time.timeScale = 1f;
        }

        private void OnPauseTriggered(InputAction.CallbackContext context)
        {
            if (haveClickedOnAudio)
            {
                RenablePauseComponents();
            }

            if (SceneManager.GetActiveScene().buildIndex == 0) return;
            if (visualPanel == null) return;

            if (visualPanel.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void OnCancelTriggered(InputAction.CallbackContext context)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) return;
            if (visualPanel != null && visualPanel.activeSelf)
            {
                Resume();
            }
        }

        public void Pause()
        {
            if (visualPanel != null)
            {
                visualPanel.SetActive(true);
                Time.timeScale = 0f;
            
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Resume()
        {
            if (visualPanel != null)
            {
                visualPanel.SetActive(false);
                Time.timeScale = 1f;
            
                // Clear UI selection when closing
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync(0);
        }

        public void OnAudioClicked()
        {
            foreach (Transform child in uiComponentRoot)
            {
               
              child.gameObject.SetActive(child.gameObject == uiComponentRoot);

            }

            audioManager.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);

            haveClickedOnAudio = true;

        }

        public void RenablePauseComponents()
        {
            foreach (Transform child in uiComponentRoot)
            {
                child.gameObject.SetActive(child.gameObject != uiComponentRoot);
            }

            audioManager.gameObject.SetActive(false);

            haveClickedOnAudio = false;
        }


        public void OnAudioMasterSlider(float value)
        {
            //Debug.Log("Master Slider " +  value);
            SoundManager.instance.SetMasterVolume(value);
        }

        public void OnAudioSfxSlider(float value)
        {
            //Debug.Log("Audio Slider " + value);
            SoundManager.instance.SetSfxVolume(value);

        }

        public void OnAudioMusicSlider(float value)
        {
            //Debug.Log("Music Slider " + value);
            SoundManager.instance.SetMusicVolume(value);
        }

    }
}