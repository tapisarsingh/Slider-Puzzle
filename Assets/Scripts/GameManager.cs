using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject prefab;
    public Transform parent;
    public GameObject imagesSwitch;
    public GameObject numbersSwitch;
    public TextMeshProUGUI movesText;
    public GameObject finish;

    public List<ObjectData> data;

    private bool isNumbers = true;
    private int moves;
    private int pivot;
    private ObjectManager pivotObject;
    private List<ObjectManager> spawnedObjects = new List<ObjectManager>();
    private List<ObjectManager> moveableObjects = new List<ObjectManager>();

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        SpawnObjects();
        moves = 0;
        movesText.text = "Moves: 0";
        finish.SetActive(false);
        CheckMovableObjects();
        RandomizeMovement();
        EnsureSolvable();  // Ensure the generated puzzle is solvable
    }

    public void RandomizeMovement()
    {
        for (int i = 0; i < 16000; i++)
        {
            var target = moveableObjects[Random.Range(0, moveableObjects.Count)];
            Move(target.data, target, false);
        }
    }

    public void Move(ObjectData data, ObjectManager manager, bool countMovement)
    {
        manager.UpdateData(pivotObject.data, isNumbers);
        pivotObject.UpdateData(data, isNumbers);
        pivotObject = manager;

        if (countMovement)
        {
            moves++;
            movesText.text = "Moves: " + moves;
        }

        CheckMovableObjects();
    }

    public void CheckMovableObjects()
    {
        moveableObjects = new List<ObjectManager>();
        pivot = pivotObject.transform.GetSiblingIndex();

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            spawnedObjects[i].isMovable = i == pivot - 1 || i == pivot - 4 || i == pivot + 1 || i == pivot + 4;

            if (spawnedObjects[i].isMovable)
            {
                moveableObjects.Add(spawnedObjects[i]);
            }
        }

        CheckGameState();
    }

    public void CheckGameState()
    {
        int fixedNumbers = 0;
        for (int i = 0; i < spawnedObjects.Count - 1; i++)
        {
            if (spawnedObjects[i].data.number == i + 1)
            {
                fixedNumbers++;
            }
        }

        if (fixedNumbers == spawnedObjects.Count - 1)
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                spawnedObjects[i].isMovable = false;
            }

            finish.GetComponent<TextMeshProUGUI>().text = $"Good Job! You finished in {moves} moves!";
            finish.SetActive(true);
        }
    }

    public void SwitchType()
    {
        isNumbers = !isNumbers;
        imagesSwitch.SetActive(!isNumbers);
        numbersSwitch.SetActive(isNumbers);

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].data.number != 0)
                spawnedObjects[i].UpdateData(spawnedObjects[i].data, isNumbers);
        }
    }

    public void SpawnObjects()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            DestroyImmediate(spawnedObjects[i].gameObject);
        }
        spawnedObjects = new List<ObjectManager>();

        for (int i = 0; i < data.Count; i++)
        {
            ObjectManager obj = Instantiate<GameObject>(prefab, parent).GetComponent<ObjectManager>();
            obj.gameManager = this;
            obj.UpdateData(data[i], isNumbers);

            if (i == 0)
            {
                pivotObject = obj;
                pivot = i;
            }

            spawnedObjects.Add(obj);
        }
    }

    // New method to ensure the puzzle is always solvable
    public void EnsureSolvable()
    {
        int inversionCount = CountInversions();
        int blankRowFromBottom = GetBlankRowFromBottom();

        // If the puzzle is not solvable, make it solvable by swapping two adjacent tiles
        if (!IsSolvable(inversionCount, blankRowFromBottom))
        {
            // Swap the first two non-zero tiles to toggle solvability
            for (int i = 0; i < spawnedObjects.Count - 1; i++)
            {
                if (spawnedObjects[i].data.number != 0 && spawnedObjects[i + 1].data.number != 0)
                {
                    ObjectData temp = spawnedObjects[i].data;
                    spawnedObjects[i].UpdateData(spawnedObjects[i + 1].data, isNumbers);
                    spawnedObjects[i + 1].UpdateData(temp, isNumbers);
                    break;
                }
            }
        }
    }

    // Helper to count the number of inversions
    public int CountInversions()
    {
        int inversions = 0;
        List<int> tiles = new List<int>();

        // Flatten the puzzle, skipping the blank tile (0)
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i].data.number != 0)
            {
                tiles.Add(spawnedObjects[i].data.number);
            }
        }

        // Count inversions
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                if (tiles[i] > tiles[j])
                {
                    inversions++;
                }
            }
        }

        return inversions;
    }

    // Helper to get the row number of the blank space (pivot) from the bottom
    public int GetBlankRowFromBottom()
    {
        int blankIndex = pivotObject.transform.GetSiblingIndex();
        return 4 - (blankIndex / 4);  // 4x4 grid: calculate row from the bottom
    }

    // Check if the puzzle is solvable based on inversion count and blank row position
    public bool IsSolvable(int inversionCount, int blankRowFromBottom)
    {
        // Solvability rule for 4x4 grid
        return (inversionCount % 2 == 0 && blankRowFromBottom % 2 == 1) ||
               (inversionCount % 2 == 1 && blankRowFromBottom % 2 == 0);
    }
}
