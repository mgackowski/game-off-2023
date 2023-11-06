using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class FileSwitcher : MonoBehaviour
{
    public event Action<EvidenceFile> FileSwitched;

    [SerializeField] List<EvidenceFile> files;
    int selectedIndex = 0;

    public EvidenceFile GetFile()
    {
        return files[selectedIndex];
    }

    void SelectNext(InputAction.CallbackContext ctx)
    {
        SelectFile(1);
    }

    void SelectPrevious(InputAction.CallbackContext ctx)
    {
        SelectFile(-1);
    }

    int SelectFile(int offset)
    {
        //TODO: Ignore hidden files
        selectedIndex = Modulo(selectedIndex + offset, files.Count);
        FileSwitched?.Invoke(files[selectedIndex]);
        return selectedIndex;
    }

    int Modulo(int lhs, int rhs)
    {
        return (lhs % rhs + rhs) % rhs;
    }

    private void OnEnable()
    {
        InputManager.Instance.Gameplay.SwitchLeft.performed += SelectPrevious;
        InputManager.Instance.Gameplay.SwitchRight.performed += SelectNext;
    }

}