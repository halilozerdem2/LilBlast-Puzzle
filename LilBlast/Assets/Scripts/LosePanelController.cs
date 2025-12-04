using UnityEngine;

public class LosePanelController : MonoBehaviour
{
    public void OpenAdsPanel()
    {
        //Open Ads Panel logic
        ContinueGame();

    }

    public void UseExtraLifeAndContinue()
    {
        //Use Extra llife logic
        ContinueGame();
    }
    public void GoToMainMenu()
    {


    }
    private void ContinueGame(int aAmount = 5)
    {
        if(GameManager.Instance._state == GameManager.GameState.Lose)
            GameOverHandler.Instance.moves += aAmount;
        GameManager.Instance.ResumeGame();
        
    }
    public void TryAgain()
    {
        
    }
    
}
