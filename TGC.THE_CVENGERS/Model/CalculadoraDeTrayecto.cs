﻿using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;

namespace TGC.Group.Model
{
    internal class CalculadoraDeTrayecto
    {
        public Vector3 StartLocation { get; set; }
        public Vector3 EndLocation { get; set; }
        public TgcScene map { get; set; }
        public List<Objeto> objetosMapa { get; set; }
        public TgcSkeletalMesh personaje { get; set; }
        public bool[,] mapBool = new bool[1000, 1000];
        private bool flag = false;
        private static List<PuntoMapa> puntosCerrados = new List<PuntoMapa>();

        public void analizarPuntosPared()
        {
            List<TgcMesh> meshes;
            meshes = map.Meshes;

            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    personaje.Position = new Vector3(i, 5, j);

                    flag = false;

                    foreach (TgcMesh mesh in meshes)
                    {
                        if (TgcCollisionUtils.testAABBAABB(personaje.BoundingBox, mesh.BoundingBox))
                        {
                            mapBool[i, j] = false;
                            flag = true;
                            break;
                        }
                    }

                    foreach (Objeto obj in objetosMapa)
                    {
                        if (TgcCollisionUtils.testAABBAABB(personaje.BoundingBox, obj.getMesh().BoundingBox))
                        {
                            mapBool[i, j] = false;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        mapBool[i, j] = true;
                    }
                }
            }
        }

        private int width;
        private int height;
        public PuntoMapa[,] nodes { get; set; }
        private PuntoMapa startNode;
        private PuntoMapa endNode;
        private SearchParameters searchParameters;

        /// <summary>
        /// Create a new instance of PathFinder
        /// </summary>
        /// <param name="searchParameters"></param>
        public CalculadoraDeTrayecto(SearchParameters searchParameters, PuntoMapa[,] nodess)
        {
            this.searchParameters = searchParameters;
            this.nodes = nodess;
            this.width = nodes.GetLength(0);
            this.height = nodes.GetLength(1);
            this.startNode = this.nodes[searchParameters.StartLocation.X, searchParameters.StartLocation.Y];
            this.startNode.State = EstadoPunto.Open;
            this.endNode = this.nodes[searchParameters.EndLocation.X, searchParameters.EndLocation.Y];
        }

        public CalculadoraDeTrayecto()
        {
        }

        public static void resetearNodos()
        {
            foreach (PuntoMapa puntoUsado in puntosCerrados)
            {
                puntoUsado.State = EstadoPunto.Untested;
                puntoUsado.G = 0;
                puntoUsado.H = 0;

                puntoUsado.ParentNode = null;
            }
            puntosCerrados.Clear();
        }

        /// <summary>
        /// Attempts to find a path from the start location to the end location based on the supplied SearchParameters
        /// </summary>
        /// <returns>A List of Points representing the path. If no path was found, the returned list is empty.</returns>
        public List<Point> FindPath(Point endLocation)
        {
            // The start node is the first entry in the 'open' list
            List<Point> path = new List<Point>();
            bool success = Search(startNode, endLocation);
            if (success)
            {
                // If a path was found, follow the parents from the end node to build a list of locations
                PuntoMapa node = this.endNode;
                while (node.ParentNode != null)
                {
                    path.Add(node.Location);
                    node = node.ParentNode;
                }

                // Reverse the list so it's in the correct order when returned
                path.Reverse();
            }

            resetearNodos();
            return path;
        }

        /// <summary>
        /// Builds the node grid from a simple grid of booleans indicating areas which are and aren't walkable
        /// </summary>
        /// <param name="map">A boolean representation of a grid in which true = walkable and false = not walkable</param>
        public void InitializeNodes(bool[,] map)
        {
            this.width = map.GetLength(0);
            this.height = map.GetLength(1);
            this.nodes = new PuntoMapa[this.width, this.height];
            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    this.nodes[x, y] = new PuntoMapa(x, y, map[x, y]);
                }
            }
        }

        /// <summary>
        /// Attempts to find a path to the destination node using <paramref name="currentNode"/> as the starting location
        /// </summary>
        /// <param name="currentNode">The node from which to find a path</param>
        /// <returns>True if a path to the destination has been found, otherwise false</returns>
        private bool Search(PuntoMapa currentNode, Point endLocation)
        {
            // Set the current node to Closed since it cannot be traversed more than once
            currentNode.State = EstadoPunto.Closed;
            puntosCerrados.Add(currentNode);
            List<PuntoMapa> nextNodes = GetAdjacentWalkableNodes(currentNode);

            foreach (PuntoMapa punti in nextNodes)
            {
                punti.H = PuntoMapa.GetTraversalCost(punti.Location, endLocation);
            }

            // Sort by F-value so that the shortest possible routes are considered first
            nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
            foreach (var nextNode in nextNodes)
            {
                // Check whether the end node has been reached
                if (nextNode.Location == this.endNode.Location)
                {
                    return true;
                }
                else
                {
                    // If not, check the next set of nodes
                    if (Search(nextNode, endLocation)) // Note: Recurses back into Search(Node)
                        nextNodes.Clear();
                    return true;
                }
            }
            nextNodes.Clear();
            // The method returns false if this path leads to be a dead end
            return false;
        }

        /// <summary>
        /// Returns any nodes that are adjacent to <paramref name="fromNode"/> and may be considered to form the next step in the path
        /// </summary>
        /// <param name="fromNode">The node from which to return the next possible nodes in the path</param>
        /// <returns>A list of next possible nodes in the path</returns>
        private List<PuntoMapa> GetAdjacentWalkableNodes(PuntoMapa fromNode)
        {
            List<PuntoMapa> walkableNodes = new List<PuntoMapa>();
            IEnumerable<Point> nextLocations = GetAdjacentLocations(fromNode.Location);

            foreach (var location in nextLocations)
            {
                int x = location.X;
                int y = location.Y;

                // Stay within the grid's boundaries
                if (x < 0 || x >= this.width || y < 0 || y >= this.height)
                    continue;

                PuntoMapa node = this.nodes[x, y];

                // Ignore non-walkable nodes
                if (!node.IsWalkable)
                    continue;

                // Ignore already-closed nodes
                if (node.State == EstadoPunto.Closed)
                    continue;

                // Already-open nodes are only added to the list if their G-value is lower going via this route.
                if (node.State == EstadoPunto.Open)
                {
                    float traversalCost = PuntoMapa.GetTraversalCost(node.Location, node.ParentNode.Location);
                    float gTemp = fromNode.G + traversalCost;
                    if (gTemp < node.G)
                    {
                        node.ParentNode = fromNode;
                        walkableNodes.Add(node);
                        puntosCerrados.Add(node);
                    }
                }
                else
                {
                    // If it's untested, set the parent and flag it as 'Open' for consideration
                    node.ParentNode = fromNode;
                    node.State = EstadoPunto.Open;
                    walkableNodes.Add(node);
                    puntosCerrados.Add(node);
                }
            }

            return walkableNodes;
        }

        /// <summary>
        /// Returns the eight locations immediately adjacent (orthogonally and diagonally) to <paramref name="fromLocation"/>
        /// </summary>
        /// <param name="fromLocation">The location from which to return all adjacent points</param>
        /// <returns>The locations as an IEnumerable of Points</returns>
        private static IEnumerable<Point> GetAdjacentLocations(Point fromLocation)
        {
            return new Point[]
            {
                new Point(fromLocation.X-1, fromLocation.Y-1),
                new Point(fromLocation.X-1, fromLocation.Y  ),
                new Point(fromLocation.X-1, fromLocation.Y+1),
                new Point(fromLocation.X,   fromLocation.Y+1),
                new Point(fromLocation.X+1, fromLocation.Y+1),
                new Point(fromLocation.X+1, fromLocation.Y  ),
                new Point(fromLocation.X+1, fromLocation.Y-1),
                new Point(fromLocation.X,   fromLocation.Y-1)
            };
        }
    }
}