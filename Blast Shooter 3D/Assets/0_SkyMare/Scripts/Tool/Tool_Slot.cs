using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ToolManager;

public class Tool_Slot : MonoBehaviour
{
    public int row,col;
    public KeyboardNumberInput input;
    public MeshRenderer main;
    public TMP_Text mainText;
    public SpriteRenderer type;
    public TMP_Text typeText;
  
    bool isShift = false;

    public string outPut = "";
    [Header("Gara only using")]
    public List<string> dataIngaras = new List<string>();

    bool isSelect = false;
    public void Update()
    {
        if (!isSelect) return;
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShift = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShift = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmData();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            InitSlot();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            InitPipe();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Clear();
        }

        if (id == 6)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangePipeDirection(1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangePipeDirection(2);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangePipeDirection(3);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangePipeDirection(4);
            }
        }
       
    }
    public void ChangePipeDirection(int direction)
    {
        typeText.text = direction.ToString();   
    }
    TypeAndIDAndButton select;
    int id = 0;
    int colorID;

    public void InitPipe()
    {
        if (id != 6) return;
        string data = $"{20}_{ToolManager.I.colorID}";
    }
    bool isOpen = false;
    public void OpenGara()
    {
        for (int i = 0; i < dataIngaras.Count; i++)
        {
            int index = i;
            ToolManager.I.tool_SlotTileInGaras[index].Init(this, index);
        }
    }
    public void CloseGara()
    {
        for (int i = 0; i < ToolManager.I.tool_SlotTileInGaras.Count; i++)
        {
            int index = i;
            ToolManager.I.tool_SlotTileInGaras[index].Close();
        }
        dataIngaras.RemoveAll(x => x == "0");
    }
    public void InitSlot()
    {
        main.gameObject.SetActive(true);
        type.gameObject.SetActive(true );

       select = ToolManager.I.selectTypeAndIDandButton;
        id = select.type.tileID;
        colorID = ToolManager.I.colorID;
        if (id == 8 || id == 81)
        {
            main.gameObject.SetActive(false);
        }
        else if(id == 6)
        {
            main.material.color = Color.white;
            mainText.text = "Gara";
        }
        else 
        {
            main.material.color = ToolManager.I.color;
            mainText.text = "0";
        }

        type.color = select.GetButtonColor();

        switch (id)
        {
            case 1:
                typeText.gameObject.SetActive(false);
                break;
            case 2:
                typeText.gameObject.SetActive(false);
                break;
            case 3:
                typeText.gameObject.SetActive(true);
                break;
            case 4:
                typeText.gameObject.SetActive(true);
                break;
            case 5:
                typeText.gameObject.SetActive(true);
                break;
            case 6:
                typeText.gameObject.SetActive(false);
                break;
            case 7:
                typeText.gameObject.SetActive(true);
                break;
            case 81:
                typeText.gameObject.SetActive(true);
                break;
            case 8:
                break;

        }
    }
    public void ConfirmData()
    {
        int text1 = int.Parse(mainText.text);
        int text2 = int.Parse(typeText.text);
        switch (id)
        {
            case 1:
                outPut = $"{id}_{text1}_{colorID}";
                break;
            case 2:
                outPut = $"{id}_{text1}_{colorID}";
                break;
            case 3:
                outPut = $"{id}{text2}_{text1}_{colorID}";
                break;
            case 4:
                outPut = $"{id}{text2}_{text1}_{colorID}";
                typeText.gameObject.SetActive(true);
                break;
            case 5:
                outPut = $"{id}{text2}_{text1}_{colorID}";
                typeText.gameObject.SetActive(true);
                break;
            case 6:
                outPut = $"{id}{text2}_";
                for (int i = 0; i < dataIngaras.Count; i++)
                {
                    outPut +=  dataIngaras[i].ToString() + (i == dataIngaras.Count -1 ? "" :"+");
                }
                typeText.gameObject.SetActive(false);
                break;
            case 7:
                outPut = $"{id}{text2}_{text1}_{colorID}";
                typeText.gameObject.SetActive(true);
                break;
            case 81:
                outPut = $"{id}{text2}";
                typeText.gameObject.SetActive(true);
                break;
            case 8:
                outPut = $"{id}{text2}";
                break;

        }
    }
    public void Clear()
    {
        id = 0;

        outPut = "";

        main.material.color = Color.white;
        main.gameObject.SetActive(false);

        typeText.gameObject.SetActive(true);
        type.gameObject.SetActive(false);
    }
    public void OnMouseDown()
    {
        isSelect = true;

        if (!isOpen)
        {
            OpenGara();
            isOpen = true;
        }
        else
        {
            CloseGara();
            isOpen = false;
        }
        if (input.isInputting) return;

        if (!isShift)
        {
            input.StartInput(mainText);
        }
        else
        {
            input.StartInput(typeText);
        }
    }
    public void OnMouseUp()
    {
        isSelect = false;
    }
}
