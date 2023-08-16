
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
	public class SceneTransToPointReqHandler : IYuanShenHandler
	{
		public SceneTransToPointReqHandler() { }
		public async Task HandleAsync(YuanShenPacket packet, Player player)
		{
			MsgSceneTransToPointReq req = (MsgSceneTransToPointReq)packet;

			MsgSceneTransToPointRsp rsp = new MsgSceneTransToPointRsp
			{
				metaData = new PacketHead
				{
					SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
					ClientSequenceId = req.metaData.ClientSequenceId,
				},
				packetBody = new SceneTransToPointRsp
				{
					Retcode = 0,
					SceneId = req.packetBody.SceneId,
					PointId = req.packetBody.PointId,
				},
            };
	
			try
            {
				Vector point_to_transition_to = await player.AuthCalls.GetAccountMeta<Vector> (player.Account.UID, "last_saved_waypoint");
				MsgPlayerEnterSceneNotify scenePointUnlockNotify = new MsgPlayerEnterSceneNotify
				{
					metaData = new PacketHead
					{
						SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
						ClientSequenceId = req.metaData.ClientSequenceId,
					},
					packetBody = new PlayerEnterSceneNotify
					{
						SceneId = req.packetBody.SceneId,
						Pos = point_to_transition_to,
					},
				};

				await player.Session.SendAsync(scenePointUnlockNotify.AsBytes());
				Console.WriteLine("scenePointUnlockNotify");
				Console.WriteLine(scenePointUnlockNotify.PacketBody.ToString());
			} catch (Exception e)
            {
				Console.WriteLine("cant get scene point to transition to, aborting");
				return;
            }

			await player.Session.SendAsync(rsp.AsBytes());
			Console.WriteLine("MsgSceneTransToPointRsp");
			Console.WriteLine(rsp.PacketBody.ToString());
			return;
		}
	}
}

