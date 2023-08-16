using System;
using System.Collections.Generic;
using YSFreedom.Common.Protocol;

namespace YSFreedom.Server.Game
{
    public class PlayerDataProperties : Dictionary<uint, PropValue>
    {
        public long XP {
            get { return GetIVal((uint)EPlayerDataProperties.XP); }
            set { SetIVal((uint)EPlayerDataProperties.XP, value); }
        }

        public long OriginalResin
        {
            get { return GetIVal((uint)EPlayerDataProperties.OriginalResin); }
            set { SetIVal((uint)EPlayerDataProperties.OriginalResin, value); }
        }

        public long Primogems
        {
            get { return GetIVal((uint)EPlayerDataProperties.Primogems); }
            set { SetIVal((uint)EPlayerDataProperties.Primogems, value); }
        }

        public long WorldLevel
        {
            get { return GetIVal((uint)EPlayerDataProperties.WorldLevel); }
            set { SetIVal((uint)EPlayerDataProperties.WorldLevel, value); }
        }
        public void SetIVal(uint key, long val)
        {
            this[key] = new PropValue { Type = key, Val = val, Ival = val };
        }

        public long GetIVal(uint key)
        {
            PropValue propValue;
            if (!this.TryGetValue(key, out propValue))
                return 0;

            return propValue.Ival;
        }
    }
}