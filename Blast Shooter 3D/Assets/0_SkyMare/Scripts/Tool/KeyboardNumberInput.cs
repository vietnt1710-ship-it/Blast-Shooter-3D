using UnityEngine;
using TMPro;
using DG.Tweening;

public class KeyboardNumberInput : MonoBehaviour
{
    [Header("UI References")]
    [HideInInspector] public TMP_Text displayText;

    [Header("Settings")]
    [SerializeField] private int maxDigits = 10;
    [SerializeField] private Color inputColor = Color.white;

    private string currentNumber = "";
    public bool isInputting = false;

    Tween fade;
    public void StartInput(TMP_Text text)
    {
        if (isInputting) return;
        this.displayText = text;
        isInputting = true;
        fade = displayText.DOFade(0.5f, 0.3f).SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        if (!isInputting) return;

        // Kiểm tra các phím số (0-9)
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                AddDigit(i.ToString());
            }
        }

        //// Kiểm tra phím Backspace để xóa
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveLastDigit();
        }

        // Kiểm tra Enter hoặc Space để xác nhận
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            fade.Kill();
            displayText.DOFade(1f, 0);
            isInputting = false;
            ConfirmInput();

        }

        // Kiểm tra Escape để hủy
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearInput();
        }
    }


    private void AddDigit(string digit)
    {
        if (currentNumber.Length < maxDigits)
        {
            currentNumber += digit;

            UpdateDisplay();

            // Có thể thêm hiệu ứng âm thanh ở đây
            Debug.Log($"Đã nhập: {digit}, Số hiện tại: {currentNumber}");
        }
    }

    private void RemoveLastDigit()
    {
        if (currentNumber.Length > 0)
        {
            currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
            UpdateDisplay();
        }
    }

    private void ConfirmInput()
    {
        if (!string.IsNullOrEmpty(currentNumber))
        {
            int finalNumber = int.Parse(currentNumber);

            // Hiển thị số đã xác nhận
            if (displayText != null)
            {
                displayText.color = inputColor;
            }

            Debug.Log($"Số đã xác nhận: {finalNumber}");

            // Gọi event hoặc method xử lý số đã nhập
            OnNumberConfirmed(finalNumber);

            // Reset sau 1 giây
        }
    }


    public void ClearInput()
    {
        currentNumber = "";

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            if (string.IsNullOrEmpty(currentNumber))
            {
                displayText.text = "0";
                displayText.color = inputColor;
            }
            else
            {
                displayText.text = currentNumber;
                displayText.color = inputColor;
            }
        }
    }

    private void OnNumberConfirmed(int number)
    {
    }

}
