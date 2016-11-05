﻿using MetroOverhaul.Redirection;

namespace MetroOverhaul.Detours
{
    [TargetType(typeof(MetroTrainAI))]
    public class MetroTrainAIDetour : PassengerTrainAI
    {
        [RedirectMethod]
        public override void CreateVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.CreateVehicle(vehicleID, ref data);
            //begin mod
            if (vehicleID > 0)
            {
                var position = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_frame0.m_position;
                if (position.y < TerrainManager.instance.SampleDetailHeightSmooth(position.x, position.z)) //how about sunken stations?
                {
                    data.m_flags |= Vehicle.Flags.Underground;
                }
            }
            //end mod
        }

        [RedirectMethod]
        public override void LoadVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.LoadVehicle(vehicleID, ref data);
            //begin mod
            if (vehicleID > 0)
            {
                var position = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_frame0.m_position;
                if (position.y < TerrainManager.instance.SampleDetailHeightSmooth(position.x, position.z)) //how about sunken stations?
                {
                    data.m_flags |= Vehicle.Flags.Underground;
                }
            }
            //end mod
        }
    }
}