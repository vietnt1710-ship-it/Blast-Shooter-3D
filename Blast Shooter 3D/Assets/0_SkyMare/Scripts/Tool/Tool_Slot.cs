using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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

    public string outPut = "0";
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

        if(direction == 1)
        {
            mainText.text = "Up";
        }
        else if(direction == 2)
        {
            mainText.text = "Down";
        }
        else if(direction == 3)
        {
            mainText.text = ">";
        }
        else
        {
            mainText.text = "<";
        }
    }
    TypeAndIDAndButton select;
    public int id = 0;
    public int colorID;
    public int bulletCount = 0;

    public void InitPipe()
    {
        if (id != 6) return;
        string data = $"{20}_{ToolManager.I.colorID}";
        dataIngaras.Add(data);
        OpenGara();
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
            dataIngaras[i] = ToolManager.I.tool_SlotTileInGaras[index].source;
            ToolManager.I.tool_SlotTileInGaras[index].Close();
        }
        for(int i = 0; i <= dataIngaras.Count; i++)
        {
            if (dataIngaras[i] == "0")
            {
                dataIngaras.Remove(dataIngaras[i]);
                i--;
            }
        }
        

        ToolManager.I.UpdateCount();
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
        if(id == 1)
        {
            type.gameObject.SetActive(false);
        }
        else 
        { type.gameObject.SetActive(true); }

        type.color = select.GetButtonColor();

        switch (id)
        {
     
            case 2:
                typeText.gameObject.SetActive(true);
                typeText.text = "?";
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
                typeText.gameObject.SetActive(true);
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
        int text2 = 0;
            if (int.TryParse(typeText.text, out text2))
                 Debug.Log("Hehe");
        if (id == 6)
        {
            outPut = $"{id}{text2}_";
            for (int i = 0; i < dataIngaras.Count; i++)
            {
                outPut += dataIngaras[i].ToString() + (i == dataIngaras.Count - 1 ? "" : "+");
            }
            //typeText.gameObject.SetActive(false);
            return;
        }
        int text1 = int.Parse(mainText.text);
       

        bulletCount = text1;
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
        ToolManager.I.UpdateCount();
    }
    public void LoadData(string source)
    {
        this.outPut = source;

        string be;
        string af;
        try
        {
            GridParse.OnSplitBeAf(source, out be, out af);

        }
        catch
        {
            var latchBe = GridParse.IdAfter1Char(source);

            if (latchBe >= 10)// có 2 chữ số thì là head, số sau là key
            {
                int latchID = GridParse.IdAfter1Char(latchBe.ToString());
                typeText.gameObject.SetActive(true); typeText.text = latchID.ToString();
                id = 81;
            }
            else
            {
                typeText.gameObject.SetActive(true); typeText.text = latchBe.ToString();
                id = 8;
            }
        }
        
    }
    public void Clear()
    {
        id = 0;

        outPut = "0";

        dataIngaras.Clear();

        main.material.color = Color.white;
        main.gameObject.SetActive(false);

        typeText.gameObject.SetActive(true);
        type.gameObject.SetActive(false);
    }
    public void OnMouseEnter()
    {
        isSelect = true;
    }
    public void OnMouseExit()
    {
        isSelect = false;
    }
    public void OnMouseDown()
    {
        if (id == 6)
        {
            if (!isOpen)
            {
                Debug.Log("Opening Gara");
                isOpen = true;
                OpenGara();
               
            }
            else
            {
                Debug.Log("Closing Gara");
                isOpen = false;
                CloseGara();
               
            }
            return;
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
       
    }
}
