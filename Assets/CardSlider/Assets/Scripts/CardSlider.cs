using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tkitfacn.UI
{
    public class CardSlider : MonoBehaviour
    {
        [SerializeField] Transform content = default;
        [SerializeField] float space = 200;
        [SerializeField] float sizePrevious = 0.8f;
        [SerializeField] float minSizePrevious = 0.1f;
        [SerializeField] float maxSizePrevious = 1f;
        [System.Serializable]
        public class OnCardChangeEvent : UnityEngine.Events.UnityEvent<Transform> { }
        public OnCardChangeEvent onCardChange = new OnCardChangeEvent();


        int length = 0;
        int indexLeft = 0;
        int indexRight = 0;

        int indexCard = 0;
        

        private void Start()
        {
            ResetCardIndex();
            Sort();
            GenerateButton();
            GenerateSlider();
        }

        public int CardLength => content.childCount;
        public int CardIndex
        {
            get => indexCard; set
            {
                SetCardIndex(value);
            }
        }

        public void ResetCardIndex()
        {
            indexCard = 0;
        }

        public void OpenCard(Transform cardTransform)
        {
            if (cardTransform.parent != content) return;
            if (content.GetChild(content.childCount - 1) == cardTransform) return;
            int count = 0;
            bool increase = (cardTransform.localPosition - content.GetChild(content.childCount - 1).localPosition).x > 0;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                if (increase)
                {
                    if (cardTransform.localPosition.x >= content.GetChild(i).localPosition.x &&
                        content.GetChild(content.childCount - 1).localPosition.x < content.GetChild(i).localPosition.x)
                    {
                        count++;
                    }
                }
                else
                {
                    if (cardTransform.localPosition.x <= content.GetChild(i).localPosition.x &&
                        content.GetChild(content.childCount - 1).localPosition.x > content.GetChild(i).localPosition.x)
                    {
                        count--;
                    }
                }
            }
            SetCardIndex(indexCard + count, true);
        }

        public void SetCardIndex(int value, bool anim = false)
        {
            if (value >= 0 && value < content.childCount)
            {
                if (value < indexCard)
                {
                    for (int i = indexCard - 1; i >= value; i--)
                    {
                        //move previous item
                        content.GetChild(content.childCount - 1).SetSiblingIndex(content.childCount - 2 - i);
                    }
                }
                else if (value > indexCard)
                {
                    for (int i = indexCard; i < value; i++)
                    {
                        //move next item
                        content.GetChild(content.childCount - 2 - i).SetSiblingIndex(content.childCount - 1);
                    }
                }

                indexCard = value;
                if (anim) SortAnim();
                else
                {
                    Sort();
                }

                UpdateSlider();
                onCardChange.Invoke(content.GetChild(content.childCount - 1));
            }
        }

        public RectTransform GetCardSelectRect()
        {
            return content.GetChild(indexCard).GetComponent<RectTransform>();
        }

        public void Sort()
        {
            length = content.childCount;
            if (length == 0) return;

            SetLocalTransform(content.GetChild(length - 1), Vector3.zero, Vector3.one);

            var cPos = Vector3.zero;
            var cScale = Vector3.one;

            indexLeft = length - 1 - indexCard;
            indexRight = length - 2 - indexCard;

            for (int i = length - 2; i >= indexLeft; i--)
            {
                OutCardTransformParameter(Vector3.left, ref cPos, ref cScale);
                SetLocalTransform(content.GetChild(i), cPos, cScale);
            }

            cPos = Vector3.zero;
            cScale = Vector3.one;


            for (int i = indexRight; i >= 0; i--)
            {
                OutCardTransformParameter(Vector3.right, ref cPos, ref cScale);
                SetLocalTransform(content.GetChild(i), cPos, cScale);
            }
        }

        List<ItemUI> itemUIs = new List<ItemUI>();
        bool isAnim = false;
        float timeAnim = 0;
        
        public void SortAnim()
        {
            itemUIs.Clear();

            length = content.childCount;
            if (length == 0) return;

            var cPos = Vector3.zero;
            var cScale = Vector3.one;

            itemUIs.Add(new ItemUI()
            {
                obj = content.GetChild(length - 1),
                nextLocalPos = cPos,
                nextLocalScale = cScale
            });

            indexLeft = length - 1 - indexCard;
            indexRight = length - 2 - indexCard;

            for (int i = length - 2; i >= indexLeft; i--)
            {
                OutCardTransformParameter(Vector3.left, ref cPos, ref cScale);
                itemUIs.Add(new ItemUI()
                {
                    obj = content.GetChild(i),
                    nextLocalPos = cPos,
                    nextLocalScale = cScale
                });
            }

            cPos = Vector3.zero;
            cScale = Vector3.one;


            for (int i = indexRight; i >= 0; i--)
            {
                OutCardTransformParameter(Vector3.right, ref cPos, ref cScale);

                itemUIs.Add(new ItemUI()
                {
                    obj = content.GetChild(i),
                    nextLocalPos = cPos,
                    nextLocalScale = cScale
                });
            }

            isAnim = true; timeAnim = 0;
        }

        [SerializeField] float speed = 1;
        private void LateUpdate()
        {
            if (!isAnim) return;

            timeAnim += Time.deltaTime * speed;

            if (timeAnim >= 1)
            {
                isAnim = false;
                timeAnim = 1;
            }

            foreach(var item in itemUIs)
            {
                item.LerpLocalPos(timeAnim);
            }
        }

        void SetLocalTransform(Transform o, Vector3 pos, Vector3 scale)
        {
            o.localPosition = pos;
            o.localScale = scale;
        }

        void OutCardTransformParameter(Vector3 direction, ref Vector3 pos, ref Vector3 scale)
        {
            scale.x = Mathf.Clamp(scale.x * sizePrevious, minSizePrevious, maxSizePrevious);
            scale.y = Mathf.Clamp(scale.y * sizePrevious, minSizePrevious, maxSizePrevious);
            scale.z = Mathf.Clamp(scale.z * sizePrevious, minSizePrevious, maxSizePrevious);
            pos += direction * Mathf.Clamp(scale.x, minSizePrevious, maxSizePrevious) * space;
        }

        [SerializeField] Button buttonPrevious = default;
        [SerializeField] Button buttonNext = default;
        
        void GenerateButton()
        {
            if (buttonNext != null)
            {
                buttonNext.onClick.AddListener(() =>
                {
                    if (indexCard < content.childCount - 1)
                    {
                        SetCardIndex(indexCard + 1, true);
                    }
                });
            }
            if (buttonPrevious != null)
            {
                buttonPrevious.onClick.AddListener(() =>
                {
                    if (indexCard > 0)
                    {
                        SetCardIndex(indexCard - 1, true);
                    }
                });
            }
        }

        [SerializeField] Slider slider = default;

        void GenerateSlider()
        {
            if (slider == null) return;
            slider.wholeNumbers = true;
            slider.maxValue = content.childCount - 1;

            slider.onValueChanged.AddListener((value) =>
            {
                SetCardIndex((int)value, true);
            });
        }

        public void UpdateSlider()
        {
            if (slider == null) return;
            slider.value = indexCard;
            slider.maxValue = content.childCount - 1;
        }

#if UNITY_EDITOR
        int lastid = 0;
        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            if (content == null) return;
            if (content.childCount == 0) return;
            if (content.childCount != length)
            {
                ResetCardIndex();
                Sort();
            }
            if (content.GetChild(content.childCount - 1).GetInstanceID() != lastid)
            {
                ResetCardIndex();
                lastid = content.GetChild(content.childCount - 1).GetInstanceID();
                Sort();
            }
        }
#endif
    }

    public class ItemUI
    {
        public Transform obj = default;
        public Vector3 nextLocalPos = default;
        public Vector3 nextLocalScale = default;

        public void LerpLocalPos(float time)
        {
            if (obj == null) return;
            obj.localPosition = Vector3.Lerp(obj.localPosition, nextLocalPos, time);
            obj.localScale = Vector3.Lerp(obj.localScale, nextLocalScale, time);
        }
    }
}
