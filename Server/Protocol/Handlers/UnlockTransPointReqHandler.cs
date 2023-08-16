
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
	public class UnlockTransPointReqHandler : IYuanShenHandler
	{
		public UnlockTransPointReqHandler() { }
		public async Task HandleAsync(YuanShenPacket packet, Player player)
		{
			MsgUnlockTransPointReq req = (MsgUnlockTransPointReq)packet;

			Console.WriteLine(req.ToString());

			MsgUnlockTransPointRsp rsp = new MsgUnlockTransPointRsp
			{
				metaData = new PacketHead
				{
					SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
					ClientSequenceId = req.metaData.ClientSequenceId,
				},
				packetBody = new UnlockTransPointRsp
				{
					Retcode = 0,
				},
			};

			MsgScenePointUnlockNotify scenePointUnlockNotify = new MsgScenePointUnlockNotify
			{
				metaData = new PacketHead
				{
					SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
					ClientSequenceId = req.metaData.ClientSequenceId,
				},
				packetBody = new ScenePointUnlockNotify
				{
					SceneId = req.packetBody.SceneId,
				},
			};

			scenePointUnlockNotify.packetBody.PointList.Add(req.packetBody.PointId);

			await player.Session.SendAsync(scenePointUnlockNotify.AsBytes());
			Console.WriteLine("MsgScenePointUnlockNotify");
			Console.WriteLine(scenePointUnlockNotify.PacketBody.ToString());

			await player.Session.SendAsync(rsp.AsBytes());
			Console.WriteLine("MsgUnlockTransPointRsp");
			Console.WriteLine(rsp.PacketBody.ToString());

			PlayerEnterSceneNotify playerEnterSceneNotify = PlayerEnterSceneNotify.Parser.ParseFrom(File.ReadAllBytes("PlayerEnterSceneNotify.bin"));
			Console.WriteLine($"saving this waypoint position {playerEnterSceneNotify.Pos}");

			await player.AuthCalls.SetAccountMeta(player.Account.UID, "last_saved_waypoint", playerEnterSceneNotify.Pos);

			return;
		}
	}
}

