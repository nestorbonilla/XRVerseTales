using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    // Variable to track toggle state
    private bool isToggled = false;

    // Reference to the Button component
    public Button yourButton;

    void Start()
    {
        // Add listener to the button
        yourButton.onClick.AddListener(ToggleState);
    }

    void ToggleState()
    {
        // Toggle the state
        isToggled = !isToggled;

        // Perform action based on the state
        if (isToggled)
        {
            // Action when button is toggled on
            Debug.Log("Button is toggled on");
        }
        else
        {
            // Action when button is toggled off
            Debug.Log("Button is toggled off");
        }
    }
}