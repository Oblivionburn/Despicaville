using System;

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
            bool action = false;

            TimeToAction++;
            if (TimeToAction >= Handler.ActionRate)
            {
                action = true;
                TimeToAction = 0;
            }

            Scene scene = SceneManager.GetScene("Gameplay");

            Squad citizens = CharacterManager.GetArmy("Characters").GetSquad("Citizens");

            Character[] citizens_array = citizens.Characters.ToArray();
            int count = citizens_array.Length;
            for (int i = 0; i < count; i++)
            {
                Character character = citizens_array[i];
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

                    if (!sleeping)
                    {
                        if (!character.Unconscious)
                        {
                            Tasker.Character_StartAction(scene.World, character);

                            if (action)
                            {
                                if (character.Moving)
                                {
                                    character.Move_TotalDistance = Main.Game.TileSize.X;
                                    character.Update();
                                }
                                else
                                {
                                    Tasker.Character_DoAction(scene.World, character);
                                }
                            }

                            if (!character.Moving)
                            {
                                if (task != null)
                                {
                                    if (task.Name == "Sneak" ||
                                        task.Name == "Walk" ||
                                        task.Name == "Run")
                                    {
                                        task.EndTime = new TimeHandler(TimeManager.Now);

                                        if (character.Location == character.Destination)
                                        {
                                            Something stamina = character.GetStat("Stamina");
                                            Something endurance = character.GetStat("Endurance");

                                            if (task.Name == "Sneak")
                                            {
                                                stamina.DecreaseValue(0.0385f / endurance.Value);
                                            }
                                            else if (task.Name == "Walk")
                                            {
                                                stamina.DecreaseValue(0.077f / endurance.Value);
                                            }
                                            else if (task.Name == "Run")
                                            {
                                                stamina.DecreaseValue(0.154f / endurance.Value);
                                            }
                                        }

                                        WorldUtil.SetCurrentMap(character);

                                        if (character.Path.Count > 0)
                                        {
                                            ALocation first_path = character.Path[0];
                                            character.Path.Remove(first_path);

                                            character.Job.Tasks.Add(new Task
                                            {
                                                Name = "GoTo_" + task.Name,
                                                OwnerIDs = GameUtil.OwnerIDs(character),
                                                StartTime = new TimeHandler(TimeManager.Now)
                                            });
                                        }
                                    }
                                    else
                                    {
                                        Tasker.Character_EndAction(scene.World, character);
                                    }
                                }
                                else
                                {
                                    Tasker.GiveTask_Citizen(scene.World, character);
                                }
                            }

                            character.Job.Update(TimeManager.Now);
                        }
                        else
                        {
                            CharacterUtil.Rest(character);
                        }
                    }
                }
            }

            Character player = Handler.GetPlayer();
            if (!player.Unconscious)
            {
                if (action)
                {
                    if (!player.Moving)
                    {
                        Tasker.Character_DoAction(scene.World, player);
                    }
                }
            }
            else
            {
                CharacterUtil.Rest(player);
            }

            if (player.GetStat("Pain").Value < 100 &&
                player.GetStat("Stamina").Value > 0)
            {
                Something consciousness = player.GetStat("Consciousness");
                consciousness.IncreaseValue(0.001f);

                if (consciousness.Value >= 20)
                {
                    player.Unconscious = false;
                }
            }

            player.Job.Update(TimeManager.Now);
        }

        public static void SecondsChanged(object sender, EventArgs e)
        {
            Army army = CharacterManager.GetArmy("Characters");
            foreach (Squad squad in army.Squads)
            {
                Character[] characters = squad.Characters.ToArray();

                int count = characters.Length;
                for (int i = 0; i < count; i++)
                {
                    Character character = characters[i];

                    Something blood = character.GetStat("Blood");
                    if (blood.Value <= 0)
                    {
                        CharacterUtil.Kill(character);
                        squad.Characters.Remove(character);
                        i--;
                    }

                    if (!character.Dead)
                    {
                        bool sleeping = false;
                        Task task = character.Job.CurrentTask;
                        if (task != null)
                        {
                            if (task.Name == "Sleep")
                            {
                                sleeping = true;
                                CharacterUtil.Sleep(character);
                            }
                        }

                        CharacterUtil.UpdateWounds(character);
                        CharacterUtil.UpdateBloodLoss(character);

                        character.GetStat("Thirst").IncreaseValueByRate();
                        character.GetStat("Hunger").IncreaseValueByRate();

                        if (character.Type == "Citizen" &&
                            !sleeping)
                        {
                            character.GetStat("Boredom").IncreaseValueByRate();
                        }

                        Something consciousness = character.GetStat("Consciousness");

                        Something stamina = character.GetStat("Stamina");
                        if (stamina.Value <= 0)
                        {
                            consciousness.DecreaseValue(1);
                        }

                        Something pain = character.GetStat("Pain");
                        if (pain.Value >= 100)
                        {
                            consciousness.DecreaseValue(5);
                        }

                        if (consciousness.Value <= 0)
                        {
                            character.Unconscious = true;
                        }
                        else if (consciousness.Value < 100)
                        {
                            if (pain.Value < 100 &&
                                stamina.Value > 0)
                            {
                                consciousness.IncreaseValue(1);

                                if (consciousness.Value >= 20)
                                {
                                    character.Unconscious = false;
                                }
                            }
                        }
                    }
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
                    if (!character.Dead)
                    {
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

            Character player = Handler.GetPlayer();
            if (!player.Dead)
            {
                Something poisoned = player.GetStatusEffect("Poisoned");
                if (poisoned != null)
                {
                    player.GetStat("Blood").DecreaseValue(1);

                    poisoned.DecreaseValue(1);
                    if (poisoned.Value <= 0)
                    {
                        player.StatusEffects.Remove(poisoned);
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
