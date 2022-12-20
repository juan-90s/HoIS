using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;



public struct Hex : IEquatable<Hex>, IFormattable
{
    private int m_q;

    private int m_r;

    public static float SQRT_3 = Mathf.Sqrt(3);
    //
    // Summary:
    //     q component of the Hex Coordinate.
    public int q
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_q;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            m_q = value;
        }
    }


    //
    // Summary:
    //     r component of the Hex Coordinate.
    public int r
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return m_r;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            m_r = value;
        }
    }

    //
    // Summary:
    //     s component of the Hex Coordinate.(readonly)
    public int s
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return -m_q - m_r;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Hex(int q, int r)
    {
        m_q = q;
        m_r = r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Hex(int q, int r, int s)
    {
        m_q = q;
        m_r = r;
        if (s != -q - r)
            Debug.LogError("Unvalid Cube Coordinate");
    }

    //
    // Summary:
    //     Get all the neighbors of axial coordinate
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<Hex> AxialNeighbors()
    {
        return new List<Hex>() { 
            new Hex(q + 1, r), new Hex(q + 1, r - 1), new Hex(q, r - 1),
            new Hex(q - 1, r), new Hex(q - 1, r + 1), new Hex(q, r + 1)
        };
    }

    //
    // Summary:
    //     Returns the distance between a and b.
    //
    // Parameters:
    //   a:
    //
    //   b:
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance(Hex a, Hex b)
    {
        return (Math.Abs(a.q - b.q) + Math.Abs(a.q + a.r - b.q - b.r) + Math.Abs(a.r - b.r)) / 2;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Hex Round(float f_q, float f_r)
    {
        float q = Mathf.Round(f_q);
        float r = Mathf.Round(f_r);
        f_q -= q;
        f_r -= r;
        if (Math.Abs(f_q) >= Math.Abs(f_r))
        {
            return new Hex((int)(q + Mathf.Round(f_q + 0.5f * f_r)), (int)r);
        }
        else
        {
            return new Hex((int)q, (int)(r + Mathf.Round(f_r + 0.5f * f_q)));
        }
    }

    public static HashSet<Hex> GetReachable(Hex origin, int movement, Dictionary<Hex, Content> obstacleMap)
    {
        HashSet<Hex> visited = new HashSet<Hex>();
        visited.Add(origin);
        List<List<Hex>> fringes = new();
        fringes.Add(new List<Hex>() { origin });
        for (int k = 1; k <= movement; k++)
        {
            fringes.Add(new List<Hex>());
            foreach (Hex hex in fringes[k - 1])
            {
                foreach (Hex neighbor in hex.AxialNeighbors())
                {
                    if (!obstacleMap.ContainsKey(neighbor) || obstacleMap[neighbor] != Content.Empty) continue;
                    if (visited.Add(neighbor)) fringes[k].Add(neighbor);
                }
            }
        }
        return visited;
    }

    public static List<Hex> PathFinding(Hex start, Hex goal, Dictionary<Hex, Content> obstacleMap)
    {
        Dictionary<Hex, Hex> cameFrom = new();
        Dictionary<Hex, int> costSoFar = new();
        var frontier = new PriorityQueue<Hex, int>();
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.Equals(goal))
            {
                break;
            }

            foreach (var next in current.AxialNeighbors())
            {
                if (!obstacleMap.ContainsKey(next) || obstacleMap[next] != Content.Empty) continue;

                int newcost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newcost < costSoFar[next])
                {
                    costSoFar[next] = newcost;
                    int priority = newcost + Hex.Distance(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        // back tracking
        LinkedList<Hex> path = new();
        path.AddFirst(goal);
        while (path.First.Value != start && cameFrom.ContainsKey(path.First.Value))
        {
            path.AddFirst(cameFrom[path.First.Value]);
        }
        return path.ToList();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Hex lhs, Hex rhs)
    {
        return lhs.q == rhs.q && lhs.r == rhs.r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Hex lhs, Hex rhs)
    {
        return !(lhs == rhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Hex operator -(Hex hex)
    {
        return new Hex(-hex.q, -hex.r);
    }

    //
    // Summary:
    //     Returns true if the objects are equal.
    //
    // Parameters:
    //   other:
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object other)
    {
        if (!(other is Hex))
        {
            return false;
        }

        return Equals((Hex)other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Hex other)
    {
        return q == other.q && r == other.r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return q.GetHashCode() ^ (r.GetHashCode() << 2);
    }

    public override string ToString()
    {
        return ToString(null, null);
    }
    public string ToString(string format)
    {
        return ToString(format, null);
    }
    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (formatProvider == null)
        {
            formatProvider = CultureInfo.InvariantCulture.NumberFormat;
        }

        return String.Format("({0}, {1}, {2})", q.ToString(format, formatProvider), r.ToString(format, formatProvider), s.ToString(format, formatProvider));
    }
}
