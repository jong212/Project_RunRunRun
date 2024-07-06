using Photon.Pun;
using UnityEngine;

    public class MovingPlatform : MonoBehaviourPun
    {
        public float speed = 2.0f;          // �̵� �ӵ�
        public float distance = 3.0f;       // �̵� �Ÿ�
        private Vector3 startPosition;      // ���� ��ġ
        private Vector3 targetPosition;     // ��ǥ ��ġ
        private bool movingRight = true;    // �̵� ����

        void Start()
        {
            startPosition = transform.position;
            targetPosition = startPosition + new Vector3(distance, 0, 0);
        }

        void Update()
        {
            // �������� ���� �÷����� �̵���Ŵ
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
