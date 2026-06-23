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
            TimeManager.Now?.AddMilliseconds(milliseconds);
        }

        public static void MillisecondsChanged(object? sender, EventArgs e)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            Handler.Action = false;

            Handler.TimeToAction++;
            if (Handler.TimeToAction >= Handler.ActionRate)
            {
                Handler.Action = true;
                Handler.TimeToAction = 0;
            }

            Squad? citizens = CharacterManager.GetArmy("Characters")?.GetSquad("Citizens");
            if (citizens == null)
            {
                return;
            }

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

                JobTask? task = character.Job.CurrentTask;
                if (task?.Name == "Sleep")
                {
                    continue;
                }

                if (character.Unconscious)
                {
                    CharacterUtil.Sleep(character);
                    continue;
                }
                else if (character.Laying)
                {
                    CharacterUtil.Rest(character);
                    continue;
                }

                if (task == null &&
                    !character.Unconscious &&
                    !character.Laying &&
                    !CharacterUtil.PulledByPlayer(character))
                {
                    Tasker.GiveTask_Citizen(character);
                }
            }

            JobTask? playerTask = Handler.Player?.Job.CurrentTask;
            if (playerTask != null)
            {
                if (playerTask.Name != "Sneak" &&
                    playerTask.Name != "Walk" &&
                    playerTask.Name != "Run" &&
                    playerTask.Name != "Push")
                {
                    Handler.Player?.Job.Update(TimeManager.Now);
                }
            }
        }

        public static void SecondsChanged(object? sender, EventArgs e)
        {
            Army? army = CharacterManager.GetArmy("Characters");
            if (army == null)
            {
                return;
            }

            foreach (Squad squad in army.Squads)
            {
                for (int i = 0; i < squad.Characters.Count; i++)
                {
                    Character character = squad.Characters[i];

                    if (character.Dead)
                    {
                        continue;
                    }

                    if (character.Stats.Blood <= 0)
                    {
                        character.Dead = true;
                        i--;
                        continue;
                    }

                    JobTask? task = character.Job.CurrentTask;
                    if (task?.Name == "Sleep")
                    {
                        CharacterUtil.Sleep(character);
                    }
                    else if (character.Type == "Citizen")
                    {
                        character.Stats.Boredom += Handler.BoredomRate;
                        if (character.Stats.Boredom > 100)
                        {
                            character.Stats.Boredom = 100;
                        }
                    }

                    character.Stats.Thirst += Handler.ThirstRate;
                    if (character.Stats.Thirst > 100)
                    {
                        character.Stats.Thirst = 100;
                    }

                    character.Stats.Hunger += Handler.HungerRate;
                    if (character.Stats.Hunger > 100)
                    {
                        character.Stats.Hunger = 100;
                    }

                    CharacterUtil.UpdateWounds(character);
                    CharacterUtil.UpdatePain(character);
                    CharacterUtil.UpdateBloodLoss(character);
                    CharacterUtil.UpdateConsciousness(character);
                }
            }
        }

        public static void MinutesChanged(object? sender, EventArgs e)
        {
            if (RenderingManager.Lighting?.LerpAmount < 1)
            {
                RenderingManager.Lighting.LerpAmount += 0.0167f;
            }
            RenderingManager.Lighting?.Update();

            Army? characters = CharacterManager.GetArmy("Characters");
            if (characters == null)
            {
                return;
            }

            foreach (Squad squad in characters.Squads)
            {
                foreach (Character character in squad.Characters)
                {
                    if (character.Dead)
                    {
                        continue;
                    }

                    Property? poisoned = character.GetStatusEffect("Poisoned");
                    if (poisoned != null)
                    {
                        character.Stats.Blood--;
                        if (character.Stats.Blood < 0)
                        {
                            character.Stats.Blood = 0;
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

        public static void HoursChanged(object? sender, EventArgs e)
        {
            if (RenderingManager.Lighting != null)
            {
                RenderingManager.Lighting.LerpAmount = 0;
            }

            Army? characters = CharacterManager.GetArmy("Characters");
            if (characters == null)
            {
                return;
            }

            foreach (Squad squad in characters.Squads)
            {
                foreach (Character character in squad.Characters)
                {
                    if (!character.Dead)
                    {
                        if (character.Stats.Thirst >= 100)
                        {
                            character.Stats.Blood--;
                            if (character.Stats.Blood < 0)
                            {
                                character.Stats.Blood = 0;
                                character.Dead = true;
                            }
                        }

                        if (character.Stats.Hunger >= 100)
                        {
                            character.Stats.Stamina--;
                            if (character.Stats.Stamina < 0)
                            {
                                character.Stats.Stamina = 0;
                            }
                        }
                    }
                }
            }
        }

        public static void Reset()
        {
            TimeManager.Init(1984, 6, 1, 6);
            RenderingManager.Lighting?.Reset();

            if (TimeManager.Now == null)
            {
                return;
            }

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
