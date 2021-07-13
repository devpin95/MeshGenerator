using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Button perspectiveButton;
    public TextMeshProUGUI perspectiveLabel;
    public Sprite perspectiveSprite;
    public Sprite orthgraphicSprite;
    private SpriteState _perspectiveModeState;
    private SpriteState _orthographicModeState;
    private bool _inPerspectiveMode = true;
    
    // Start is called before the first frame update
    void Start()
    {
        _perspectiveModeState.selectedSprite = perspectiveSprite;
        _perspectiveModeState.highlightedSprite = perspectiveSprite;
        _perspectiveModeState.pressedSprite = perspectiveSprite;
        _perspectiveModeState.disabledSprite = perspectiveSprite;
        
        _orthographicModeState.selectedSprite = orthgraphicSprite;
        _orthographicModeState.highlightedSprite = orthgraphicSprite;
        _orthographicModeState.pressedSprite = orthgraphicSprite;
        _orthographicModeState.disabledSprite = orthgraphicSprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPerspectiveButtonClick()
    {
        _inPerspectiveMode = !_inPerspectiveMode;

        if (_inPerspectiveMode)
        {
            perspectiveButton.spriteState = _perspectiveModeState;
            perspectiveButton.GetComponent<Image>().sprite = perspectiveSprite;
            perspectiveLabel.text = "Perspective";
        }
        else
        {
            perspectiveButton.spriteState = _orthographicModeState;
            perspectiveButton.GetComponent<Image>().sprite = orthgraphicSprite;
            perspectiveLabel.text = "Orthographic";
        }
    }
}
