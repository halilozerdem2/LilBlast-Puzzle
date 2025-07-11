using System.Collections.Generic;
using UnityEngine;

public class BlockShapeShifter : MonoBehaviour
{
    [SerializeField] private Sprite[] IconTypes = new Sprite[4];
    private SpriteRenderer currentIcon;
    private Block block;

    private void Awake()
    {
        block = GetComponent<Block>();
        currentIcon = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameManager.OnGridReady += DetermineIcon;
    }

    private void OnDisable()
    {
        GameManager.OnGridReady -= DetermineIcon;
    }


    public void DetermineIcon()
    {
        HashSet<Block> group = block.DetermineGroup();
        if (group.Count <=2)
        {
            currentIcon.sprite = IconTypes[0];
        }
        else if (group.Count == 3)
        {
            currentIcon.sprite = IconTypes[1];
        }
        else if (group.Count == 4)
        {
            currentIcon.sprite = IconTypes[2];
        }
        else if(group.Count == 5)
        {
            currentIcon.sprite = IconTypes[3];
        }
        else
        {
            currentIcon.sprite = IconTypes[4];
        }
    }
}
