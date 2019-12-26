using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.SimAntics.NetPlay.EODs.Model;
using FSO.SimAntics.NetPlay.EODs.Utils;

namespace FSO.SimAntics.NetPlay.EODs.Handlers
{
    public class VMEODMazeTwoPlugin : VMEODHandler
    {
        private AbstractMaze<FreeSOMazeData> CurrentMaze;
        private AbstractMazeCell<FreeSOMazeData>[,] RawMaze;
        private FreeSOMazePlayer CharismaPlayer;
        private FreeSOMazePlayer LogicPlayer;
        private FreeSOMazeStates GameState;
        private FreeSOMazeStates NextState;
        private FreeSOMazeDifficulties ChosenMazeDifficulty;
        private List<AbstractMazeCell<FreeSOMazeData>> EveryDeadEndCell;
        private List<AbstractMazeCell<FreeSOMazeData>> Solution;
        private Random ThankU;
        private VMEODClient Controller;
        private int CumulativeHintProbability;
        private int MazeTimeRemaining;
        private int MinimumSolutionMoves;
        private int Tock;

        private readonly object DifficultyLock = new object();
        private readonly object MoveLock = new object();
        private const decimal SKILL_PAYOUT_MULTIPLIER = 20m;
        internal static readonly int GLOBAL_COOLDOWN = 4;

        public Dictionary<FreeSOMazeDifficulties, int> MazeSizes = new Dictionary<FreeSOMazeDifficulties, int>()
        {
            { FreeSOMazeDifficulties.Easy, 8 },
            { FreeSOMazeDifficulties.Normal, 10 },
            { FreeSOMazeDifficulties.Hard, 15 }
        };
        // Item1 = Per Solution Cell, Item2 = Traversing Cell initial, Traversing Cell rising interval
        public Dictionary<FreeSOMazeDifficulties, Tuple<int, int, int>> HintProbabilities = new Dictionary<FreeSOMazeDifficulties, Tuple<int, int, int>>()
        {
            { FreeSOMazeDifficulties.Easy, new Tuple<int, int, int>(25, 20, 10) },
            { FreeSOMazeDifficulties.Normal, new Tuple<int, int, int>(10, 20, 5) },
            { FreeSOMazeDifficulties.Hard, new Tuple<int, int, int>(5, 10, 5) }
        };
        // Round time in seconds
        public Dictionary<FreeSOMazeDifficulties, int> RoundTimes = new Dictionary<FreeSOMazeDifficulties, int>()
        {
            { FreeSOMazeDifficulties.Easy, 300 },
            { FreeSOMazeDifficulties.Normal, 480 },
            { FreeSOMazeDifficulties.Hard, 600 }
        };
        public Dictionary<FreeSOMazeDifficulties, int> BasePayouts = new Dictionary<FreeSOMazeDifficulties, int>()
        {
            { FreeSOMazeDifficulties.Easy, 300 },
            { FreeSOMazeDifficulties.Normal, 600 },
            { FreeSOMazeDifficulties.Hard, 1000 }
        };


        public VMEODMazeTwoPlugin(VMEODServer server) : base(server)
        {
            NextState = FreeSOMazeStates.Invalid;
            ThankU = new Random();
            BinaryHandlers["FreeSOMaze_choose_difficulty"] = ChooseDifficultyHandler;
            BinaryHandlers["FreeSOMaze_loaded"] = FirstLoadHandler;
            BinaryHandlers["FreeSOMaze_partner_failsafe"] = FirstLoadHandler;
            BinaryHandlers["FreeSOMaze_move_request"] = MoveRequestHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public override void OnConnection(VMEODClient client)
        {
            if (client.Avatar != null)
            {
                // get the temps, [0] == 1 is logic, 2 is charisma
                var args = client.Invoker.Thread.TempRegisters;
                if (args[0] == 1)
                {
                    LogicPlayer = new FreeSOMazePlayer(client, client.Avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.LogicSkill) / 100m, true);
                    client.Send("FreeSOMaze_show_l", BitConverter.GetBytes(client.Avatar.ObjectID));
                }
                else
                {
                    CharismaPlayer = new FreeSOMazePlayer(client, client.Avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.CharismaSkill) / 100m, false);
                    client.Send("FreeSOMaze_show_c", BitConverter.GetBytes(client.Avatar.ObjectID));
                }
                EnqueueGotoState(FreeSOMazeStates.Lobby);
                base.OnConnection(client);
            }
            else
                Controller = client;
        }

        public override void Tick()
        {
            if (!NextState.Equals(FreeSOMazeStates.Invalid))
            {
                var state = NextState;
                NextState = FreeSOMazeStates.Invalid;
                GotoState(state);
            }
            switch (GameState)
            {
                case FreeSOMazeStates.LoadingMaze:
                    { 
                        if (++Tock > 30)
                        {
                            Tock = 0;
                            if (++MazeTimeRemaining >= GLOBAL_COOLDOWN)
                                EnqueueGotoState(FreeSOMazeStates.NavigatingMaze);
                        }
                        break;
                    }
                case FreeSOMazeStates.NavigatingMaze:
                    {
                        if (++Tock > 30)
                        {
                            Tock = 0;
                            BroadcastSharedEvent("FreeSOMaze_time", BitConverter.GetBytes(--MazeTimeRemaining));
                            if (MazeTimeRemaining <= 0)
                                DoTimeExpired();
                            else
                            {
                                if (CharismaPlayer != null)
                                    CharismaPlayer.Cooldown++;
                                if (LogicPlayer != null)
                                    LogicPlayer.Cooldown++;
                            }
                        }
                        break;
                    }
                case FreeSOMazeStates.Gameover:
                    {
                        if (++Tock > 30)
                        {
                            Tock = 0;

                            if (++MazeTimeRemaining >= GLOBAL_COOLDOWN * 2)
                                EnqueueGotoState(FreeSOMazeStates.Lobby);
                            else
                            {
                                if (LogicPlayer != null && ++LogicPlayer.Cooldown >= GLOBAL_COOLDOWN)
                                    LogicPlayer.ProcessPayout();
                                if (CharismaPlayer != null && ++CharismaPlayer.Cooldown >= GLOBAL_COOLDOWN)
                                    CharismaPlayer.ProcessPayout();
                            }
                        }
                        break;
                    }
            }
            base.Tick();
        }

        /// <summary>
        /// Send a disposing event to the player leaving, alert the partner UIEOD that their partner just left.
        /// </summary>
        /// <param name="client">quitter</param>
        public override void OnDisconnection(VMEODClient client)
        {
            // identify the person that left and determine if there's a partner
            FreeSOMazePlayer disconnecting = null;
            FreeSOMazePlayer partner = null;
            if (CharismaPlayer?.Client?.Equals(client) ?? false)
            {
                disconnecting = CharismaPlayer;
                CharismaPlayer = null;
                partner = LogicPlayer;
            }
            else if (LogicPlayer?.Client?.Equals(client) ?? false)
            {
                disconnecting = LogicPlayer;
                LogicPlayer = null;
                partner = CharismaPlayer;
            }
            disconnecting?.Send("FreeSOMaze_dispose", new byte[0]);
            partner?.Send("FreeSOMaze_partner_disconnected", new byte[0]);
            EnqueueGotoState(FreeSOMazeStates.Lobby);
            base.OnDisconnection(client);
        }

        #region Event_Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maze"></param>
        private void MazeGeneratedHandler(AbstractMazeCell<FreeSOMazeData>[,] maze)
        {
            RawMaze = maze;
            try
            {
                var charIndex = ThankU.Next(0, EveryDeadEndCell.Count);
                CharismaPlayer.Location = EveryDeadEndCell[charIndex];
                lock (EveryDeadEndCell)
                    EveryDeadEndCell.Remove(CharismaPlayer.Location);
                var logicIndex = ThankU.Next(0, EveryDeadEndCell.Count);
                LogicPlayer.Location = EveryDeadEndCell[logicIndex];
            }
            catch (Exception e)
            {
                Console.WriteLine("oops " + e.Message);
            }
            EveryDeadEndCell = new List<AbstractMazeCell<FreeSOMazeData>>();

            Solution = CurrentMaze.GetPathFromOriginToExit(CharismaPlayer.Location, LogicPlayer.Location, -1);

            // add hints based on hint probability
            if (Solution != null)
            {
                MinimumSolutionMoves = Solution.Count;
                var hintProbability = HintProbabilities[ChosenMazeDifficulty].Item1;
                for (int cell = 0; cell < Solution.Count; cell++)
                {
                    Solution[cell].CellData.UnvisitedSolution = true;
                    var roll = ThankU.Next(1, 101);
                    if (roll <= hintProbability)
                        Solution[cell].CellData.ShowHint = true;
                }
                EnqueueGotoState(FreeSOMazeStates.LoadingMaze);
                CumulativeHintProbability = HintProbabilities[ChosenMazeDifficulty].Item2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        private void OnDeadEndHandler(AbstractMazeCell<FreeSOMazeData> cell)
        {
            cell.CellData.IsDeadEnd = true;
            EveryDeadEndCell.Add(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        private void OnProcessedCellHandler(AbstractMazeCell<FreeSOMazeData> cell)
        {
            // set the barrier data now for ease of access later
            byte north = 0;
            byte east = 0;
            byte west = 0;
            byte south = 0;
            if (cell.North_Neighbor == null)
                north = 1;
            if (cell.East_Neighbor == null)
                east = 1;
            if (cell.West_Neighbor == null)
                west = 1;
            if (cell.South_Neighbor == null)
                south = 1;
            cell.CellData.SetBarriers(north, east, west, south);
        }

        /// <summary>
        /// When the user has loaded all of the textures for the first time. If their partner exists, send their object id and their difficulty choice, if applicable.
        /// </summary>
        /// <param name="evt">"FreeSOMaze_loaded"</param>
        /// <param name="nothing"></param>
        /// <param name="client"></param>
        private void FirstLoadHandler(string evt, byte[] nothing, VMEODClient client)
        {
            FreeSOMazePlayer caller = null;
            FreeSOMazePlayer partner = null;
            if (LogicPlayer != null && client.Equals(LogicPlayer.Client))
            {
                caller = LogicPlayer;
                partner = CharismaPlayer;
            }
            else
            {
                caller = CharismaPlayer;
                partner = LogicPlayer;
            }
            caller.Loaded();
            SendLobbyInfoEvent(caller, partner);
            SendLobbyInfoEvent(partner, caller);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="chosenDifficulty"></param>
        /// <param name="client"></param>
        private void ChooseDifficultyHandler(string evt, byte[] chosenDifficulty, VMEODClient client)
        {
            if (GameState.Equals(FreeSOMazeStates.Lobby) && NextState.Equals(FreeSOMazeStates.Invalid))
            {
                FreeSOMazePlayer caller = null;
                FreeSOMazePlayer partner = null;
                if (LogicPlayer != null && client.Equals(LogicPlayer.Client))
                {
                    caller = LogicPlayer;
                    partner = CharismaPlayer;
                }
                else
                {
                    caller = CharismaPlayer;
                    partner = LogicPlayer;
                }
                int difficultyNum = BitConverter.ToInt32(chosenDifficulty, 0);
                FreeSOMazeDifficulties difficulty = FreeSOMazeDifficulties.Unselected;
                if (Enum.IsDefined(typeof(FreeSOMazeDifficulties), difficultyNum))
                    difficulty = (FreeSOMazeDifficulties)Enum.ToObject(typeof(FreeSOMazeDifficulties), difficultyNum);

                ValidateDifficulty(caller, difficulty, partner);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="chosenCardinal"></param>
        /// <param name="client"></param>
        private void MoveRequestHandler(string evt, byte[] chosenCardinal, VMEODClient client)
        {
            if (GameState.Equals(FreeSOMazeStates.NavigatingMaze) && NextState.Equals(FreeSOMazeStates.Invalid))
            {
                FreeSOMazePlayer player = null;
                FreeSOMazePlayer partner = null;
                if (LogicPlayer?.Client.Equals(client) ?? false)
                {
                    player = LogicPlayer;
                    partner = CharismaPlayer;
                }
                else if (CharismaPlayer?.Client.Equals(client) ?? false)
                {
                    player = CharismaPlayer;
                    partner = LogicPlayer;
                }
                if (player != null)
                {
                    if (player.Cooldown >= GLOBAL_COOLDOWN && Enum.IsDefined(typeof(FreeSOMazeCardinals), chosenCardinal[0]))
                    {
                        var location = player.Location;
                        var cardinal = (FreeSOMazeCardinals)Enum.ToObject(typeof(FreeSOMazeCardinals), chosenCardinal[0]);
                        AbstractMazeCell<FreeSOMazeData> target = null;
                        switch (cardinal)
                        {
                            case FreeSOMazeCardinals.North: target = location.North_Neighbor; break;
                            case FreeSOMazeCardinals.East: target = location.East_Neighbor; break;
                            case FreeSOMazeCardinals.West: target = location.West_Neighbor; break;
                            case FreeSOMazeCardinals.South: target = location.South_Neighbor; break;
                        }
                        if (target != null) // it is a legal move
                        {
                            ValidateLegalMove(player, target, cardinal, partner);
                            return;
                        }
                    }
                    SendAllowMazeEvent(player, false); // re-enables input
                }
            }
        }
        #endregion

        #region Private

        /// <summary>
        /// Thread safe, isn't handled until next tick.
        /// </summary>
        /// <param name="next"></param>
        private void EnqueueGotoState(FreeSOMazeStates next)
        {
            if (!next.Equals(GameState))
                NextState = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void GotoState(FreeSOMazeStates state)
        {
            GameState = state;
            Tock = 0;
            switch (state)
            {
                case FreeSOMazeStates.Lobby:
                    {
                        ChosenMazeDifficulty = FreeSOMazeDifficulties.Unselected;
                        CharismaPlayer?.Reset();
                        if (CharismaPlayer?.IsLoaded ?? false)
                            SendLobbyInfoEvent(CharismaPlayer, LogicPlayer);
                        LogicPlayer?.Reset();
                        if (LogicPlayer?.IsLoaded ?? false)
                            SendLobbyInfoEvent(LogicPlayer, CharismaPlayer);

                        if (CurrentMaze != null)
                        {
                            CurrentMaze.OnMazeGenerated -= MazeGeneratedHandler;
                            CurrentMaze.OnFinalProcessingCell -= OnProcessedCellHandler;
                            CurrentMaze.OnDeadEndCreation -= OnDeadEndHandler;
                            Solution = null;
                            RawMaze = null;
                            MazeTimeRemaining = 0;
                            BroadcastSharedEvent("FreeSOMaze_time", BitConverter.GetBytes(MazeTimeRemaining));
                        }
                        else
                            EveryDeadEndCell = new List<AbstractMazeCell<FreeSOMazeData>>();
                        break;
                    }
                case FreeSOMazeStates.GeneratingMaze:
                    {
                        CurrentMaze = AbstractMazeGenerator<FreeSOMazeData>.GetEmptyMaze(MazeSizes[ChosenMazeDifficulty], MazeSizes[ChosenMazeDifficulty]);
                        CurrentMaze.OnMazeGenerated += MazeGeneratedHandler;
                        CurrentMaze.OnFinalProcessingCell += OnProcessedCellHandler;
                        CurrentMaze.OnDeadEndCreation += OnDeadEndHandler;
                        var origin = ThankU.Next(0, (int)BuildFromOrigins.Dead_Center + 1);
                        CurrentMaze.BuildFromOrigin(origin);
                        BroadcastSharedEvent("FreeSOMaze_goto_maze", BitConverter.GetBytes((int)ChosenMazeDifficulty));
                        break;
                    }
                case FreeSOMazeStates.LoadingMaze:
                    {
                        MazeTimeRemaining = 0;
                        int cardinal = 0;
                        if (CharismaPlayer != null)
                        {
                            cardinal = GetSolutionCardinal(CharismaPlayer);
                            CharismaPlayer.CurrentFacingCardinal = (FreeSOMazeCardinals)Enum.ToObject(typeof(FreeSOMazeCardinals), cardinal);
                            CharismaPlayer.Send("FreeSOMaze_show_maze", CharismaPlayer.GetLocationData((int)FreeSOMazeCardinals.Invalid));
                        }
                        if (LogicPlayer != null)
                        {
                            cardinal = GetSolutionCardinal(LogicPlayer);
                            LogicPlayer.CurrentFacingCardinal = (FreeSOMazeCardinals)Enum.ToObject(typeof(FreeSOMazeCardinals), cardinal);
                            LogicPlayer.Send("FreeSOMaze_show_maze", LogicPlayer.GetLocationData((int)FreeSOMazeCardinals.Invalid));
                        }
                        break;
                    }
                case FreeSOMazeStates.NavigatingMaze:
                    {
                        CharismaPlayer.Cooldown = GLOBAL_COOLDOWN;
                        LogicPlayer.Cooldown = GLOBAL_COOLDOWN;
                        SendAllowMazeEvent(CharismaPlayer, false);
                        SendAllowMazeEvent(LogicPlayer, false);
                        MazeTimeRemaining = RoundTimes[ChosenMazeDifficulty];
                        BroadcastSharedEvent("FreeSOMaze_time", BitConverter.GetBytes(MazeTimeRemaining));
                        break;
                    }
                case FreeSOMazeStates.Gameover:
                    {
                        MazeTimeRemaining = 0;
                        break;
                    }
            }
        }

        /// <summary>
        /// Send identical events to both players at the same time.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="data"></param>
        private void BroadcastSharedEvent(string evt, byte[] data)
        {
            if (LogicPlayer != null)
                LogicPlayer.Send(evt, data);
            if (CharismaPlayer != null)
                CharismaPlayer.Send(evt, data);
        }
        /// <summary>
        /// Send to the partner the the object id of this player and their difficulty choice, if applicable.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="sendFrom"></param>
        private void SendLobbyInfoEvent(FreeSOMazePlayer sendTo, FreeSOMazePlayer sendFrom)
        {
            if (sendTo != null)
            {
                string concat = "";
                short objectID = 0;
                if (sendFrom != null)
                {
                    if (sendFrom.Client != null)
                        objectID = sendFrom.Client.Avatar.ObjectID;
                    if (!sendFrom.Difficulty.Equals(FreeSOMazeDifficulties.Unselected))
                        concat = "" + (int)sendFrom.Difficulty;
                }
                sendTo.Send("FreeSOMaze_lobby_info" + concat, BitConverter.GetBytes(objectID));
            }
        }
        /// <summary>
        /// Send to the player the event that will allow their input doing the maze gameplay, and their direction/barrier/hint data.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rollForHint">If true, execute the random to determine if the hint should be sent, if false just send only if hint already exists</param>
        private void SendAllowMazeEvent(FreeSOMazePlayer player, bool rollForHint)
        {
            if (player?.Cooldown >= GLOBAL_COOLDOWN)
                player?.Send("FreeSOMaze_allow_maze", player?.GetLocationData(GetHint(player, rollForHint)));
            else
                player?.QueueAllowMazeEvent("FreeSOMaze_allow_maze", player?.GetLocationData(GetHint(player, rollForHint)));

        }
        /// <summary>
        /// Validates the difficulties of both players, triggering the start of the game only if they both agree.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="chosenDifficulty"></param>
        /// <param name="partner"></param>
        private void ValidateDifficulty(FreeSOMazePlayer caller, FreeSOMazeDifficulties chosenDifficulty, FreeSOMazePlayer partner)
        {
            lock (DifficultyLock)
            {
                if (GameState.Equals(FreeSOMazeStates.Lobby) && NextState.Equals(FreeSOMazeStates.Invalid))
                {
                    caller.Difficulty = chosenDifficulty;
                    if (partner != null)
                    {
                        if (caller.Difficulty.Equals(partner.Difficulty))
                        {
                            ChosenMazeDifficulty = chosenDifficulty;
                            EnqueueGotoState(FreeSOMazeStates.GeneratingMaze);
                        }
                        else
                            partner.Send("FreeSOMaze_partner_difficulty", BitConverter.GetBytes((int)chosenDifficulty));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="target"></param>
        /// <param name="cardinal"></param>
        /// <param name="partner"></param>
        private void ValidateLegalMove(FreeSOMazePlayer caller, AbstractMazeCell<FreeSOMazeData> target, FreeSOMazeCardinals cardinal, FreeSOMazePlayer partner)
        {
            if (caller != null && partner != null)
            {
                lock (MoveLock)
                {
                    if (GameState.Equals(FreeSOMazeStates.NavigatingMaze) && NextState.Equals(FreeSOMazeStates.Invalid) && caller.Cooldown >= GLOBAL_COOLDOWN)
                    {
                        bool rollForHint = true;
                        caller.MoveTo(target);
                        caller.Cooldown = 0;
                        caller.CurrentFacingCardinal = cardinal;
                        // is the partner at the target, meaning have the two met in any cell
                        if (partner.Location.Equals(target))
                        {
                            // gameover win!
                            rollForHint = false;
                            EnqueueGotoState(FreeSOMazeStates.Gameover);

                            // payout data
                            var skillPayout = (int)Math.Round((caller.Skill + partner.Skill) * SKILL_PAYOUT_MULTIPLIER, 0);
                            var callerPayoutData = new string[] { BasePayouts[ChosenMazeDifficulty] + "", skillPayout + "", RoundTimes[ChosenMazeDifficulty] - MazeTimeRemaining + "", caller.TotalMoves + "", partner.TotalMoves + "" };
                            var partnerPayoutData = new string[] { BasePayouts[ChosenMazeDifficulty] + "", skillPayout + "", RoundTimes[ChosenMazeDifficulty] - MazeTimeRemaining + "", partner.TotalMoves + "", caller.TotalMoves + "" };
                            var totalPayout = skillPayout + BasePayouts[ChosenMazeDifficulty];

                            // queue gameover for caller
                            caller.QueuePayoutEvent("FreeSOMaze_win", callerPayoutData, totalPayout);
                            // queue gameover for partner
                            partner.QueuePayoutEvent("FreeSOMaze_win", partnerPayoutData, totalPayout);

                            // pay the object owner now, keeping in tradition with 10% of participant(s) payout
                            Controller.SendOBJEvent(new VMEODEvent((short)FreeSOMazeEvents.PayOwner, new short[] { (short)(totalPayout / 10) }));
                        }
                        caller.Send("FreeSOMaze_move_to", caller.GetLocationData(GetHint(caller, rollForHint)));
                        if (rollForHint) // don't send allow maze event on a gameover
                            SendAllowMazeEvent(caller, false);
                    }
                }
            }
        }

        /// <summary>
        /// Send the time ran out event and disallow players' input.
        /// </summary>
        private void DoTimeExpired()
        {
            BroadcastSharedEvent("FreeSOMaze_time_expired", new byte[0]);
            if (LogicPlayer != null)
                LogicPlayer.Cooldown = 0;
            if (CharismaPlayer != null)
                CharismaPlayer.Cooldown = 0;
            EnqueueGotoState(FreeSOMazeStates.Gameover);
        }

        /// <summary>
        /// Pay the player from Maxis the amount specified.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        private void ExecutePlayerPayout(FreeSOMazePlayer player, int amount)
        {
            if (player != null && player.Client != null)
            {
                var vm = player.Client.vm;
                // payout from Maxis to the player
                vm.GlobalLink.PerformTransaction(vm, false, uint.MaxValue, player.Client.Avatar.PersistID, amount,
                    (bool success, int transferAmount, uint uid1, uint budget1, uint uid2, uint budget2) => { });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int GetHint(FreeSOMazePlayer player, bool rollForHint)
        {
            if (player != null)
            {
                if (player.Location.CellData.IsDeadEnd)
                {
                    player.Send("FreeSOMaze_alert", "Server: I sent hint: " + (int)FreeSOMazeDirections.None);
                    return (int)FreeSOMazeDirections.None;
                }
                else if (player.Location.CellData.ShowHint)
                {
                    var hint = GetSolutionCardinal(player);
                    player.Send("FreeSOMaze_alert", "Server: I sent hint: " + hint);
                    return hint;
                    //return GetSolutionCardinal(player);
                }
                else if (rollForHint)
                {
                    // roll for a hint
                    var roll = ThankU.Next(0, 100);
                    if (roll < CumulativeHintProbability)
                    {
                        CumulativeHintProbability = HintProbabilities[ChosenMazeDifficulty].Item2;
                        player.Location.CellData.ShowHint = true;
                        var hint = GetSolutionCardinal(player);
                        player.Send("FreeSOMaze_alert", "Server: I sent hint: " + hint);
                        return hint;
                        //return GetSolutionCardinal(player);
                    }
                    else
                        CumulativeHintProbability += HintProbabilities[ChosenMazeDifficulty].Item3;

                }
            }
            player.Send("FreeSOMaze_alert", "Server: I sent no hint");
            return (int)FreeSOMazeCardinals.Invalid; // no hint
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int GetSolutionCardinal(FreeSOMazePlayer player)
        {
            var solution = player.PeekSolution();
            if (solution != null)
                return (int)player.GetTargetCardinal(player.Location, solution);
            // if there are no nodes left in Solution and this player has no bread crumbs, then they're next to the partner's very first breadcrumb.
            else
            {
                FreeSOMazePlayer partner = (player.Equals(LogicPlayer) ? CharismaPlayer : LogicPlayer);
                solution = partner?.FirstCrumb;
                if (solution != null)
                    return (int)partner.GetTargetCardinal(player.Location, solution);
            }
            return (int)FreeSOMazeCardinals.Invalid; // 10 is no hint, but if this is reached then something went very wrong
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal class FreeSOMazePlayer
    {
        private VMEODClient _Client;
        private int _Cooldown;
        private int _Payout;
        private int _TotalMoves;
        private FreeSOMazeCardinals _CurrentFacingCardinal;
        private FreeSOMazeDifficulties _Difficulty;
        private bool _FirstLoadCompleted;
        private bool IsLogicPlayer;
        private AbstractMazeCell<FreeSOMazeData> _Location;
        private List<AbstractMazeCell<FreeSOMazeData>> _BreadCrumbs;
        private decimal _Skill;
        private List<Tuple<string, byte[]>> DelayedEvents = new List<Tuple<string, byte[]>>();

        internal FreeSOMazePlayer(VMEODClient client, decimal skill, bool iAmLogicPlayer)
        {
            _Client = client;
            _Skill = skill;
            IsLogicPlayer = iAmLogicPlayer;
            Reset();
        }
        internal VMEODClient Client
        {
            get { return _Client; }
        }
        /// <summary>
        /// Converts Invalid into North
        /// </summary>
        internal FreeSOMazeCardinals CurrentFacingCardinal
        {
            get { return _CurrentFacingCardinal; }
            set
            {
                if (value.Equals(FreeSOMazeCardinals.Invalid))
                    _CurrentFacingCardinal = FreeSOMazeCardinals.North;
                else
                    _CurrentFacingCardinal = value;
            }
        }
        internal int Cooldown
        {
            get { return _Cooldown; }
            set
            {
                _Cooldown = value;
                if (_Cooldown >= VMEODMazeTwoPlugin.GLOBAL_COOLDOWN)
                {
                    lock (DelayedEvents)
                    {
                        if (DelayedEvents.Count > 0)
                            Send(DelayedEvents[0].Item1, DelayedEvents[0].Item2);
                        DelayedEvents.Clear();
                    }
                }
            }
        }
        internal int Crumbs
        {
            get { return _BreadCrumbs.Count; }
        }
        internal int TotalMoves
        {
            get { return _TotalMoves; }
        }
        internal int PendingPayout
        {
            get { return _Payout; }
        }
        internal FreeSOMazeDifficulties Difficulty
        {
            get { return _Difficulty; }
            set { _Difficulty = value; }
        }
        internal AbstractMazeCell<FreeSOMazeData> FirstCrumb
        {
            get
            {
                lock (_BreadCrumbs)
                {
                    if (_BreadCrumbs.Count > 0)
                        return _BreadCrumbs[_BreadCrumbs.Count - 1];
                    return null;
                }
            }
        }
        internal AbstractMazeCell<FreeSOMazeData> LastCrumb
        {
            get
            {
                lock (_BreadCrumbs)
                {
                    if (_BreadCrumbs.Count > 0)
                        return _BreadCrumbs[0];
                    return null;
                }
            }
        }
        internal AbstractMazeCell<FreeSOMazeData> Location
        {
            get { return _Location; }
            set { _Location = value; }
        }
        internal decimal Skill
        {
            get { return _Skill; }
        }
        internal bool IsLoaded
        {
            get { return _FirstLoadCompleted; }
        }
        internal void Loaded()
        {
            _FirstLoadCompleted = true;
            lock (DelayedEvents)
            {
                for (int index = 0; index < DelayedEvents.Count; index++)
                    Send(DelayedEvents[index].Item1, DelayedEvents[index].Item2);
                DelayedEvents.Clear();
            }
        }
        internal byte[] GetLocationData(int hint)
        {
            var data = new byte[7];
            data[0] = (byte)CurrentFacingCardinal;
            // N E W S barriers
            data[1] = Location.CellData.Barriers[0];
            data[2] = Location.CellData.Barriers[1];
            data[3] = Location.CellData.Barriers[2];
            data[4] = Location.CellData.Barriers[3];
            data[5] = (byte)hint;
            bool visited = (IsLogicPlayer) ? Location.CellData.LogicVisited : Location.CellData.CharismaVisited;
            data[6] = (visited) ? (byte)1 : (byte)0;
            return data;
        }
        internal void MoveTo(AbstractMazeCell<FreeSOMazeData> target)
        {
            // mark the location we're leaving as visited
            Location.CellData.UnvisitedSolution = false;
            if (IsLogicPlayer)
                Location.CellData.LogicVisited = true;
            else
                Location.CellData.CharismaVisited = true;
            lock (_BreadCrumbs)
            {
                if (target.CellData.UnvisitedSolution)
                    _BreadCrumbs = new List<AbstractMazeCell<FreeSOMazeData>>();
                else
                {
                    if (!_BreadCrumbs.Contains(target))
                        _BreadCrumbs.Insert(0, Location);
                    else
                    {
                        _BreadCrumbs.Remove(Location);
                        _BreadCrumbs.Remove(target);
                    }
                }
            }
            Location = target;
            _TotalMoves++;
        }
        internal AbstractMazeCell<FreeSOMazeData> PeekSolution()
        {
            var crumb = LastCrumb;
            if (crumb != null)
                return crumb;
            else
            {
                if (Location.North_Neighbor?.CellData.UnvisitedSolution ?? false)
                    return Location.North_Neighbor;
                if (Location.East_Neighbor?.CellData.UnvisitedSolution ?? false)
                    return Location.East_Neighbor;
                if (Location.West_Neighbor?.CellData.UnvisitedSolution ?? false)
                    return Location.West_Neighbor;
                if (Location.South_Neighbor?.CellData.UnvisitedSolution ?? false)
                    return Location.South_Neighbor;
            }
            return null;
        }
        internal FreeSOMazeCardinals GetTargetCardinal(AbstractMazeCell<FreeSOMazeData> source, AbstractMazeCell<FreeSOMazeData> target)
        {
            _Client.Send("FreeSOMaze_alert", "Server: source is: " +source.Row + " " + source.Column + " and target: " + target.Row + " " + target.Column);
            if (source?.North_Neighbor?.Equals(target) ?? false)
                return FreeSOMazeCardinals.North;
            if (source?.East_Neighbor?.Equals(target) ?? false)
                return FreeSOMazeCardinals.East;
            if (source?.West_Neighbor?.Equals(target) ?? false)
                return FreeSOMazeCardinals.West;
            if (source?.South_Neighbor?.Equals(target) ?? false)
                return FreeSOMazeCardinals.South;
            return FreeSOMazeCardinals.Invalid;
        }
        internal void Reset()
        {
            _BreadCrumbs = new List<AbstractMazeCell<FreeSOMazeData>>();
            _Difficulty = FreeSOMazeDifficulties.Unselected;
            _Location = null;
            _Cooldown = 0;
            _TotalMoves = 0;
            _Payout = 0;
            DelayedEvents = new List<Tuple<string, byte[]>>();
        }
        internal void Send(string evt, byte[] data)
        {
            if (_Client != null)
            {
                if (_FirstLoadCompleted)
                    _Client.Send(evt, data);
                else
                {
                    lock (DelayedEvents)
                        DelayedEvents.Add(new Tuple<string, byte[]>(evt, data));
                }
            }
        }
        internal void Send(string evt, string msg)
        {
            if (_Client != null)
                _Client.Send(evt, msg);
        }
        internal void QueueAllowMazeEvent(string allowMaze, byte[] data)
        {
            lock (DelayedEvents)
            {
                if (DelayedEvents.Count == 0)
                    DelayedEvents.Add(new Tuple<string, byte[]>(allowMaze, data));
            }
        }
        internal void QueuePayoutEvent(string gameoverEvent, string[] payoutData, int totalPayoutAmount)
        {
            _Payout = totalPayoutAmount;
            lock (DelayedEvents)
            {
                DelayedEvents.Clear();
                DelayedEvents.Add(new Tuple<string, byte[]>( gameoverEvent, Data.VMEODGameCompDrawACardData.SerializeStrings(payoutData)));
            }
        }
        internal int ProcessPayout()
        {
            lock (DelayedEvents)
            {
                if (DelayedEvents.Count > 0)
                    Send(DelayedEvents[0].Item1, DelayedEvents[0].Item2);
                DelayedEvents.Clear();
            }
            var payout = _Payout;
            _Payout = 0;
            return payout;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class FreeSOMazeData
    {
        private byte[] _Barriers; // N E W S 
        private bool _UnvisitedSolution;
        private bool _CharismaVisited;
        private bool _LogicVisited;
        private bool _ShowHint;
        private bool _IsDeadEnd;

        internal byte[] Barriers
        {
            get { return _Barriers; }
        }
        internal bool UnvisitedSolution
        {
            get { return _UnvisitedSolution; }
            set { _UnvisitedSolution = value; }
        }
        internal bool CharismaVisited
        {
            get { return _CharismaVisited; }
            set { _CharismaVisited = value; }
        }
        internal bool LogicVisited
        {
            get { return _LogicVisited; }
            set { _LogicVisited = value; }
        }
        internal bool IsDeadEnd
        {
            get { return _IsDeadEnd; }
            set { _IsDeadEnd = value; }
        }
        internal bool ShowHint
        {
            get { return _ShowHint; }
            set { _ShowHint = value; }
        }
        internal void SetBarriers(byte north, byte east, byte west, byte south)
        {
            _Barriers = new byte[] { north, east, west, south };
        }
    }
    // not N E W S, and I wish I knew why. Three Cardinal direction enums because they couldn't all be the same?
    public enum FreeSOMazeCardinals: byte
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
        Invalid = 10
    }

    public enum FreeSOMazeDirections: int
    {
        None = 5, // dead end
        Left = -1,
        Backward = -2,
        Forward = 0,
        Right = 1
    }

    public enum FreeSOMazeDifficulties
    {
        Unselected = 0,
        Easy = 12,
        Normal = 13,
        Hard = 14
    }

    public enum FreeSOMazeStates
    {
        Invalid = 0,
        Lobby = 1,
        GeneratingMaze = 2,
        LoadingMaze = 3,
        NavigatingMaze = 4,
        Gameover = 5
    }

    public enum FreeSOMazeEvents : short
    {
        Success = 1,
        Failure = 2,
        PayOwner = 4
    }
}
