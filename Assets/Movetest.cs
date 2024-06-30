using Photon.Pun;
using UnityEngine;

    public class MovingPlatform : MonoBehaviourPun
    {
        public float speed = 2.0f;          // 이동 속도
        public float distance = 3.0f;       // 이동 거리
        private Vector3 startPosition;      // 시작 위치
        private Vector3 targetPosition;     // 목표 위치
        private bool movingRight = true;    // 이동 방향

        void Start()
        {
            startPosition = transform.position;
            targetPosition = startPosition + new Vector3(distance, 0, 0);
        }

        void Update()
        {
            // 소유자일 때만 플랫폼을 이동시킴
            if (photonView.IsMine)
            {
                MovePlatform();
            }
        }

        void MovePlatform()
        {
            if (movingRight)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                if (transform.position == targetPosition)
                {
                    movingRight = false;
                    targetPosition = startPosition;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
                if (transform.position == startPosition)
                {
                    movingRight = true;
                    targetPosition = startPosition + new Vector3(distance, 0, 0);
                }
            }
        }
}
