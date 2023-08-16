
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSFreedom.Common.Protocol;
using YSFreedom.Common.Protocol.Messages;
using YSFreedom.Server.Game;
using Google.Protobuf;

namespace YSFreedom.Server.Protocol.Handlers
{
	public class UnionCmdNotifyHandler : IYuanShenHandler
	{
		public UnionCmdNotifyHandler() { }
		public async Task HandleAsync(YuanShenPacket packet, Player player)
		{
			MsgUnionCmdNotify req = (MsgUnionCmdNotify)packet;

			foreach (var item in req.PacketBody.CmdList)
            {
				Console.WriteLine($"got MessageId {(EMsgType)item.MessageId}");
				//Console.WriteLine(item.ToString());

				switch (item.MessageId)
                {
					case 1102:
						AbilityInvocationsNotify abilityInvocationsNotify = AbilityInvocationsNotify.Parser.ParseFrom(item.Body);
						//Console.WriteLine(abilityInvocationsNotify.ToString());
						break;
					case 1112:
						ClientAbilityChangeNotify clientAbilityChangeNotify = ClientAbilityChangeNotify.Parser.ParseFrom(item.Body);
						//Console.WriteLine(clientAbilityChangeNotify.ToString());
						break;
					case 350:
						CombatInvocationsNotify combatInvocationsNotify = CombatInvocationsNotify.Parser.ParseFrom(item.Body);
						//Console.WriteLine(combatInvocationsNotify.ToString());

						foreach (var combatitem in combatInvocationsNotify.InvokeList)
						{
							//Console.WriteLine(combatitem.ToString());
							EntityMoveInfo entityMoveInfo = EntityMoveInfo.Parser.ParseFrom(combatitem.CombatData);
							//Console.WriteLine(entityMoveInfo.ToString());
							if ((combatitem.ArgumentType == CombatTypeArgument.EntityMove) && (entityMoveInfo.EntityId == 16779279)) // current player entity id
                            {
								// save player location TOTALLY IO FRIENDLY :D
								PlayerEnterSceneNotify playerEnterSceneNotify = PlayerEnterSceneNotify.Parser.ParseFrom(File.ReadAllBytes("PlayerEnterSceneNotify.bin"));
								playerEnterSceneNotify.Pos = entityMoveInfo.MotionInfo.Pos;
								File.WriteAllBytes("PlayerEnterSceneNotify.bin", playerEnterSceneNotify.ToByteArray());
								SceneEntityAppearNotify sceneEntityAppearNotify = SceneEntityAppearNotify.Parser.ParseFrom(File.ReadAllBytes("SceneEntityAppearNotify.bin"));
								foreach (var mentity_list in sceneEntityAppearNotify.EntityList)
                                {
									if (mentity_list.EntityType == ProtEntityType.ProtEntityAvatar && mentity_list.EntityId == 16779279)
                                    {
										mentity_list.MotionInfo.Pos = entityMoveInfo.MotionInfo.Pos;
										mentity_list.MotionInfo.Rot = entityMoveInfo.MotionInfo.Rot;
									}
                                }
								File.WriteAllBytes("SceneEntityAppearNotify.bin", sceneEntityAppearNotify.ToByteArray());
							}

							if (combatitem.ArgumentType == CombatTypeArgument.CombatEvtBeingHit)
                            {
								// Handle some entity being hit...
								Console.WriteLine("someone hit someone...");

								EvtBeingHitInfo evtBeingHitInfo = EvtBeingHitInfo.Parser.ParseFrom(combatitem.CombatData);
								Console.WriteLine(evtBeingHitInfo.ToString());

								int player_id = await player.AuthCalls.GetAccountMeta<int>(player.UID, "curr_avatar_id");
								Console.WriteLine($"player entity_id: {player_id}");

								if (evtBeingHitInfo.AttackResult.DefenseId != player_id)
                                {
									Console.WriteLine("player attacks");
									// handle enemy damage
									Console.WriteLine($"entity {evtBeingHitInfo.AttackResult.DefenseId} gets hit with damage {evtBeingHitInfo.AttackResult.DamageShield}");

									MsgEntityFightPropUpdateNotify entityFightPropUpdateNotify = new MsgEntityFightPropUpdateNotify
                                    {
										metaData = new PacketHead
										{
											SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
											ClientSequenceId = req.metaData.ClientSequenceId,
										},
										packetBody = new EntityFightPropUpdateNotify
										{
											EntityId = evtBeingHitInfo.AttackResult.DefenseId,
										},
                                    };

									SceneEntityAppearNotify mmm = SceneEntityAppearNotify.Parser.ParseFrom(File.ReadAllBytes("SceneEntityAppearNotify1.bin"));

									float current_hp = 0F;
									float new_hp = 0F;

									foreach (var entity in mmm.EntityList)
                                    {
										if (entity.EntityId == evtBeingHitInfo.AttackResult.DefenseId)
                                        {
											Console.WriteLine(entity.FightPropList.ToString());
											foreach (var prop in entity.FightPropList)
                                            {
												if (prop.PropType == (uint)EFightProperties.BaseHp)
                                                {

                                                }
												if (prop.PropType == (uint)EFightProperties.CurHp)
                                                {
													current_hp = prop.PropValue;
													new_hp = current_hp - evtBeingHitInfo.AttackResult.DamageShield;
													Console.WriteLine($"found defending entity with current HP: {current_hp}, new_hp: {new_hp}");
													entity.FightPropList.Remove(prop);
													entity.FightPropList.Add(new FightPropPair { PropType = 1010, PropValue = new_hp});
													File.WriteAllBytes("SceneEntityAppearNotify1.bin", mmm.ToByteArray());

													if (new_hp < 0)
                                                    {
														// let entity die
														MsgLifeStateChangeNotify lifeStateChangeNotify = new MsgLifeStateChangeNotify {
															metaData = new PacketHead
															{
																SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
																ClientSequenceId = req.metaData.ClientSequenceId,
															},
															packetBody = new LifeStateChangeNotify
															{
																EntityId = entity.EntityId,
															    LifeState = 2,
																DieType = PlayerDieType.PlayerDieDrawn,
																MoveReliableSeq = 50,
															},
                                                        };

														await player.Session.SendAsync(lifeStateChangeNotify.AsBytes());

														MsgSceneEntityDisappearNotify sceneEntityDisappearNotify = new MsgSceneEntityDisappearNotify
                                                        {
															metaData = new PacketHead
															{
																SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
																ClientSequenceId = req.metaData.ClientSequenceId,
															},
															packetBody = new SceneEntityDisappearNotify
															{
																DisappearType = VisionType.VisionDie, // TODO fix vision disappear type to VisionDie (does not do anything atm weirdly)
															},
                                                        };
														sceneEntityDisappearNotify.packetBody.EntityList.Add(entity.EntityId);
														Console.WriteLine($"sending die to entity {evtBeingHitInfo.AttackResult.DefenseId}");
														await player.Session.SendAsync(sceneEntityDisappearNotify.AsBytes());
														Console.WriteLine(sceneEntityDisappearNotify.packetBody.ToString());
                                                    }
													break;
                                                }
                                            }
                                        }
                                    }

									var mdamage = new Dictionary<uint, float>();
									mdamage.Add((uint)EFightProperties.CurHp, new_hp);
									entityFightPropUpdateNotify.packetBody.FightPropMap.Add(mdamage);

									await player.Session.SendAsync(entityFightPropUpdateNotify.AsBytes()); // sends back damage to client...

                                } else
                                {
									Console.WriteLine("player defends..");
									
									float current_hp = 0F;
									float new_hp = 0F;
									try { 
										if (player.AuthCalls.ExistsMeta(player.Account.UID, "cur_avatar_health"))
										{
											current_hp = await player.AuthCalls.GetAccountMeta<float>(player.Account.UID, "cur_avatar_health");
										}
									} catch (Exception e)
                                    {
										Console.WriteLine("health not existing atm...");
										await player.AuthCalls.SetAccountMeta(player.Account.UID, "cur_avatar_health", 1363F);
										current_hp = await player.AuthCalls.GetAccountMeta<float>(player.Account.UID, "cur_avatar_health");
                                    }

									try { 

									new_hp = current_hp - evtBeingHitInfo.AttackResult.DamageShield;
									Console.WriteLine($"found defending entity with current HP: {current_hp}, new_hp: {new_hp}");
									await player.AuthCalls.SetAccountMeta(player.Account.UID, "cur_avatar_health", new_hp);


									// handle player damage

									MsgEntityFightPropUpdateNotify entityFightPropUpdateNotify = new MsgEntityFightPropUpdateNotify
                                    {
										metaData = new PacketHead
										{
											SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
											ClientSequenceId = req.metaData.ClientSequenceId,
										},
										packetBody = new EntityFightPropUpdateNotify
										{
											EntityId = evtBeingHitInfo.AttackResult.DefenseId,
										},
                                    };

									var mdamage = new Dictionary<uint, float>();
									mdamage.Add((uint)EFightProperties.CurHp, new_hp);
									entityFightPropUpdateNotify.packetBody.FightPropMap.Add(mdamage);

									Console.WriteLine(entityFightPropUpdateNotify.PacketBody.ToString());

									await player.Session.SendAsync(entityFightPropUpdateNotify.AsBytes()); // sends back damage to client...
									} catch (Exception e)
                                    {
										Console.WriteLine("cur avatar id not set up yet, cant get basehp....");
                                    }
                                }

								// send back the combat invok notify

								MsgCombatInvocationsNotify msgCombatInvocationsNotify = new MsgCombatInvocationsNotify
                                {
									metaData = new PacketHead
									{
										SentMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
										ClientSequenceId = req.metaData.ClientSequenceId,
										},
									packetBody = new CombatInvocationsNotify {},
                                };

								msgCombatInvocationsNotify.packetBody.InvokeList.Add(combatitem);

								await player.Session.SendAsync(msgCombatInvocationsNotify.AsBytes()); // shows the attack damage on screen

                            }
						}
						break;
					default:
						break;
                }
            }

			return;
		}
	}
}

