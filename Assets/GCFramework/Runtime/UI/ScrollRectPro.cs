using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GCFramework.Utility.ObjectPool;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GCFramework.Runtime.UI
{
    [System.Serializable]
    public class ScrollRectLayout
    {
        /// <summary>
        /// 布局类型
        /// </summary>
        public enum ScrollRectLayoutType
        {
            Vertical = 0b0001, // 垂直
            Horizontal = 0b0010, // 水平
            VerticalThenHorizontal = 0b0100, // 先垂直后水平
            HorizontalThenVertical = 0b0101  // 先水平后垂直
        }
        
        public float left;
        public float right;
        public float top;
        public float bottom;
        public Vector2 spacing;
        public ScrollRectLayoutType layoutType = ScrollRectLayoutType.Vertical;
    }
    
    public class ScrollRectPro : ScrollRect
    {
        public static class CriticalItemIdx
        {
            public const int TOP_HIDE = 0;
            public const int DOWN_HIDE = 1;
            public const int TOP_SHOW = 2;
            public const int DOWN_SHOW = 3;
        }
        
        #region Inspector

        [SerializeField]
        protected ScrollRectLayout scrollRectLayout;
        [SerializeField]
        protected RectTransform itemPrototype;
        [SerializeField]
        protected Vector2 defaultItemSize = new Vector2(100, 100);
        [SerializeField]
        protected int poolSize;
        [SerializeField]
        protected int pageSize;
        [SerializeField]
        protected bool usePage = true;

        #endregion
        
        #region Veriables

        #region Event

        protected Action<int, RectTransform> updateAction;
        protected Func<int, Vector2> itemSizeSetFunc;
        protected Func<int> itemCountSetFunc;

        #endregion
        
        private readonly List<ScrollItem> _itemsList = new List<ScrollItem>();
        private int[] _criticalItemIndexes = new int[4];
        private EasyObjPool<RectTransform> _itemPool;
        private Rect _itemViewRect;
        protected bool isInitialized;
        protected bool updateScrollRect = false;
        protected bool isApplicationQuit;
        protected int realItemTotalCount;
        protected int itemTotalCount;

        private Vector3[] _viewWorldCorners = new Vector3[4];
        private Vector3[] _rectCorners = new Vector3[2];

        public Vector2 DefaultItemSize => defaultItemSize;
        public int ItemCount => itemTotalCount;
        public int RealItemCount => realItemTotalCount;

        #endregion

        #region System Methods

        protected override void Awake()
        {
            base.Awake();
            
            _lastPosition = Vector2.up;
            onValueChanged.AddListener(OnValueChange);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

#if UNITY_EDITOR
            if (EditorApplication.isPlaying && updateScrollRect)
                StartCoroutine(DelayCallInternalUpdateItemData());
#else
            if (Application.isPlaying && updateScrollRect)
                StartCoroutine(DelayCallInternalUpdateItemData());
#endif
        }

        protected override void OnDisable()
        {
            isInitialized = false;
            base.OnDisable();
        }

        private void OnApplicationQuit()
        {
            isApplicationQuit = true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (EditorApplication.isPlaying && IsActive())
            {
                UpdateScrollRect(true);
            }
        }
#endif

        #endregion

        #region Public Methods

        public void SetScrollItemEvent(Action<int, RectTransform> onUpdateAction = null
            , Func<int, Vector2> onItemSizeSetFunc = null
            , Func<int> onItemCountSetFunc = null)
        {
            updateAction = onUpdateAction;
            itemSizeSetFunc = onItemSizeSetFunc;
            itemCountSetFunc = onItemCountSetFunc;
        }

        public void UpdateScrollRect(bool immediately = false)
        {
            if (immediately)
            {
                updateScrollRect = true;
                InternalUpdateItemData();
            }
            else
            {
                if (!updateScrollRect && enabled)
                {
                    StartCoroutine(DelayCallInternalUpdateItemData());
                }

                updateScrollRect = true;
            }
        }
        
        

        #endregion
        
        #region Override

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            
            UpdateItemsShowOrHide();
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            
            UpdateCriticalItems();
        }

        #endregion

        #region Pagination（分页算法）

        private Vector2 _lastPosition;
        private int _startOffset;
        private void OnValueChange(Vector2 position)
        {
            if (!usePage)
                return;
            
            int showIdx = 0, edgeIdx = 0, offsetIdx = 0;
            bool isIncrease; // 是否递增

            Vector2 delta = position - _lastPosition;
            _lastPosition = position;
            bool isVertical = ((int)scrollRectLayout.layoutType & 1) == 1;

            if (isVertical)
            {
                if (Mathf.Approximately(delta.y, 0))
                    return;
                
                // content位置向上，y减小，idx递增
                if (delta.y < 0)
                {
                    showIdx = _criticalItemIndexes[CriticalItemIdx.DOWN_SHOW];
                    edgeIdx = pageSize - 1;
                    if (showIdx < edgeIdx)
                        return;
                    offsetIdx = edgeIdx - 1;
                    isIncrease = true;
                }
                else
                {
                    showIdx = _criticalItemIndexes[CriticalItemIdx.TOP_SHOW];
                    if (showIdx > 1)
                        return;

                    offsetIdx = 1;
                    isIncrease = false;
                }
            }
            else
            {
                if (Mathf.Approximately(delta.x, 0))
                    return;
                
                // content 向左，x减小，idx递增
                if (delta.x < 0)
                {
                    showIdx = _criticalItemIndexes[CriticalItemIdx.DOWN_SHOW];
                    edgeIdx = pageSize - 1;
                    if (showIdx < edgeIdx)
                        return;

                    offsetIdx = edgeIdx - 1;
                    isIncrease = true;
                }
                else
                {
                    showIdx = _criticalItemIndexes[CriticalItemIdx.TOP_SHOW];
                    if (showIdx > 1)
                        return;

                    offsetIdx = 1;
                    isIncrease = false;
                }
            }

            int lastStartOffset = _startOffset;
            if (isIncrease)
            {
                _startOffset += pageSize / 2;
            }
            else
            {
                _startOffset -= pageSize / 2;
            }

            _startOffset = Math.Clamp(_startOffset, 0, Math.Max(realItemTotalCount - pageSize, 0));
            
            if (lastStartOffset == _startOffset)
                return;

            Rect topRect = GetItemRectAtIndex(offsetIdx);

            Vector2 topRectWorldPos = content.TransformPoint(topRect.position);

            UpdateItemRectAtIndex(0);
            UpdateItemRectAtIndex(itemTotalCount - 1);

            int idx = offsetIdx + (lastStartOffset - _startOffset);
            Rect centerRect = GetItemRectAtIndex(idx);
            Vector2 centerWorldPos = content.TransformPoint(centerRect.position);
            Vector2 diffWorld = centerWorldPos - topRectWorldPos;
            Vector2 diffLocal = content.InverseTransformVector(diffWorld);
            Vector2 scrollVel = velocity;
            // bool positive = delta.y > 0 || delta.x > 0;
            // Vector2 vector = (content.sizeDelta / 2) * (positive ? 1 : -1);
            // if (isVertical)
            //     vector.x = 0;
            // else
            //     vector.y = 0;
            // SetContentAnchoredPosition(content.anchoredPosition - vector);
            SetContentAnchoredPosition(content.anchoredPosition - diffLocal);
            UpdateScrollRect(true);
            velocity = scrollVel;
        }

        #endregion

        #region Initialize ScrollRect

        private IEnumerator DelayCallInternalUpdateItemData()
        {
            yield return null;
            InternalUpdateItemData();
        }
        
        private void InitItemPool()
        {
            var itemRoot = new GameObject("ItemPool");
            itemRoot.SetActive(false);
            itemRoot.transform.SetParent(transform, false);

            _itemPool = new EasyObjPool<RectTransform>(
                onCreate: () =>
                {
                    var item = Instantiate(itemPrototype, itemRoot.transform, false);

                    item.anchorMin = Vector2.up;
                    item.anchorMax = Vector2.up;
                    item.pivot = Vector2.zero;

                    item.gameObject.SetActive(true);
                    return item;
                },
                onReturn: rect =>
                {
                    rect.SetParent(itemRoot.transform, false);
                },
                onRelease: rect =>
                {
                    if (isApplicationQuit)
                    {
                        rect.transform.SetParent(null, false);
                        Destroy(rect.gameObject);
                    }
                }, size: poolSize);
        }

        private void InitScrollRect()
        {
            vertical = ((int)scrollRectLayout.layoutType & 1) == 1;
            horizontal = ((int)scrollRectLayout.layoutType & 1) == 0;
            
            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;
            content.anchoredPosition = Vector2.zero;

            InitItemPool();
            SetContentRect();
            isInitialized = true;
        }
        
        private void SetContentRect()
        {
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
            
            // TODO:: ?为什么不直接用ViewRect呢？
            viewRect.sizeDelta =
                new Vector2(vertical ? viewRect.sizeDelta.x : 0, horizontal ? viewRect.sizeDelta.y : 0);
            viewRect.GetWorldCorners(_viewWorldCorners);
            _rectCorners[0] = content.transform.InverseTransformPoint(_viewWorldCorners[0]);
            _rectCorners[1] = content.transform.InverseTransformPoint(_viewWorldCorners[2]);
            _itemViewRect = new Rect((Vector2)_rectCorners[0] - content.anchoredPosition,
                _rectCorners[1] - _rectCorners[0]);
        }
        
        #endregion

        #region Core Methods

        private void InternalUpdateItemData()
        {
            if (!isInitialized)
                InitScrollRect();

            UpdateItemCountOnChange();
            UpdateCriticalItems();
            updateScrollRect = false;
        }

        private void UpdateItemCountOnChange()
        {
            int newItemCount = 0;
            if (itemCountSetFunc != null)
                newItemCount = itemCountSetFunc();
            else
                newItemCount = 100;

            if (newItemCount != _itemsList.Count)
            {
                if (newItemCount < realItemTotalCount)
                {
                    _startOffset = Math.Clamp(_startOffset, 0, Math.Max(realItemTotalCount - pageSize, 0));
                }

                pageSize += Math.Abs(newItemCount - realItemTotalCount) * 2;
            }
            
            realItemTotalCount = newItemCount;

            itemTotalCount = Math.Min(realItemTotalCount, pageSize);

            bool needUpdate = updateScrollRect;
            
            if (itemTotalCount != _itemsList.Count)
            {
                if (_itemsList.Count < itemTotalCount)
                {
                    while (_itemsList.Count < itemTotalCount)
                    {
                        _itemsList.Add(new ScrollItem());
                    }
                }
                else // 大于总数，优化空间
                {
                    for (int i = 0, count = _itemsList.Count; i < count; i++)
                    {
                        if (i < itemTotalCount)
                        {
                            if (needUpdate)
                                _itemsList[i].needUpdate = true;
                        }

                        if (i >= itemTotalCount)
                        {
                            _itemsList[i].needUpdate = true;
                            if (_itemsList[i].item != null)
                            {
                                ReturnItemToPool(_itemsList[i].item);
                                _itemsList[i].item = null;
                            }
                        }
                    }
                }
            }
            else
            {
                if (needUpdate)
                {
                    _itemsList.ForEach(item =>
                    {
                        item.needUpdate = true;
                    });
                }
            }
        }

        private void UpdateCriticalItems()
        {
            int firstItemIdx = -1, lastItemIdx = -1;
     
            for (int i = 0; i < itemTotalCount; i++)
            {
                // 判断该项是否需要显示
                bool shouldShow = ShouldItemBeShow(i);
                // 找到最前与最后项的索引

                if (shouldShow)
                {
                    if (firstItemIdx == -1)
                        firstItemIdx = i;
                    lastItemIdx = i;
                    
                    if (_itemsList[i].item == null)
                    {
                        CreateItem(i);
                    }
                    else
                    {
                        if (updateAction != null)
                        {
                            updateAction(i + _startOffset, _itemsList[i].item);
                            SetItemRectInfoAtIndex(_itemsList[i].item, i);
                        }
                    }
                }
                else
                {
                    // 当滑动快的时候，会出现item丢失，这时需要及时回收那些超出屏幕外的item
                    var item = _itemsList[i].item;
                    if (item)
                    {
                        ReturnItemToPool(item);
                        _itemsList[i].item = null;
                    }
                }
            }
            
            // 更新索引
            _criticalItemIndexes[CriticalItemIdx.TOP_HIDE] = firstItemIdx;
            _criticalItemIndexes[CriticalItemIdx.DOWN_HIDE] = lastItemIdx;
            _criticalItemIndexes[CriticalItemIdx.TOP_SHOW] = Math.Max(firstItemIdx - 1, 0); // 当上拉列表时，顶部Item需显示出来
            _criticalItemIndexes[CriticalItemIdx.DOWN_SHOW] = Math.Min(lastItemIdx + 1, itemTotalCount - 1);
        }

        private void UpdateItemsShowOrHide()
        {
            bool needUpdate = true;
            while (needUpdate)
            {
                needUpdate = false;
                for (int type = CriticalItemIdx.TOP_HIDE; type <= CriticalItemIdx.DOWN_SHOW; type++)
                {
                    if (type is CriticalItemIdx.TOP_HIDE or CriticalItemIdx.DOWN_HIDE)
                        needUpdate = needUpdate || TryHideItems(type);
                    else
                        needUpdate = needUpdate || TryShowItems(type);
                }
            }
        }

        private bool TryShowItems(int type)
        {
            int idx = _criticalItemIndexes[type];
            RectTransform item = GetCriticalItemRect(type);

            if (item == null && ShouldItemBeShow(idx))
            {
                CreateItem(idx);

                // 顶部Or左边显示，向下Or右划
                if (type == CriticalItemIdx.TOP_SHOW)
                {
                    _criticalItemIndexes[type - 2] = Math.Min(idx, _criticalItemIndexes[type - 2]);
                    _criticalItemIndexes[type]--;
                }
                else // 底部Or右边显示，向上Or右划
                {
                    _criticalItemIndexes[type - 2] = Math.Max(idx, _criticalItemIndexes[type - 2]);
                    _criticalItemIndexes[type]++;
                }

                _criticalItemIndexes[type] = Math.Clamp(_criticalItemIndexes[type], 0, itemTotalCount - 1);

                if (_criticalItemIndexes[CriticalItemIdx.TOP_SHOW] >= _criticalItemIndexes[CriticalItemIdx.DOWN_SHOW])
                {
                    UpdateCriticalItems();
                    return false;
                }
            
                return true;
            }

            return false;
        }

        
        private bool TryHideItems(int type)
        {
            int idx = _criticalItemIndexes[type];
            RectTransform item = GetCriticalItemRect(type);

            if (item != null && !ShouldItemBeShow(idx))
            {
                ReturnItemToPool(item);
                _itemsList[idx].item = null;

                // 顶部 Or 左边隐藏，向下 Or 左 划
                if (type == CriticalItemIdx.TOP_HIDE)
                {
                    _criticalItemIndexes[type + 2] = Math.Max(idx, _criticalItemIndexes[type + 2]);
                    _criticalItemIndexes[type]++;
                }
                else // 底部 Or 右边隐藏，向下 Or 右 划
                {
                    _criticalItemIndexes[type + 2] = Math.Min(idx, _criticalItemIndexes[type + 2]);
                    _criticalItemIndexes[type]--;
                }

                _criticalItemIndexes[type] = Math.Clamp(_criticalItemIndexes[type], 0, itemTotalCount - 1);

                if (_criticalItemIndexes[CriticalItemIdx.TOP_HIDE] >= _criticalItemIndexes[CriticalItemIdx.DOWN_HIDE])
                {
                    UpdateCriticalItems();
                    return false;
                }

                return true;
            }

            return false;
        }
        
        private void UpdateItemRectAtIndex(int idx)
        {
            if (!_itemsList[idx].needUpdate)
                return;

            if (_itemsList[0].needUpdate)
            {
                var firstItem = _itemsList.First();
                var firstItemSize = GetItemSize(0);
                firstItem.rect = new Rect(Vector2.zero 
                                          - Vector2.up * firstItemSize.y
                                          + Vector2.right * scrollRectLayout.left
                                            - Vector2.up * scrollRectLayout.top, firstItemSize);
                firstItem.needUpdate = false;
            }

            // 如果索引是直接更新中间位置项，我们就需要找到最近的Item，来计算出它的位置
            int nearestUpdatedIdx = 0;
            for (int i = idx; i >= 0; i--)
            {
                if (!_itemsList[i].needUpdate)
                {
                    nearestUpdatedIdx = i;
                    break;
                }
            }

            Vector2 currentPos;
            Vector2 size;
            Rect nearestUpdatedRect = _itemsList[nearestUpdatedIdx].rect;
            Vector2 nearestPos = GetLeftTopCorner(nearestUpdatedRect);
            size = nearestUpdatedRect.size;
            currentPos = CalcItemPos(nearestPos, size);
            
            // if (nearestUpdatedIdx != 0)
            // {
            //     Rect nearestUpdatedRect = _itemsList[nearestUpdatedIdx].rect;
            //     Vector2 nearestPos = GetLeftTopCorner(nearestUpdatedRect);
            //     size = nearestUpdatedRect.size;
            //     currentPos = CalcItemPos(nearestPos, size);
            // }
            // else
            // {
            //     currentPos = GetLeftTopCorner(_itemsList[nearestUpdatedIdx].rect);
            // }

            for (int i = nearestUpdatedIdx + 1; i <= idx; i++)
            {
                // 获取当前item的size
                size = GetItemSize(i);
                ScrollItem item = _itemsList[i];
                item.rect = new Rect(currentPos - Vector2.up * size.y, size);
                item.needUpdate = false;
                currentPos = CalcItemPos(currentPos, size); // 计算下一个item的
            }

            Vector2 contentSize = new Vector2(Mathf.Abs(currentPos.x), Mathf.Abs(currentPos.y));

            switch (scrollRectLayout.layoutType)
            {
                case ScrollRectLayout.ScrollRectLayoutType.VerticalThenHorizontal:
                    contentSize.x += size.x;
                    contentSize.y = _itemViewRect.height;
                    break;
                case ScrollRectLayout.ScrollRectLayoutType.HorizontalThenVertical:
                    contentSize.x = _itemViewRect.width;
                    // if (currentPos.x != 0)
                    //     contentSize.y += size.y;
                    break;
            }
            
            content.sizeDelta = contentSize;
        }

        private Vector2 CalcItemPos(Vector2 pos, Vector2 size)
        {
            float width = _itemViewRect.width - scrollRectLayout.right;
            float height = _itemViewRect.height - scrollRectLayout.bottom;
            float leftOffset = scrollRectLayout.left;
            float topOffset = scrollRectLayout.top;

            if (leftOffset < 0) leftOffset = 0;
            if (topOffset < 0) topOffset = 0;

            Vector2 itemPos = pos;
            
            switch (scrollRectLayout.layoutType)
            {
                case ScrollRectLayout.ScrollRectLayoutType.Vertical:
                    if (itemPos.y == 0)
                        itemPos.y = topOffset;
                    itemPos.y -= size.y + scrollRectLayout.spacing.y;
                    break;
                case ScrollRectLayout.ScrollRectLayoutType.Horizontal:
                    if (itemPos.x == 0)
                        itemPos.x = leftOffset;
                    itemPos.x += size.x + scrollRectLayout.spacing.x;
                    break;
                case ScrollRectLayout.ScrollRectLayoutType.VerticalThenHorizontal:
                    itemPos.y -= size.y - scrollRectLayout.spacing.y;
                    if (itemPos.x == 0)
                        itemPos.x = leftOffset;
                    if (itemPos.y - size.y - scrollRectLayout.spacing.y < -height)
                    {
                        itemPos.y = topOffset;
                        itemPos.x += size.x + scrollRectLayout.spacing.x;
                    }
                    break;
                case ScrollRectLayout.ScrollRectLayoutType.HorizontalThenVertical:
                    itemPos.x += size.x + scrollRectLayout.spacing.x;
                    if (itemPos.y == 0)
                        itemPos.y = topOffset;
                    if (itemPos.x + size.x + scrollRectLayout.spacing.x > width)
                    {
                        itemPos.x = leftOffset;
                        itemPos.y -= size.y + scrollRectLayout.spacing.y;
                    }
                    break;
            }

            return itemPos;
        }

        #endregion

        #region Get Something

        protected Rect GetItemRectAtIndex(int idx)
        {
            if (idx >= 0 && idx < itemTotalCount)
                return _itemsList[idx].rect;
            return default;
        }
        
        private RectTransform GetCriticalItemRect(int type)
        {
            int idx = _criticalItemIndexes[type];
            if (idx >= 0 && idx <= itemTotalCount - 1)
                return _itemsList[idx].item;
            return null;
        }

        private RectTransform GetNewItem(int idx)
        {
            RectTransform item = null;
            if (_itemPool != null)
            {
                item = _itemPool.Get();
                item.transform.SetParent(content, false);
                return item;
            }
            return null;
        }

        private void OnGetItemAtIndex(RectTransform item, int idx)
        {
            if (updateAction != null)
                updateAction(idx + _startOffset, item);

            SetItemRectInfoAtIndex(item, idx);
        }

        private Vector2 GetItemSize(int idx)
        {
            if (itemSizeSetFunc != null)
                return itemSizeSetFunc(idx + _startOffset);
            return defaultItemSize;
        }
        
        private Vector2 GetLeftTopCorner(Rect rect)
        {
            return rect.position + Vector2.up * rect.size.y;
        }
        
        #endregion

        #region Check Methods
        
        private bool ShouldItemBeShow(int idx)
        {
            if (idx < 0 || idx >= itemTotalCount)
            {
                return false;
            }

            UpdateItemRectAtIndex(idx);
            var itemRect = _itemsList[idx].rect;
            return new Rect(_itemViewRect.position - content.anchoredPosition, _itemViewRect.size).Overlaps(itemRect);
        }

        #endregion
        
        private void CreateItem(int idx)
        {
            RectTransform newItem = GetNewItem(idx);
            OnGetItemAtIndex(newItem, idx);
            _itemsList[idx].item = newItem;
        }
        private void ReturnItemToPool(RectTransform item)
        {
            if (_itemPool == null)
                return;

            _itemPool.Return(item);
        }
        
        private void SetItemRectInfoAtIndex(RectTransform item, int idx)
        {
            // 更新下Item的Rect
            UpdateItemRectAtIndex(idx);
            Rect rect = _itemsList[idx].rect;
            item.localPosition = rect.position;
            item.sizeDelta = rect.size;
        }
    }
    
    public class ScrollItem
    {
        public RectTransform item;

        public Rect rect;

        public bool needUpdate = true;
    }
}