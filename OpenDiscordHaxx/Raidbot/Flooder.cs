﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace DiscordHaxx
{
    public class Flooder : Bot
    {
        private readonly FloodRequest _request;

        public Flooder(FloodRequest request)
        {
            Attack = new Attack(this) { Type = RaidOpcode.Flood, Bots = Server.Bots.Count };

            _request = request;
        }


        public override void Start()
        {
            List<DiscordClient> validBots = Server.Bots;
            List<DiscordClient> nextBots = new List<DiscordClient>(validBots);

            while (true)
            {
                if (ShouldStop)
                    break;

                Parallel.ForEach(validBots, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, bot =>
                {
                    try
                    {
                        if (ShouldStop)
                            return;

                        bot.SendMessage(_request.ChannelId, _request.Message);
                    }
                    catch (DiscordHttpException e)
                    {
                        switch (e.Code)
                        {
                            case DiscordError.AccountUnverified:
                                Console.WriteLine($"[ERROR] {bot.User} is unverified");
                                break;
                            case DiscordError.ChannelVerificationTooHigh:
                                Console.WriteLine("[ERROR] channel verification too high");
                                break;
                            case DiscordError.CannotSendEmptyMessage:
                                Console.WriteLine("[ERROR] cannot send empty messages");
                                break;
                            case DiscordError.UnknownChannel:
                                Console.WriteLine("[ERROR] unknown channel");
                                break;
                            default:
                                Console.WriteLine($"[ERROR] Unknown: {e.Code} | {e.ErrorMessage}");
                                break;
                        }

                        nextBots.Remove(bot);
                    }
                    catch (RateLimitException) { }
                });


                validBots = nextBots;

                if (validBots.Count == 0)
                    break;
            }

            Server.OngoingAttacks.Remove(Attack);
        }
    }
}
