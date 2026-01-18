using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ForceSelection : MonoBehaviour
    {
        [SerializeField] private Selectable firstButton;
    
        private void OnEnable()
        {
            StartCoroutine(SelectFirstButtonNextFrame());
        }

        private System.Collections.IEnumerator SelectFirstButtonNextFrame()
        {
            yield return null;
    
            if (firstButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }
        }
    }
}
