
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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


