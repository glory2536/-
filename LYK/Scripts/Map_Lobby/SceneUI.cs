using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SceneUI : MonoBehaviour
{
    [SerializeField] Scene nextScene;
    Button sceneButton;

    private void Start()
    {
        if(TryGetComponent<Button>(out Button button))
        {
            button.onClick.AddListener(() => SceneManagerLYK.LoadScene(nextScene));
        }
    }

}
