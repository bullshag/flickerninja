using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInputLogger : MonoBehaviour
{
    // Number of joystick buttons and axes to check
    public int maxJoystickButtons = 20;
    public int maxJoystickAxes = 10;

    void Update()
    {
        // Check for button presses
        for (int i = 0; i < maxJoystickButtons; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
            {
                Debug.Log("Button Pressed: joystick button " + i);
            }
        }
    
        
    }
}
