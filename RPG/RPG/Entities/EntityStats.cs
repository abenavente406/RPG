using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RPG.Entities
{
    [Serializable]
    public class EntityStats : ISerializable
    {
        Entity entity;

        int level;
        int hp, maxHp;
        float attackPower;
        float headArmour, bodyArmour, legsArmour;

        public float headMultiplier, bodyMultiplier, legsMultiplier;

        public EntityStats(SerializationInfo info, StreamingContext cntxt) {
            level = (int) info.GetValue("Stats_Level", typeof(int));
            hp = (int) info.GetValue("Stats_Hp", typeof(int));
            maxHp = (int) info.GetValue("Stats_MaxHp", typeof(int));
            attackPower = (float) info.GetValue("Stats_AP", typeof(float));
            headArmour = (float) info.GetValue("Stats_HeadArmour", typeof(float));
            bodyArmour = (float) info.GetValue("Stats_BodyArmour", typeof(float));
            legsArmour = (float) info.GetValue("Stats_LegsArmour", typeof(float));
            headMultiplier = (float) info.GetValue("Stats_headMultiplier", typeof(float));
            bodyMultiplier = (float) info.GetValue("Stats_bodyMultiplier", typeof(float));
            legsMultiplier = (float) info.GetValue("Stats_legsMultiplier", typeof(float));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("Stats_Level", level);
            info.AddValue("Stats_Hp", hp);
            info.AddValue("Stats_MaxHp", maxHp);
            info.AddValue("Stats_AP", attackPower);
            info.AddValue("Stats_HeadArmour", headArmour);
            info.AddValue("Stats_BodyArmour", bodyArmour);
            info.AddValue("Stats_LegsArmour", legsArmour);
            info.AddValue("Stats_headMultiplier", headMultiplier);
            info.AddValue("Stats_bodyMultiplier", bodyMultiplier);
            info.AddValue("Stats_legsMultiplier", legsMultiplier);
        }

        public void setEntity(Entity e) {
            if (this.entity == null)
                this.entity = e;
            else
                throw new ArgumentException("Entity already set!");
        }

        public EntityStats(Entity e, int hp, float atkPower) {
            entity = e;
            level = 1;
            this.hp = maxHp = hp;
            attackPower = atkPower;

            headArmour = bodyArmour = legsArmour = 0f;
            headMultiplier = bodyMultiplier = legsMultiplier = 1f;
        }

        public void resetReducers() {
            headMultiplier = 1 - headArmour;
            bodyMultiplier = 1 - bodyArmour;
            legsMultiplier = 1 - legsArmour;
        }

        public void levelUp() {
            level++;
            float hpPercent = hp / (float) maxHp;
            maxHp = (int) (maxHp * 1.1f);
            hp = (int) (maxHp * hpPercent);
            addHp((int) (maxHp * 0.1f)); // Heal by 10% hp

            attackPower = attackPower * 1.1f;
        }

        public void addHp(int amount) {
            hp += amount;
            if (hp > maxHp)
                hp = maxHp;
        }

        public float HpPercent { get { return (hp / (float) maxHp); } }
        public int Hp { get { return hp; } }
        public int MaxHp { get { return maxHp; } }
        public float AttackPower { get { return attackPower; } }
        public int Level { get { return level; } }

        public float THeadMultiplier { get { return Entity.BASE_HEAD_MULT * headMultiplier - entity.Equipment.Head.Mult; } }
        public float TBodyMultiplier { get { return Entity.BASE_BODY_MULT * bodyMultiplier - entity.Equipment.Body.Mult; } }
        public float TLegsMultiplier { get { return Entity.BASE_LEGS_MULT * legsMultiplier - entity.Equipment.Legs.Mult; } }
    }
}
