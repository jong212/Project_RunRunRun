using Photon.Pun;
using UnityEngine;

namespace ithappy
{
    public class Rnd_Animation : MonoBehaviourPun, IPunObservable
    {
        Animator anim;
        float offsetAnim;
        Quaternion syncedRotation;

        [SerializeField] string titleAnim;

        void Awake()
        {
            // 애니메이터를 Awake에서 초기화
            anim = GetComponent<Animator>();
        }

        void Start()
        {
            if (titleAnim != "Swing_X") return;

            if (PhotonNetwork.IsMasterClient)
            {
                // 마스터 클라이언트에서 랜덤 오프셋 생성
                offsetAnim = Random.Range(0f, 1f);
                Debug.Log($"MasterClient: Playing animation {titleAnim} with offset {offsetAnim}");
                anim.Play(titleAnim, 0, offsetAnim);  // 마스터 클라이언트에서 애니메이션 실행
            }
        }

        void Update()
        {
            if (titleAnim != "Swing_X") return;
            if (!photonView.IsMine)
            {
                // 로컬 회전 보간
                transform.rotation = Quaternion.RotateTowards(transform.rotation, syncedRotation, Time.deltaTime * 100);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 마스터 클라이언트가 현재 회전 값을 전송
                stream.SendNext(transform.rotation);
            }
            else
            {
                // 클라이언트가 회전 값을 수신
                syncedRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
