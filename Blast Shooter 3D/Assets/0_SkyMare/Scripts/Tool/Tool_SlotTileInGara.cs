using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Tool_SlotTileInGara : MonoBehaviour
{
    public int index;
    public Tool_Slot root;

    public KeyboardNumberInput input;
    public MeshRenderer main;
    public TMP_Text mainText;

    public string source;
    public int bulletCount;
    public int colorID;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            InitColor();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmData();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Clear();
        }
    }
    public void Clear()
    {
        main.gameObject.SetActive(false);
        source = "0";
    }
    public void Init(Tool_Slot root, int index)
    {
        this.gameObject.SetActive(true);
        source = root.dataIngaras[index];

        string blc;
        string cli;
        GridParse.OnSplitBeAf(source, out blc, out cli);

        this.root = root;
        this.index = index;
        bulletCount = int.Parse(blc);
        colorID = int.Parse(cli);
        main.material.color = ToolManager.I.colorWithID.ColorWithID2(colorID).color;

        main.gameObject.SetActive(true);
    }
    public void Close()
    {
        this.gameObject.SetActive(false);
    }
    public void InitColor()
    {
        colorID = ToolManager.I.colorID;
        main.material.color = ToolManager.I.color;
    }
    public void Clode()
    {
        this.gameObject.SetActive(true);
    }
    public void ConfirmData()
    {
        int text1 = int.Parse(mainText.text);

        source = $"{text1}_{colorID}";
    }

    public void OnMouseDown()
    {
        if (input.isInputting) return;

        input.StartInput(mainText);
    }
}
