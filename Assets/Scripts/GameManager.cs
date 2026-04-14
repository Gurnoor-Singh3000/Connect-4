using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject red, blue;

    [SerializeField]
    private Column[] allColumns;

    bool isPlayer, hasGameFinished, playAgainstAI;

    [SerializeField]
    TMP_Text turnMessage;

    const string RED_MESSAGE = "Red's Chance";
    const string BLUE_MESSAGE = "Blue's Chance";

    Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    Color BLUE_COLOR = new Color(53, 62, 236, 255) / 255;

    Board myBoard;


    private void Awake()
{
    isPlayer = true;
    hasGameFinished = false;
    turnMessage.text = RED_MESSAGE;
    turnMessage.color = RED_COLOR;
    myBoard = new Board();
    
    playAgainstAI = GameSettings.playAgainstAI; 
}

    public void GameStart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }


    private void Update()
{
    if (hasGameFinished) return;

    if (!isPlayer && playAgainstAI) return; 

    if (Input.GetMouseButtonDown(0))
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        
        if (!hit.collider) return;

        if (hit.collider.CompareTag("Press"))
        {
            Column clickedCol = hit.collider.gameObject.GetComponent<Column>();
            ExecuteMove(clickedCol);
        }
    }
}

public void ExecuteMove(Column selectedColumn)
{
    if (selectedColumn.targetlocation.y > 1.5f) return;

    Vector3 spawnPos = selectedColumn.spawnLocation;
    Vector3 targetPos = selectedColumn.targetlocation;
    GameObject circle = Instantiate(isPlayer ? red : blue);
    circle.transform.position = spawnPos;
    circle.GetComponent<Mover>().targetPostion = targetPos;

    selectedColumn.targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

    myBoard.UpdateBoard(selectedColumn.col - 1, isPlayer);
    if (myBoard.Result(isPlayer))
    {
        turnMessage.text = (isPlayer ? "Red" : "Blue") + " Wins!";
        hasGameFinished = true;
        return;
    }

    turnMessage.text = !isPlayer ? RED_MESSAGE : BLUE_MESSAGE;
    turnMessage.color = !isPlayer ? RED_COLOR : BLUE_COLOR;

    isPlayer = !isPlayer;

    if (!isPlayer && playAgainstAI && !hasGameFinished)
    {
        StartCoroutine(AITakeTurn());
    }
}

IEnumerator AITakeTurn()
{
    yield return new WaitForSeconds(1f);

    int chosenColIndex = -1;

    for (int i = 0; i < 7; i++)
    {
        if (allColumns[i].targetlocation.y > 1.5f) continue; 
        
        int logicalCol = allColumns[i].col - 1; 

        myBoard.UpdateBoard(logicalCol, false); 
        if (myBoard.Result(false)) 
        {
            chosenColIndex = i;
            myBoard.UndoMove(logicalCol); 
            break; 
        }
        myBoard.UndoMove(logicalCol); 
    }

    if (chosenColIndex == -1) 
    {
        for (int i = 0; i < 7; i++)
        {
            if (allColumns[i].targetlocation.y > 1.5f) continue;

            int logicalCol = allColumns[i].col - 1;

            myBoard.UpdateBoard(logicalCol, true); 
            if (myBoard.Result(true)) 
            {
                chosenColIndex = i; 
                myBoard.UndoMove(logicalCol);
                break;
            }
            myBoard.UndoMove(logicalCol);
        }
    }

    if (chosenColIndex == -1)
    {
        bool validRandomMove = false;
        while (!validRandomMove)
        {
            int random = Random.Range(0, 7);
            if (allColumns[random].targetlocation.y <= 1.5f)
            {
                chosenColIndex = random;
                validRandomMove = true;
            }
        }
    }

    ExecuteMove(allColumns[chosenColIndex]);
}

}