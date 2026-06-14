using UnityEngine;
using UnityEngine.UI;

public class VisualAssetBinder : MonoBehaviour
{
    // 按钮素材
    public Sprite buttonPrimarySprite;
    public Sprite buttonSecondarySprite;
    
    // 列车占位图像
    public Sprite trainPlaceholderSprite;
    
    // 底部按钮
    public Button scheduleButton;
    public Button staffButton;
    public Button maintenanceButton;
    public Button stationServiceButton;
    public Button externalAffairsButton;
    public Button endDayButton;
    
    // 列车对象
    public GameObject trainObject;
    
    // Tilemap 相关
    public GameObject tilemapObject;
    
    private void Start()
    {
        // 加载并绑定按钮素材
        LoadButtonSprites();
        BindButtonSprites();
        
        // 加载并绑定列车占位图像
        LoadTrainPlaceholder();
        BindTrainPlaceholder();
        
        // 确保 tilemap 处于激活状态
        if (tilemapObject != null)
        {
            tilemapObject.SetActive(true);
        }
    }
    
    private void LoadButtonSprites()
    {
        // 从 Resources 目录加载按钮素材
        buttonPrimarySprite = Resources.Load<Sprite>("Art/UI/Prototype/button_primary");
        if (buttonPrimarySprite == null)
        {
            Debug.LogWarning("Failed to load button_primary sprite from Resources/Art/UI/Prototype/button_primary");
        }
        
        buttonSecondarySprite = Resources.Load<Sprite>("Art/UI/Prototype/button_secondary");
        if (buttonSecondarySprite == null)
        {
            Debug.LogWarning("Failed to load button_secondary sprite from Resources/Art/UI/Prototype/button_secondary");
        }
    }
    
    private void LoadTrainPlaceholder()
    {
        // 从 Resources 目录加载列车占位图像
        trainPlaceholderSprite = Resources.Load<Sprite>("Art/Vehicles/Prototype/train_placeholder");
        if (trainPlaceholderSprite == null)
        {
            Debug.LogWarning("Failed to load train_placeholder sprite from Resources/Art/Vehicles/Prototype/train_placeholder");
        }
    }
    
    private void BindButtonSprites()
    {
        // 绑定主要按钮
        if (buttonPrimarySprite != null)
        {
            BindButtonSprite(scheduleButton, buttonPrimarySprite);
            BindButtonSprite(staffButton, buttonPrimarySprite);
            BindButtonSprite(maintenanceButton, buttonPrimarySprite);
            BindButtonSprite(stationServiceButton, buttonPrimarySprite);
            BindButtonSprite(externalAffairsButton, buttonPrimarySprite);
        }
        
        // 绑定结束当天按钮（使用次要按钮样式）
        if (buttonSecondarySprite != null)
        {
            BindButtonSprite(endDayButton, buttonSecondarySprite);
        }
    }
    
    private void BindButtonSprite(Button button, Sprite sprite)
    {
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = sprite;
                buttonImage.type = Image.Type.Sliced;
            }
        }
    }
    
    private void BindTrainPlaceholder()
    {
        if (trainPlaceholderSprite != null && trainObject != null)
        {
            SpriteRenderer spriteRenderer = trainObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = trainPlaceholderSprite;
            }
            else
            {
                // 如果没有 SpriteRenderer，添加一个
                spriteRenderer = trainObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = trainPlaceholderSprite;
            }
        }
    }
}