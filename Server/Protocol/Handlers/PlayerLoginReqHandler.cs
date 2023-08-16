
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSFreedom.Common.Protocol;
using YSFreedom.Common.Protocol.Messages;
using YSFreedom.Server.Game;
using Google.Protobuf;
using System.IO;
using Newtonsoft.Json;

namespace YSFreedom.Server.Protocol.Handlers
{
    public class PlayerLoginReqHandler : IYuanShenHandler
    {
        public PlayerLoginReqHandler() { }
        public async Task HandleAsync(YuanShenPacket packet, Player player)
        {
            MsgPlayerLoginReq req = (MsgPlayerLoginReq)packet;

            MsgActivityScheduleInfoNotify msgActivityScheduleInfoNotify = new MsgActivityScheduleInfoNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = ActivityScheduleInfoNotify.Parser.ParseFrom(File.ReadAllBytes("ActivityScheduleInfoNotify.bin")),
            };

            await player.Session.SendAsync(msgActivityScheduleInfoNotify.AsBytes());

            Console.WriteLine("msgActivityScheduleInfoNotify");
            Console.WriteLine(msgActivityScheduleInfoNotify.PacketBody.ToString());

            MsgPlayerDataNotify msgPlayerDataNotify = new MsgPlayerDataNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = PlayerDataNotify.Parser.ParseFrom(File.ReadAllBytes("PlayerDataNotify.bin")),
            };

            //msgPlayerDataNotify.PacketBody.ServerTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await player.Session.SendAsync(msgPlayerDataNotify.AsBytes());

            Console.WriteLine("msgPlayerDataNotify");
            Console.WriteLine(msgPlayerDataNotify.PacketBody.ToString());

            MsgOpenStateUpdateNotify msgOpenStateUpdateNotify = new MsgOpenStateUpdateNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = OpenStateUpdateNotify.Parser.ParseFrom(File.ReadAllBytes("OpenStateUpdateNotify.bin")),
            };

            await player.Session.SendAsync(msgOpenStateUpdateNotify.AsBytes());

            Console.WriteLine("msgOpenStateUpdateNotify");
            Console.WriteLine(msgOpenStateUpdateNotify.PacketBody.ToString());

            MsgStoreWeightLimitNotify msgStoreWeightLimitNotify = new MsgStoreWeightLimitNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = new StoreWeightLimitNotify
                {
                    StoreType = (StoreType)1,
                    WeightLimit = 30000,
                    MaterialCountLimit = 2000,
                    WeaponCountLimit = 2000,
                    ReliquaryCountLimit = 1000,
                },
            };

            await player.Session.SendAsync(msgStoreWeightLimitNotify.AsBytes());

            Console.WriteLine("msgStoreWeightLimitNotify");
            Console.WriteLine(msgStoreWeightLimitNotify.PacketBody.ToString());

            MsgPlayerStoreNotify msgPlayerStoreNotify = new MsgPlayerStoreNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = PlayerStoreNotify.Parser.ParseFrom(File.ReadAllBytes("PlayerStoreNotify.bin")),
            };

            await player.Session.SendAsync(msgPlayerStoreNotify.AsBytes());

            Console.WriteLine("msgPlayerStoreNotify");
            Console.WriteLine(msgPlayerStoreNotify.PacketBody.ToString());

            var mavatarlist = AvatarDataNotify.Parser.ParseFrom(File.ReadAllBytes("AvatarDataNotify.bin"));
            
            foreach (var en in mavatarlist.AvatarList)
            {
                    for (uint i = 0;  i < 2000; i++)
                    {
                    if ((i >= 70 && i <= 76) || (i >= 1000 && i <= 1006))
                    {
                           Console.WriteLine($"replacing fightpropmap at key: {i}");
                           var mdict = new Dictionary<uint, float>();
                     en.FightPropMap.Remove(i);
                     mdict.Add(i, 80F);
                     en.FightPropMap.Add(mdict);
                    }
                }

                Console.WriteLine(en.FightPropMap.ToString());
            }

            MsgAvatarDataNotify msgAvatarDataNotify = new MsgAvatarDataNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = mavatarlist,
            };

            await player.Session.SendAsync(msgAvatarDataNotify.AsBytes());
            Console.WriteLine("msgAvatarDataNotify");
            Console.WriteLine(msgAvatarDataNotify.PacketBody.ToString());

            foreach (var e in msgAvatarDataNotify.packetBody.AvatarList)
            {
                var avatarkeystring = Convert.ToString(e.AvatarId);
                await player.AuthCalls.SetAccountMeta((int)player.UID, avatarkeystring + "prop_map", e.PropMap.ToString());
                Console.WriteLine(e.PropMap.ToString());
            }

            foreach (var e in msgAvatarDataNotify.packetBody.AvatarList)
            {
                var avatarkeystring = Convert.ToString(e.AvatarId);
                var mpropmap = player.AuthCalls.GetAccountMeta<string>((int)player.UID, avatarkeystring + "prop_map");
                Console.WriteLine(mpropmap.Result.ToString());
            }

            MsgAvatarSatiationDataNotify msgAvatarSatiationDataNotify = new MsgAvatarSatiationDataNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = AvatarSatiationDataNotify.Parser.ParseFrom(File.ReadAllBytes("AvatarSatiationDataNotify.bin")),
            };

            await player.Session.SendAsync(msgAvatarSatiationDataNotify.AsBytes());
            Console.WriteLine("msgAvatarSatiationDataNotify");
            Console.WriteLine(msgAvatarSatiationDataNotify.PacketBody.ToString());

            MsgRegionSearchNotify msgRegionSearchNotify = new MsgRegionSearchNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = new RegionSearchNotify
                {
                    Uid = 708845657,
                }
            };

            await player.Session.SendAsync(msgRegionSearchNotify.AsBytes());

            Console.WriteLine("msgRegionSearchNotify");
            Console.WriteLine(msgRegionSearchNotify.PacketBody.ToString());


            MsgPlayerEnterSceneNotify msgPlayerEnterSceneNotify = new MsgPlayerEnterSceneNotify
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = PlayerEnterSceneNotify.Parser.ParseFrom(File.ReadAllBytes("PlayerEnterSceneNotify.bin")),
            };

            msgPlayerEnterSceneNotify.PacketBody.EnterSceneToken = 12383;

            await player.Session.SendAsync(msgPlayerEnterSceneNotify.AsBytes());

            Console.WriteLine("msgPlayerEnterSceneNotify");
            Console.WriteLine(msgPlayerEnterSceneNotify.PacketBody.ToString());

            MsgPlayerLoginRsp rsp = new MsgPlayerLoginRsp
            {
                metaData = new PacketHead
                {
                    SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ClientSequenceId = req.metaData.ClientSequenceId,
                },
                packetBody = new PlayerLoginRsp
                {
                    GameBiz = "hk4e_global",
                    ClientDataVersion = 2762856,
                    ClientSilenceDataVersion = 2771742,
                    ClientMd5 = "{\"fileSize\": 4401, \"remoteName\": \"data_versions\", \"md5\": \"81cbe55ec93d6f24d8761218a7b0d1ad\"}",
                    ClientSilenceMd5 = "{\"fileSize\": 514, \"remoteName\": \"data_versions\", \"md5\": \"d11d71a0f0f17b3acc4b50efce94783e\"}",
                    ClientVersionSuffix = "4ebb02e19b",
                    ClientSilenceVersionSuffix = "99f775138c",
                    CountryCode = player.Account.CountryCode,
                    RegisterCps = "mihoyo",
                    IsScOpen = true,
                    ScInfo = ByteString.FromBase64("zxAB/CgAAAD/VXx9ptBGOXJfySpdYe35hicDFhYfIH3g/r8It1kl4w=="),
                },
            };

            ResVersionConfig resVersionConfig = new ResVersionConfig
            {
                Version = 2663089,
                Branch = "1.5_live",
                Md5 = "{\"remoteName\": \"res_versions_external\", \"md5\": \"3d3adf66078d764db9e96e405c80363f\", \"fileSize\": 252221}\r\n{\"remoteName\": \"res_versions_medium\", \"md5\": \"2ad2e114fd935275f7f7f6da867b0f83\", \"fileSize\": 116106}\r\n{\"remoteName\": \"release_res_versions_external\", \"md5\": \"388607b8521cd615501c8c4668c273f6\", \"fileSize\": 252221}\r\n{\"remoteName\": \"release_res_versions_medium\", \"md5\": \"85cdd216cabc74115955a584403e01ab\", \"fileSize\": 116106}\r\n{\"remoteName\": \"base_revision\", \"md5\": \"0da8998739a005179d3ae8fad702c19e\", \"fileSize\": 18}",
                ReleaseTotalSize = "0",
                VersionSuffix = "25a12eeea5",
            };

            rsp.PacketBody.ResVersionConfig = resVersionConfig;

            await player.Session.SendAsync(rsp.AsBytes());

            Console.WriteLine("rsp");
            Console.WriteLine(rsp.PacketBody.ToString());
        }
    }
}

