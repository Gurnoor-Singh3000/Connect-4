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
    
    // Read the choice from the Main Menu!
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
        // Make sure "MainMenu" perfectly matches the name of your menu scene!
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }


    private void Update()
{
    // 1. If the game is over, do nothing.
    if (hasGameFinished) return;

    // 2. If it's the AI's turn, IGNORE mouse clicks so the player can't cheat!
    if (!isPlayer && playAgainstAI) return; 

    // 3. Player's Turn: Wait for a mouse click
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

// 4. We moved all the dropping logic into this new method!
public void ExecuteMove(Column selectedColumn)
{
    // Check out of Bounds (Make sure to use your updated 2.4f number here if you kept it!)
    if (selectedColumn.targetlocation.y > 1.5f) return;

    // Spawn the GameObject
    Vector3 spawnPos = selectedColumn.spawnLocation;
    Vector3 targetPos = selectedColumn.targetlocation;
    GameObject circle = Instantiate(isPlayer ? red : blue);
    circle.transform.position = spawnPos;
    circle.GetComponent<Mover>().targetPostion = targetPos;

    // Increase the targetLocationHeight
    selectedColumn.targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

    // UpdateBoard
    myBoard.UpdateBoard(selectedColumn.col - 1, isPlayer);
    if (myBoard.Result(isPlayer))
    {
        turnMessage.text = (isPlayer ? "Red" : "Blue") + " Wins!";
        hasGameFinished = true;
        return;
    }

    // TurnMessage
    turnMessage.text = !isPlayer ? RED_MESSAGE : BLUE_MESSAGE;
    turnMessage.color = !isPlayer ? RED_COLOR : BLUE_COLOR;

    // Change PlayerTurn
    isPlayer = !isPlayer;

    // 5. If it is now the AI's turn, tell it to think!
    if (!isPlayer && playAgainstAI && !hasGameFinished)
    {
        StartCoroutine(AITakeTurn());
    }
}

IEnumerator AITakeTurn()
{
    // Wait for 1 second so the AI feels like it's "thinking"
    yield return new WaitForSeconds(1f);

    int chosenColIndex = -1;

    // --- RULE 1: CAN THE AI WIN? (Offense) ---
    for (int i = 0; i < 7; i++)
    {
        if (allColumns[i].targetlocation.y > 1.5f) continue; 
        
        // Grab the EXACT logical column mapped to this physical column!
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

    // --- RULE 2: CAN THE PLAYER WIN? (Defense / Blocking) ---
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

    // --- RULE 3: RANDOM MOVE (If no one is about to win) ---
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

    // Physically execute the move!
    ExecuteMove(allColumns[chosenColIndex]);
}

}