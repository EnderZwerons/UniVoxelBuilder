using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject blockMenu;

    public TextMeshProUGUI popup;

    public static bool inventoryOpen;

    public static UIController instance;

    private bool canMoveToNextPopup;

    private int popupIndex;

    public YesNoPopup[] yesNoPopups;

    [Serializable]
    public class YesNoPopup
    {
        public GameObject popup;

        public TextMeshProUGUI popupText;
    }

    public class PopupInstance
    {
        public string popupText;

        public float timeLeft;

        public PopupInstance(string text, float time)
        {
            this.popupText = text;
            this.timeLeft = time;
        }
    }

    public List<PopupInstance> popups = new List<PopupInstance>();

    void Awake()
    {
        instance = this;
    }

    public void SetBadControls(bool bad)
    {
        PlayerPrefs.SetInt("badcontrols", (bad ? 1 : 0));
    }

    void Update()
    {
        for (int i = 0; i < popups.Count; i++)
        {
            PopupInstance popup = popups[i];
            if (popup.timeLeft <= 0f)
            {
                popups.RemoveAt(popups.IndexOf(popup));
                RemovePopup();
                canMoveToNextPopup = true;
                continue;
            }
            popup.timeLeft -= 1f * Time.deltaTime;
        }
        if (popups.Count == 0)
        {
            popupIndex = 1;
        }
        if (canMoveToNextPopup && popups.Count != 0)
        {
            DoPopup(popups[popupIndex]);
        }
        if (Input.GetKeyDown("e") && !Typing)
        {
            Helper.LockCursor(inventoryOpen);
            blockMenu.SetActive(!blockMenu.activeInHierarchy);
            inventoryOpen = !inventoryOpen;
        }
    }

    public bool Typing
    {
        get
        {
            foreach (TMP_InputField IF in Resources.FindObjectsOfTypeAll<TMP_InputField>())
            {
                if (IF.gameObject.activeInHierarchy && IF.isFocused)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public void LoadMapWithButton(TMP_InputField IF)
    {
        if (IF.text == "")
        {
            UIController.instance.Popup("make sure to press enter!", 0.5f);
            return;
        }
        AreYouSurePopup(string.Format("this will delete your current build! are you sure?", (IF.text.EndsWith(".uvbmap") ? IF.text : IF.text + ".uvbmap")), 1);
    }

    public void ForceLoad(TMP_InputField IF)
    {
        StartCoroutine(WorldGen.instance.GenerateFromUVBMapPreformace(IF.text));
    }

    public void SaveMapWithButton(TMP_InputField IF)
    {
        if (IF.text == "")
        {
            UIController.instance.Popup("make sure to press enter!", 0.5f);
            return;
        }
        else if (File.Exists(IF.text.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + IF.text : StreamingAssets.UVBMapPath + "/" + IF.text + ".uvbmap"))
        {
            AreYouSurePopup(string.Format("\"{0}\" already exists! would you like to replace it?", (IF.text.EndsWith(".uvbmap") ? IF.text : IF.text + ".uvbmap")), 0);
        }
        GameData.SaveToUVBMAPFile(IF.text);
    }

    public void ForceSave(TMP_InputField IF)
    {
        GameData.SaveToUVBMAPFile(IF.text);
    }

    public void Popup(string text, float time)
    {
        popups.Add(new PopupInstance(text, time));
        DoPopup(popups[popupIndex - 1]);
    }

    public void DoPopup(PopupInstance PI)
    {
        canMoveToNextPopup = false;
        popup.text = PI.popupText;
        popup.gameObject.SetActive(true);
        popupIndex++;
    }

    public void RemovePopup()
    {
        popup.text = "";
        popup.gameObject.SetActive(false);
    }

    public void AreYouSurePopup(string text, int index)
    {
        yesNoPopups[index].popupText.text = text;
        yesNoPopups[index].popup.SetActive(true);
    }
}
