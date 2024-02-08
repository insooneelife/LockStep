using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IPhysics 
{
	public static void EnforceByCellSpace(Entity e1, CellSpacePartition<Entity> others, Func<Entity, Entity, bool> filter) 
	{
        others.CalculateNeighbors(e1.Pos, e1.Radius);

        Point2D sum;
        sum.x = 0;
        sum.y = 0;
        int cnt = 0;

        for (int i = 0; i < others.NeighberCnt; i++)
        {
            Entity e2 = others.Neighbers[i];
            if (e1.Id == e2.Id)
                continue;

            // Character && Character
            if (filter(e1, e2))
            {
                Point2D toEnt;
                toEnt.x = e1.Pos.x - e2.Pos.x;
                toEnt.y = e1.Pos.y - e2.Pos.y;

                int distFromEachOther = IMath.Max(toEnt.Length(), 1);
                int amountOfOverlap = e1.Radius + e2.Radius - distFromEachOther;

                if (amountOfOverlap > 0)
                {
                    Point2D force;
                    force.x = (toEnt.x * amountOfOverlap);
                    force.y = (toEnt.y * amountOfOverlap);
                    force.x = force.x / 2;
                    force.y = force.y / 2;
                    force.x = force.x / distFromEachOther;
                    force.y = force.y / distFromEachOther;

                    //Point2D ePos;
                    //ePos.x = e2.Pos.x - force.x;
                    //ePos.y = e2.Pos.y - force.y;
                    //e2.SetPos(ePos);

                    sum.x += force.x;
                    sum.y += force.y;
                    cnt++;
                }
            }
        }

        if (cnt > 0)
        {
            sum.x = sum.x / cnt;
            sum.y = sum.y / cnt;
            Point2D pos;
            pos.x = e1.Pos.x + (sum.x);
            pos.y = e1.Pos.y + (sum.y);
            e1.SetPos(pos);
        }
    }
}
