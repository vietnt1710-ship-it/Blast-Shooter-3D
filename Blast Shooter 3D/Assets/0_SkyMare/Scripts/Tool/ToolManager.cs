
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class ToolManager : Singleton<ToolManager>
{
    [Serializable]
    public class TypeAndID
    {
        public TileType type;
        public int tileID;
    }

    [Serializable]
    public class TypeAndIDAndButton
    {
        public TypeAndID type;
        public Button button;

        public string GetButtonName()
        {
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            return text.text;
        }
        public Color GetButtonColor()
        {
            return button.image.color;
        }
    }

    public List<TypeAndIDAndButton> typeButtonList;
    
    protected void Awake()
    {
        for(int i = 0; i < typeButtonList.Count; i++)
        {
            int index = i;
            typeButtonList[index].button.onClick.AddListener( ()=> { ChangeType(typeButtonList[index]); });
        }
    }
    public TMP_Text typeName;
    public TypeAndIDAndButton selectTypeAndIDandButton;
    public Color currentColor;
    public int colorID;
    public ColorID colorWithID;
    public VoxelLayerPainter painter;
    public Tool_SlotManager slotManager;
    public List<int> cubeVerUniqueColors;
    public LevelDataConfigPicker picker;
    public void UpdateCount()
    {
        cubeVerUniqueColors = new List<int>(painter.cubeVerUniqueColors);

        Dictionary<int, int> colorCount = new Dictionary<int, int>();

        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
               Tool_Slot slot = slotManager.gridTile[row, col];

               if (slot.id != 6)
               {
                    int colorID = slot.colorID;

                    if (colorCount.ContainsKey(colorID))
                    {
                        colorCount[colorID]+= slot.bulletCount;
                    }
                    else
                    {
                        colorCount[colorID] = slot.bulletCount;
                    }
                }
                else
                {
                    for (int k = 0; k < slot.dataIngaras.Count; k++)
                    {
                        string blc;
                        string cli;
                        GridParse.OnSplitBeAf(slot.dataIngaras[k], out blc, out cli);

                        int colorID = int.Parse(cli);
                        int bulletCount = int.Parse(blc);
                        if (colorCount.ContainsKey(colorID))
                        {
                            colorCount[colorID] += bulletCount;
                        }
                        else
                        {
                            colorCount[colorID] = bulletCount;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < painter.uniqueColors.Count; i++)
        {
            int ID = colorWithID.ColorWithID3(painter.uniqueColors[i]).ID;
            if (colorCount.ContainsKey(ID))
            {
                cubeVerUniqueColors[i] -= colorCount[ID];
                painter._colorButtons[i].GetComponentInChildren<TMP_Text>().text = cubeVerUniqueColors[i].ToString();
            }
            //else
            //{
            //    bottle.UpdateValue(0);
            //}
        }
    }

    public void ChangeColor(Color color)
    {
        currentColor = color;
        colorID = colorWithID.ColorWithID3(currentColor).ID;
    }
    public Color color => colorWithID.ColorWithID2(colorID).color;
    public void ChangeType(TypeAndIDAndButton typeAndIDAndButton)
    {
        typeName.text = typeAndIDAndButton.GetButtonName();
        this.selectTypeAndIDandButton = typeAndIDAndButton;
    }
    public List<Tool_SlotTileInGara> tool_SlotTileInGaras;
}


