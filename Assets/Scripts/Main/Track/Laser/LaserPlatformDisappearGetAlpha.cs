using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DrawOutline에 레이저로 인해 사라지는 플랫폼이라는 것과 세부 정보를 알립니다.
public class LaserPlatformDisappearGetAlpha : MonoBehaviour
{
    [HideInInspector] public bool isDisappearing = false;
    [HideInInspector] public float alpha = 1f;
}