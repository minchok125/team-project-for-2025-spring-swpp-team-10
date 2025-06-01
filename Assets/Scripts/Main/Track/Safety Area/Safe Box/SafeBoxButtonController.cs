using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeBoxButtonController : MonoBehaviour, IWireClickButton
{
    [SerializeField] private int myNumber;

    private SafeBoxKeypadController _keypad;

    private void Start()
    {
        _keypad = transform.parent.GetComponent<SafeBoxKeypadController>();
    }

    public void Click()
    {
        _keypad.GetInput(myNumber);
        GameManager.PlaySfx(SfxType.KeypadInput);
    }
}
