using System;
using System.Collections.Generic;
using SFS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace SFS.UI
{
	public class TabScrollElement : MonoBehaviour
	{
		public class MoveCommand
		{
			public bool issuedByVelocityThreshold;

			public bool issuedByDistanceThreshold;

			public int velocityTarget;

			public int distanceTarget;

			private int maxTarget;

			public MoveCommand(int maxTarget)
			{
				this.maxTarget = maxTarget;
			}

			public void NextTabD()
			{
				if (!issuedByDistanceThreshold)
				{
					issuedByDistanceThreshold = true;
					distanceTarget = 1;
				}
			}

			public void PreviousTabD()
			{
				if (!issuedByDistanceThreshold)
				{
					issuedByDistanceThreshold = true;
					distanceTarget = -1;
				}
			}

			public void NextTabV()
			{
				if (!issuedByVelocityThreshold)
				{
					issuedByVelocityThreshold = true;
					velocityTarget = 1;
				}
			}

			public void PreviousTabV()
			{
				if (!issuedByVelocityThreshold)
				{
					issuedByVelocityThreshold = true;
					velocityTarget = -1;
				}
			}

			public int GetTarget(int current, bool debug = false)
			{
				int num = 0;
				return Math.Max(0, Math.Min(val2: current + Math.Sign((!issuedByDistanceThreshold || velocityTarget == distanceTarget) ? (num + (distanceTarget + velocityTarget)) : (num + distanceTarget)), val1: maxTarget));
			}
		}

		public Button dragSource;

		public GridLayoutGroup contentHolder;

		public bool sizeToParent;

		public float deceleration;

		public float rubberbandingDistance = 10f;

		public float rubberbandingTime = 0.2f;

		public float percentThreshold = 0.2f;

		public float velocityThreshold = 400f;

		public int initialTab;

		private List<Transform> elementsToIgnore = new List<Transform>();

		public (int, RectTransform) targetTab;

		[HideInInspector]
		public RectTransform contentRect;

		[HideInInspector]
		public bool holding;

		public float velocity;

		private RectTransform parentRect;

		private MoveCommand moveCommand;

		public RectTransform ParentRect
		{
			get
			{
				if (parentRect == null)
				{
					parentRect = base.gameObject.Rect();
				}
				return parentRect;
			}
		}

		private void Start()
		{
			if (contentRect == null)
			{
				contentRect = contentHolder.GetComponent<RectTransform>();
			}
			dragSource.onDown += new Action<OnInputStartData>(OnTouchStart);
			dragSource.onHold += new Action<OnInputStayData>(OnTouchStay);
			dragSource.onUp += new Action<OnInputEndData>(OnTouchEnd);
			if (sizeToParent)
			{
				SizeCellsToParent();
			}
			targetTab = GetTab(initialTab);
		}

		private void Update()
		{
			if (sizeToParent)
			{
				SizeCellsToParent();
			}
			if (!holding)
			{
				contentRect.localPosition = new Vector3(Rubberbanding.SmoothDamp(contentRect.localPosition.x, 0f - targetTab.Item2.localPosition.x, ref velocity, rubberbandingTime, Time.unscaledDeltaTime), 0f);
			}
		}

		private void SizeCellsToParent()
		{
			Rect rect = base.gameObject.Rect().rect;
			contentHolder.cellSize = new Vector2(rect.width, rect.height);
		}

		private List<(int, RectTransform)> GetTabs()
		{
			List<(int, RectTransform)> list = new List<(int, RectTransform)>();
			RectTransform rectTransform = null;
			for (int i = 0; i < contentRect.childCount; i++)
			{
				Transform child = contentRect.GetChild(i);
				if (!elementsToIgnore.Contains(child) && child.gameObject.activeInHierarchy)
				{
					RectTransform rectTransform2 = contentRect.GetChild(i).Rect();
					if (!(rectTransform2 == null) && (!(rectTransform != null) || rectTransform2.GetLocalPosition().x != rectTransform.GetLocalPosition().x))
					{
						rectTransform = rectTransform2;
						list.Add((list.Count, rectTransform2));
					}
				}
			}
			return list;
		}

		private (int, RectTransform) GetTab(int index)
		{
			return GetTabs()[index];
		}

		public (int, RectTransform) GetClosestTab(Func<(int, RectTransform), bool> filter = null)
		{
			return GetClosestTab(contentRect.GetLocalPosition().x);
		}

		public (int, RectTransform) GetClosestTab(float x, Func<(int, RectTransform), bool> filter = null)
		{
			(int, RectTransform) result = (-1, null);
			float num = float.MaxValue;
			foreach (var tab in GetTabs())
			{
				float num2 = Mathf.Abs(tab.Item2.GetLocalPosition().x + x);
				if ((filter == null || filter(tab)) && num2 < num)
				{
					result = tab;
					num = num2;
				}
			}
			return result;
		}

		private void OnTouchStart(OnInputStartData data)
		{
			holding = true;
			moveCommand = new MoveCommand(GetTabs().Count - 1);
		}

		private void OnTouchStay(OnInputStayData data)
		{
			float x = contentRect.localPosition.x + data.delta.deltaPixel.x;
			contentRect.localPosition = new Vector3(ClampWidthCoordinate(x), 0f);
			velocity = data.delta.deltaPixel.x / Time.unscaledDeltaTime;
		}

		private void OnTouchEnd(OnInputEndData data)
		{
			holding = false;
			(int, RectTransform) closestTab = GetClosestTab();
			float num = 0f - contentRect.GetLocalPosition().x - closestTab.Item2.GetLocalPosition().x;
			if (closestTab.Item1 != targetTab.Item1)
			{
				if ((double)percentThreshold <= 0.5 && !moveCommand.issuedByVelocityThreshold)
				{
					if (closestTab.Item1 > targetTab.Item1)
					{
						moveCommand.NextTabD();
					}
					else
					{
						moveCommand.PreviousTabD();
					}
				}
			}
			else
			{
				float num2 = num / contentHolder.cellSize.x;
				if (num2 > percentThreshold)
				{
					moveCommand.NextTabD();
				}
				else if (num2 < 0f - percentThreshold)
				{
					moveCommand.PreviousTabD();
				}
			}
			if (Math.Abs(velocity) > velocityThreshold)
			{
				if (velocity > 0f)
				{
					if (closestTab.Item1 < targetTab.Item1)
					{
						moveCommand.PreviousTabV();
					}
					else if (closestTab.Item1 == targetTab.Item1)
					{
						moveCommand.PreviousTabV();
					}
				}
				else if (closestTab.Item1 >= targetTab.Item1)
				{
					moveCommand.NextTabV();
				}
			}
			(int, RectTransform) tab = GetTab(moveCommand.GetTarget(targetTab.Item1, debug: true));
			targetTab = tab;
		}

		private float ClampWidthCoordinate(float x)
		{
			float min = contentRect.rect.width * (contentRect.pivot.x - 1f) + ParentRect.rect.width * ParentRect.pivot.x - rubberbandingDistance;
			float max = contentRect.rect.width * contentRect.pivot.x - ParentRect.rect.width * ParentRect.pivot.x + rubberbandingDistance;
			return Mathf.Clamp(x, min, max);
		}
	}
}
