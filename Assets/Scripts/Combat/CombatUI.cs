using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CombatUI : MonoBehaviour
{
    private BattleManager manager;
    private VisualElement root;
    private Button AIbuttonA;
    private Button AIbuttonB;
    private Button QuitButton;
    // Start is called before the first frame update
    private void OnEnable()
    {
        manager = GameObject.Find("GameManager").GetComponent<BattleManager>();
        root = GetComponent<UIDocument>().rootVisualElement;
        AIbuttonA = root.Q<Button>("AIbuttonA");
        AIbuttonB = root.Q<Button>("AIbuttonB");
        AIbuttonA.RegisterCallback<ClickEvent>(ToggleAIforA);
        AIbuttonB.RegisterCallback<ClickEvent>(ToggleAIforB);

        QuitButton = root.Q<Button>("QuitButton");
        QuitButton.RegisterCallback<ClickEvent>(QuitScene);
    }

    private void ToggleAIforA(ClickEvent evt)
    {
        if (manager.AisAI)
        {
            AIbuttonA.text = "a human";
            manager.AisAI = false;
        }
        else
        {
            AIbuttonA.text = "a ai";
            manager.AisAI = true;
        }
    }

    private void ToggleAIforB(ClickEvent evt)
    {
        if (manager.BisAI)
        {
            AIbuttonB.text = "b human";
            manager.BisAI = false;
        }
        else
        {
            AIbuttonB.text = "b ai";
            manager.BisAI = true;
        }
    }

    private void QuitScene(ClickEvent evt)
    {
        SceneManager.LoadScene("MainMenu");
    }
}
