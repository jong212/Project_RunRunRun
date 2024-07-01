using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ithappy
{
    public class Rnd_Animation : MonoBehaviourPunCallbacks
    {
        Animator anim;
        float offsetAnim;
        Quaternion syncedRotation;
        private PhotonView PV;  // PhotonView 변수 추가
        [SerializeField] string titleAnim;

        void Awake()
        {
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 30;
            PV = GetComponent<PhotonView>();
            anim = GetComponent<Animator>();
        }

        private void OnDisable()
        {
            anim.enabled = false;

        }
        void Start()
        {
            if (PV == null) {
                this.enabled = false;
            }
            /*if (titleAnim != "Swing_X") return;*/

            if (PV != null && PV.IsMine)
            {
                EnableAnimatorAndPlay();
            }
            else
            {
                if (anim != null)
                {
                    anim.enabled = false; // 애니메이터 비활성화
                }
            }
        }

        void EnableAnimatorAndPlay()
        {
            if (anim != null)
            {
                anim.enabled = true; // 애니메이터 활성화
                // 마스터 클라이언트에서 랜덤 오프셋 생성
                offsetAnim = Random.Range(0f, 1f);
                Debug.Log($"MasterClient: Playing animation {titleAnim} with offset {offsetAnim}");
                anim.Play(titleAnim, 0, offsetAnim);  // 마스터 클라이언트에서 애니메이션 실행
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PV == null) PV = GetComponent<PhotonView>();
            if (anim == null) anim = GetComponent<Animator>();

            if (PV != null && PV.IsMine && titleAnim == "Swing_X")
            {
                EnableAnimatorAndPlay();
            }
        }

        /*void Update()
        {
            if (titleAnim != "Swing_X") return;
            if (!photonView.IsMine)
            {
                // 로컬 회전 보간
                transform.rotation = Quaternion.RotateTowards(transform.rotation, syncedRotation, Time.deltaTime * 100);
            }
        }*/

        /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
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
        }*/
    }
}