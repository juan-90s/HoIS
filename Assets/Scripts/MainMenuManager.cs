using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    private VisualElement root;
    private Button playButton;
    private Button exitButton;
    // Start is called before the first frame update
    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        playButton = root.Q<Button>("play");
        playButton.RegisterCallback<ClickEvent>(OnPlayButton);
        exitButton = root.Q<Button>("exit");
        exitButton.RegisterCallback<ClickEvent>(OnExitButton);
    }

    private void OnPlayButton(ClickEvent evt)
    {
        SceneManager.LoadScene("CombatScene");
    }

    private void OnExitButton(ClickEvent evt)
    {
        Application.Quit();
    }

}
