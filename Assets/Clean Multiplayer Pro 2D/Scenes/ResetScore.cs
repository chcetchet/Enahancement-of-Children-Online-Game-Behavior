using UnityEngine;

public class ResetScore : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.HasKey("TotalScore"))
        {
            PlayerPrefs.DeleteKey("TotalScore"); // Reset only if it exists
        }
    }

}
