using UnityEngine;


public class CanvasManager : MonoBehaviour
{
    public void LoadPlayScene()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
