using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Reflection;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;
    [SerializeField] private GameObject bossNameObject;
    
    private Component bossComponent;
    private Slider slider;
    private TMP_Text bossNameText; 
    private System.Reflection.MethodInfo getHealthMethod;
    private System.Reflection.MethodInfo getNameMethod;
    
    void Start()
    {
        // Găsește slider-ul
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogError("No Slider component found!");
            return;
        }
        
        // Verifică dacă GameObject-ul text a fost asignat prin Inspector
        if (bossNameObject == null)
        {
            Debug.LogError("Boss Name GameObject not assigned in Inspector!");
            gameObject.SetActive(false);
            return;
        }
        
        // Obține componenta TMP_Text din GameObject
        bossNameText = bossNameObject.GetComponent<TMP_Text>();
        if (bossNameText == null)
        {
            Debug.LogError($"Assigned GameObject '{bossNameObject.name}' has no TMP_Text component!");
            gameObject.SetActive(false);
            return;
        }
        
        // Găsește bossul
        FindBoss();
        
        if (bossComponent == null)
        {
            Debug.LogWarning("No boss found with GetHealthPercent method!");
            gameObject.SetActive(false);
            return;
        }
        
        // Actualizează numele și health-ul
        UpdateBossName();
        slider.value = GetBossHealth();
    }
    
    void Update()
    {
        if (bossComponent == null || slider == null) return;
        
        slider.value = GetBossHealth();
        
        if (slider.value <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    private void FindBoss()
    {
        // Dacă ai assignat bossul din inspector
        if (bossObject != null)
        {
            FindBossComponent(bossObject);
        }
     
        // Dacă nu ai assignat, caută în scena
        if (bossComponent == null)
        {
            // Folosește metoda nouă non-deprecated
            MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (MonoBehaviour behaviour in allBehaviours)
            {
                MethodInfo healthMethod = behaviour.GetType().GetMethod("GetHealthPercent");
                if (healthMethod != null && healthMethod.ReturnType == typeof(float))
                {
                    bossComponent = behaviour;
                    getHealthMethod = healthMethod;
                    getNameMethod = behaviour.GetType().GetMethod("GetBossName");
                    
                    Debug.Log($"Found boss: {behaviour.GetType().Name}");
                    break;
                }
            }
        }
    }
    
    private float GetBossHealth()
    {
        if (getHealthMethod != null)
        {
            try
            {
                return (float)getHealthMethod.Invoke(bossComponent, null);
            }
            catch
            {
                return 0f;
            }
        }
        return 0f;
    }
    
    private string GetBossName()
    {
        if (getNameMethod != null)
        {
            try
            {
                return (string)getNameMethod.Invoke(bossComponent, null);
            }
            catch
            {
                return "Unknown Boss";
            }
        }
        return "Unknown Boss";
    }
    
    private void UpdateBossName()
    {
        if (bossNameText != null)
        {
            string name = GetBossName();
            bossNameText.text = name;
        }
    }
    
    private void FindBossComponent(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        foreach (Component component in components)
        {
            MethodInfo healthMethod = component.GetType().GetMethod("GetHealthPercent");
            if (healthMethod != null && healthMethod.ReturnType == typeof(float))
            {
                bossComponent = component;
                getHealthMethod = healthMethod;
                getNameMethod = component.GetType().GetMethod("GetBossName");
                
                Debug.Log($"Found boss component: {component.GetType().Name}");
                break;
            }
        }
    }
    
    public void SetBoss(GameObject boss)
    {
        if (boss != null)
        {
            FindBossComponent(boss);
            if (bossComponent != null)
            {
                UpdateBossName();
                if (slider != null)
                    slider.value = GetBossHealth();
            }
        }
    }
    
    public void SetBossNameObject(GameObject nameObject)
    {
        if (nameObject != null)
        {
            bossNameObject = nameObject;
            bossNameText = nameObject.GetComponent<TMP_Text>();
            
            if (bossNameText == null)
            {
                Debug.LogError($"Set GameObject '{nameObject.name}' has no TMP_Text component!");
            }
            else
            {
                UpdateBossName();
            }
        }
    }
    
    public void SetBossComponent(Component bossComp)
    {
        if (bossComp != null)
        {
            bossComponent = bossComp;
            getHealthMethod = bossComp.GetType().GetMethod("GetHealthPercent");
            getNameMethod = bossComp.GetType().GetMethod("GetBossName");
            
            if (getHealthMethod != null)
            {
                UpdateBossName();
                if (slider != null)
                    slider.value = GetBossHealth();
            }
        }
    }
}