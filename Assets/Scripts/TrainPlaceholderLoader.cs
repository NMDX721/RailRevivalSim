using UnityEngine;

public class TrainPlaceholderLoader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        // 获取 SpriteRenderer 组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            // 从 Resources 目录加载列车占位图像
            Sprite trainSprite = Resources.Load<Sprite>("Art/Vehicles/Prototype/train_placeholder");
            if (trainSprite != null)
            {
                spriteRenderer.sprite = trainSprite;
            }
            else
            {
                Debug.LogWarning("Failed to load train_placeholder sprite from Resources/Art/Vehicles/Prototype/train_placeholder");
            }
        }
    }
}