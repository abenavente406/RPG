using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Entities
{
    public class NearEntity
    {
        public Entity Entity;
        public double Distance;

        public NearEntity(Entity e, double dist) {
            Entity = e;
            Distance = dist;
        }
    }
}
