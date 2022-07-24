using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolsAndMechanics.CarMechanic
{
    public enum Axel
    {
        Front,
        Rear
    }

    public enum DriveMode
    {
        FWD,
        RWD,
        FullWD
    }

    [Serializable]
    public class Wheel
    {
        public Axel Axel;
        public Transform WheelModel;
        public WheelCollider WheelCollider;
    }

    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [Header("Engine")]
        [SerializeField]
        [Min(0.1f)]
        private float maxEngineForce = 30f;
        [SerializeField]
        [Min(0.1f)]
        private float brakeForce = 50f;
        [SerializeField]
        private DriveMode driveMode = DriveMode.FullWD;

        [Space]
        [SerializeField]
        private Transform centerOfMass;

        [Header("Steering")]
        [SerializeField]
        private float turnSensitivity = 1f;
        [SerializeField]
        private float maxSteerAngle = 30f;

        [Header("Wheels")]
        [SerializeField]
        private bool needSteerRotateFront = true;
        [SerializeField]
        private bool needSteerRotateRear;
        [SerializeField]
        private List<Wheel> wheels;

        private Rigidbody rb;
        private Vector2 input;
        private bool isBrake;

        private const float ENGINE_FORCE_MULTIPLIER = 600f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = centerOfMass.localPosition;
        }

        private void Update()
        {
            AnimateWheels();
        }

        private void FixedUpdate()
        {
            WheelsTorque();
            Steer();
            Brake();
        }

        public void UpdateInput(Vector2 input)
        {
            this.input = input;
        }

        public void UpdateBrakeInput(bool isBrake)
        {
            this.isBrake = isBrake;
        }

        private void WheelsTorque()
        {
            foreach (var wheel in wheels)
            {
                if (driveMode == DriveMode.FullWD || driveMode == DriveMode.FWD && wheel.Axel == Axel.Front || driveMode == DriveMode.RWD && wheel.Axel == Axel.Rear)
                {
                    wheel.WheelCollider.motorTorque = input.y * maxEngineForce * Time.fixedDeltaTime * ENGINE_FORCE_MULTIPLIER;
                }
                else
                {
                    wheel.WheelCollider.motorTorque = 0;
                }
            }
        }

        private void Steer()
        {
            foreach (var wheel in wheels)
            {
                if (wheel.Axel == Axel.Front && needSteerRotateFront || wheel.Axel == Axel.Rear && needSteerRotateRear)
                {
                    float steerAngle = input.x * turnSensitivity * maxSteerAngle;
                    wheel.WheelCollider.steerAngle = Mathf.Lerp(wheel.WheelCollider.steerAngle, wheel.Axel == Axel.Front ? steerAngle : -steerAngle, 0.6f);
                }
                else
                {
                    wheel.WheelCollider.steerAngle = 0f;
                }
            }
        }

        private void AnimateWheels()
        {
            foreach (var wheel in wheels)
            {
                Quaternion rot;
                Vector3 pos;
                wheel.WheelCollider.GetWorldPose(out pos, out rot);
                wheel.WheelModel.transform.position = pos;
                wheel.WheelModel.transform.rotation = rot;
            }
        }

        private void Brake()
        {
            if (isBrake || input.y == 0)
            {
                foreach (var wheel in wheels)
                {
                    wheel.WheelCollider.brakeTorque = 300 * brakeForce * Time.fixedDeltaTime;
                }
            }
            else
            {
                foreach (var wheel in wheels)
                {
                    wheel.WheelCollider.brakeTorque = 0;
                }
            }
        }

        [ContextMenu("Create Wheels Colliders")]
        private void CreateWheelsColliders()
        {
            Transform parent = new GameObject("Wheels Colliders").transform;
            parent.SetParent(wheels[0].WheelModel.parent.parent);
            parent.localPosition = wheels[0].WheelModel.parent.localPosition;

            foreach (var wheel in wheels)
            {
                Transform tr = new GameObject(wheel.WheelModel.name).transform;
                tr.transform.SetParent(parent);
                tr.localPosition = wheel.WheelModel.localPosition;
                WheelCollider coll = tr.gameObject.AddComponent<WheelCollider>();
                coll.mass = 50;
                coll.center = new Vector3(0f, 0.15f, 0f);
                JointSpring spring = coll.suspensionSpring;
                spring.spring = 8000;
                spring.damper = 2000;
                coll.suspensionSpring = spring;
                WheelFrictionCurve curve = coll.forwardFriction;
                curve.stiffness = 2;
                coll.forwardFriction = curve;
                curve = coll.sidewaysFriction;
                curve.stiffness = 2.4f;
                coll.sidewaysFriction = curve;
                wheel.WheelCollider = coll;
            }
        }
    }    
}