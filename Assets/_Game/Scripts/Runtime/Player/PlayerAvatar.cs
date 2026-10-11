using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PanicMechanics.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerAvatar : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private Camera viewCamera;

        private readonly NetworkVariable<byte> _role = new NetworkVariable<byte>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private CharacterController _controller;
        private float _pitch;

        public byte RoleValue
        {
            get { return _role.Value; }
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                if (viewCamera != null)
                {
                    viewCamera.enabled = true;
                }

                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (viewCamera != null)
            {
                viewCamera.enabled = false;
            }
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            HandleLook();
            HandleMove();
            HandleKeys();
        }

        private void HandleMove()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            float z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            var direction = transform.right * x + transform.forward * z;
            _controller.SimpleMove(direction * moveSpeed);
        }

        private void HandleLook()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var delta = mouse.delta.ReadValue();
            transform.Rotate(Vector3.up, delta.x * lookSensitivity, Space.Self);

            _pitch = Mathf.Clamp(_pitch - delta.y * lookSensitivity, -80f, 80f);
            if (viewCamera != null)
            {
                var euler = viewCamera.transform.localEulerAngles;
                viewCamera.transform.localEulerAngles = new Vector3(_pitch, euler.y, euler.z);
            }
        }

        private void HandleKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                RequestRoleServerRpc(0);
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                RequestRoleServerRpc(1);
            }
        }

        private void TryInteract()
        {
            if (viewCamera == null)
            {
                return;
            }

            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out var hit, interactRange))
            {
                return;
            }

            var station = hit.collider.GetComponentInParent<InteractableStation>();
            if (station == null)
            {
                return;
            }

            var manager = NetworkGameManager.Instance;
            if (manager != null)
            {
                manager.SubmitInteractionServerRpc(station.TargetId, station.ActionId, hit.distance);
            }
        }

        [ServerRpc]
        private void RequestRoleServerRpc(byte role)
        {
            _role.Value = role;
        }
    }
}
