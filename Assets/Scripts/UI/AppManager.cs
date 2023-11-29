using UnityEngine;
using Yarn.Unity;

public class AppManager
{

    [YarnCommand("quitGame")]
    public static void Quit()
    {
        Application.Quit();
    }

    // Set quality to "full" or "half"
    [YarnCommand("setQuality")]
    public static void SetQuality(string quality)
    {

            int full = 0;
            int half = 1;

            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals("High Fidelity"))
                {
                    full = i;
                }
                else if (names[i].Equals("High 0.5x"))
                {
                    half = i;
                }
            }

        switch (quality.ToLower())
        {
            case "full":
                QualitySettings.SetQualityLevel(full);
                break;
            case "half":
                QualitySettings.SetQualityLevel(half);
                break;
            default:
                Debug.LogErrorFormat("Couldn't find quality preset {0}, check if the Yarn script calls the right parameter?", quality);
                break;
        }

    }
}
