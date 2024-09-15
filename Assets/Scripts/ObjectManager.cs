using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public Image background;
    
    [HideInInspector] public bool isMovable;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public ObjectData data;
    
    public void Swap()
    {
        if (isMovable)
        {
            gameManager.Move(data, this, true);
        }
    }

    public void UpdateData(ObjectData data, bool isNumbers)
    {
        numberText.text = data.number.ToString();
        background.sprite = data.sprite;
        background.color = data.number == 0 ? Color.black : new Color(255, 255, 255, isNumbers ? 0 : 255);
        this.data = data;
        
        if (data.number != 0)
        {
            if (isNumbers)
                numberText.text = data.number.ToString();
            else
                background.sprite = data.sprite;
                
            numberText.gameObject.SetActive(isNumbers);
        }
    }
}