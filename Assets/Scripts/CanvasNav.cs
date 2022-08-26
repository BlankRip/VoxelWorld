using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasNav : MonoBehaviour
{
    public List<Button> buttons;
    int buttonIndex = 0;

    void Start() {
        if (buttons != null) {
            buttons[0].Select();
            buttonIndex = 0;
        }
    }

    void Update() {
		if (Input.GetKeyDown(KeyCode.Tab) && buttons.Count > 1) {
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				//If shift is pressed, move up on the list - or, if at the top of the list, move to the bottom
				if (buttonIndex <= 0)
					buttonIndex = buttons.Count;
				buttonIndex--;
				buttons[buttonIndex].Select();
			} else {
				//if shift is not pressed, move down on the list - or, if at the bottom, move to the top
				if (buttons.Count <= buttonIndex + 1)
					buttonIndex = -1;
				buttonIndex++;
				buttons[buttonIndex].Select();
			}
		}
	}
}
