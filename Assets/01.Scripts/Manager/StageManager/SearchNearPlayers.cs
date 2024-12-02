using UnityEngine;

public class SearchNearPlayers : MonoBehaviour
{
    public StageManager stageManager;

    int needPlayerCounts = 0;
    int currentPlayerCount = 0;
    public void SetPlayerCount(int playerCount) => needPlayerCounts = playerCount;

    private void OnTriggerEnter(Collider other)
    {
        currentPlayerCount++;

        if (currentPlayerCount == needPlayerCounts)
        {
            stageManager.isAroundPlayers = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        currentPlayerCount--;

        stageManager.isAroundPlayers = false;
    }
}
