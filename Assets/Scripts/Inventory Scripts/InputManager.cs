using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager inputManager;
    public InputActionAsset controls;
    public InputAction openInventory;

    // Start is called before the first frame update
    void Start()
    {
        if (inputManager == null)
        {
            inputManager = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        openInventory = controls.FindAction("OpenInventory");


    }

    // Update is called once per frame
    void Update()
    {

    }
}
