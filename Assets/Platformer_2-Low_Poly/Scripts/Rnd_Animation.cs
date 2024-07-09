using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ithappy
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(PhotonTransformView))]
    public class Rnd_Animation : MonoBehaviourPunCallbacks, IPunObservable
    {
        private Animator anim;
        private PhotonView PV;
        private float offsetAnim;
        [SerializeField] private string titleAnim;

        void Awake()
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 30;
            PV = GetComponent<PhotonView>();
            anim = GetComponent<Animator>();

            Debug.Log("Awake called");
        }

        void Start()
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }
            Debug.Log("Start called");

            if (PV == null) PV = GetComponent<PhotonView>();
            if (anim == null) anim = GetComponent<Animator>();

            // Initial state, disable animator if not master client
            anim.enabled = PhotonNetwork.IsMasterClient;
        }

        // Function to be called to initialize the obstacle
        public void InitializeObstacle()
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }

            Debug.Log("Initializing Obstacle");

            if (PhotonNetwork.IsMasterClient)
            {
                EnableAnimatorAndPlay();
            }
            else
            {
                if (anim != null)
                {
                    anim.enabled = false;
                }
            }
        }

        private void EnableAnimatorAndPlay()
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }
            if (anim != null)
            {
                anim.enabled = true;
                offsetAnim = Random.Range(0f, 1f);
                Debug.Log($"MasterClient: Playing animation {titleAnim} with offset {offsetAnim}");
                anim.Play(titleAnim, 0, offsetAnim);
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }
            Debug.Log("OnMasterClientSwitched called");

            if (PhotonNetwork.IsMasterClient)
            {
                EnableAnimatorAndPlay();
            }
            else
            {
                if (anim != null)
                {
                    anim.enabled = false;
                }
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }
            Debug.Log("OnPlayerLeftRoom called");

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("A player left the room. New Master Client is handling animation.");
                EnableAnimatorAndPlay();
            }
        }

        // Synchronize animator state
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (titleAnim != "Swing_X")
            {
                return;
            }
            if (stream.IsWriting)
            {
                stream.SendNext(anim.enabled);
                stream.SendNext(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
            }
            else
            {
                bool isAnimatorEnabled = (bool)stream.ReceiveNext();
                float normalizedTime = (float)stream.ReceiveNext();

                if (anim != null)
                {
                    anim.enabled = isAnimatorEnabled;
                    if (isAnimatorEnabled)
                    {
                        anim.Play(titleAnim, 0, normalizedTime);
                    }
                }
            }
        }
    }
}