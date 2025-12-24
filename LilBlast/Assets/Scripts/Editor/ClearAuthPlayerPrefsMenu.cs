#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Provides a quick editor menu option to wipe the saved auth player prefs so Unity can fetch a fresh session.
/// </summary>
public static class ClearAuthPlayerPrefsMenu
{
    [MenuItem("Tools/Auth/Clear Auth PlayerPrefs")] 
    public static void ClearAuthPrefs()
    {
        // Ensure progress, inventory, and stats are wiped before clearing auth IDs.
        LevelManager.ResetProgress();

        PlayerPrefs.DeleteKey("LilGames.AuthToken");
        PlayerPrefs.DeleteKey("LilGames.UserId");
        PlayerPrefs.DeleteKey("LilGames.AuthProvider");
        PlayerPrefs.DeleteKey("LilGames.Username");

        PlayerPrefs.Save();
        Debug.Log("[Tools/Auth] Cleared auth prefs plus local progress and inventory.");
    }
}
#endif
