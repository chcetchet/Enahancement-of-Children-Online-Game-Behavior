using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ActivityReminder : MonoBehaviour
{
    public GameObject popupPanel;
    public TextMeshProUGUI reminderText;
    public string[] reminders; // Fill with different reminders in Inspector
    public float reminderInterval = 300f; // 5 minutes

    private float timer;
    private int currentReminderIndex = 0;

    private static ActivityReminder instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        timer = reminderInterval;
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void Update()
    {
        timer -= Time.unscaledDeltaTime;

        if (timer <= 0f)
        {
            ShowReminder();
            timer = reminderInterval;
        }
    }

    void ShowReminder()
    {
        if (popupPanel != null && reminderText != null && reminders.Length > 0)
        {
            reminderText.text = reminders[currentReminderIndex];
            currentReminderIndex = (currentReminderIndex + 1) % reminders.Length;
            popupPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseReminder()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nothing to do here unless you want to refresh UI
    }
}
