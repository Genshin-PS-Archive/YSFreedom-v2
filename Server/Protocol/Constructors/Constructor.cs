using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSFreedom.Common.Protocol;
using YSFreedom.Common.Protocol.Messages;
using YSFreedom.Server.Game;

namespace YSFreedom.Server.Protocol
{
    class Constructor
    { 
        public Constructor() { }

        public async Task save_avatar_prop(Player player, int AvatarID, PropValue prop)
        {
            await player.AuthCalls.SetAccountMeta(player.UID, Convert.ToString(AvatarID) + "PropValue" + Convert.ToString(prop.Type), prop);
        }

        public async Task<PropValue> get_avatar_prop(Player player, int AvatarID, PropValue prop)
        {
            prop = await player.AuthCalls.GetAccountMeta<PropValue>(player.UID, Convert.ToString(AvatarID) + "PropValue" + Convert.ToString(prop.Type));
            return prop;
        }

        public async Task edit_avatar_prop(Player player, int AvatarID, PropValue prop)
        {
            delete_avatar_prop(player, AvatarID, prop.Type);
            await save_avatar_prop(player, AvatarID, prop);
        }

        public void delete_avatar_prop(Player player, int AvatarID, uint type)
        {
            player.AuthCalls.DeleteMeta(player.Account.UID, Convert.ToString(AvatarID) + "PropValue" + Convert.ToString(type));
        }

        public void delete_avatar_prop(Player player, int AvatarID, PropValue prop)
        {
            player.AuthCalls.DeleteMeta(player.Account.UID, Convert.ToString(AvatarID) + "PropValue" + Convert.ToString(prop.Type));
        }
    }
}
