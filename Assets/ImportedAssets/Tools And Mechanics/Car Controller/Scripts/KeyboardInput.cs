using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolsAndMechanics.CarMechanic
{
    [RequireComponent(typeof(CarController))]
    public class KeyboardInput : MonoBehaviour
    {
        private CarController car;

        private void Awake()
        {
            car = GetComponent<CarController>();
        }

        private void Update()
        {
            car.UpdateInput(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
            car.UpdateBrakeInput(Input.GetKey(KeyCode.Space));
        }
    }
}