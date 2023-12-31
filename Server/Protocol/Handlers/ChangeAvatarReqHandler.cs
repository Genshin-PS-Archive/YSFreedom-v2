
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSFreedom.Common.Protocol;
using YSFreedom.Common.Protocol.Messages;
using YSFreedom.Server.Game;

namespace YSFreedom.Server.Protocol.Handlers
{
	public class ChangeAvatarReqHandler : IYuanShenHandler
	{
		public ChangeAvatarReqHandler() { }
		public async Task HandleAsync(YuanShenPacket packet, Player player)
		{
			MsgChangeAvatarReq req = (MsgChangeAvatarReq)packet;

			Console.WriteLine(req.ToString());

			MsgChangeAvatarRsp rsp = new MsgChangeAvatarRsp
			{
				metaData = new PacketHead
				{
					SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
					ClientSequenceId = req.metaData.ClientSequenceId,
				},
				packetBody = new ChangeAvatarRsp
				{
					CurGuid = req.PacketBody.Guid,
					Retcode = 0,
				},
			};

			MsgSceneEntityDisappearNotify msgSceneEntityDisappearNotify = new MsgSceneEntityDisappearNotify
			{
				metaData = new PacketHead
				{
					SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
					ClientSequenceId = req.metaData.ClientSequenceId,
				},
				packetBody = new SceneEntityDisappearNotify
				{
					DisappearType = VisionType.VisionReplace,
				},
			};

			PlayerEnterSceneInfoNotify playerEnterSceneInfoNotify = PlayerEnterSceneInfoNotify.Parser.ParseFrom(File.ReadAllBytes("PlayerEnterSceneInfoNotify"));
			msgSceneEntityDisappearNotify.PacketBody.EntityList.Add(playerEnterSceneInfoNotify.CurAvatarEntityId);


			await player.Session.SendAsync(msgSceneEntityDisappearNotify.AsBytes());
			Console.WriteLine("msgSceneEntityDisappearNotify");
			Console.WriteLine(msgSceneEntityDisappearNotify.PacketBody.ToString());

			foreach (var avatar in playerEnterSceneInfoNotify.AvatarEnterInfo)
			{
				if (avatar.AvatarGuid == req.PacketBody.Guid)
				{
					playerEnterSceneInfoNotify.CurAvatarEntityId = avatar.AvatarEntityId;
				}
			}

			MsgSceneEntityAppearNotify msgSceneEntityAppearNotify = new MsgSceneEntityAppearNotify
			{
				metaData = new PacketHead
				{
					SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
					ClientSequenceId = req.metaData.ClientSequenceId,
				},
				packetBody = new SceneEntityAppearNotify
				{
					AppearType = VisionType.VisionReplace,
				},
			};

			SceneEntityAppearNotify sceneEntityAppearNotify = SceneEntityAppearNotify.Parser.ParseFrom(File.ReadAllBytes("SceneEntityAppearNotify.bin"));
			SceneTeamUpdateNotify avatarDataNotify = SceneTeamUpdateNotify.Parser.ParseFrom(File.ReadAllBytes("SceneTeamUpdateNotify.bin"));

			foreach (var entity in sceneEntityAppearNotify.EntityList)
            {
				Console.WriteLine("found entity");
				entity.EntityId = playerEnterSceneInfoNotify.CurAvatarEntityId;
				foreach (var avatar in avatarDataNotify.SceneTeamAvatarList)
                {
					if (avatar.AvatarGuid== req.PacketBody.Guid)
                    {
						Console.WriteLine("found avatar");
						avatar.SceneEntityInfo.MotionInfo = entity.MotionInfo;
						msgSceneEntityAppearNotify.PacketBody.EntityList.Add(avatar.SceneEntityInfo);
					}
                }
			}

			await player.AuthCalls.SetAccountMeta(player.Account.UID, "cur_avatar_entity_id", playerEnterSceneInfoNotify.CurAvatarEntityId);

			foreach (var en in msgSceneEntityAppearNotify.PacketBody.EntityList)
			{
				for (uint i = 0; i < 2000; i++)
				{
					if ((i >= 70 && i <= 76) || (i >= 1000 && i <= 1006))
					{
						foreach (var item in en.FightPropList)
						{
							if (item.PropType == i)
							{
								Console.WriteLine($"replacing fightpropmap at key: {i}");
								en.FightPropList.Remove(item);
								break;
							}
						}
						en.FightPropList.Add(new FightPropPair { PropType = i, PropValue = 80F });
					}
				}
				en.FightPropList.Add(new FightPropPair { PropType = 11, PropValue = 200F });
				Console.WriteLine(en.FightPropList.ToString());
				Console.WriteLine($"current avatar entity_id changed to {en.EntityId}");
				await player.AuthCalls.SetAccountMeta(player.UID, "curr_avatar_id", en.EntityId);
			}

			await player.Session.SendAsync(msgSceneEntityAppearNotify.AsBytes());
			Console.WriteLine("msgSceneEntityAppearNotify");
			Console.WriteLine(msgSceneEntityAppearNotify.PacketBody.ToString());

			await player.Session.SendAsync(rsp.AsBytes());
			Console.WriteLine("MsgChangeAvatarRsp");
			Console.WriteLine(rsp.PacketBody.ToString());

			return;
		}
	}
}

