using PlayableAd.Core;
using UnityEngine;
namespace PlayableAd.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotateSpeed = 720f;
        [SerializeField] private float dragDeadZone = 10f;

        private CharacterController _cc;
        private Vector3 _moveDir;
        private Camera _cam;
        private Vector2 _dragStart;
        private bool _isDragging;
        private bool _inputBlocked = false;

        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cam = Camera.main;
        }

        private void OnEnable()
        {
            GameEventBus.OnCutsceneStart += BlockInput;
            GameEventBus.OnCutsceneEnd += UnblockInput;
        }

        private void OnDisable()
        {
            GameEventBus.OnCutsceneStart -= BlockInput;
            GameEventBus.OnCutsceneEnd -= UnblockInput;
        }

        private void BlockInput()
        {
            _inputBlocked = true;
            _moveDir = Vector3.zero;
            _isDragging = false;
            _dragStart = Vector2.zero;
            CurrentSpeed = 0f; 
        }

        private void UnblockInput()
        {
            _inputBlocked = false;
            _moveDir = Vector3.zero;
            _isDragging = false;
            _dragStart = Vector2.zero;
            CurrentSpeed = 0f;
        }

        private void Update()
        {
            if (_inputBlocked)
            {
                CurrentSpeed = 0f;
                ApplyGravity();
                return;
            }
            HandleInput();
            Move();
        }

        private void HandleInput()
        {
            bool began, pressed;
            Vector2 pos;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                began = touch.phase == TouchPhase.Began;
                pressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                pos = touch.position;
            }
            else
            {
                began = Input.GetMouseButtonDown(0);
                pressed = Input.GetMouseButton(0);
                pos = Input.mousePosition;
            }

            if (began)
            {
                _dragStart = pos;
                _isDragging = true;
            }
            if (!pressed) _isDragging = false;

            if (_isDragging)
            {
                var delta = pos - _dragStart;
                _moveDir = delta.sqrMagnitude > dragDeadZone * dragDeadZone
                    ? new Vector3(delta.x, 0f, delta.y).normalized
                    : Vector3.zero;
            }
            else
            {
                _moveDir = Vector3.zero;
            }
        }

        private void Move()
        {
            if (_moveDir == Vector3.zero)
            {
                CurrentSpeed = 0f; 
                ApplyGravity();
                return;
            }

            var motion = _moveDir * moveSpeed;
            motion.y = Physics.gravity.y;
            _cc.Move(motion * Time.deltaTime);

            CurrentSpeed = _moveDir.magnitude;

            var targetRot = Quaternion.LookRotation(_moveDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            _cc.Move(new Vector3(0, Physics.gravity.y, 0) * Time.deltaTime);
        }
    }
}