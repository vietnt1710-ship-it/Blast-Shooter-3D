using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public ShooterColor shooterColor;
    public ShooterAnimationController shooterAnimationController;

    public ShooterData shooterData;

    public TMP_Text bulletCount;

    public void InitShooter(ShooterData shooterData)
    {
        this.bulletCount.text = shooterData.bulletCount.ToString();
        this.shooterData = shooterData;
        this.shooterColor.ChangeColor(shooterData.color);
    }
}
