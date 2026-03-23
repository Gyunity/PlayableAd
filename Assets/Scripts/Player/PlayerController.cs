using UnityEngine;

namespace PlayableAd.Player
{
    /// <summary>
    /// 플레이어 이동만 담당합니다 (SRP).
    /// 플레이어블 광고 특성상 터치/마우스 드래그로 조작합니다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotateSpeed = 720f;  // deg/sec

        private CharacterController _cc;
        private Vector3 _moveDir;
        private Camera _cam;

        // 터치 드래그 상태
        private Vector2 _dragStart;
        private bool _isDragging;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cam = Camera.main;
        }

        private void Update()
        {
            HandleInput();
            Move();
        }

        private void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            // 에디터: 마우스 드래그
            if (Input.GetMouseButtonDown(0))
            {
                _dragStart = Input.mousePosition;
                _isDragging = true;
            }
            if (Input.GetMouseButtonUp(0)) _isDragging = false;

            if (_isDragging)
            {
                var delta = (Vector2)Input.mousePosition - _dragStart;
                _moveDir = new Vector3(delta.x, 0f, delta.y).normalized;
            }
            else
            {
                _moveDir = Vector3.zero;
            }
#else
            // 모바일: 터치
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)  _dragStart = touch.position;
                if (touch.phase == TouchPhase.Ended)  { _moveDir = Vector3.zero; return; }

                var delta = touch.position - _dragStart;
                _moveDir  = new Vector3(delta.x, 0f, delta.y).normalized;
            }
            else
            {
                _moveDir = Vector3.zero;
            }
#endif
        }

        private void Move()
        {
            if (_moveDir == Vector3.zero) return;

            // 중력 적용
            var motion = _moveDir * moveSpeed;
            motion.y = Physics.gravity.y;

            _cc.Move(motion * Time.deltaTime);

            // 이동 방향으로 회전
            var targetRot = Quaternion.LookRotation(_moveDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }
}