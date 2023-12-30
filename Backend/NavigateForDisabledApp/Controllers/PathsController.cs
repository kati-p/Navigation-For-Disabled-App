using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavigateForDisabledApp.Models;
using System.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using NavigateForDisabledApp.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using Microsoft.VisualBasic;

namespace NavigateForDisabledApp.Controllers;

[ApiController]
[Route("[controller]")]
public class PathsController : ControllerBase
{
     private readonly ILogger<PathsController> _logger;

     public PathsController(ILogger<PathsController> logger)
     {
          _logger = logger;
     }

     [HttpPost]
     [Authorize(Roles = "user")]
     public IActionResult Post([FromBody] DTOs.PathRequest data)
     {
          uint Source;
          uint Destination;

          try
          {
               Source = uint.Parse(data.Source);
               Destination = uint.Parse(data.Destination);
          }
          catch (FormatException)
          {
               Console.WriteLine("Invalid format for uint.");
               return BadRequest(new { ErrorMessage = "Invalid format for uint." });
          }
          catch (OverflowException)
          {
               Console.WriteLine("Overflow: The value is too large for a uint.");
               return BadRequest(new { ErrorMessage = "Overflow: The value is too large for a uint." });
          }

          IActionResult result = FindAllPaths(Source, Destination);
          return result;
     }

     private IActionResult FindAllPaths(uint Source, uint Destination)
     {

          /* (AllPaths, weight) = (List<path>) , (List<weight>)
               (Path = List<PathBus> , weight)
               PathBus = Dist(BusID, List<Station_ID>) */

          List<List<Dictionary<uint, List<uint>>>> AllPaths = new List<List<Dictionary<uint, List<uint>>>>();
          List<float> WeightAllPaths = new List<float>();
          //List<Dictionary<uint, List<uint>>> Path = new List<Dictionary<uint, List<uint>>>();
          //int weight;
          //Dictionary<uint, List<uint>> PathBus = new Dictionary<uint, List<uint>>();

          // isVisited Stations by key: station_ID; value: count
          Dictionary<uint, int> VisitedStations = new Dictionary<uint, int>();

          // database connected
          var db = new NavigateSoftwareDbContext();

          // first station
          var PrevStation = Source;
          var PrevBuses = db.StationBus.Where(s => s.StationId == PrevStation).Select(s => s.BusId).ToList();
          if (!PrevBuses.Any()) return NoContent();
          var NextStations = db.StationGraphs.Where(s => s.StationId == PrevStation).Select(s => s.NearbyStationId).ToList();
          if (!NextStations.Any()) return NoContent();

          foreach (uint NextStation in NextStations)
          {

               var NextBuses = db.StationBus
               .Where(s => s.StationId == NextStation)
               .Select(s => s.BusId)
               .ToList();
               if (!PrevBuses.Any()) return NoContent();

               foreach (uint busID in PrevBuses)
               {

                    if (NextBuses.Contains(busID))
                    {

                         // this is single path
                         List<Dictionary<uint, List<uint>>> Path = new List<Dictionary<uint, List<uint>>>();
                         Dictionary<uint, List<uint>> PathBus = new Dictionary<uint, List<uint>>();
                         float weight = db.StationGraphs
                         .Where(s =>
                              s.StationId == PrevStation
                              && s.NearbyStationId == NextStation)
                         .Select(s => s.Weight)
                         .FirstOrDefault();
                         PathBus.Add(busID, new List<uint>() { PrevStation, NextStation });

                         if (VisitedStations.ContainsKey(PrevStation))
                         {
                              VisitedStations[PrevStation] = VisitedStations[PrevStation] + 1;
                         }
                         else
                         {
                              VisitedStations.Add(PrevStation, 1);
                         }
                         if (VisitedStations.ContainsKey(NextStation))
                         {
                              VisitedStations[NextStation] = VisitedStations[NextStation] + 1;
                         }
                         else
                         {
                              VisitedStations.Add(NextStation, 1);
                         }

                         if (NextStation == Destination)
                         {
                              Path.Add(PathBus);
                              AllPaths.Add(Path);
                              WeightAllPaths.Add(weight);
                         }
                         else
                         {
                              IActionResult result = FindPathByDFS(Destination, NextStation, busID, AllPaths, Path, PathBus, VisitedStations, WeightAllPaths, weight);
                              HttpStatusCode statusCode = (HttpStatusCode)result.GetType().GetProperty("StatusCode").GetValue(result, null);
                              if (!statusCode.Equals(HttpStatusCode.OK)) return result;
                         }

                    }

               }
          }


          // Pruning All Path that the least weight and the least number of buses
          // is empty ?
          if (!AllPaths.Any()) return NotFound();

          // the least weight
          int leastWeightIndex = 0;
          for (int i = 1; i < WeightAllPaths.Count; i++)
          {
               if (WeightAllPaths[leastWeightIndex] > WeightAllPaths[i])
               {
                    leastWeightIndex = i;
               }
          }
          List<Dictionary<uint, List<uint>>> pathLeastWeight = AllPaths[leastWeightIndex];
          float leastWeight = WeightAllPaths[leastWeightIndex];

          // the least number of buses
          int leastNumberOfBusesIndex = 0;
          for (int i = 1; i < AllPaths.Count; i++)
          {
               if (AllPaths[leastNumberOfBusesIndex].Count > AllPaths[i].Count)
               {
                    leastNumberOfBusesIndex = i;
               }
          }
          List<Dictionary<uint, List<uint>>> pathLeastNumberOfBuses = AllPaths[leastNumberOfBusesIndex];
          float leastNumberOfBusesWeight = WeightAllPaths[leastNumberOfBusesIndex];

          // All Paths to Json All paths
          PathResponse pathResponse = new PathResponse();
          DTOs.Path[] paths;
          if (leastNumberOfBusesWeight == leastWeight)
          {
               paths = new DTOs.Path[1];
          }
          else
          {
               paths = new DTOs.Path[2];
          }
          // Path 1
          DTOs.BusPath[] busPaths1 = new DTOs.BusPath[pathLeastWeight.Count];
          for (int p = 0; p < pathLeastWeight.Count; p++)
          {

               Dictionary<uint, List<uint>> pathBus = pathLeastWeight[p];

               // foreach but single value
               foreach (var pb in pathBus)
               {
                    string busName = db.Buses
                    .Where(b => b.BusId == pb.Key)
                    .Select(b => b.BusName)
                    .First();
                    List<uint> stationValue = pb.Value;
                    string[] stations = new string[stationValue.Count];
                    for (int i = 0; i < stationValue.Count; i++)
                    {
                         stations[i] = db.Stations
                              .Where(s => s.StationId == stationValue[i])
                              .Select(s => s.StationName)
                              .First();
                    }
                    BusPath BusPath = new BusPath
                    {
                         bus = busName,
                         stations = stations
                    };
                    busPaths1[p] = BusPath;
               }
          }
          paths[0] = new DTOs.Path
          {
               path = busPaths1,
               weight = leastWeight
          };

          if (leastNumberOfBusesWeight != leastWeight)
          {
               // Path 2
               DTOs.BusPath[] busPaths2 = new DTOs.BusPath[pathLeastNumberOfBuses.Count];
               for (int p = 0; p < pathLeastNumberOfBuses.Count; p++)
               {

                    Dictionary<uint, List<uint>> pathBus = pathLeastNumberOfBuses[p];

                    // foreach but single value
                    foreach (var pb in pathBus)
                    {
                         string busName = db.Buses
                         .Where(b => b.BusId == pb.Key)
                         .Select(b => b.BusName)
                         .First();
                         List<uint> stationValue = pb.Value;
                         string[] stations = new string[stationValue.Count];
                         for (int i = 0; i < stationValue.Count; i++)
                         {
                              stations[i] = db.Stations
                                   .Where(s => s.StationId == stationValue[i])
                                   .Select(s => s.StationName)
                                   .First();
                         }
                         BusPath BusPath = new BusPath
                         {
                              bus = busName,
                              stations = stations
                         };
                         busPaths2[p] = BusPath;
                    }
               }
               paths[1] = new DTOs.Path
               {
                    path = busPaths2,
                    weight = leastNumberOfBusesWeight
               };
          }
          pathResponse.Paths = paths;
          return Ok(pathResponse);
     }

     // use DFS to find path and backtracking
     // by use this method can do recursive function to DFS
     private IActionResult FindPathByDFS(uint Destination, uint CurrentStation, uint CurrentBus
                         , List<List<Dictionary<uint, List<uint>>>> AllPaths
                         , List<Dictionary<uint, List<uint>>> Path
                         , Dictionary<uint, List<uint>> PathBus
                         , Dictionary<uint, int> VisitedStations
                         , List<float> WeightAllPaths
                         , float weight)
     {
          // database connected
          var db = new NavigateSoftwareDbContext();

          // 1. All Edge
          // 2. current busID in path bus
          // 3. contains in next station, add that station to pathBus
          // 3.1 if next VisitedStations' count is equal number of bus, so that station is visited.
          // 4. not contains in next station all buses are create new pathBus and recursive this method
          // 4.1 if find bus that passed throw this path 

          // if find destination add the path to All paths
          // if not find edge (next edge are contain in path) , do nothing. it won't and to all paths

          // if find destination add the path to All paths
          if (CurrentStation == Destination)
          {
               Path.Add(PathBus);
               AllPaths.Add(Path);
               WeightAllPaths.Add(weight);
               return Ok();
          }

          var NextStations = db.StationGraphs
          .Where(s => s.StationId == CurrentStation)
          .Select(s => s.NearbyStationId)
          .ToList();
          if (!NextStations.Any()) return NoContent();

          //1.
          //Console.WriteLine(1);
          foreach (uint NextStation in NextStations)
          {

               //Console.WriteLine(2);
               // 2. 
               //CurrentBus
               var NextBuses = db.StationBus
               .Where(s => s.StationId == NextStation)
               .Select(s => s.BusId)
               .ToList();

               // weight Current to Next station
               float newWeight = db.StationGraphs
                         .Where(s =>
                              s.StationId == CurrentBus
                              && s.NearbyStationId == NextStation)
                         .Select(s => s.Weight)
                         .FirstOrDefault();

               // 3.1
               //Console.WriteLine("3.1");
               if (VisitedStations.ContainsKey(NextStation)
               && VisitedStations[NextStation] >= NextBuses.Count)
               {
                    return Ok();
               }

               // clone path to protect shallow copy

               // clone path
               List<Dictionary<uint, List<uint>>> NewPath = new List<Dictionary<uint, List<uint>>>();

               foreach (Dictionary<uint, List<uint>> p in Path)
               {
                    Dictionary<uint, List<uint>> ClonePathBus = new Dictionary<uint, List<uint>>();
                    foreach (var pb in p)
                    {
                         List<uint> newStations = new List<uint>();
                         List<uint> stations = pb.Value;
                         foreach (uint s in stations)
                         {
                              newStations.Add(s);
                         }

                         uint newBus = pb.Key;
                         ClonePathBus.Add(newBus, newStations);
                    }
                    NewPath.Add(ClonePathBus);
               }

               // clone PathBus
               Dictionary<uint, List<uint>> newPathBus = new Dictionary<uint, List<uint>>();
               foreach (var pb in PathBus)
               {
                    List<uint> newStations = new List<uint>();
                    List<uint> stations = pb.Value;
                    foreach (uint s in stations)
                    {
                         newStations.Add(s);
                    }

                    uint newBus = pb.Key;
                    newPathBus.Add(newBus, newStations);
               }

               float newPathWeight = weight + newWeight;
               uint newDestination = Destination;
               uint newCurrentStation = CurrentStation;
               uint newCurrentBus = CurrentBus;
               // end clone

               if (NextBuses.Contains(newCurrentBus))
               {
                    //Console.WriteLine(3);
                    // 3.
                    newPathBus[newCurrentBus].Add(NextStation);
                    if (VisitedStations.ContainsKey(NextStation))
                    {
                         VisitedStations[NextStation] = VisitedStations[NextStation] + 1;
                    }
                    else
                    {
                         VisitedStations.Add(NextStation, 1);
                    }
                    FindPathByDFS(Destination, NextStation, newCurrentBus, AllPaths, NewPath, newPathBus, VisitedStations, WeightAllPaths, newPathWeight);
               }
               else
               {
                    //Console.WriteLine(4);
                    // 4.
                    var PrevBuses = db.StationBus
                    .Where(s => s.StationId == newCurrentStation)
                    .Select(s => s.BusId)
                    .ToList();
                    foreach (uint busID in PrevBuses)
                    {

                         if (busID == newCurrentBus) continue; // same bus
                         //Console.WriteLine(4.1);
                         // 4.1
                         foreach (Dictionary<uint, List<uint>> p in NewPath)
                         {
                              if (p.ContainsKey(busID))
                              {
                                   return Ok();
                              }
                         }

                         if (NextBuses.Contains(busID))
                         {
                              NewPath.Add(newPathBus);

                              if (VisitedStations.ContainsKey(NextStation))
                              {
                                   VisitedStations[NextStation] = VisitedStations[NextStation] + 1;
                              }
                              else
                              {
                                   VisitedStations.Add(NextStation, 1);
                              }

                              // create new busPath
                              Dictionary<uint, List<uint>> NewPathBus = new Dictionary<uint, List<uint>>();
                              NewPathBus.Add(busID, new List<uint>() { newCurrentStation, NextStation });


                              FindPathByDFS(Destination, NextStation, busID, AllPaths, NewPath, NewPathBus, VisitedStations, WeightAllPaths, newPathWeight);

                         }
                    }
               }
          }

          return Ok();

     }

}


