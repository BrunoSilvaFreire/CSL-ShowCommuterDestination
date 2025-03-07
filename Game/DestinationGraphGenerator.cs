﻿using ColossalFramework;
using CSLShowCommuterDestination.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSLShowCommuterDestination.Game
{
    /// <summary>
    /// This class is responsible for generating a {@link DestinationGraph} for a given stop.
    /// </summary>
    public class DestinationGraphGenerator
    {
        /// <summary>
        /// Generate a {@link DestinationGraph} for a given stop.
        /// </summary>
        /// <remarks>This logic is taken from LoadPassengers in the game logic (e.g. in BusAI)</remarks>
        /// <param name="stopId">the origin stop</param>
        /// <returns>the generated graph</returns>
        public static DestinationGraph GenerateGraph(ushort stopId)
        {            
            var transitRange = GetStopRange(stopId);
            
            Vector3 stopPosition = Bridge.GetStopPosition(stopId);

            int LOWER_BOUND_X = Mathf.Max((int)((stopPosition.x - transitRange) / 8.0 + 1080.0), 0);
            int UPPER_BOUND_X = Mathf.Min((int)((stopPosition.x + transitRange) / 8.0 + 1080.0), 2159);
            int LOWER_BOUND_Z = Mathf.Max((int)((stopPosition.z - transitRange) / 8.0 + 1080.0), 0);
            int UPPER_BOUND_Z = Mathf.Min((int)((stopPosition.z + transitRange) / 8.0 + 1080.0), 2159);

            // lookup stops by id for improved performance
            Dictionary<ushort, DestinationGraphStop> stops = new Dictionary<ushort, DestinationGraphStop>();

            for (int z = LOWER_BOUND_Z; z <= UPPER_BOUND_Z; ++z)
            {
                for (int x = LOWER_BOUND_X; x <= UPPER_BOUND_X; ++x)
                {
                    ushort citizenInstanceId = Singleton<CitizenManager>.instance.m_citizenGrid[z * 2160 + x];
                    while (citizenInstanceId != (ushort)0)
                    {
                        CitizenInstance citizen = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)citizenInstanceId];

                        ushort nextGridInstance = citizen.m_nextGridInstance;

                        var citizenIsAtStop = IsCitizenAtStop(ref citizen, citizenInstanceId, stopId, transitRange);

                        if (citizenIsAtStop)
                        {
                            var destinationStopId = Bridge.GetDestinationStopId(stopId, citizen);

                            if (!stops.ContainsKey(destinationStopId))
                            {
                                stops.Add(
                                    destinationStopId,
                                    new DestinationGraphStop(destinationStopId)
                                );
                            }

                            stops[destinationStopId].AddJourney(citizen.m_targetBuilding);
                        }

                        citizenInstanceId = nextGridInstance;
                    }
                }
            }

            return new DestinationGraph(stops.Select(j => j.Value));
        }

        /// <summary>
        /// Check if a given citizen is waiting at a given stop.
        /// </summary>
        /// <param name="citizen">the citizen to check</param>
        /// <param name="citizenInstanceId">the citizen's instance ID</param>
        /// <param name="stopId">the stop to check</param>
        /// <param name="stopRange">the range of the stop</param>
        /// <returns>`true` if the citizen is waiting at the stop, false otherwise</returns>
        private static bool IsCitizenAtStop(ref CitizenInstance citizen, ushort citizenInstanceId, ushort stopId, float stopRange)
        {
            var citizenIsAtStop = Bridge.IsCitizenInRangeOfStop(citizenInstanceId, stopId, (double)stopRange);

            // If the citizen is not within range, no need to check further
            if (!citizenIsAtStop)
            {
                return false;
            }

            ushort nextStop = TransportLine.GetNextStop(stopId);
            Vector3 stopPosition = Bridge.GetStopPosition(stopId);
            Vector3 nextStopPosition = Bridge.GetStopPosition(nextStop);

            return (
                (citizen.m_flags & CitizenInstance.Flags.WaitingTransport) != CitizenInstance.Flags.None
                && citizen.Info.m_citizenAI.TransportArriveAtSource(citizenInstanceId, ref citizen, stopPosition, nextStopPosition)
            );
        }

        /// <summary>
        /// Get the transit range of a given stop.
        /// </summary>
        /// <remarks>
        /// These values are taken from the LoadPassengers game methods.<br/>
        /// TODO should this live in Bridge instead?
        /// </remarks>
        /// <param name="stopId">the stop to get the transit range for</param>
        /// <returns>the transit range</returns>
        private static float GetStopRange(ushort stopId)
        {
            // TODO take transit type into account
            
            // this should be 32 for buses, cable cars, 
            //          64 for trains, ships, blimps, ferries, helicopters, planes, trams, trolleybuses
            return 64f;
        }
    }
}