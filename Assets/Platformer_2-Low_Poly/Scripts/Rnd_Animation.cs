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
        private PhotonView PV;  // PhotonView ���� �߰�
        [SerializeField] string titleAnim;

        void Awake()
        {
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 30;
            PV = GetComponent<PhotonView>();
            anim = GetComponent<Animator>();
        }

        public override void OnDisable() // override Ű���� �߰�
        {
            anim.enabled = false;
        }
        public override void OnEnable()
        {
            if (PV == null) PV = GetComponent<PhotonView>();
            if (anim == null) anim = GetComponent<Animator>();

            if (PV != null && PV.IsMine && titleAnim == "Swing_X")
            {
                EnableAnimatorAndPlay();
            }
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
                    anim.enabled = false; // �ִϸ����� ��Ȱ��ȭ
                }
            }
        }

        void EnableAnimatorAndPlay()
        {
            if (anim != null)
            {
                anim.enabled = true; // �ִϸ����� Ȱ��ȭ
                // ������ Ŭ���̾�Ʈ���� ���� ������ ����
                offsetAnim = Random.Range(0f, 1f);
                Debug.Log($"MasterClient: Playing animation {titleAnim} with offset {offsetAnim}");
                anim.Play(titleAnim, 0, offsetAnim);  // ������ Ŭ���̾�Ʈ���� �ִϸ��̼� ����
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
                // ���� ȸ�� ����
                transform.rotation = Quaternion.RotateTowards(transform.rotation, syncedRotation, Time.deltaTime * 100);
            }
        }*/

        /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // ������ Ŭ���̾�Ʈ�� ���� ȸ�� ���� ����
                stream.SendNext(transform.rotation);
            }
            else
            {
                // Ŭ���̾�Ʈ�� ȸ�� ���� ����
                syncedRotation = (Quaternion)stream.ReceiveNext();
            }
        }*/
    }
}