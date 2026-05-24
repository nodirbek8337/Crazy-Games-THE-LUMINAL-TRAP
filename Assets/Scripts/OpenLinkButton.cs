using UnityEngine;

public class OpenLinkButton : MonoBehaviour
{
    public void OpenTelegram()
    {
        Application.OpenURL("https://t.me/nqalslic_games"); 
    }

    public void OpenYouTube()
    {
        Application.OpenURL("https://www.youtube.com/@nqalslic_games");
    }
}
