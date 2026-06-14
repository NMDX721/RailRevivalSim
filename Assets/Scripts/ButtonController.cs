using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public UIManager uiManager;
    public string buttonType;
    
    private void Start()
    {
        // 获取按钮组件并添加点击事件
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    private void OnButtonClick()
    {
        if (uiManager == null)
        {
            Debug.LogError("UIManager not assigned to ButtonController");
            return;
        }
        
        // 根据按钮类型调用相应的方法
        switch (buttonType)
        {
            case "schedule":
                uiManager.OnScheduleButtonClick();
                break;
            case "staff":
                uiManager.OnStaffButtonClick();
                break;
            case "maintenance":
                uiManager.OnMaintenanceButtonClick();
                break;
            case "stationService":
                uiManager.OnStationServiceButtonClick();
                break;
            case "externalAffairs":
                uiManager.OnExternalAffairsButtonClick();
                break;
            case "endDay":
                uiManager.OnEndDayButtonClick();
                break;
            default:
                Debug.LogWarning("Unknown button type: " + buttonType);
                break;
        }
    }
}
