﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetroOverhaul.UI
{
    public class StyleSelectionSmallUI : MonoBehaviour
    {
        public Rect window;
        public bool showWindow;
        public bool move;
        public int style;
        public bool fence;

        private NetInfo concretePrefabSmall;
        private NetInfo concretePrefabSmallNoBar;
        private NetInfo steelPrefabSmall;
        private NetInfo steelPrefabSmallNoBar;

        public StyleSelectionSmallUI()
        {
            this.window = new Rect((float)(Screen.width - 305), (float)(Screen.height - 300), 300f, 134f);
            this.fence = true;
        }

        public void Awake()
        {
            concretePrefabSmall = PrefabCollection<NetInfo>.FindLoaded("Metro Track Ground OneLaneOneWay");
            concretePrefabSmallNoBar = PrefabCollection<NetInfo>.FindLoaded("Metro Track Ground OneLaneOneWay NoBar");
            steelPrefabSmall = PrefabCollection<NetInfo>.FindLoaded("Steel Metro Track Ground");
            steelPrefabSmallNoBar = PrefabCollection<NetInfo>.FindLoaded("Steel Metro Track Ground NoBar");
        }

        public void Update()
        {
            try
            {
                if (ToolsModifierControl.GetCurrentTool<ToolBase>() is NetTool)
                {
                    if (this.IsMetroTrack(ToolsModifierControl.GetCurrentTool<NetTool>().m_prefab))
                    {
                        this.showWindow = true;
                        this.SetNetToolPrefab();
                    }
                    else
                        this.showWindow = false;
                }
                else if (ToolsModifierControl.GetCurrentTool<ToolBase>().GetType().Name == "NetToolFine")
                {
                    if (this.IsMetroTrack((ToolsModifierControl.GetCurrentTool<ToolBase>() as NetTool).m_prefab))
                    {
                        this.showWindow = true;
                        this.SetNetToolPrefab();
                    }
                    else
                        this.showWindow = false;
                }
                else
                    this.showWindow = false;
            }
            catch
            {
            }
        }

        private void OnGUI()
        {
            if (!this.showWindow)
            {
                return;
            }
            this.window = GUI.Window(29292929, this.window, Window, "Metro Track Style");
        }

        private void Window(int id)
        {
            if (this.move)
            {
                GUI.DragWindow(new Rect(0.0f, 0.0f, 300f, 100f));
                GUI.Label(new Rect(100f, 45f, 200f, 30f), "<size=20>Move window!</size>");
            }
            else
            {
                var styleList = new List<string>();
                if (concretePrefabSmall != null)
                {
                    styleList.Add("Concrete");
                }
                if (steelPrefabSmall != null)
                {
                    styleList.Add("Steel");
                }
                this.style = GUI.Toolbar(new Rect(5f, 28f, 290f, 32f), this.style, styleList.ToArray());
                var showFenceOption = false;

                if (style == 0)
                {
                    if (concretePrefabSmall != null && concretePrefabSmallNoBar != null)
                    {
                        showFenceOption = true;
                    }
                }
                else if (style == 1)
                {
                    if (steelPrefabSmall != null && concretePrefabSmallNoBar != null)
                    {
                        showFenceOption = true;
                    }
                }
                this.fence = showFenceOption && GUI.Toggle(new Rect(5f, 65f, 140f, 30f), this.fence, "Fenced track");
                if (GUI.changed)
                    this.SetNetToolPrefab();
            }
            this.move = GUI.Toggle(new Rect(198f, 100f, 100f, 28f), this.move, "Move window");
        }

        private static bool IsSubversion(NetInfo info, NetInfo metroGroundTrack)
        {
            if (info == null)
            {
                return false;
            }
            var ai = metroGroundTrack?.m_netAI as TrainTrackAI;
            if (ai == null)
            {
                return false;
            }
            if (metroGroundTrack == info)
            {
                return true;
            }
            if (ai.m_bridgeInfo != null && ai.m_bridgeInfo == info)
            {
                return true;
            }
            if (ai.m_elevatedInfo != null && ai.m_elevatedInfo == info)
            {
                return true;
            }
            if (ai.m_slopeInfo != null && ai.m_elevatedInfo == info)
            {
                return true;
            }
            if (ai.m_tunnelInfo != null && ai.m_elevatedInfo == info)
            {
                return true;
            }
            return false;
        }

        private bool IsMetroTrack(NetInfo info)
        {
            return IsSubversion(info, concretePrefabSmall) || IsSubversion(info, concretePrefabSmallNoBar) ||
                   IsSubversion(info, steelPrefabSmall) || IsSubversion(info, steelPrefabSmallNoBar);
        }

        private void SetNetToolPrefab()
        {
            var netTool = ToolsModifierControl.SetTool<NetTool>();
            NetInfo prefab = null;
            switch (style)
            {
                case 0:
                    prefab = fence ? concretePrefabSmall : concretePrefabSmallNoBar;
                    break;
                case 1:
                    prefab = fence ? steelPrefabSmall : steelPrefabSmallNoBar;
                    break;
                default:
                    throw new Exception($"Style handling wasn't implemened! style={style}");
            }
            if (prefab != null)
            {
                netTool.m_prefab = prefab;
            }
        }

    }
}