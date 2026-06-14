using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private bool hasShownTutorial = false;
    private int tutorialStep = 0;

    public GameObject tutorialPanel;
    public Text tutorialText;
    public Button nextButton;
    public Button skipButton;

    private List<string> tutorialMessages = new List<string>
    {
        "欢迎来到铁路复兴模拟！这是一个慢节奏的铁路经营模拟游戏。\n\n今天你需要：\n1. 安排发车计划\n2. 分配人员\n3. 维护机车\n4. 提供站务服务\n5. 处理对外事务\n\n完成后点击【结束当天】推进到下一天。",
        "底部有六个核心按钮：\n\n【发车安排】：调整今日的发车计划\n【人员安排】：分配人员到不同岗位\n【维护整备】：维护和检修机车\n【站务服务】：提供乘客服务\n【对外事务】：处理与政府和外界的关系\n【结束当天】：结算当天并推进到下一天",
        "操作方式：\n• 第一次点击底部按钮打开面板\n• 再点一次按钮切换方案\n• 选好后去【结束当天】结算\n\n现在你可以开始尝试经营你的铁路支线了！"
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextTutorialStep);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }

        ShowTutorial();
    }

    public void ShowTutorial()
    {
        if (!hasShownTutorial && tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            // 添加淡入效果
            CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                StartCoroutine(FadeIn(canvasGroup, 0.3f));
            }
            tutorialStep = 0;
            UpdateTutorialText();
        }
    }

    private System.Collections.IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        float startTime = Time.time;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 1f;

        while (Time.time < startTime + duration)
        {
            float progress = (Time.time - startTime) / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void UpdateTutorialText()
    {
        if (tutorialText != null && tutorialStep < tutorialMessages.Count)
        {
            tutorialText.text = tutorialMessages[tutorialStep];
        }
    }

    public void NextTutorialStep()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        tutorialStep++;
        if (tutorialStep < tutorialMessages.Count)
        {
            UpdateTutorialText();
        }
        else
        {
            EndTutorial();
        }
    }

    public void SkipTutorial()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        EndTutorial();
    }

    private void EndTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        hasShownTutorial = true;
    }

    public bool HasShownTutorial()
    {
        return hasShownTutorial;
    }

    public void ResetTutorial()
    {
        hasShownTutorial = false;
    }
}