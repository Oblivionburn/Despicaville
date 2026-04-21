using System;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Rendering;
using Despicaville.Util;

namespace Despicaville
{
    public static class TimeTracker
    {
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

                if (Handler.Action)
                {
                    character.Job.Update(TimeManager.Now);
                }

                if (character.Moving)
                {
                    continue;
                }

                Task task = character.Job.CurrentTask;
                if (task?.Name == "Sleep")
                {
                    continue;
                }

                if (character.Unconscious &&
                    Handler.Action)
                {
                    CharacterUtil.Rest(character);
                    continue;
                }

                if (task == null &&
                    !CharacterUtil.HeldByPlayer(character))
                {
                    Tasker.GiveTask_Citizen(character);
                }
            }

            Task playerTask = Handler.Player.Job.CurrentTask;
            if (playerTask != null)
            {
                if (playerTask.Name != "Sneak" &&
                    playerTask.Name != "Walk" &&
                    playerTask.Name != "Run" &&
                    playerTask.Name != "Push")
                {
                    Handler.Player.Job.Update(TimeManager.Now);
                }
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

                    Property blood = character.GetStat("Blood");
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
                        Property boredom = character.GetStat("Boredom");
                        boredom.Value += boredom.Rate;
                        if (boredom.Value > boredom.Max_Value)
                        {
                            boredom.Value = boredom.Max_Value;
                        }
                    }

                    Property thirst = character.GetStat("Thirst");
                    thirst.Value += thirst.Rate;
                    if (thirst.Value > thirst.Max_Value)
                    {
                        thirst.Value = thirst.Max_Value;
                    }

                    Property hunger = character.GetStat("Hunger");
                    hunger.Value += hunger.Rate;
                    if (hunger.Value > hunger.Max_Value)
                    {
                        hunger.Value = hunger.Max_Value;
                    }

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

                    Property poisoned = character.GetStatusEffect("Poisoned");
                    if (poisoned != null)
                    {
                        Property blood = character.GetStat("Blood");
                        blood.Value--;
                        if (blood.Value < 0)
                        {
                            blood.Value = 0;
                            character.Dead = true;
                        }

                        poisoned.Value--;
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

                        Property thirst = character.GetStat("Thirst");
                        if (thirst.Value >= 100)
                        {
                            Property blood = character.GetStat("Blood");
                            blood.Max_Value--;

                            if (blood.Max_Value < 0)
                            {
                                blood.Max_Value = 0;
                            }
                            if (blood.Value > blood.Max_Value)
                            {
                                blood.Value = blood.Max_Value;
                            }
                        }

                        Property hunger = character.GetStat("Hunger");
                        if (hunger.Value >= 100)
                        {
                            Property stamina = character.GetStat("Stamina");
                            stamina.Max_Value--;

                            if (stamina.Max_Value < 0)
                            {
                                stamina.Max_Value = 0;
                            }
                            if (stamina.Value > stamina.Max_Value)
                            {
                                stamina.Value = stamina.Max_Value;
                            }
                        }

                        if (character.Type == "Citizen" &&
                            !sleeping &&
                            !character.Unconscious)
                        {
                            Property boredom = character.GetStat("Boredom");
                            if (boredom.Value >= 100)
                            {
                                Property depression = character.GetStat("Depression");
                                depression.Value++;

                                if (depression.Value > depression.Max_Value)
                                {
                                    depression.Value = depression.Max_Value;
                                }
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
