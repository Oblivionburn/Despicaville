using System;
using System.Collections.Generic;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Scenes;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Rendering;
using Despicaville.Util;

namespace Despicaville
{
    public static class TimeTracker
    {
        public static int TimeToAction;

        public static void Init()
        {
            Reset();
        }

        public static void Tick(long milliseconds)
        {
            TimeManager.Now.AddMilliseconds(milliseconds);
        }

        public static void MillisecondsChanged(object sender, EventArgs e)
        {
            Handler.Action = false;

            Handler.TimeToAction++;
            if (Handler.TimeToAction >= Handler.ActionRate)
            {
                Handler.Action = true;
                Handler.TimeToAction = 0;
            }

            Squad citizens = CharacterManager.GetArmy("Characters").GetSquad("Citizens");

            Character[] citizens_array = citizens.Characters.ToArray();
            int count = citizens_array.Length;
            for (int i = 0; i < count; i++)
            {
                Character character = citizens_array[i];
                if (character.Dead)
                {
                    continue;
                }

                character.Job.Update(TimeManager.Now);

                if (character.Moving)
                {
                    continue;
                }

                Task task = character.Job.CurrentTask;
                if (task?.Name == "Sleep")
                {
                    continue;
                }

                if (character.Unconscious)
                {
                    CharacterUtil.Rest(character);
                    continue;
                }

                if (task == null)
                {
                    Tasker.GiveTask_Citizen(character);
                }
            }

            Character player = Handler.GetPlayer();
            if (player.Unconscious)
            {
                CharacterUtil.Rest(player);
            }

            if (!player.Moving)
            {
                player.Job.Update(TimeManager.Now);
            }
        }

        public static void SecondsChanged(object sender, EventArgs e)
        {
            Army army = CharacterManager.GetArmy("Characters");
            foreach (Squad squad in army.Squads)
            {
                Character[] characters = squad.Characters.ToArray();

                for (int i = 0; i < characters.Length; i++)
                {
                    Character character = characters[i];

                    Something blood = character.GetStat("Blood");
                    if (blood.Value <= 0)
                    {
                        CharacterUtil.Kill(character);
                        squad.Characters.Remove(character);
                        i--;
                    }

                    if (character.Dead)
                    {
                        continue;
                    }

                    Task task = character.Job.CurrentTask;
                    if (task?.Name == "Sleep")
                    {
                        CharacterUtil.Sleep(character);
                    }
                    else if (character.Type == "Citizen")
                    {
                        character.GetStat("Boredom").IncreaseValueByRate();
                    }

                    character.GetStat("Thirst").IncreaseValueByRate();
                    character.GetStat("Hunger").IncreaseValueByRate();

                    CharacterUtil.UpdateWounds(character);
                    CharacterUtil.UpdatePain(character);
                    CharacterUtil.UpdateBloodLoss(character);
                    CharacterUtil.UpdateConsciousness(character);
                }
            }
        }

        public static void MinutesChanged(object sender, EventArgs e)
        {
            if (RenderingManager.Lighting.LerpAmount < 1)
            {
                RenderingManager.Lighting.LerpAmount += 0.0167f;
            }
            RenderingManager.Lighting.Update();

            Army characters = CharacterManager.GetArmy("Characters");
            foreach (Squad squad in characters.Squads)
            {
                foreach (Character character in squad.Characters)
                {
                    if (character.Dead)
                    {
                        continue;
                    }

                    Something poisoned = character.GetStatusEffect("Poisoned");
                    if (poisoned != null)
                    {
                        character.GetStat("Blood").DecreaseValue(1);

                        poisoned.DecreaseValue(1);
                        if (poisoned.Value <= 0)
                        {
                            character.StatusEffects.Remove(poisoned);
                        }
                    }
                }
            }
        }

        public static void HoursChanged(object sender, EventArgs e)
        {
            RenderingManager.Lighting.LerpAmount = 0;

            Army characters = CharacterManager.GetArmy("Characters");
            foreach (Squad squad in characters.Squads)
            {
                foreach (Character character in squad.Characters)
                {
                    if (!character.Dead)
                    {
                        bool sleeping = false;
                        Task task = character.Job.CurrentTask;
                        if (task != null)
                        {
                            if (task.Name == "Sleep")
                            {
                                sleeping = true;
                            }
                        }

                        Something thirst = character.GetStat("Thirst");
                        if (thirst.Value >= 100)
                        {
                            character.GetStat("Blood").DecreaseMaxValue(1);
                        }

                        Something hunger = character.GetStat("Hunger");
                        if (hunger.Value >= 100)
                        {
                            character.GetStat("Stamina").DecreaseMaxValue(1);
                        }

                        if (character.Type == "Citizen" &&
                            !sleeping &&
                            !character.Unconscious)
                        {
                            Something boredom = character.GetStat("Boredom");
                            if (boredom.Value >= 100)
                            {
                                character.GetStat("Depression").IncreaseValue(1);
                            }
                        }
                    } 
                }
            }
        }

        public static void Reset()
        {
            TimeManager.Init(1984, 6, 1, 6);
            RenderingManager.Lighting.Reset();

            TimeManager.Now.OnMillisecondsChange -= MillisecondsChanged;
            TimeManager.Now.OnMillisecondsChange += MillisecondsChanged;

            TimeManager.Now.OnSecondsChange -= SecondsChanged;
            TimeManager.Now.OnSecondsChange += SecondsChanged;

            TimeManager.Now.OnMinutesChange -= MinutesChanged;
            TimeManager.Now.OnMinutesChange += MinutesChanged;

            TimeManager.Now.OnHoursChange -= HoursChanged;
            TimeManager.Now.OnHoursChange += HoursChanged;
        }
    }
}
