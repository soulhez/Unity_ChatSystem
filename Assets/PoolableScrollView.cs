﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NodeEditorFramework;
using NodeEditorFramework.Standard;

public class PoolableScrollView : MonoBehaviour {
	public NodeItemProxy[] prefabs;
	Stack<NodeItemProxy>[] _pools;
	public OptionButton[] optionButtons;
	#if UNITY_EDITOR
	[SerializeField]
	#endif
	List<NodeItemProxy> _activeItems = new List<NodeItemProxy>();
	RectTransform viewPortTrans;
	RectTransform contextTrans;
	// Use this for initialization
	void Start()
	{
		Init ();
		ChatManager.Instance.AddFriend ("Jerry");
		ChatManager.Instance.EnterChat ("Jerry");
	}
	public void RefreshFriendsList(List<ChatInstance> chatLst)
	{
		
	}
	void Init () {
		ChatManager.Instance.OnRefresh += RefreshFriendsList;
		for (int i = 0; i < prefabs.Length; i++) {
			prefabs [i].gameObject.SetActive (false);
		}
		viewPortTrans = GetComponent<ScrollRect> ().viewport;
		contextTrans = GetComponent<ScrollRect> ().content;
		//_activeItems.Clear ();
		_pools = new Stack<NodeItemProxy>[prefabs.Length];
		for (int i = 0; i < _pools.Length; i++) {
			_pools [i] = new Stack<NodeItemProxy> ();
		}
	}
	void TryOpen()
	{
		Node front = ChatManager.Instance.curInstance.curRunningNode.GetFront ();
		if (front == null)
			return;
		NodeItemProxy item = GetItem (front.name==ChatManager.Instance.curName?1:0);
		float height = ChatManager.Instance.curInstance.saveData.totalRectHeight;
		float itemHeight = item.SetData (front);
		ChatManager.Instance.curInstance.saveData.totalRectHeight += itemHeight;
		contextTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height+itemHeight);
		item.cachedRectTransform.anchoredPosition = new Vector2 (0.0f,itemHeight-contextTrans.sizeDelta.y);
		_activeItems.Add (item);
	}
	// Update is called once per frame
	void Update () {
		CheckBorder ();
		//while (CheckBorder()) {}
	}
	bool CheckBorder()
	{
		if (_activeItems.Count == 0) {
			TryOpen ();
			return false;
		}
		if (NeedCull (_activeItems [0])) {
			PoolUp (_activeItems [0]);
			return true;
		}
		if (NeedCull (_activeItems [_activeItems.Count-1])) {
			PoolDown (_activeItems [_activeItems.Count-1]);
			return true;
		}
		if (_activeItems [0].pos.y + contextTrans.anchoredPosition.y < 0 && TryAddUp ())
			return true;
		if (_activeItems [_activeItems.Count - 1].pos.y + _activeItems [_activeItems.Count - 1].height + contextTrans.anchoredPosition.y > -viewPortTrans.sizeDelta.y && TryAddDown ())
			return true;
		return false;
	}
	bool TryAddDown()
	{
		Node down = _activeItems[_activeItems.Count-1].linkedNode.GetNext();
		if (down == null)
			return false;
		float height = ChatManager.Instance.curInstance.saveData.totalRectHeight;
		NodeItemProxy item = GetItem (down.name==ChatManager.Instance.curName?1:0);
		float itemHeight = item.SetData (down);
		float itemY = _activeItems [_activeItems.Count - 1].cachedRectTransform.anchoredPosition.y - _activeItems [_activeItems.Count - 1].height;
		if (itemY - itemHeight * 0.5f <= -ChatManager.Instance.curInstance.saveData.totalRectHeight) {
			ChatManager.Instance.curInstance.saveData.totalRectHeight += itemHeight;
			contextTrans.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, height + itemHeight);
		}
		item.cachedRectTransform.anchoredPosition = new Vector2 (0.0f,itemY);
		_activeItems.Add (item);
		return true;
	}
	bool TryAddUp ()
	{
		Node up = _activeItems[0].linkedNode.GetFront();
		if (up == null)
			return false;
		NodeItemProxy item = GetItem (up.name==ChatManager.Instance.curName?1:0);
		float itemHeight = item.SetData (up);
		float itemY = _activeItems [0].cachedRectTransform.anchoredPosition.y + itemHeight;
		item.cachedRectTransform.anchoredPosition = new Vector2 (0.0f,itemY);
		_activeItems.Insert (0,item);
		return true;
	}
	void PoolUp(NodeItemProxy node)
	{
		Pool (node);
	}
	void PoolDown(NodeItemProxy node)
	{
		Pool (node);
		contextTrans.sizeDelta = new Vector2 (contextTrans.sizeDelta.x,contextTrans.sizeDelta.y-node.height);
	}
	NodeItemProxy GetItem(int index)
	{
		if (_pools [index].Count > 0)
			return _pools [index].Pop ();
		else {
			RectTransform t = GameObject.Instantiate (prefabs [index].cachedRectTransform);
			t.SetParent (contextTrans);
			t.localScale = Vector3.one;
			t.anchoredPosition3D = Vector3.zero;
			return t.GetComponent<NodeItemProxy> ();
		}
	}
	void Pool(NodeItemProxy node)
	{
		_activeItems.Remove (node);
		node.gameObject.SetActive (false);
		_pools [node.prefabId].Push (node);
	}
	bool NeedCull(NodeItemProxy node)
	{
		if(_activeItems.IndexOf(node)==0)
		if (node.pos.y - node.height + contextTrans.anchoredPosition.y > 0)
			return true;
		if (node.pos.y + contextTrans.anchoredPosition.y < -viewPortTrans.sizeDelta.y)
			return true;
		return false;
	}
}
