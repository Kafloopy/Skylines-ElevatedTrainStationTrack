﻿using System.Collections.Generic;
using System.Linq;
using MetroOverhaul.Extensions;
using MetroOverhaul.NEXT;
using UnityEngine;

namespace MetroOverhaul
{
    public class AssetsUpdater
    {
        public void UpdateExistingAssets()
        {
            UpdateVanillaMetroStation();
            UpdateTrainTracks();

            UpdateMetroStations();
            UpdateMetroTrainEffects();
        }

        private static void UpdateTrainTracks()
        {
            var vanillaTracksNames = new[] { "Train Track", "Train Track Elevated", "Train Track Bridge", "Train Track Slope", "Train Track Tunnel" };
            var vanillaTracksCosts = vanillaTracksNames.ToDictionary(Initializer.DetectVersion, GetTrackCost);
            var toGroundMultipliers = vanillaTracksCosts.ToDictionary(keyValue => keyValue.Key,
                keyValue => keyValue.Value == vanillaTracksCosts[NetInfoVersion.Ground] ? 1f : keyValue.Value / (float)vanillaTracksCosts[NetInfoVersion.Ground]);

            var baseMultiplier = GetTrackCost("Metro Track Ground") / (float)GetTrackCost("Train Track");
            for (ushort i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                var netInfo = PrefabCollection<NetInfo>.GetLoaded(i);
                var ai = netInfo?.m_netAI as PlayerNetAI;
                if (ai == null || netInfo.m_class.m_service != ItemClass.Service.PublicTransport || netInfo.m_class.m_subService != ItemClass.SubService.PublicTransportTrain)
                {
                    continue;
                }
                var version = Initializer.DetectVersion(netInfo.name);
                var wasCost = GetTrackCost(netInfo);
                if (wasCost == 0)
                {
                    continue;
                }
                var newCost = wasCost / toGroundMultipliers[version] *
                                     Initializer.GetCostMultiplier(version) * GetAdditionalCostMultiplier(version) * baseMultiplier;
                UnityEngine.Debug.Log($"Updating asset {netInfo.name} cost. Was cost: {wasCost}. New cost: {newCost}");
                ai.m_constructionCost = (int)newCost;
                ai.m_maintenanceCost = (int)(newCost / 10f);
            }
        }

        private static float GetAdditionalCostMultiplier(NetInfoVersion version)
        {
            return (version == NetInfoVersion.Tunnel || version == NetInfoVersion.Slope || version == NetInfoVersion.Elevated || version == NetInfoVersion.Bridge) ? 1.5f : 1.0f;
        }

        private static int GetTrackCost(string prefabName)
        {
            var netInfo = PrefabCollection<NetInfo>.FindLoaded(prefabName);
            return GetTrackCost(netInfo);
        }

        private static int GetTrackCost(NetInfo netInfo)
        {
            return ((PlayerNetAI)netInfo.m_netAI).m_constructionCost;
        }

        private static void UpdateVanillaMetroStation()
        {
            var vanillaMetroStation = PrefabCollection<BuildingInfo>.FindLoaded("Metro Entrance");
            foreach (var path in vanillaMetroStation.m_paths)
            {
                if (path == null || path.m_netInfo == null)
                {
                    continue;
                }
                if (path.m_netInfo.IsUndergroundMetroStationTrack())
                {
                    path.m_netInfo = PrefabCollection<NetInfo>.FindLoaded("Metro Station Track Tunnel");
                }
            }

        }

        private static void UpdateMetroStations()
        {
            var vanillaMetroStation = PrefabCollection<BuildingInfo>.FindLoaded("Metro Entrance");

            foreach (var info in Resources.FindObjectsOfTypeAll<BuildingInfo>())
            {
                if (!info.IsMetroDepot())
                {
                    continue;
                }
                if (info.m_buildingAI.GetType() != typeof(DepotAI))
                {
                    var transportStationAi = (TransportStationAI)info.m_buildingAI;
                    transportStationAi.m_maxVehicleCount = 0;
                }

                info.m_UnlockMilestone = vanillaMetroStation.m_UnlockMilestone;
                ((DepotAI)info.m_buildingAI).m_createPassMilestone = ((DepotAI)vanillaMetroStation.m_buildingAI).m_createPassMilestone;
            }
        }

        //this method is supposed to be called before level loading
        public static void PreventVanillaMetroTrainSpawning()
        {
            var metro = PrefabCollection<VehicleInfo>.FindLoaded("Metro");
            metro.m_class = ScriptableObject.CreateInstance<ItemClass>();
        }

        private static void UpdateMetroTrainEffects()
        {
            var vanillaMetro = PrefabCollection<VehicleInfo>.FindLoaded("Metro");
            var arriveEffect = ((MetroTrainAI)vanillaMetro.m_vehicleAI).m_arriveEffect;
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.LoadedCount(); i++)
            {
                var info = PrefabCollection<VehicleInfo>.GetLoaded(i);
                var metroTrainAI = info?.m_vehicleAI as MetroTrainAI;
                if (metroTrainAI == null)
                {
                    continue;
                }
                info.m_effects = vanillaMetro.m_effects;
                metroTrainAI.m_arriveEffect = arriveEffect;
            }
        }
    }
}