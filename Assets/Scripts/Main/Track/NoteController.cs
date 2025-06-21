using UnityEngine;
using TMPro;

public class NoteController : MonoBehaviour, IWireClickButton
{
    [SerializeField] private GameObject note; 
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField, TextArea(5, 12)] private string noteStr;

    public void Click()
    {
        AudioManager.Instance.PlaySfx2D(AudioSystem.SfxType.NoteOpen);
        MainSceneManager.Instance.PauseGame(false);
        note.SetActive(true);
        noteText.text = noteStr;
    }
}
