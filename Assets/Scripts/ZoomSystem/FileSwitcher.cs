using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

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

    /* Switch focus to an unlocked file by providing its name (case-insensitive). */
    [YarnCommand("selectFile")]
    public void SelectFile(string name)
    {
        EvidenceFile file = files.Where(x => x.name.ToLower() == name.ToLower()).First();
        if (file!= null && !file.Hidden)
        {
            FileSwitched?.Invoke(file);
        }
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
        for (int i = 0; i < files.Count; i++)
        {
            selectedIndex = Modulo(selectedIndex + offset, files.Count);
            if (!files[selectedIndex].Hidden)
            {
                FileSwitched?.Invoke(files[selectedIndex]);
                break;
            }
        }   
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