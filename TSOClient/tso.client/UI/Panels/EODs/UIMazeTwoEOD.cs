using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Framework.Parser;
using FSO.Content.Model;
using FSO.Common.Utils;
using FSO.HIT;
using FSO.SimAntics.NetPlay.EODs.Handlers;
using FSO.SimAntics.NetPlay.EODs.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FSO.Client.UI.Panels.EODs
{
    public class UIMazeTwoEOD : UIEOD
    {
        private Vector2 BaseOffset = new Vector2(-120, 57);
        private UIMazeTwoEODDirections CurrentFacingCardinal = UIMazeTwoEODDirections.North;
        private float CurrentRotation;
        private bool LobbyVisible;
        private bool PlayerInputAllowed;
        private bool PlayerInputAllowedDelayed;
        private bool SwapBGs;
        private UITween TweenQueue;
        private Timer ThreadLostTimer;
        private Timer YouAreHereTimer;
        private List<Texture2D> DisposeOfMe = new List<Texture2D>();
        private Random T3hPeNgU1NoFd00m = new Random();
        private ulong MaleOutfitID = 0x3620000000d;
        private ulong FemaleOutfitID = 0x1d0000000d;
        private UIAlert ResultsScreen;
        private int FloorTurningTicks;
        private int FloorTurningTextureIndex;

        // texture filepaths
        private string BackgroundEasyTexturePath = "Content/Textures/EOD/Maze2/maze2compasstexe";
        private string BackgroundRegularTexturePath = "Content/Textures/EOD/Maze2/maze2compasstexe";
        private string CompassEdgeTexPath = "Content/Textures/EOD/Maze2/maze2compassedgetex.png";
        private string FloorTurningTexPath = "Content/Textures/EOD/Maze2/maze2floorf";
        private string SkyEasyTexPath = "Content/Textures/EOD/Maze2/maze2skyeasy.png";
        private string HintGoBackwardTexPath = "Content/Textures/EOD/Maze2/maze2hintbackward.png";
        private string HintGoForwardTexPath = "Content/Textures/EOD/Maze2/maze2hintgoforward.png";
        private string HintGoLeftTexPath = "Content/Textures/EOD/Maze2/maze2hintgoleft.png";
        private string HintGoRightTexPath = "Content/Textures/EOD/Maze2/maze2hintgoright.png";
        private string HintDeadEndTexPath = "Content/Textures/EOD/Maze2/maze2hintdeadend.png";
        private string MainScreenBGTexPath = "Content/Textures/EOD/Maze2/maze2mainscreenbgtex.png";
        private string WallsChoiceEasyTexPath = "Content/Textures/EOD/Maze2/wallschoiceeasy.png";
        private string WallsGrowEasyTexPath = "Content/Textures/EOD/Maze2/wallsgroweasy.png";
        private string WallsChoiceRegularTexPath = "Content/Textures/EOD/Maze2/wallschoiceregular.png";
        private string WallsGrowRegularTexPath = "Content/Textures/EOD/Maze2/wallsgrowregular.png";
        private string MapScreenBGTexPath = "Content/Textures/EOD/Maze2/maze2mapscreenbgtex.png";
        private string MapRoomTexPath = "Content/Textures/EOD/Maze2/maze2maproom.png";
        private string MomiLogoTexPath = "Content/Textures/EOD/Maze2/maze2momi.png";
        private string LoadingLogoTexPath = "Content/Textures/EOD/Maze2/maze2loadthinking.png";
        private string EasyLoadingLogoTexPath = "Content/Textures/EOD/Maze2/maze2easyload.png";
        private string RegularLoadingLogoTexPath = "Content/Textures/EOD/Maze2/maze2regularload.png";
        private string HardLoadingLogoTexPath = "Content/Textures/EOD/Maze2/maze2hardload.png";
        private string ScreensEdgeTexPath = "Content/Textures/EOD/Maze2/maze2screensedgetex.png";
        private string CompassTexPath = "Content/Textures/EOD/Maze2/maze2compasstex.png";
        private string DPadTexPath = "Content/Textures/EOD/Maze2/maze2dpad.png";
        private string DPadUpTexPath = "Content/Textures/EOD/Maze2/maze2dpadup.png";
        private string DPadLeftTexPath = "Content/Textures/EOD/Maze2/maze2dpadleft.png";
        private string DPadRightTexPath = "Content/Textures/EOD/Maze2/maze2dpadright.png";
        private string DPadDownTexPath = "Content/Textures/EOD/Maze2/maze2dpaddown.png";
        private string FrontBarrierTexPath = "Content/Textures/EOD/Maze2/maze2frontbarrier.png";
        private string LeftBarrierTexPath = "Content/Textures/EOD/Maze2/maze2leftbarrier.png";
        private string RightBarrierTexPath = "Content/Textures/EOD/Maze2/maze2rightbarrier.png";
        private string PartnerDisconnectedTexPath = "Content/Textures/EOD/Maze2/maze2partnerdisconnect.png";

        // non-moving images
        private UIImage MainScreenBG;
        private UIImage MapScreenBG;
        private UIImage ScreensEdge;
        private UIImage CompassEdge;

        // directional pad and buttons
        private UIImage DirectionalPad;
        private Texture2D DPad;
        private Texture2D DPadUp;
        private Texture2D DPadLeft;
        private Texture2D DPadRight;
        private Texture2D DPadDown;
        private UIInvisibleButton PressUp;
        private UIInvisibleButton PressRight;
        private UIInvisibleButton PressDown;
        private UIInvisibleButton PressLeft;

        // moving images
        private UIImage CompassInner;
        private UIImage AltBackgroundPan;
        private UIMaskedContainer Background;
        private UIImage BackgroundPan;
        private UIImage BackgroundA;
        private UIImage BackgroundB;
        private UIImage HorizonLeft;
        private UIImage HorizonRight;
        private UIImage HorizonWall;
        private UIImage HorizonWallTurnBack;
        private UIImage MainLeft;
        private UIImage MainRight;
        private UIImage MainChoiceWall;
        private UIImage MainTurnWall;
        private UIImage MainTurnBackWall;
        private UIImage MicroWall;
        private UIImage Floor;
        private UIImage FloorDissolve;
        private UIImage Sky;
        private Texture2D BackgroundEasyTexture;
        private Texture2D BackgroundRegularTexture;
        private Texture2D SkyTexture;
        private Texture2D WallsChoiceEasyTexture;
        private Texture2D WallsChoiceRegularTexture;
        private Texture2D WallsGrowEasyTexture;
        private Texture2D WallsGrowRegularTexture;
        private Texture2D[] FloorTurningTextures;

        // lobby items
        private UIImage LoadingLogo;
        private UIImage EasyChoiceCharisma;
        private UIImage EasyChoiceEmoji;
        private UIImage EasyChoiceLogic;
        private UIImage EasyLoadEmoji;
        private UIImage RegularChoiceCharisma;
        private UIImage RegularChoiceEmoji;
        private UIImage RegularChoiceLogic;
        private UIImage RegularLoadEmoji;
        private UIImage HardChoiceCharisma;
        private UIImage HardChoiceEmoji;
        private UIImage HardChoiceLogic;
        private UIImage HardLoadEmoji;
        private UIImage ThreadLostScreen;
        private UIImage ThinkingEmoji;
        private UIInvisibleButton EasyInvisibleButton;
        private UIInvisibleButton RegularInvisibleButton;
        private UIInvisibleButton HardInvisibleButton;
        private UIInvisibleButton EasyChoiceCharismaButton;
        private UIInvisibleButton EasyChoiceLogicButton;
        private UIInvisibleButton RegularChoiceCharismaButton;
        private UIInvisibleButton RegularChoiceLogicButton;
        private UIInvisibleButton HardChoiceCharismaButton;
        private UIInvisibleButton HardChoiceLogicButton;
        private List<UIElement> LobbyGraphics;
        public UISim PartnerSimBox;
        public UISim CharismaSimBox;
        public UISim LogicSimBox;
        public UISim WinningSimBox;
        private UIVMPersonButton MyPersonButton;
        private UIVMPersonButton PartnerPersonButton;
        private Texture2D PersonButtonOutlineTex = GetTexture(0x000002B300000001); // #path = "Runtime\UIGraphics\EODS\JobObjectPizza\EOD_PizzaHeadPlaceholder1.bmp" 24x24 px
        private Vector2 PersonButtonOffset = new Vector2(2, 2);

        // barriers and hints
        private UIImage Hint;
        private UIImage FrontBarrier;
        private UIImage LeftBarrier;
        private UIImage RightBarrier;
        private UIImage[] Hints;
        private List<UIImage> OpaqueRoomFeatures;
        private byte[] DestinationRoomFeatures;

        // map stuff
        private Timer ScrambleTimer;
        private UIMaskedContainer LogoContainer;
        private UIMaskedContainer Map;
        private Texture2D MapRoomTex;
        private Texture2D MapWallHorizontalTex;
        private Texture2D MapWallVerticalTex;
        private UIImage YouAreHereImage;
        private Vector2 NorthWallOffset = new Vector2(0, -14);
        private Vector2 EastWallOffset = new Vector2(14, 0);
        private Vector2 WestWallOffset = new Vector2(-14, 0);
        private Vector2 SouthWallOffset = new Vector2(0, 14);
        private UIImage[] MapScrambleImages;
        private int ScrambleIndex;
        private float LogoXScaleFactor = -0.04f;
        private UIImage MomiLogo;

        // text
        private UITextEdit PartnerSkillLabel;
        private UITextEdit CharismaSkillLabel;
        private UITextEdit LogicSkillLabel;
        private UITextEdit MainLobbyHeader;
        private UITextEdit SubLobbyHeader;

        // constants
        private readonly float OFFSCREEN_NORTH_ABSTRACT_X = -2076f;
        private readonly int STAGE_MASK_HEIGHT = 230;
        private readonly int STAGE_MASK_WIDTH = 346;
        // scales
        private readonly float SCALE_MAIN_DECISION = 0.50f;
        private readonly float SCALE_MAIN_GROW = 1f;
        private readonly float SCALE_MAIN_TURN = 0.10f;
        private readonly float SCALE_SMALL_DECISION = 0.025f;
        private readonly float SCALE_SMALL_ROTATION = 0.05f;
        // frequently used size references
        private int GrowWallTextureHeight = 0;
        private int GrowWallTextureWidth = 0;

        private delegate void UIAlertEvent();
        private event UIAlertEvent AlertClosed;

        /*
         * Pelts you with dictionaries
         */
        private readonly Dictionary<UIMazeTwoEODDirections, float> CompassCardinalTrueAngles = new Dictionary<UIMazeTwoEODDirections, float>()
        {
            { UIMazeTwoEODDirections.North, 0f * (float)(Math.PI / 180) },
            { UIMazeTwoEODDirections.East, -90f * (float)(Math.PI / 180) },
            { UIMazeTwoEODDirections.West, 90f * (float)(Math.PI / 180) },
            { UIMazeTwoEODDirections.South, -180f * (float)(Math.PI / 180) }
        };
        // for compass
        private readonly Dictionary<UIMazeTwoEODControls, float> AngleChangeByRelativeDirection = new Dictionary<UIMazeTwoEODControls, float>()
        {
            { UIMazeTwoEODControls.None, 0f * (float)(Math.PI / 180) },
            { UIMazeTwoEODControls.Up, 0f },
            { UIMazeTwoEODControls.Right, -90f * (float)(Math.PI / 180)},
            { UIMazeTwoEODControls.Down, -180f * (float)(Math.PI / 180) }, // turn to the right twice
            { UIMazeTwoEODControls.Left, 90f * (float)(Math.PI / 180) }
        };
        // UIVMPersonButton vectors based on invisible button pressed, charisma player
        private readonly Dictionary<string, Vector2> CharismaPlayerFaceButtonLocations = new Dictionary<string, Vector2>()
        {
            { GameFacade.Strings.GetString("f125", "12") , new Vector2(137.5f, 170) }, // "Easy"
            { GameFacade.Strings.GetString("f125", "13") , new Vector2(137.5f, 230) }, // "Regular"
            { GameFacade.Strings.GetString("f125", "14") , new Vector2(137.5f, 290) } // "Hard"
        };
        // UIVMPersonButton vectors based on invisible button pressed, logic player
        private readonly Dictionary<string, Vector2> LogicPlayerFaceButtonLocations = new Dictionary<string, Vector2>()
        {
            { GameFacade.Strings.GetString("f125", "12") , new Vector2(221.5f, 170) }, // "Easy"
            { GameFacade.Strings.GetString("f125", "13") , new Vector2(221.5f, 230) }, // "Regular"
            { GameFacade.Strings.GetString("f125", "14") , new Vector2(221.5f, 290) } // "Hard"
        };
        private readonly Dictionary<UIMazeTwoEODControls, int> HintIndices = new Dictionary<UIMazeTwoEODControls, int>()
        {
            { UIMazeTwoEODControls.Left, 0 },
            { UIMazeTwoEODControls.Up, 1 },
            { UIMazeTwoEODControls.Right, 2 },
            { UIMazeTwoEODControls.Down, 3 },
            { UIMazeTwoEODControls.DeadEnd, 4 } // dead end
        };
        // Depricated
        private readonly Dictionary<UIMazeTwoEODDirections, float> BackgroundAbstractXByCardinal = new Dictionary<UIMazeTwoEODDirections, float>()
        {
            { UIMazeTwoEODDirections.North, 0f },
            { UIMazeTwoEODDirections.East, -519f },
            { UIMazeTwoEODDirections.West, -1557f },
            { UIMazeTwoEODDirections.South, -1038f }
        };

        public UIMazeTwoEOD(UIEODController controller) : base(controller)
        {
            YouAreHereTimer = new Timer(500);
            ScrambleTimer = new Timer(100);
            ThreadLostTimer = new Timer(3000);
            YouAreHereTimer.Elapsed += new ElapsedEventHandler(FlashYouAreHere);
            ScrambleTimer.Elapsed += new ElapsedEventHandler(DoScrambleMap);
            ThreadLostTimer.Elapsed += new ElapsedEventHandler(ThreadLostTimerHandler);
            LobbyGraphics = new List<UIElement>();
            DisposeOfMe = new List<Texture2D>();
            MapScrambleImages = new UIImage[4];
            FloorTurningTextures = new Texture2D[10];
            Hints = new UIImage[5];
            DestinationRoomFeatures = new byte[5];
            AlertClosed += AlertClosedHandler;
            InitUI();
            BinaryHandlers["FreeSOMaze_allow_maze"] = AllowMazeHandler; // enables player input
            BinaryHandlers["FreeSOMaze_dispose"] = DisconnectDisposeHandler;
            BinaryHandlers["FreeSOMaze_goto_maze"] = GotoMazeHandler;
            BinaryHandlers["FreeSOMaze_show_maze"] = LoadFirstMazeHandler;
            BinaryHandlers["FreeSOMaze_lobby_info"] = LobbyInfoHandler; // enables player input
            BinaryHandlers["FreeSOMaze_lobby_info12"] = LobbyInfoHandler; // enables player input
            BinaryHandlers["FreeSOMaze_lobby_info13"] = LobbyInfoHandler; // enables player input
            BinaryHandlers["FreeSOMaze_lobby_info14"] = LobbyInfoHandler; // enables player input
            BinaryHandlers["FreeSOMaze_move_to"] = MoveToHandler;
            BinaryHandlers["FreeSOMaze_partner_difficulty"] = PartnerDifficultyHandler;
            BinaryHandlers["FreeSOMaze_win"] = ThreadConnectedHandler;
            BinaryHandlers["FreeSOMaze_partner_disconnected"] = ThreadLostHandler;
            BinaryHandlers["FreeSOMaze_time"] = MazeTimeHandler;
            BinaryHandlers["FreeSOMaze_time_expired"] = ThreadLostHandler;
            BinaryHandlers["FreeSOMaze_toggle_scramble"] = ScrambleMapHandler;
            BinaryHandlers["FreeSOMaze_show_c"] = ShowPlayerUIHandler;
            BinaryHandlers["FreeSOMaze_show_l"] = ShowPlayerUIHandler;
            //PlaintextHandlers["FreeSOMaze_alert"] = AlertHandler;
        }

        #region Events

        private void AlertHandler(string evt, string msg)
        {
            //ShowUIAlert("debug", msg, null);
        }

        /// <summary>
        /// Dispose of any textures created. Server sends message when user disconnects. It is not called upon closing the UIEOD but only disconnecting from the plugin.
        /// </summary>
        /// <param name="evt">"FreeSOMaze_dispose"</param>
        /// <param name="nothing"></param>
        private void DisconnectDisposeHandler(string evt, byte[] nothing)
        {
            foreach (var tex in DisposeOfMe)
                tex.Dispose();
        }

        /// <summary>
        /// Allows for the delay of player input if they don't close their UIAlert in a timely manner
        /// </summary>
        private void AlertClosedHandler()
        {
            PlayerInputAllowed = PlayerInputAllowedDelayed;
        }

        /// <summary>
        /// Sets the EOD Time
        /// </summary>
        /// <param name="evt">"FreeSOMaze_time</param>
        /// <param name="timeBytes"></param>
        private void MazeTimeHandler(string evt, byte[] timeBytes)
        {
            var time = BitConverter.ToInt32(timeBytes, 0);
            SetTime(time);
            Parent.Invalidate();
        }

        /// <summary>
        /// Show the UIEOD
        /// </summary>
        /// <param name="evt">"FreeSOMaze_show_c" or "FreeSOMaze_show_l"</param>
        /// <param name="data">Serialized short containing the player's Sim's objectid</param>
        private void ShowPlayerUIHandler(string evt, byte[] data)
        {
            var myObjectID = BitConverter.ToInt16(data, 0);
            var avatar = (FSO.SimAntics.VMAvatar)EODController.Lot.vm.GetObjectById((short)myObjectID);
            if (avatar == null)
                return;

            MyPersonButton = new UIVMPersonButton(avatar, EODController.Lot.vm, true);

            decimal skill;
            UISim mySimBox;
            UITextEdit mySkillLabel;
            if (evt[evt.Length - 1].Equals('c')) // charisma
            {
                MyPersonButton.X = CharismaPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "12")].X + PersonButtonOffset.X;
                skill = avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.CharismaSkill) / 100m;
                mySimBox = CharismaSimBox;
                PartnerSimBox = LogicSimBox;
                mySkillLabel = CharismaSkillLabel;
                PartnerSkillLabel = LogicSkillLabel;
                EasyChoiceLogicButton.Disabled = true;
                LobbyGraphics.Remove(EasyChoiceLogicButton);
                RegularChoiceLogicButton.Disabled = true;
                LobbyGraphics.Remove(RegularChoiceLogicButton);
                HardChoiceLogicButton.Disabled = true;
                LobbyGraphics.Remove(HardChoiceLogicButton);
            }
            else // logic
            {
                MyPersonButton.X = LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "12")].X + PersonButtonOffset.X;
                skill = avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.LogicSkill) / 100m;
                mySimBox = LogicSimBox;
                PartnerSimBox = CharismaSimBox;
                mySkillLabel = LogicSkillLabel;
                PartnerSkillLabel = CharismaSkillLabel;
                EasyChoiceCharismaButton.Disabled = true;
                LobbyGraphics.Remove(EasyChoiceCharismaButton);
                RegularChoiceCharismaButton.Disabled = true;
                LobbyGraphics.Remove(RegularChoiceCharismaButton);
                HardChoiceCharismaButton.Disabled = true;
                LobbyGraphics.Remove(HardChoiceCharismaButton);
            }

            bool isMale = avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.Gender) == 0;
            var costume = Content.Content.Get().AvatarOutfits.Get((isMale) ? MaleOutfitID : FemaleOutfitID);
            // update UISim
            mySimBox.Avatar.Appearance = avatar.Avatar.Appearance;
            mySimBox.Avatar.Head = avatar.Avatar.Head;
            mySimBox.Avatar.Body = costume;
            mySimBox.Avatar.Handgroup = costume;
            mySimBox.AutoRotate = true;

            // update skill field under the UISim
            mySkillLabel.CurrentText = mySkillLabel.Tooltip + ": " + skill; // "Logic: " or "Charisma: " plus their skill xx.xx

            // show EOD
            EODController.ShowEODMode(new EODLiveModeOpt
            {
                Buttons = 0,
                Height = EODHeight.ExtraTall,
                Length = EODLength.Full,
                Tips = EODTextTips.None,
                Timer = EODTimer.Normal
            });

            // scramble the mini map
            ScrambleMapHandler("", new byte[] { 1 });
            PlayLoadingAnimation();
        }
        /// <summary>
        /// Scrambles the Map by hiding it and toggling the visibility of the static textures
        /// </summary>
        /// <param name="evt">"FreeSOMaze_toggle_scramble" if called from server</param>
        /// <param name="scrambleSetting">scrableSetting[0], 0 if stop scrambling, 1 if scrambling</param>
        private void ScrambleMapHandler(string evt, byte[] scrambleSetting)
        {
            if (scrambleSetting[0] == 1)
            {
                YouAreHereTimer.Stop();
                ScrambleTimer.Start();
                Map.Visible = false;
                LogoContainer.Visible = true;
            }
            else
            {
                ScrambleTimer.Stop();
                HideAllScrambledMaps();
                LogoContainer.Visible = false;
                Map.Visible = true;
                YouAreHereTimer.Start();
            }
        }

        /// <summary>
        /// Moves user to lobby if they're not already there, enables input. If partner objectID is present, handles updating the UI to reflect their appearance and difficulty choice.
        /// </summary>
        /// <param name="evt">"FreeSOMaze_lobby_info" OR "FreeSOMaze_lobby_info##" where ## is "12", "13", or "14"</param>
        /// <param name="objectID">[0] is a short containing the objectID of the partner. If == 0, there's no partner yet</param>
        private void LobbyInfoHandler(string evt, byte[] objectID)
        {
            var partnerObjectID = BitConverter.ToInt16(objectID, 0);
            if (PartnerPersonButton == null && partnerObjectID != 0) // if 0, there is not partner yet. otherwise this is their objectID
            {
                var avatar = (FSO.SimAntics.VMAvatar)EODController.Lot.vm.GetObjectById(partnerObjectID);
                if (avatar != null)
                {
                    PlaySound("ui_online_sim");
                    // update the SimBoxes with the avatar
                    bool isMale = avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.Gender) == 0;
                    var costume = Content.Content.Get().AvatarOutfits.Get((isMale) ? MaleOutfitID : FemaleOutfitID);

                    PartnerSimBox.Avatar.Appearance = avatar.Avatar.Appearance;
                    PartnerSimBox.Avatar.Head = avatar.Avatar.Head;
                    PartnerSimBox.Avatar.Body = costume;
                    PartnerSimBox.Avatar.Handgroup = costume;
                    PartnerSimBox.AutoRotate = true;

                    WinningSimBox.Avatar.Appearance = avatar.Avatar.Appearance;
                    WinningSimBox.Avatar.Head = avatar.Avatar.Head;
                    WinningSimBox.Avatar.Body = costume;
                    WinningSimBox.Avatar.Handgroup = costume;
                    WinningSimBox.AutoRotate = true;

                    // update their label with their skill
                    decimal skill;
                    float x = 0;
                    if (PartnerSkillLabel.Tooltip.Equals(GameFacade.Strings.GetString("f125", "15")))
                    { // "Charisma"
                        skill = avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.CharismaSkill) / 100m;
                        x = CharismaPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "13")].X;
                    }
                    else
                    {
                        skill = avatar.GetPersonData(SimAntics.Model.VMPersonDataVariable.LogicSkill) / 100m;
                        x = LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "13")].X;
                    }
                    PartnerSkillLabel.CurrentText = PartnerSkillLabel.Tooltip + ": " + skill; // "Logic: " or "Charisma: " plus their skill xx.xx

                    // create person button
                    PartnerPersonButton = new UIVMPersonButton(avatar, EODController.Lot.vm, true)
                    {
                        X = x + PersonButtonOffset.X
                    };
                }
            }
            // move their button if they have made a choice already
            if (PartnerPersonButton != null && Int32.TryParse(evt.Substring(evt.Length - 2), out int choice)) // 12, 13, or 14, or 0 if they have not made a choice but are connected
                MovePersonButton(PartnerPersonButton, LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "" + choice)].Y);

            // go to lobby if not already there and enable input
            if (!LobbyVisible)
            {
                if (ResultsScreen == null)
                    GotoLobby();
            }
            // in case they haven't closed the win/loss dialog from last round
            if (ResultsScreen == null)
                PlayerInputAllowed = true;
            PlayerInputAllowedDelayed = true;

            Background.UpdateChildMasks();
        }

        /// <summary>
        /// Updates the partners choice of maze difficultly by moving their person button
        /// </summary>
        /// <param name="evt">"FreeSOMaze_partner_difficulty"</param>
        /// <param name="chosenDifficulty">A byte array representing 12, 13, or 14 correspondong to the string index in the _f125_.cst file, whose contents at those indicies are used in the dictionaries above.</param>
        private void PartnerDifficultyHandler(string evt, byte[] chosenDifficulty)
        {
            if (LobbyVisible)
            {
                if (PartnerPersonButton != null)
                    MovePersonButton(PartnerPersonButton, LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "" + BitConverter.ToInt32(chosenDifficulty, 0))].Y);
                else // we need the partner's info to recreate the button, something went wrong
                    Send("FreeSOMaze_partner_failsafe", new byte[0]);
            }
        }

        /// <summary>
        /// If partner disconnects anywhere except in the lobby, including transitions, display a pensive face screen and take user back to lobby
        /// </summary>
        /// <param name="evt">"FreeSOMaze_partner_disconnected" or "FreeSOMaze_time_expired"</param>
        /// <param name="nothing"></param>
        private void ThreadLostHandler(string evt, byte[] nothing)
        {
            PlayerInputAllowed = false;
            string sound = "dj_booth_turnoff";

            string message;
            if (evt.Equals("FreeSOMaze_partner_disconnected"))
            {
                message = "" + 19; //  "Your partner disconnected"
                ResetPartner();
                sound = "ui_offline_sim";
            }
            else
                message = "" + 20; //  "You ran out of time"

            // show the partner disconnected graphic
            ThreadLostScreen.Visible = true;
            MainLobbyHeader.Visible = true;
            SubLobbyHeader.Visible = true;
            MainLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "18"); // "Thread Lost"
            SubLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", message);
            ThreadLostTimer.Start();
            PlaySound(sound);

            // if we're tweening, modify the complete action to return to lobby
            if (TweenQueue.HasQueue)
                TweenQueue.OverrideCompleteAction(GotoLobby);
            else
                GotoLobby();
        }

        /// <summary>
        /// Event that occurs when the two players find each other: a "win"
        /// </summary>
        /// <param name="evt">"FreeSOMaze_win"</param>
        /// <param name="gameResults">Serialized strings: 0 - $Base Win Amount, 1 - $Skill Bonus Amount, 2 - Total Time, 3 - This user's moves, 4 - Partner's moves</param>
        private void ThreadConnectedHandler(string evt, byte[] gameResults)
        {
            PlayerInputAllowed = false;
            // deserialized the data
            var gameOverData = SimAntics.NetPlay.EODs.Handlers.Data.VMEODGameCompDrawACardData.DeserializeStrings(gameResults);
            if (gameOverData != null && gameOverData.Length > 4)
            {
                if (!TweenQueue.HasQueue)
                    DisplayWinScreen(gameOverData);
                else
                    TweenQueue.OverrideCompleteAction(() =>
                    {
                        DisplayWinScreen(gameOverData);
                    });
            }
        }

        /// <summary>
        /// When user clicks on a difficulty emoji.
        /// </summary>
        /// <param name="chosenDifficulty">12 13 or 14 for easy, regular, or hard, respectively</param>
        private void MyDiffcultyHandler(int chosenDifficulty)
        {
            if (LobbyVisible && PlayerInputAllowed)
            {
                PlaySound("ctrunk_scifi_button");
                MovePersonButton(MyPersonButton, LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "" + chosenDifficulty)].Y);
                Send("FreeSOMaze_choose_difficulty", BitConverter.GetBytes(chosenDifficulty));
            }
        }

        /// <summary>
        /// Transition from the lobby to the maze using a loading icon representing the difficutly chosen.
        /// </summary>
        /// <param name="evt">"FreeSOMaze_goto_maze"</param>
        /// <param name="chosenDifficulty">A byte array representing 12, 13, or 14 correspondong to the string index in the _f125_.cst file, whose contents at those indicies are used in the dictionaries above.</param>
        private void GotoMazeHandler(string evt, byte[] chosenDifficulty)
        {
            var difficulty = BitConverter.ToInt32(chosenDifficulty, 0);
            if (difficulty == 12)
                LoadingLogo = EasyLoadEmoji;
            else if (difficulty == 13)
                LoadingLogo = RegularLoadEmoji;
            else
                LoadingLogo = HardLoadEmoji;
            // hide all of the lobby graphics except the main textfield and the chosen difficulty's emoji symbol
            Remove(CharismaSimBox);
            Remove(LogicSimBox);
            Remove(MyPersonButton);
            Remove(PartnerPersonButton);
            for (int index = 0; index < LobbyGraphics.Count; index++)
                LobbyGraphics[index].Visible = false;
            MainLobbyHeader.Visible = true;
            LoadingLogo.Visible = true;
            LoadingLogo.DissolveOpacity = 0f;

            // Update the text and make a tween to move the emoji and grow it
            MainLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "9") + " " + GameFacade.Strings.GetString("f125", "" + difficulty) + "...";
            TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous,
                new UITweenInstanceMembers(GameFacade.Screens.Tween, LoadingLogo, 2f, new Dictionary<string, float>() { { "DissolveOpacity", 1f } }, TweenLinear.EaseNone));
            TweenQueue.PlayQueue();
            PlaySound("computer_boot_exp");
        }

        /// <summary>
        /// Finally fade in the maze and then barriers/hints.
        /// </summary>
        /// <param name="evt">"FreeSOMaze_show_maze"</param>
        /// <param name="wallHintData">[0] is the true cardinal direction user is facing; [1]-[4] are the cardinal barriers == 1 for yes; [5] is the hint type, == 10 for no hint</param>
        private void LoadFirstMazeHandler(string evt, byte[] wallHintData)
        {
            if (wallHintData.Length > 5)
            {
                LobbyVisible = false;
                if (TweenQueue.HasQueue)
                    TweenQueue.StopQueue(true, true);

                UIMazeTwoEODDirections targetCardinal = UIMazeTwoEODDirections.North;

                // get the cardinal direction
                if (Enum.IsDefined(typeof(UIMazeTwoEODDirections), (int)wallHintData[0]))
                    targetCardinal = (UIMazeTwoEODDirections)Enum.ToObject(typeof(UIMazeTwoEODDirections), (int)wallHintData[0]);

                TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous,
                    // remove loading logo
                    new UITweenInstanceMembers(GameFacade.Screens.Tween, LoadingLogo, 3f, new Dictionary<string, float>() { { "DissolveOpacity", 0f } }, TweenLinear.EaseNone),
                    //rotate the compass
                    new UITweenInstanceMembers(GameFacade.Screens.Tween, CompassInner, 1f, new Dictionary<string, float>() { { "Rotation", CompassCardinalTrueAngles[targetCardinal] } }, TweenLinear.EaseNone),
                    // make walls visible
                    new UITweenInstanceMembers(GameFacade.Screens.Tween, MainChoiceWall, 3f, new Dictionary<string, float>() { { "DissolveOpacity", 1f } }, TweenLinear.EaseNone),
                    new UITweenInstanceMembers(GameFacade.Screens.Tween, HorizonWall, 3f, new Dictionary<string, float>() { { "DissolveOpacity", 1f } }, TweenLinear.EaseNone),
                    // sky
                    new UITweenInstanceMembers(GameFacade.Screens.Tween, Sky, 3f, new Dictionary<string, float>() { { "DissolveOpacity", 1f } }, TweenLinear.EaseNone),
                    // floor
                    new UITweenInstanceMembers(GameFacade.Screens.Tween, FloorDissolve, 3f, new Dictionary<string, float>() { { "DissolveOpacity", 1f } }, TweenLinear.EaseNone));

                // copy over wall and hint data
                for (int index = 1; index < wallHintData.Length - 1; index++)
                    DestinationRoomFeatures[index - 1] = wallHintData[index];

                // make hints/barriers visible
                TweenQueue.CompleteAction = () =>
                {
                    MainLobbyHeader.Visible = false;
                    FloorDissolve.Visible = false;
                    Floor.Visible = true;
                    Sky.DissolveOpacity = 1f;
                    PostTweenCleanup(targetCardinal);
                    LoadingLogo.DissolveOpacity = 0f;
                    // stop scrambling the minimap
                    ScrambleMapHandler(evt, new byte[] { 0 });
                    // add a new room
                    Map.Add(new UIImage(MapRoomTex));
                };
                TweenQueue.PlayQueue();
            }
        }

        /// <summary>
        /// This enables input to allow the user to choose the next direction, but it also acts as a failsafe against any weird tween behavior by sending the "room" details again to make sure
        /// the user sees the proper choices before making a selection.
        /// </summary>
        /// <param name="evt">"FreeSOMaze_allow_maze"</param>
        /// <param name="data">[0] is the true cardinal direction user is facing; [1]-[4] are the cardinal barriers == 1 for yes; [5] is the hint type, == 10 for no hint</param>
        private void AllowMazeHandler(string evt, byte[] cardinalBarriersHint)
        {
            //ShowUIAlert("client", "AllowMaze: I received: " + cardinalBarriersHint[5], null);
            if (cardinalBarriersHint.Length > 6)
            {
                // Stop the Queue
                if (TweenQueue.HasQueue)
                {
                    TweenQueue.StopQueue(false, false);
                    ResetAllWalls();
                }

                // cardinal direction, affecting compass, barriers, and hint
                if (Enum.IsDefined(typeof(UIMazeTwoEODDirections), (int)cardinalBarriersHint[0]))
                {
                    CurrentFacingCardinal = (UIMazeTwoEODDirections)Enum.ToObject(typeof(UIMazeTwoEODDirections), cardinalBarriersHint[0]);
                    CompassInner.Rotation = CurrentRotation = CompassCardinalTrueAngles[CurrentFacingCardinal];
                }
                // barriers
                UIImage barrier;
                if (cardinalBarriersHint[1] == 1)
                {
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.North);
                    if (barrier != null)
                        barrier.DissolveOpacity = 1f;
                }
                if (cardinalBarriersHint[2] == 1)
                {
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.East);
                    if (barrier != null)
                        barrier.DissolveOpacity = 1f;
                }
                if (cardinalBarriersHint[3] == 1)
                {
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.West);
                    if (barrier != null)
                        barrier.DissolveOpacity = 1f;
                }
                if (cardinalBarriersHint[4] == 1)
                {
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.South);
                    if (barrier != null)
                        barrier.DissolveOpacity = 1f;
                }
                // hint
                if (cardinalBarriersHint[5] < 6) // equals 10 if there's no hint, 5 is dead end
                {
                    /*var relativeDirection = UIMazeTwoEODControls.None;
                    if (cardinalBarriersHint[5] < 5)
                        relativeDirection = GetRelativeDirectionFromCardinal(cardinalBarriersHint[5]);
                    var correctHint = Hints[HintIndices[relativeDirection]];
                    if (!correctHint.Equals(Hint))
                    {
                        Hint.DissolveOpacity = 0f;
                        OpaqueRoomFeatures.Add(correctHint);
                        Hint = correctHint;
                    }*/
                    if (Hint != null)
                        Hint.DissolveOpacity = 1f;
                }

                // Update the map barriers: this is not a failsafe, but the only place that this happens
                var walls = new byte[] { cardinalBarriersHint[1], cardinalBarriersHint[2], cardinalBarriersHint[3], cardinalBarriersHint[4] };
                AddMapWalls(walls);
                Map.UpdateChildMasks();

                // not a failsafe, the only place where the user gets their input permissions back
                PlayerInputAllowed = true;
            }
        }

        /// <summary>
        /// The server has validated the selected resulting cardinal direction and the user will now move there via animations
        /// </summary>
        /// <param name="evt">"FreeSOMaze_move_to"</param>
        /// <param name="cardinalBarriersHint">[0] is target cardinal direction, and since direction:Left is -1, this value is += 1; [1]-[4] are barriers in this new room, [5] is the hint, [6] == 1 if room was visited previously</param>
        private void MoveToHandler(string evt, byte[] cardinalBarriersHint)
        {
            if (!LobbyVisible)
            {
                if (cardinalBarriersHint != null && cardinalBarriersHint.Length > 6)
                {
                    //ShowUIAlert("client", "MoveTo: I received: " + cardinalBarriersHint[5], null);
                    // save the barriers and hint for later
                    for (int index = 1; index < cardinalBarriersHint.Length - 1; index++)
                        DestinationRoomFeatures[index - 1] = cardinalBarriersHint[index];
                    var intDirection = (int)cardinalBarriersHint[0];
                    if (Enum.IsDefined(typeof(UIMazeTwoEODDirections), intDirection))
                    {
                        var targetCardinal = (UIMazeTwoEODDirections)Enum.ToObject(typeof(UIMazeTwoEODDirections), intDirection);
                        var relativeDirection = GetRelativeDirectionFromCardinal(intDirection);
                        if (Enum.IsDefined(typeof(UIMazeTwoEODControls), relativeDirection) && !relativeDirection.Equals(UIMazeTwoEODControls.None))
                            AnimateUIEOD(relativeDirection, targetCardinal, (cardinalBarriersHint[6] == 1));
                    }
                }
            }
        }

        /// <summary>
        /// Flashes the pink square representing the player's position on the minimap by changing its visible status every half real life second.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void FlashYouAreHere(object source, ElapsedEventArgs args)
        {
            YouAreHereImage.Visible = !YouAreHereImage.Visible;
        }

        /// <summary>
        /// Srambles the map by randomly selecting a texture2D to assign to the scramble image
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void DoScrambleMap(object source, ElapsedEventArgs args)
        {
            HideAllScrambledMaps();
            if (++ScrambleIndex > 3)
                ScrambleIndex = 0;
            MapScrambleImages[ScrambleIndex].Visible = true;
            MomiLogo.ScaleX += LogoXScaleFactor;
            LogoContainer.UpdateChildMasks();
            if (MomiLogo.ScaleX <= 0)
            {
                if (MomiLogo.SpriteEffect.Equals(SpriteEffects.None))
                    MomiLogo.SpriteEffect = SpriteEffects.FlipHorizontally;
                else
                    MomiLogo.SpriteEffect = SpriteEffects.None;
                MomiLogo.ScaleX = 0;
                LogoXScaleFactor *= -1;
            }
            else if (MomiLogo.ScaleX >= 1.0f)
            {
                MomiLogo.ScaleX = 1.0f;
                LogoXScaleFactor *= -1;
            }
        }

        /// <summary>
        /// After a few seconds of showing the user that the partner disconnected, restore the lobby text
        /// </summary>
        /// <param name="source">ThreadLostTimer</param>
        /// <param name="args"></param>
        private void ThreadLostTimerHandler(object source, ElapsedEventArgs args)
        {
            ThreadLostTimer.Stop();
            MainLobbyHeader.Visible = true;
            SubLobbyHeader.Visible = true;
            MainLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "10"); // "Select Difficulty"
            SubLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "11"); // "Both players must agree to continue."
            ThreadLostScreen.Visible = false;
            Remove(LogicSimBox);
            Remove(CharismaSimBox);
            Add(LogicSimBox);
            Add(CharismaSimBox);
            HideMazeElements(true);
        }

        #endregion

        /// <summary>
        /// Hides all the images containing static textures representing the scrambled map
        /// </summary>
        private void HideAllScrambledMaps()
        {
            for (int i = 0; i < MapScrambleImages.Length; i++)
                MapScrambleImages[i].Visible = false;
        }

        /// <summary>
        /// Import the textures and assemble the UI.
        /// </summary>
        private void InitUI()
        {
            var gd = GameFacade.GraphicsDevice;

            // get the lobby textures
            AbstractTextureRef compassEdgeTexRef = new FileTextureRef(CompassEdgeTexPath);
            AbstractTextureRef compassInnerTexRef = new FileTextureRef(CompassTexPath);
            AbstractTextureRef mainScreenBGTexRef = new FileTextureRef(MainScreenBGTexPath);
            AbstractTextureRef mapScreenBGTexRef = new FileTextureRef(MapScreenBGTexPath);
            AbstractTextureRef screenEdgeTexRef = new FileTextureRef(ScreensEdgeTexPath);
            AbstractTextureRef dPadTexRef = new FileTextureRef(DPadTexPath);
            AbstractTextureRef dPadUpTexRef = new FileTextureRef(DPadUpTexPath);
            AbstractTextureRef dPadLeftTexRef = new FileTextureRef(DPadLeftTexPath);
            AbstractTextureRef dPadRightTexRef = new FileTextureRef(DPadRightTexPath);
            AbstractTextureRef dPadDownTexRef = new FileTextureRef(DPadDownTexPath);
            AbstractTextureRef momiLogoRef = new FileTextureRef(MomiLogoTexPath);
            AbstractTextureRef thinkingLogoRef = new FileTextureRef(LoadingLogoTexPath);
            AbstractTextureRef easyLoadRef = new FileTextureRef(EasyLoadingLogoTexPath);
            AbstractTextureRef regLoadRef = new FileTextureRef(RegularLoadingLogoTexPath);
            AbstractTextureRef hardLoadRef = new FileTextureRef(HardLoadingLogoTexPath);
            AbstractTextureRef partnerDisconnect = new FileTextureRef(PartnerDisconnectedTexPath);

            Texture2D screenTex = null;
            Texture2D compassEdgeTex = null;
            Texture2D compassInnerTex = null;
            Texture2D mainScreenBGTex = null;
            Texture2D mapScreenBGTex = null;
            Texture2D momiLogoTex = null;
            Texture2D thinkingTex = null;
            Texture2D partnerDisconnctTex = null;
            Texture2D[] difficultyTextures = new Texture2D[3];

            // maze textures

            AbstractTextureRef wallsChoiceEasyTexRef = new FileTextureRef(WallsChoiceEasyTexPath);
            AbstractTextureRef wallsChoiceRegTexRef = new FileTextureRef(WallsChoiceRegularTexPath);
            AbstractTextureRef mapRoomTexRef = new FileTextureRef(MapRoomTexPath);
            AbstractTextureRef backgroundEasyTexRef = new FileTextureRef(BackgroundEasyTexturePath);
            AbstractTextureRef backgroundRegularTexRef = new FileTextureRef(BackgroundRegularTexturePath);
            AbstractTextureRef skyTexRef = new FileTextureRef(SkyEasyTexPath);
            AbstractTextureRef[] turningFloorRefs = new AbstractTextureRef[10];
            for (int index = 0; index < turningFloorRefs.Length; index++)
                turningFloorRefs[index] = new FileTextureRef(FloorTurningTexPath + (index + 1) + ".png");


            try // try to retrieve the files
            {
                // lobby
                compassEdgeTex = compassEdgeTexRef.Get(gd);
                compassInnerTex = compassInnerTexRef.Get(gd);
                mainScreenBGTex = mainScreenBGTexRef.Get(gd);
                mapScreenBGTex = mapScreenBGTexRef.Get(gd);
                screenTex = screenEdgeTexRef.Get(gd);
                DPad = dPadTexRef.Get(gd);
                DPadUp = dPadUpTexRef.Get(gd);
                DPadLeft = dPadLeftTexRef.Get(gd);
                DPadRight = dPadRightTexRef.Get(gd);
                DPadDown = dPadDownTexRef.Get(gd);
                momiLogoTex = momiLogoRef.Get(gd);
                thinkingTex = thinkingLogoRef.Get(gd);
                difficultyTextures[0] = easyLoadRef.Get(gd);
                difficultyTextures[1] = regLoadRef.Get(gd);
                difficultyTextures[2] = hardLoadRef.Get(gd);
                partnerDisconnctTex = partnerDisconnect.Get(gd);
                SkyTexture = skyTexRef.Get(gd);
                for (int index = 0; index < FloorTurningTextures.Length; index++)
                    FloorTurningTextures[index] = turningFloorRefs[index].Get(gd);

                // maze
                WallsChoiceEasyTexture = wallsChoiceEasyTexRef.Get(gd);
                WallsChoiceRegularTexture = wallsChoiceRegTexRef.Get(gd);
                MapRoomTex = mapRoomTexRef.Get(gd);
                BackgroundEasyTexture = backgroundEasyTexRef.Get(gd);
                BackgroundRegularTexture = backgroundRegularTexRef.Get(gd);

            }
            catch (Exception error)
            {
                // todo: files not found, handle it
            }

            // Manually drawn textures that are reused
            MapWallVerticalTex = TextureUtils.TextureFromColor(gd, new Color(0, 255, 255), 4, 8);
            MapWallHorizontalTex = TextureUtils.TextureFromColor(gd, new Color(0, 255, 255), 8, 4);
            DisposeOfMe.Add(MapWallVerticalTex);
            DisposeOfMe.Add(MapWallHorizontalTex);

            /*
             * 
             * Main Screen area; lobby items
             * 
             */

            // Screen backgrounds added first
            MainScreenBG = new UIImage(mainScreenBGTex)
            {
                X = 138.5f,
                Y = 50
            };
            MainScreenBG.Position += BaseOffset;
            Add(MainScreenBG);

            Background = new UIMaskedContainer(new Rectangle(0, 0, STAGE_MASK_WIDTH, STAGE_MASK_HEIGHT))
            {
                Position = MainScreenBG.Position
            };
            Add(Background);

            // logos for loading a maze
            ThinkingEmoji = new UIImage(thinkingTex)
            {
                DissolveOpacity = 0
            };
            Background.Add(ThinkingEmoji);
            LoadingLogo = ThinkingEmoji;
            EasyLoadEmoji = new UIImage(difficultyTextures[0])
            {
                DissolveOpacity = 0
            };
            Background.Add(EasyLoadEmoji);
            RegularLoadEmoji = new UIImage(difficultyTextures[1])
            {
                DissolveOpacity = 0
            };
            Background.Add(RegularLoadEmoji);
            HardLoadEmoji = new UIImage(difficultyTextures[2])
            {
                DissolveOpacity = 0
            };
            Background.Add(HardLoadEmoji);

            /*
             * PersonButtons, Outlines by difficulty
             */

            Texture2D invisibleTexture = TextureUtils.TextureFromColor(gd, new Color(new Vector4(0, 0, 0, 0)), 1, 1);
            DisposeOfMe.Add(invisibleTexture);

            EasyChoiceCharisma = new UIImage(PersonButtonOutlineTex)
            {
                Position = CharismaPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "12")], // "Easy"
                Visible = false
            };
            Add(EasyChoiceCharisma);
            LobbyGraphics.Add(EasyChoiceCharisma);
            EasyChoiceCharismaButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                Position = EasyChoiceCharisma.Position,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "12") // "Easy"
            };
            Add(EasyChoiceCharismaButton);
            LobbyGraphics.Add(EasyChoiceCharismaButton);
            EasyChoiceCharismaButton.OnButtonClick += (btn) => { MyDiffcultyHandler(12); };

            RegularChoiceCharisma = new UIImage(PersonButtonOutlineTex)
            {
                Position = CharismaPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "13")], // "Regular"
                Visible = false
            };
            Add(RegularChoiceCharisma);
            LobbyGraphics.Add(RegularChoiceCharisma);
            RegularChoiceCharismaButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                Position = RegularChoiceCharisma.Position,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "13") // "Regular"
            };
            Add(RegularChoiceCharismaButton);
            LobbyGraphics.Add(RegularChoiceCharismaButton);
            RegularChoiceCharismaButton.OnButtonClick += (btn) => { MyDiffcultyHandler(13); };

            HardChoiceCharisma = new UIImage(PersonButtonOutlineTex)
            {
                Position = CharismaPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "14")], // "Hard"
                Visible = false
            };
            Add(HardChoiceCharisma);
            LobbyGraphics.Add(HardChoiceCharisma);
            HardChoiceCharismaButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                Position = HardChoiceCharisma.Position,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "14") // "Hard"
            };
            Add(HardChoiceCharismaButton);
            LobbyGraphics.Add(HardChoiceCharismaButton);
            HardChoiceCharismaButton.OnButtonClick += (btn) => { MyDiffcultyHandler(14); };

            EasyChoiceLogic = new UIImage(PersonButtonOutlineTex)
            {
                Position = LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "12")], // "Easy"
                Visible = false
            };
            Add(EasyChoiceLogic);
            LobbyGraphics.Add(EasyChoiceLogic);
            EasyChoiceLogicButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                Position = EasyChoiceLogic.Position,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "12") // "Easy"
            };
            Add(EasyChoiceLogicButton);
            LobbyGraphics.Add(EasyChoiceLogicButton);
            EasyChoiceLogicButton.OnButtonClick += (btn) => { MyDiffcultyHandler(12); };

            RegularChoiceLogic = new UIImage(PersonButtonOutlineTex)
            {
                Position = LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "13")], // "Regular"
                Visible = false
            };
            Add(RegularChoiceLogic);
            LobbyGraphics.Add(RegularChoiceLogic);
            RegularChoiceLogicButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                Position = RegularChoiceLogic.Position,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "13") // "Regular"
            };
            Add(RegularChoiceLogicButton);
            LobbyGraphics.Add(RegularChoiceLogicButton);
            RegularChoiceLogicButton.OnButtonClick += (btn) => { MyDiffcultyHandler(13); };

            HardChoiceLogic = new UIImage(PersonButtonOutlineTex)
            {
                Position = LogicPlayerFaceButtonLocations[GameFacade.Strings.GetString("f125", "14")], // "Hard"
                Visible = false
            };
            Add(HardChoiceLogic);
            LobbyGraphics.Add(HardChoiceLogic);
            HardChoiceLogicButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                Position = HardChoiceLogic.Position,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "14") // "Hard"
            };
            Add(HardChoiceLogicButton);
            LobbyGraphics.Add(HardChoiceLogicButton);
            HardChoiceLogicButton.OnButtonClick += (btn) => { MyDiffcultyHandler(14); };

            /*
             * Difficultiy selection emojis and buttons
             */


            EasyChoiceEmoji = new UIImage(difficultyTextures[0])
            {
                ScaleX = 0.2f,
                ScaleY = 0.2f,
                AbstractY = -40,
                Visible = false
            };
            Background.Add(EasyChoiceEmoji);
            LobbyGraphics.Add(EasyChoiceEmoji);
            EasyInvisibleButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                X = MainScreenBG.X + 158,
                Y = MainScreenBG.Y + 60,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "12") // "Easy"
            };
            Add(EasyInvisibleButton);
            LobbyGraphics.Add(EasyInvisibleButton);
            EasyInvisibleButton.OnButtonClick += (btn) => { MyDiffcultyHandler(12); };

            RegularChoiceEmoji = new UIImage(difficultyTextures[1])
            {
                ScaleX = 0.2f,
                ScaleY = 0.2f,
                AbstractY = 20,
                Visible = false
            };
            Background.Add(RegularChoiceEmoji);
            LobbyGraphics.Add(RegularChoiceEmoji);
            RegularInvisibleButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                X = MainScreenBG.X + 158,
                Y = MainScreenBG.Y + 120,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "13") // "Regular"
            };
            Add(RegularInvisibleButton);
            LobbyGraphics.Add(RegularInvisibleButton);
            RegularInvisibleButton.OnButtonClick += (btn) => { MyDiffcultyHandler(13); };

            HardChoiceEmoji = new UIImage(difficultyTextures[2])
            {
                ScaleX = 0.2f,
                ScaleY = 0.2f,
                AbstractY = 80,
                Visible = false
            };
            Background.Add(HardChoiceEmoji);
            LobbyGraphics.Add(HardChoiceEmoji);
            HardInvisibleButton = new UIInvisibleButton(24, 24, invisibleTexture)
            {
                X = MainScreenBG.X + 158,
                Y = MainScreenBG.Y + 180,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "14") // "Hard"
            };
            Add(HardInvisibleButton);
            LobbyGraphics.Add(HardInvisibleButton);
            HardInvisibleButton.OnButtonClick += (btn) => { MyDiffcultyHandler(14); };

            Background.UpdateChildMasks();

            /*
             * Persons
             */

            CharismaSimBox = new UISim()
            {
                X = MainScreenBG.X - 2,
                Y = MainScreenBG.Y + 40,
                Size = new Vector2(126, 180),
                AutoRotate = false,
                SpriteEffect = SpriteEffects.FlipHorizontally
            };
            var head = Content.Content.Get().AvatarOutfits.Get(0x3a00000000d); // "mah000_proxy.oft"
            var emptyBody = Content.Content.Get().AvatarOutfits.Get(0x24c0000000d); // "mab000_xy__proxy.oftr"
            CharismaSimBox.Avatar.Appearance = Vitaboy.AppearanceType.Light;
            CharismaSimBox.Avatar.Head = head;
            CharismaSimBox.Avatar.Body = emptyBody;
            CharismaSimBox.Avatar.Handgroup = emptyBody;

            LogicSimBox = new UISim()
            {
                X = MainScreenBG.X + 222,
                Y = MainScreenBG.Y + 40,
                Size = new Vector2(126, 180),
                AutoRotate = false,
            };
            LogicSimBox.Avatar.Appearance = Vitaboy.AppearanceType.Light;
            LogicSimBox.Avatar.Head = head;
            LogicSimBox.Avatar.Body = emptyBody;
            LogicSimBox.Avatar.Handgroup = emptyBody;

            WinningSimBox = new UISim()
            {
                X = MainScreenBG.X + 100,
                Y = MainScreenBG.Y + 40,
                Size = new Vector2(140, 200),
                AutoRotate = true
            };
            WinningSimBox.Avatar.Appearance = Vitaboy.AppearanceType.Light;
            WinningSimBox.Avatar.Head = head;
            WinningSimBox.Avatar.Body = emptyBody;
            WinningSimBox.Avatar.Handgroup = emptyBody;

            /*
             * Text fields
             */

            MainLobbyHeader = new UITextEdit()
            {
                X = MainScreenBG.X,
                Y = MainScreenBG.Y + 4,
                Alignment = TextAlignment.Center,
                Size = new Vector2(346, 20),
                CurrentText = GameFacade.Strings.GetString("f125", "9") + "...", // "Loading..."
                Mode = UITextEditMode.ReadOnly
            };
            var style = MainLobbyHeader.TextStyle.Clone();
            style.Color = Color.White;
            style.Size = 16;
            MainLobbyHeader.TextStyle = style;
            LobbyGraphics.Add(MainLobbyHeader);

            SubLobbyHeader = new UITextEdit()
            {
                X = MainScreenBG.X,
                Y = MainScreenBG.Y + 26,
                Alignment = TextAlignment.Center,
                Size = new Vector2(346, 20),
                CurrentText = GameFacade.Strings.GetString("f125", "11"), // "Both players must agree to continue."
                Mode = UITextEditMode.ReadOnly,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "15"), // "Logic" 
            };
            var style2 = SubLobbyHeader.TextStyle.Clone();
            style2.Color = new Color(0, 255, 255);
            style2.Size = 12;
            SubLobbyHeader.TextStyle = style2;
            LobbyGraphics.Add(SubLobbyHeader);

            CharismaSkillLabel = new UITextEdit()
            {
                X = MainScreenBG.X - 14,
                Y = MainScreenBG.Y + 206,
                Alignment = TextAlignment.Center,
                Size = new Vector2(173, 20),
                CurrentText = GameFacade.Strings.GetString("f125", "17"), // "Waiting..."
                Mode = UITextEditMode.ReadOnly,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "15"), // "Charisma" 
            };
            var style3 = CharismaSkillLabel.TextStyle.Clone();
            style3.Color = Color.White;// new Color(236, 0, 140);
            style3.Size = 12;
            CharismaSkillLabel.TextStyle = style3;
            LobbyGraphics.Add(CharismaSkillLabel);
            Add(CharismaSkillLabel);

            LogicSkillLabel = new UITextEdit()
            {
                X = MainScreenBG.X + 194,
                Y = MainScreenBG.Y + 206,
                Alignment = TextAlignment.Center,
                Size = new Vector2(173, 20),
                CurrentText = GameFacade.Strings.GetString("f125", "17"), // "Waiting..."
                Mode = UITextEditMode.ReadOnly,
                Visible = false,
                Tooltip = GameFacade.Strings.GetString("f125", "16"), // "Logic" 
            };
            LogicSkillLabel.TextStyle = style3;
            LobbyGraphics.Add(LogicSkillLabel);
            Add(LogicSkillLabel);

            ThreadLostScreen = new UIImage(partnerDisconnctTex)
            {
                Visible = false,
                Position = MainScreenBG.Position
            };
            Add(ThreadLostScreen);
            Add(MainLobbyHeader);
            Add(SubLobbyHeader);


            /*
             * 
             * Map Screen
             * 
             */

            // map screen background
            MapScreenBG = new UIImage(mapScreenBGTex)
            {
                X = 510,
                Y = 110
            };
            MapScreenBG.Position += BaseOffset;
            Add(MapScreenBG);

            Map = new UIMaskedContainer(new Rectangle(0, 0, 100, 100))
            {
                Position = MapScreenBG.Position
            };
            Add(Map);

            YouAreHereImage = new UIImage(TextureUtils.TextureFromColor(gd, new Color(236, 0, 140), 8, 8))
            {
                X = MapScreenBG.X + 46,
                Y = MapScreenBG.Y + 46,
                Visible = false
            };
            Add(YouAreHereImage);

            // Make the textures for scrambling the map
            var tex = TextureUtils.TextureFromColor(gd, new Color(150, 150, 150), (int)MapScreenBG.Width, (int)MapScreenBG.Height);
            DisposeOfMe.Add(tex);
            for (int i = 0; i < MapScrambleImages.Length; i++)
            {
                MapScrambleImages[i] = new UIImage(tex)
                {
                    Position = MapScreenBG.Position,
                    Rotation = i * -90f * (float)(Math.PI / 180),
                    Origin = new Vector2(MapScreenBG.Width / 2f, MapScreenBG.Height / 2f),
                    DissolveOpacity = 0.33f
                };
                Add(MapScrambleImages[i]);
            }

            // this momi logo will rotate
            LogoContainer = new UIMaskedContainer(new Rectangle(0, 0, 100, 100))
            {
                Position = MapScreenBG.Position
            };
            Add(LogoContainer);
            MomiLogo = new UIImage(momiLogoTex)
            {
                Position = LogoContainer.Position
            };
            LogoContainer.Add(MomiLogo);
            LogoContainer.UpdateChildMasks();

            // Screen edge added very last, which provides the border for main and map screens
            ScreensEdge = new UIImage(screenTex)
            {
                X = 126,
                Y = 8
            };
            ScreensEdge.Position += BaseOffset;
            Add(ScreensEdge);

            /*
             * 
             * Compass
             * 
             */

            // compass inner
            CompassInner = new UIImage(compassInnerTex)
            {
                X = 497,
                Y = 35,
                Origin = new Vector2(65 / 2f, 66 / 2f)
            };
            CompassInner.Position += BaseOffset;
            Add(CompassInner);

            // compass edge/border, non-moving
            CompassEdge = new UIImage(compassEdgeTex)
            {
                X = 497,
                Y = 34
            };
            CompassEdge.Position += BaseOffset;
            Add(CompassEdge);

            /*
             * 
             * Directional Pad and its functions
             * 
             */

            // Directional Pad
            DirectionalPad = new UIImage(DPad)
            {
                X = 514,
                Y = 220
            };
            DirectionalPad.Position += BaseOffset;
            Add(DirectionalPad);

            // add the invisible buttons onto the Dpad
            PressUp = new UIInvisibleButton(26, 23, invisibleTexture)
            {
                X = 24,
                Y = 2,
                Tooltip = GameFacade.Strings.GetString("f125", "1") // "Up"
            };
            PressUp.Position += DirectionalPad.Position;
            Add(PressUp);
            PressRight = new UIInvisibleButton(23, 26, invisibleTexture)
            {
                X = 50,
                Y = 26,
                Tooltip = GameFacade.Strings.GetString("f125", "2") // "Right"
            };
            PressRight.Position += DirectionalPad.Position;
            Add(PressRight);
            PressDown = new UIInvisibleButton(26, 23, invisibleTexture)
            {
                X = 25,
                Y = 50,
                Tooltip = GameFacade.Strings.GetString("f125", "3") // "Down"
            };
            PressDown.Position += DirectionalPad.Position;
            Add(PressDown);
            PressLeft = new UIInvisibleButton(23, 26, invisibleTexture)
            {
                X = 2,
                Y = 26,
                Tooltip = GameFacade.Strings.GetString("f125", "4") // "Left"
            };
            PressLeft.Position += DirectionalPad.Position;
            Add(PressLeft);

            // button listeners
            PressUp.OnButtonDown += (btn) => { ButtonDown(UIMazeTwoEODControls.Up); };
            PressUp.OnButtonClick += (btn) => { ButtonPressed(UIMazeTwoEODControls.Up); };
            PressRight.OnButtonDown += (btn) => { ButtonDown(UIMazeTwoEODControls.Right); };
            PressRight.OnButtonClick += (btn) => { ButtonPressed(UIMazeTwoEODControls.Right); };
            PressDown.OnButtonDown += (btn) => { ButtonDown(UIMazeTwoEODControls.Down); };
            PressDown.OnButtonClick += (btn) => { ButtonPressed(UIMazeTwoEODControls.Down); };
            PressLeft.OnButtonDown += (btn) => { ButtonDown(UIMazeTwoEODControls.Left); };
            PressLeft.OnButtonClick += (btn) => { ButtonPressed(UIMazeTwoEODControls.Left); };
        }

        /// <summary>
        /// This is a separate Get() because it causes a mild freeze, probably due to the size of the Grow Wall Image...
        /// </summary>
        private void LoadGrowWallTextures()
        {
            var gd = GameFacade.GraphicsDevice;
            AbstractTextureRef wallsGrowEasyTexRef = new FileTextureRef(WallsGrowEasyTexPath);
            AbstractTextureRef wallsGrowRegularTexRef = new FileTextureRef(WallsGrowRegularTexPath);
            try
            {
                WallsGrowEasyTexture = wallsGrowEasyTexRef.Get(gd);
                WallsGrowRegularTexture = wallsGrowRegularTexRef.Get(gd);
            }
            catch (Exception GrowWallException)
            {
                // todo:
            }

            // load the easy maze by default
            LoadMazeUI();
        }

        /// <summary>
        /// This loads all of the UI pieces that don't need to be loaded before the user sees anything. It's done in the background.
        /// </summary>
        private void LoadMazeUI()
        {
            var wallsChoiceTex = WallsChoiceEasyTexture;
            var wallsGrowTex = WallsGrowEasyTexture;

            GrowWallTextureHeight = wallsGrowTex.Height;
            GrowWallTextureWidth = wallsGrowTex.Width;

            // Screen stuff to load in the background
            Sky = new UIImage(SkyTexture)
            {
                DissolveOpacity = 0f
            };
            Background.Add(Sky);

            Floor = new UIImage(FloorTurningTextures[FloorTurningTextureIndex])
            {
                Visible = false
            };
            Background.Add(Floor);
            FloorDissolve = new UIImage(FloorTurningTextures[FloorTurningTextureIndex])
            {
                DissolveOpacity = 0f
            };
            Background.Add(FloorDissolve);

            float offStageAbstractX = SCALE_MAIN_TURN * GrowWallTextureWidth;

            MicroWall = new UIImage(wallsChoiceTex)
            {
                ScaleX = 0f,
                ScaleY = 0f
            };
            Background.Add(MicroWall);

            HorizonWall = new UIImage(wallsChoiceTex)
            {
                ScaleX = SCALE_SMALL_DECISION,
                ScaleY = SCALE_SMALL_DECISION,
                DissolveOpacity = 0f
            };
            Background.Add(HorizonWall);

            HorizonLeft = new UIImage(wallsChoiceTex)
            {
                ScaleX = SCALE_SMALL_ROTATION,
                ScaleY = SCALE_SMALL_ROTATION,
                AbstractX = offStageAbstractX * -1
            };
            Background.Add(HorizonLeft);

            HorizonRight = new UIImage(wallsChoiceTex)
            {
                ScaleX = SCALE_SMALL_ROTATION,
                ScaleY = SCALE_SMALL_ROTATION,
                AbstractX = offStageAbstractX
            };
            Background.Add(HorizonRight);

            HorizonWallTurnBack = new UIImage(wallsChoiceTex)
            {
                ScaleX = SCALE_SMALL_ROTATION,
                ScaleY = SCALE_SMALL_ROTATION,
                AbstractX = offStageAbstractX * 2
            };
            Background.Add(HorizonWallTurnBack);

            MainChoiceWall = new UIImage(wallsChoiceTex)
            {
                ScaleX = SCALE_MAIN_DECISION,
                ScaleY = SCALE_MAIN_DECISION,
                DissolveOpacity = 0f
            };
            Background.Add(MainChoiceWall);

            MainTurnWall = new UIImage(wallsGrowTex)
            {
                ScaleX = SCALE_MAIN_TURN,
                ScaleY = SCALE_MAIN_TURN,
                Visible = false
            };
            Background.Add(MainTurnWall);

            MainLeft = new UIImage(wallsGrowTex)
            {
                ScaleX = SCALE_MAIN_TURN,
                ScaleY = SCALE_MAIN_TURN,
                AbstractX = offStageAbstractX * -1
            };
            Background.Add(MainLeft);

            MainRight = new UIImage(wallsGrowTex)
            {
                ScaleX = SCALE_MAIN_TURN,
                ScaleY = SCALE_MAIN_TURN,
                AbstractX = offStageAbstractX
            };
            Background.Add(MainRight);

            MainTurnBackWall = new UIImage(wallsGrowTex)
            {
                ScaleX = SCALE_MAIN_TURN,
                ScaleY = SCALE_MAIN_TURN,
                AbstractX = offStageAbstractX * 2
            };
            Background.Add(MainTurnBackWall);

            LoadBarriersAndHints();
        }

        private void LoadBarriersAndHints()
        {
            var gd = GameFacade.GraphicsDevice;

            AbstractTextureRef frontBarrierTexRef = new FileTextureRef(FrontBarrierTexPath);
            AbstractTextureRef leftBarrierTexRef = new FileTextureRef(LeftBarrierTexPath);
            AbstractTextureRef rightBarrierTexRef = new FileTextureRef(RightBarrierTexPath);
            Texture2D frontBarrierTex = null;
            Texture2D leftBarrierTex = null;
            Texture2D rightBarrierTex = null;
            AbstractTextureRef hintDeadEndTexRef = new FileTextureRef(HintDeadEndTexPath);
            AbstractTextureRef hintGoBackwardTexRef = new FileTextureRef(HintGoBackwardTexPath);
            AbstractTextureRef hintGoForwardTexRef = new FileTextureRef(HintGoForwardTexPath);
            AbstractTextureRef hintGoLeftTexRef = new FileTextureRef(HintGoLeftTexPath);
            AbstractTextureRef hintGoRightTexRef = new FileTextureRef(HintGoRightTexPath);
            Texture2D[] hintTextures = new Texture2D[5];

            try
            {
                frontBarrierTex = frontBarrierTexRef.Get(gd);
                leftBarrierTex = leftBarrierTexRef.Get(gd);
                rightBarrierTex = rightBarrierTexRef.Get(gd);
                hintTextures[0] = hintGoLeftTexRef.Get(gd);
                hintTextures[1] = hintGoForwardTexRef.Get(gd);
                hintTextures[2] = hintGoRightTexRef.Get(gd);
                hintTextures[3] = hintGoBackwardTexRef.Get(gd);
                hintTextures[4] = hintDeadEndTexRef.Get(gd);
            }
            catch (Exception BarrierHintException)
            {
                // todo:
            }

            FrontBarrier = new UIImage(frontBarrierTex)
            {
                DissolveOpacity = 0f
            };
            Background.Add(FrontBarrier);
            LeftBarrier = new UIImage(leftBarrierTex)
            {
                DissolveOpacity = 0f
            };
            Background.Add(LeftBarrier);
            RightBarrier = new UIImage(rightBarrierTex)
            {
                DissolveOpacity = 0f
            };
            Background.Add(RightBarrier);

            for (int index = 0; index < Hints.Length; index++)
            {
                Hints[index] = new UIImage(hintTextures[index])
                {
                    DissolveOpacity = 0f
                };
                Background.Add(Hints[index]);
            }

            // re-add the frame pieces so they're on absolute top
            Remove(ScreensEdge);
            Add(ScreensEdge);

            OpaqueRoomFeatures = new List<UIImage>() { LeftBarrier, FrontBarrier, RightBarrier };
            foreach (var hint in Hints)
                OpaqueRoomFeatures.Add(hint);

            // send the finished loading event
            Send("FreeSOMaze_loaded", new byte[0]);

            // first time lobby
            GotoLobby();
        }

        /// <summary>
        /// The lobby is displayed after the initial load on connection, after the conclusion of any maze, AND whenever the partner disconnects
        /// </summary>
        private void GotoLobby()
        {
            ThinkingEmoji.Visible = false;
            LobbyVisible = true;
            Remove(WinningSimBox);
            Remove(LogicSimBox);
            Remove(CharismaSimBox);
            
            // reset the map
            Remove(Map);
            Map = new UIMaskedContainer(new Rectangle(0, 0, 100, 100))
            {
                Position = MapScreenBG.Position
            };
            AddBefore(Map, YouAreHereImage);

            // scramble the map
            ScrambleMapHandler("", new byte[] { 1 });

            // Hide all maze stuff
            HideMazeElements(true);

            // Make lobby stuff visible
            foreach (var element in LobbyGraphics)
                element.Visible = true;
            
            // update text
            if (!ThreadLostScreen.Visible)
            {
                MainLobbyHeader.Visible = true;
                SubLobbyHeader.Visible = true;
                MainLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "10"); // "Select Difficulty"
                SubLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "11"); // "Both players must agree to continue."
                Add(LogicSimBox);
                Add(CharismaSimBox);
            }
        }

        /// <summary>
        /// Changes the texture of the directional pad image.
        /// </summary>
        private void ButtonDown(UIMazeTwoEODControls relativeDirection)
        {
            switch (relativeDirection)
            {
                case UIMazeTwoEODControls.Up:
                    DirectionalPad.Texture = DPadUp;
                    break;
                case UIMazeTwoEODControls.Right:
                    DirectionalPad.Texture = DPadRight; // 0 - $Base Win Amount, 1 - $Skill Bonus Amount, 2 - Total Time, 3 - This user's moves, 4 - Partner's moves
                    break;
                case UIMazeTwoEODControls.Down:
                    DirectionalPad.Texture = DPadDown;
                    break;
                case UIMazeTwoEODControls.Left:
                    DirectionalPad.Texture = DPadLeft;
                    break;
                default:
                    DirectionalPad.Texture = DPad;
                    break;
            }
        }
        /// <summary>
        /// Handles the input of the direction pad clicked by the user.
        /// </summary>
        private void ButtonPressed(UIMazeTwoEODControls relativeDirection)
        {
            // change the dpad back to netural texture
            ButtonDown(UIMazeTwoEODControls.None);

            // if input is allowed, handle it
            if (!LobbyVisible && PlayerInputAllowed)
            {
                PlaySound("tutorial_lpa_sfx");
                PlayerInputAllowed = false;
                // send event to server with chosen control direction
                
                var targetCardinal = (int)relativeDirection + (int)CurrentFacingCardinal;
                if (targetCardinal > 3)
                    targetCardinal -= 4;
                else if (targetCardinal < 0)
                    targetCardinal += 4;
                Send("FreeSOMaze_move_request", BitConverter.GetBytes(targetCardinal));
            }
        }

        /// <summary>
        /// Fades in the thinking emoji loading icon while loading maze parts in the background.
        /// </summary>
        private void PlayLoadingAnimation()
        {
            PlaySound("computer_boot_vexp");
            TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous, new UITweenInstanceMembers(GameFacade.Screens.Tween, LoadingLogo, 3f, new Dictionary<string, float>() { { "DissolveOpacity", 1.0f } }, TweenLinear.EaseNone));
            TweenQueue.CompleteAction = LoadGrowWallTextures;
            TweenQueue.PlayQueue();
        }

        /// <summary>
        /// The central place responsible for animating the whole UI. It creates the Tween queue and executes it based on the the relative direction argument.
        /// </summary>
        /// <param name="relativeDirection"></param>
        private void AnimateUIEOD(UIMazeTwoEODControls relativeDirection, UIMazeTwoEODDirections targetCardinal, bool roomVisited)
        {
            // Reset the wall references - there will be no visual difference here for the user
            ResetAllWalls();

            // Update the minimap behind the scenes, but it won't update in the eyes of the user until the animations are complete
            Cartograph(targetCardinal, !roomVisited);

            // the first step is to fade out any opaque room features - barriers and walls
            TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous, GetOpaqueRoomFeaturesTweens());

            // center the user in the room - queue the mini-grow tweens
            TweenQueue = GameFacade.Screens.Tween.Queue((TweenQueue != null && TweenQueue.HasQueue) ? UITweenQueueTypes.AppendedSynchronous : UITweenQueueTypes.Synchronous, GetMiniGrowTweens());

            TweenQueue.CompleteAction = () =>
            {
                PostTweenCleanup(targetCardinal);
            };

            if (!relativeDirection.Equals(UIMazeTwoEODControls.Up))
            {
                // queue up the compass tweens, unless they're moving forward (Up)
                TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Sequential, GetCompassTween(relativeDirection));

                // queue up the backdrop tween
                TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous, GetBGTweens(relativeDirection));

                // queue up the "turning" tweens to turn toward a relative direction
                TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous, GetTurningTweens(relativeDirection));
            }
            // finally, queue the final "big grow" tweens to visible walls
            TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.AppendedSynchronous, GetBigGrowTweens(relativeDirection));

            // play the tweens
            TweenQueue.PlayQueue();
            PlaySound("computer_running");
        }
        private void SwapBackgroundImages()
        {
            var temp = BackgroundPan;
            BackgroundPan = AltBackgroundPan;
            AltBackgroundPan = temp;
            SwapBGs = false;
        }
        private void SwapMainChoicetoGrow()
        {
            MainChoiceWall.Visible = false;
            MainTurnWall.Visible = true;
        }
        /// <summary>
        /// The walls are totally reset to allow for the next animation.
        /// </summary>
        private void ResetAllWalls()
        {
            float offStageAbstractX = SCALE_MAIN_TURN * GrowWallTextureWidth;

            HorizonWall.ScaleX = HorizonWall.ScaleY = SCALE_SMALL_DECISION;
            HorizonWall.AbstractX = 0;
            HorizonLeft.ScaleX = HorizonLeft.ScaleY = SCALE_SMALL_ROTATION;
            HorizonLeft.AbstractX = offStageAbstractX * -1 + 1;
            HorizonRight.ScaleX = HorizonRight.ScaleY = SCALE_SMALL_ROTATION;
            HorizonRight.AbstractX = offStageAbstractX - 1;
            HorizonWallTurnBack.ScaleX = HorizonWallTurnBack.ScaleY = SCALE_SMALL_ROTATION;
            HorizonWallTurnBack.AbstractX = offStageAbstractX * 2 - 1;

            MainChoiceWall.ScaleX = MainChoiceWall.ScaleY = SCALE_MAIN_DECISION;
            MainChoiceWall.Visible = true;
            MainTurnWall.ScaleX = MainTurnWall.ScaleY = SCALE_MAIN_TURN;
            MainTurnWall.AbstractX = 0;
            MainTurnWall.Visible = false;
            MainLeft.ScaleX = MainLeft.ScaleY = SCALE_MAIN_TURN;
            MainLeft.AbstractX = offStageAbstractX * -1;
            MainRight.ScaleX = MainRight.ScaleY = SCALE_MAIN_TURN;
            MainRight.AbstractX = offStageAbstractX;
            MainTurnBackWall.ScaleX = MainTurnBackWall.ScaleY = SCALE_MAIN_TURN;
            MainTurnBackWall.AbstractX = offStageAbstractX * 2;

            MicroWall.ScaleX = MicroWall.ScaleY = 0f;

            Background.UpdateChildMasks();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetCardinal"></param>
        private void PostTweenCleanup(UIMazeTwoEODDirections targetCardinal)
        {
            FloorTurningTicks = 0;
            FloorTurningTextureIndex = 0;
            CurrentFacingCardinal = targetCardinal;
            Floor.Texture = FloorTurningTextures[FloorTurningTextureIndex];
            // Update any lagging animations in the main screen
            Background.UpdateChildMasks();
            // update the map for the first and last time
            Map.UpdateChildMasks();
            // barriers and hints appear last
            AnimateBarriersAndHint();
        }
        /// <summary>
        /// Reset the TweenQueue for barriers and hints, if applicable.
        /// </summary>
        private void AnimateBarriersAndHint()
        {
            // get the tweens to fade in the walls
            if (DestinationRoomFeatures[0] == 1 || DestinationRoomFeatures[1] == 1 || DestinationRoomFeatures[2] == 1 || DestinationRoomFeatures[3] == 1)
                TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous, GetBarrierTweens(DestinationRoomFeatures));

            // get the hint, if applicable
            //ShowUIAlert("client", "AnimateBarriers: Hint is still: " + DestinationRoomFeatures[4], null);
            if (DestinationRoomFeatures[4] < 6) // 10 is no hint
            {
                var relativeDirection = UIMazeTwoEODControls.DeadEnd;
                if (DestinationRoomFeatures[4] < 5) // if it's not a dead end
                {
                    // convert it from its cardinal direction, which is how it was sent and stored
                    relativeDirection = GetRelativeDirectionFromCardinal(DestinationRoomFeatures[4]);
                    //ShowUIAlert("client", "AnimateBarriers: Relative is: " + (int)relativeDirection, null);
                }
                // == 0-3 are directions, == 4 is a dead end, which always shows a "hint"
                if (!relativeDirection.Equals(UIMazeTwoEODControls.None))
                    TweenQueue = GameFacade.Screens.Tween.Queue(UITweenQueueTypes.Synchronous, GetHintTween(relativeDirection));
            }
            else
                Hint = null;

            if (TweenQueue.HasQueue)
            {
                TweenQueue.PlayQueue();
                if (Hint != null)
                    PlaySound("game_comp_buzzer");
            }

            DestinationRoomFeatures = new byte[5];
        }

        #region GetTweens
        /// <summary>
        /// Barriers that exist will fade in after user arrives in the room.
        /// </summary>
        /// <param name="barrierData">N E W S = [0] [1] [2] [3]; they equal == 1 if the barrier does exist, but then it must be converted to general direction from cardinal direction</param>
        /// <returns></returns>
        private UITweenInstanceMembers[] GetBarrierTweens(byte[] barrierData)
        {
            List<UITweenInstanceMembers> tweens = new List<UITweenInstanceMembers>();
            var barriers = new List<UIImage>();
            if (barrierData != null && barrierData.Length > 3)
            {
                var opacity = new Dictionary<string, float> { { "DissolveOpacity", 1.0f } };
                if (barrierData[0] == 1)
                {
                    UIImage barrier = null;
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.North);
                    if (barrier != null)
                        barriers.Add(barrier);
                }
                if (barrierData[1] == 1)
                {
                    UIImage barrier = null;
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.East);
                    if (barrier != null)
                        barriers.Add(barrier);
                }
                if (barrierData[2] == 1)
                {
                    UIImage barrier = null;
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.West);
                    if (barrier != null)
                        barriers.Add(barrier);
                }
                if (barrierData[3] == 1)
                {
                    UIImage barrier = null;
                    barrier = ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections.South);
                    if (barrier != null)
                        barriers.Add(barrier);
                }
                for (int index = 0; index < barriers.Count; index++)
                    tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, barriers[index], .5f, opacity, TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks));
            }
            return tweens.ToArray();
        }

        /// <summary>
        /// Changes the texture of the hint, returns a Tween for it.
        /// </summary>
        /// <param name="direction">0 = Left, 1 = Up, 2 = Right, 3 = Down, 4 = Dead End;</param>
        /// <returns></returns>
        private UITweenInstanceMembers GetHintTween(UIMazeTwoEODControls direction)
        {
            Hint = Hints[HintIndices[direction]];
            return new UITweenInstanceMembers(GameFacade.Screens.Tween, Hint, .5f, new Dictionary<string, float> { { "DissolveOpacity", 1.0f } }, TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks);
        }

        /// <summary>
        /// Any hint or barriers visible must be dissolved away.
        /// </summary>
        /// <returns></returns>
        private UITweenInstanceMembers[] GetOpaqueRoomFeaturesTweens()
        {
            List<UITweenInstanceMembers> tweens = new List<UITweenInstanceMembers>();
            var opacity = new Dictionary<string, float> { { "DissolveOpacity", 0f } };
            for (int index = 0; index < OpaqueRoomFeatures.Count; index++)
            {
                var feature = OpaqueRoomFeatures[index];
                tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, feature, 0.25f, opacity, TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks));
            }
            return tweens.ToArray();
        }

        /// <summary>
        /// Retruns a set of Tweens that "move" the user forward to the center of the "room" before moving them again. These tweens are always the same regardless of direction.
        /// They "center" the viewer in the "room" by growing everything visible before moving in a direction.
        /// </summary>
        /// <returns>UITweenInstanceMembers class in UITween.cs</returns>
        private UITweenInstanceMembers[] GetMiniGrowTweens()
        {
            List<UITweenInstanceMembers> tweens = new List<UITweenInstanceMembers>()
            {
                // the main wall in the center
                new UITweenInstanceMembers(GameFacade.Screens.Tween, MainChoiceWall, 0.5f, new Dictionary<string, float> { { "ScaleX", SCALE_MAIN_GROW }, { "ScaleY", SCALE_MAIN_GROW } },
                TweenQuad.EaseIn).OnUpdateAction(Background.UpdateChildMasks).OnCompleteAction(SwapMainChoicetoGrow),
                // small wall visible on the horizon
                new UITweenInstanceMembers(GameFacade.Screens.Tween, HorizonWall, 0.5f, new Dictionary<string, float> { { "ScaleX", SCALE_SMALL_ROTATION }, { "ScaleY", SCALE_SMALL_ROTATION } },
                TweenQuad.EaseIn).OnUpdateAction(Background.UpdateChildMasks)
            };
            return tweens.ToArray();
        }

        /// <summary>
        /// Returns an array of Tweens that simulate the background changing by moving the background images
        /// </summary>
        /// <param name="relativeDirection"></param>
        /// <returns>UITweenInstanceMembers class in UITween.cs</returns>
        private UITweenInstanceMembers[] GetBGTweens(UIMazeTwoEODControls relativeDirection)
        {

            List<UITweenInstanceMembers> tweens = new List<UITweenInstanceMembers>();
            /*float AbstractXChange = (int)relativeDirection * BackgroundAbstractXByCardinal[UIMazeTwoEODDirections.East]; // the width of the field of view of a direction (346f) plus half [times] -1, 1, or 2
            switch (CurrentFacingCardinal)
            {
                case UIMazeTwoEODDirections.North:
                    {
                        // if AbstractX change is positive, meaning user is turning left to west
                        if (AbstractXChange > 0)
                        {
                            // put altBG in place
                            AltBackgroundPan.AbstractX = OFFSCREEN_NORTH_ABSTRACT_X;

                            // queue tween for alterBG
                            SwapBGs = true;
                        }
                        break;
                    }
                case UIMazeTwoEODDirections.South:
                    {
                        // if AbstractX change indicates moving 180 degrees to north
                        var absValueAbsX = Math.Abs(AbstractXChange);
                        if (absValueAbsX == 1038f)
                        {
                            // put altBG in place
                            AltBackgroundPan.AbstractX = absValueAbsX;

                            // queue tween for alterBG
                            SwapBGs = true;
                        }
                        break;
                    }
                case UIMazeTwoEODDirections.West:
                    {
                        // if AbstractX change is negative, meaning user is turning right to North or right twice to South
                        if (AbstractXChange < 0)
                        {
                            // put altBG in place
                            AltBackgroundPan.AbstractX = BackgroundAbstractXByCardinal[UIMazeTwoEODDirections.East] * -1;

                            // queue tween for alterBG
                            SwapBGs = true;
                        }
                        break;
                    }
            }
            // queue applicable alterbg tween
            if (SwapBGs)
                tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, AltBackgroundPan, 0.5f, new Dictionary<string, float> { { "AbstractX", AltBackgroundPan.AbstractX + AbstractXChange } },
                    TweenLinear.EaseNone).OnCompleteAction(SwapBackgroundImages));

            // queue tween for the main background
            tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, BackgroundPan, 0.5f, new Dictionary<string, float>() { { "AbstractX", BackgroundPan.AbstractX + AbstractXChange } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks));*/

            /* Make the floor turn
            int direction = (int)relativeDirection * -1;
            var tweenTime = 0.090909f;
            var totalTweens = 11;
            if (direction == -2)
                totalTweens = 22;
            var cumulativeTweenTime = tweenTime;
            int textureIndex = 1;
            int propIndex = 0;
            int targetIndex = 0;
            string[] props = new string[] { "AbstractX", "AbstractY", "DissolveOpacity", "ScaleX", "ScaleY" };
            UIImage[] targets = new UIImage[] { FloorDissolve, ThinkingEmoji, EasyLoadEmoji, RegularLoadEmoji, HardLoadEmoji };

            for (int tweenIndex = 0; tweenIndex < totalTweens; tweenIndex++)
            {
                tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, targets[targetIndex], cumulativeTweenTime, new Dictionary<string, float>() { { props[propIndex], (propIndex < 3) ? 0f : 1f } },
                    TweenLinear.EaseNone).OnCompleteAction(() => { Floor.Texture = FloorTurningTextures[textureIndex]; }));
                cumulativeTweenTime += tweenTime;
                if (++textureIndex >= FloorTurningTextures.Length)
                    textureIndex = 0;
                if (++propIndex >= props.Length)
                {
                    propIndex = 0;
                    targetIndex++;
                }
            }
            // add the last one
            tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, targets[targetIndex], cumulativeTweenTime, new Dictionary<string, float>() { { props[propIndex], (propIndex < 3) ? 0f : 1f } },
                TweenLinear.EaseNone).OnCompleteAction(() => { Floor.Texture = FloorTurningTextures[0]; }));
            return tweens.ToArray();*/

            int direction = (int)relativeDirection * -1;
            bool turnBackward = false;
            var tweenTime = 0.70f;
            if (direction == -2)
                tweenTime = 1.35f;
            else if (direction == 1)
                turnBackward = true;
            tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, FloorDissolve, tweenTime, new Dictionary<string, float>() { { "AbstractX", 0f } }, TweenLinear.EaseNone).OnUpdateAction(() => { TurnFloorUpdateHander(turnBackward); }));
            return tweens.ToArray();
        }

        /// <summary>
        /// Returns a Tween that rotates the compass to the target direction calculated from the current cardinal direction coupled with the chosen relative direction.
        /// </summary>
        /// <param name="relativeDirection"></param>
        /// <returns>UITweenInstanceMembers class in UITween.cs</returns>
        private UITweenInstanceMembers GetCompassTween(UIMazeTwoEODControls relativeDirection)
        {
            var angleChange = AngleChangeByRelativeDirection[relativeDirection];
            int direction = (int)relativeDirection * -1;
            var tweenTime = 0.70f;
            if (direction == -2)
                tweenTime = 1.35f;
            CurrentRotation = CompassCardinalTrueAngles[CurrentFacingCardinal];
            var targetAngle = CurrentRotation + angleChange;
            var compInst = new UITweenInstanceMembers(GameFacade.Screens.Tween, CompassInner, tweenTime, new Dictionary<string, float>() { { "Rotation", targetAngle } }, TweenLinear.EaseNone);
            return compInst;
        }

        /// <summary>
        /// Returns an array of Tweens that "turn" the view in the specified relative direction by uniformly changing the abstractX of every visible piece.
        /// </summary>
        /// <param name="relativeDirection"></param>
        /// <returns>UITweenInstanceMembers class in UITween.cs</returns>
        private UITweenInstanceMembers[] GetTurningTweens(UIMazeTwoEODControls relativeDirection)
        {
            int direction = (int)relativeDirection * -1;
            float targetAbstractX = SCALE_MAIN_TURN * GrowWallTextureWidth * direction;
            var tweenTime = 0.70f;
            if (direction == -2)
                tweenTime = 1.35f;

            List<UITweenInstanceMembers> tweens = new List<UITweenInstanceMembers>()
            {
                // three smaller walls, center, left and right
                new UITweenInstanceMembers(GameFacade.Screens.Tween, HorizonLeft, tweenTime, new Dictionary<string, float> { { "AbstractX", SCALE_MAIN_TURN * GrowWallTextureWidth * -1 + targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks),
                new UITweenInstanceMembers(GameFacade.Screens.Tween, HorizonWall, tweenTime, new Dictionary<string, float> { { "AbstractX", targetAbstractX} },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks),
                new UITweenInstanceMembers(GameFacade.Screens.Tween, HorizonRight, tweenTime, new Dictionary<string, float> { { "AbstractX", SCALE_MAIN_TURN * GrowWallTextureWidth + targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks),
                // three main walls, center left and right
                new UITweenInstanceMembers(GameFacade.Screens.Tween, MainLeft, tweenTime, new Dictionary<string, float> { { "AbstractX", SCALE_MAIN_TURN * GrowWallTextureWidth * -1 + targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks),
                new UITweenInstanceMembers(GameFacade.Screens.Tween, MainTurnWall, tweenTime, new Dictionary<string, float> { { "AbstractX", targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks),
                new UITweenInstanceMembers(GameFacade.Screens.Tween, MainRight, tweenTime, new Dictionary<string, float> { { "AbstractX", SCALE_MAIN_TURN * GrowWallTextureWidth + targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks),
            };
            // special case for moving backward
            if (tweenTime == 1.35f)
            {
                tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, HorizonWallTurnBack, tweenTime, new Dictionary<string, float> { { "AbstractX", SCALE_MAIN_TURN * GrowWallTextureWidth * 2 + targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks));
                tweens.Add(new UITweenInstanceMembers(GameFacade.Screens.Tween, MainTurnBackWall, tweenTime, new Dictionary<string, float> { { "AbstractX", SCALE_MAIN_TURN * GrowWallTextureWidth * 2 + targetAbstractX } },
                TweenLinear.EaseNone).OnUpdateAction(Background.UpdateChildMasks));
            }
            return tweens.ToArray();
        }

        /// <summary>
        /// Returns an array of Tweens that "move" the user forward by uniformly scaling every visible piece.
        /// </summary>
        /// <param name="relativeDirection"></param>
        /// <returns>UITweenInstanceMembers class in UITween.cs</returns>
        private UITweenInstanceMembers[] GetBigGrowTweens(UIMazeTwoEODControls relativeDirection)
        {
            UIImage mainWall = null;
            UIImage horizon = null;
            // MicroWall
            switch (relativeDirection)
            {
                case UIMazeTwoEODControls.Up:
                    {
                        mainWall = MainTurnWall;
                        horizon = HorizonWall;
                        break;
                    }
                case UIMazeTwoEODControls.Left:
                    {
                        mainWall = MainLeft;
                        horizon = HorizonLeft;
                        break;
                    }
                case UIMazeTwoEODControls.Right:
                    {
                        mainWall = MainRight;
                        horizon = HorizonRight;
                        break;
                    }
                case UIMazeTwoEODControls.Down:
                    {
                        mainWall = MainTurnBackWall;
                        horizon = HorizonWallTurnBack;
                        break;
                    }
            }
            List<UITweenInstanceMembers> tweens = new List<UITweenInstanceMembers>()
            {
                // tiniest wall invisible currently 
                new UITweenInstanceMembers(GameFacade.Screens.Tween, MicroWall, 1.25f, new Dictionary<string, float> { { "ScaleX", SCALE_SMALL_DECISION }, { "ScaleY", SCALE_SMALL_DECISION } },
                TweenQuad.EaseInOut).OnUpdateAction(Background.UpdateChildMasks).OnCompleteAction(Background.UpdateChildMasks),
                // small wall visible on the horizon
                new UITweenInstanceMembers(GameFacade.Screens.Tween, horizon, 1.25f, new Dictionary<string, float> { { "ScaleX", SCALE_MAIN_DECISION }, { "ScaleY", SCALE_MAIN_DECISION } },
                TweenQuad.EaseInOut).OnUpdateAction(Background.UpdateChildMasks).OnCompleteAction(Background.UpdateChildMasks),
                // the main wall in the center
                new UITweenInstanceMembers(GameFacade.Screens.Tween, mainWall, 1.25f, new Dictionary<string, float> { { "ScaleX", SCALE_MAIN_GROW }, { "ScaleY", SCALE_MAIN_GROW } },
                TweenQuad.EaseInOut).OnUpdateAction(Background.UpdateChildMasks).OnCompleteAction(Background.UpdateChildMasks)
            };
            return tweens.ToArray();
        }
        #endregion
        /// <summary>
        /// Self explanatory
        /// </summary>
        /// <param name="cardinal"></param>
        /// <returns></returns>
        private UIMazeTwoEODControls GetRelativeDirectionFromCardinal(int cardinal)
        {
            var cardinals = (int[])Enum.GetValues(typeof(UIMazeTwoEODDirections));
            var index = Array.IndexOf(cardinals, CurrentFacingCardinal);
            int relativeDirection = 0;
            while (cardinals[index] != cardinal)
            {
                if (++index > 3)
                    index = 0;
                relativeDirection++;
            }
            if (relativeDirection == 3)
                relativeDirection = -1;
            if (Enum.IsDefined(typeof(UIMazeTwoEODControls), relativeDirection))
                return (UIMazeTwoEODControls)Enum.ToObject(typeof(UIMazeTwoEODControls), relativeDirection);
            return UIMazeTwoEODControls.None;
        }

        /// <summary>
        /// Self explanataory
        /// </summary>
        /// <param name="cardinal"></param>
        /// <returns></returns>
        private UIImage ConvertCardinalToRelativeBarrier(UIMazeTwoEODDirections cardinal)
        {
            switch (cardinal)
            {
                case UIMazeTwoEODDirections.North:
                    {
                        if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.North))
                            return FrontBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.West))
                            return RightBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.East))
                            return LeftBarrier;
                        break;
                    }
                case UIMazeTwoEODDirections.East:
                    {
                        if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.East))
                            return FrontBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.North))
                            return RightBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.South))
                            return LeftBarrier;
                        break;
                    }
                case UIMazeTwoEODDirections.West:
                    {
                        if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.West))
                            return FrontBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.South))
                            return RightBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.North))
                            return LeftBarrier;
                        break;
                    }
                case UIMazeTwoEODDirections.South:
                    {
                        if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.South))
                            return FrontBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.East))
                            return RightBarrier;
                        else if (CurrentFacingCardinal.Equals(UIMazeTwoEODDirections.West))
                            return LeftBarrier;
                        break;
                    }
            }
            return null;
        }

        private void TurnFloorUpdateHander(bool backward)
        {
            int incrememt = -1;
            if (backward)
                incrememt = 1;
            if (++FloorTurningTicks == 4)
            {
                FloorTurningTicks = 0;
                FloorTurningTextureIndex += incrememt;
                if (FloorTurningTextureIndex >= FloorTurningTextures.Length)
                    FloorTurningTextureIndex = 0;
                else if (FloorTurningTextureIndex < 0)
                    FloorTurningTextureIndex = FloorTurningTextures.Length - 1;
            }
            Floor.Texture = FloorTurningTextures[FloorTurningTextureIndex];
        }

        /// <summary>
        /// Adds walls to the map based on the original sent data from the server regarding wall configuration and hints
        /// </summary>
        /// <param name="WallsKey"></param>
        private void AddMapWalls(byte[] WallsKey)
        {
            if (WallsKey != null && WallsKey.Length > 3)
            {
                if (WallsKey[0] == 1)
                {
                    var north = new UIImage(MapWallHorizontalTex)
                    {
                        AbstractX = NorthWallOffset.X,
                        AbstractY = NorthWallOffset.Y
                    };
                    Map.Add(north);
                    Map.UpdateChildMask(north);
                }
                if (WallsKey[1] == 1)
                {
                    var east = new UIImage(MapWallVerticalTex)
                    {
                        AbstractX = EastWallOffset.X,
                        AbstractY = EastWallOffset.Y
                    };
                    Map.Add(east);
                    Map.UpdateChildMask(east);
                }
                if (WallsKey[2] == 1)
                {
                    var west = new UIImage(MapWallVerticalTex)
                    {
                        AbstractX = WestWallOffset.X,
                        AbstractY = WestWallOffset.Y
                    };
                    Map.Add(west);
                    Map.UpdateChildMask(west);
                }
                if (WallsKey[3] == 1)
                {
                    var south = new UIImage(MapWallHorizontalTex)
                    {
                        AbstractX = SouthWallOffset.X,
                        AbstractY = SouthWallOffset.Y
                    };
                    Map.Add(south);
                    Map.UpdateChildMask(south);
                }
            }
        }

        /// <summary>
        /// Handles the navigation of the minimap after a direction is chosen. Also creates a new room if it hasn't been visited before
        /// </summary>
        /// <param name="direction">The cardinal direction based on the user's choice</param>
        /// <param name="newRoom">This is added only if the user hasn't visited this room before</param>
        private void Cartograph(UIMazeTwoEODDirections direction, bool newRoom)
        {
            float xChange = 0;
            float yChange = 0;
            switch (direction)
            {
                case UIMazeTwoEODDirections.North: yChange = 32; break;
                case UIMazeTwoEODDirections.East: xChange = -32; break;
                case UIMazeTwoEODDirections.West: xChange = 32; break;
                case UIMazeTwoEODDirections.South: yChange = -32; break;
            }
            var children = Map.GetChildren();
            for (int index = 0; index < children.Count; index++)
            {
                var child = children[index] as UIImage;
                if (xChange != 0)
                    child.AbstractX += xChange;
                if (yChange != 0)
                    child.AbstractY += yChange;
            }
            if (newRoom)
                Map.Add(new UIImage(MapRoomTex));
        }

        /// <summary>
        /// Just moves the PersonButton's y
        /// </summary>
        /// <param name="btn">MyPersonButton or PartnerPersonButton</param>
        /// <param name="y">why tho</param>
        private void MovePersonButton(UIVMPersonButton btn, float y)
        {
            if (btn != null)
            {
                btn.Y = y + PersonButtonOffset.Y;
                Remove(btn);
                if (LobbyVisible)
                    Add(btn);
            }
        }

        /// <summary>
        /// Since the partner disconnected, the UIVMPersonButton partner SimBox should be unknown, Label should read "Waiting..."
        /// </summary>
        private void ResetPartner()
        {
            Remove(PartnerPersonButton);
            PartnerPersonButton = null;

            var head = Content.Content.Get().AvatarOutfits.Get(0x3a00000000d); // "mah000_proxy.oft"
            var emptyBody = Content.Content.Get().AvatarOutfits.Get(0x24c0000000d); // "mab000_xy__proxy.oftr"
            PartnerSimBox.Avatar.Appearance = Vitaboy.AppearanceType.Light;
            PartnerSimBox.Avatar.Head = head;
            PartnerSimBox.Avatar.Body = emptyBody;
            PartnerSimBox.Avatar.Handgroup = emptyBody;
            PartnerSimBox.AutoRotate = false;

            PartnerSkillLabel.CurrentText = GameFacade.Strings.GetString("f125", "17"); // "Waiting..."
        }

        /// <summary>
        /// Called by DisplayWinScreen and GotoLobby 
        /// </summary>
        private void HideMazeElements(bool includeWalls)
        {
            ResetAllWalls();
            if (includeWalls)
            {
                HorizonWall.DissolveOpacity = 0f;
                MainChoiceWall.DissolveOpacity = 0f;
                FloorDissolve.DissolveOpacity = 0f;
                FloorDissolve.Visible = true;
                Floor.Visible = false;
                Sky.DissolveOpacity = 0f;
                Floor.Texture = FloorTurningTextures[0];
                FloorTurningTextureIndex = 0;
                FloorTurningTicks = 0;
            }
            YouAreHereTimer.Stop();
            YouAreHereImage.Visible = false;

            foreach (var feature in OpaqueRoomFeatures)
                feature.DissolveOpacity = 0f;
        }

        /// <summary>
        /// Displays the Winning Sim Box, updated lobby text, and a UIAlert with appropriate round details
        /// </summary>
        /// <param name="gameResults">Serialized strings: 0 - $Base Win Amount, 1 - $Skill Bonus Amount, 2 - Total Time, 3 - This user's moves, 4 - Partner's moves</param>
        private void DisplayWinScreen(string[] gameOverData)
        {
            PlaySound("computer_boot_vexp");
            MainLobbyHeader.CurrentText = GameFacade.Strings.GetString("f125", "21"); // "Thread Connected!"
            MainLobbyHeader.Visible = true;
            Add(WinningSimBox);

            HideMazeElements(false);

            if (gameOverData.Length > 4)
            {
                // parse the necessary data
                int total = 0;
                if (Int32.TryParse(gameOverData[0], out int baseWin) && (Int32.TryParse(gameOverData[1], out int bonus)))
                    total = baseWin + bonus;

                var title = GameFacade.Strings.GetString("f125", "25"); // "Gameover"
                var message = GameFacade.Strings.GetString("f125", "26") + System.Environment.NewLine + System.Environment.NewLine// "You have successfully connected the thread. MOMI has become more stable."
                    + GameFacade.Strings.GetString("f125", "27") + ": " + gameOverData[3] + System.Environment.NewLine // "Your Moves: xxx"
                    + GameFacade.Strings.GetString("f125", "28") + ": " + gameOverData[4] + System.Environment.NewLine + System.Environment.NewLine // "Partner's Moves: xxx"
                    + GameFacade.Strings.GetString("f125", "22") + ": $" + gameOverData[0] + System.Environment.NewLine + System.Environment.NewLine // "Maze Payout: $xxxx"
                    + GameFacade.Strings.GetString("f125", "23") + ": $" + gameOverData[1] + System.Environment.NewLine + System.Environment.NewLine // "Skill Bonus: $xxxx"
                    + GameFacade.Strings.GetString("f125", "24") + ": $" + total; // "Total Profit :$xxxx"
                ResultsScreen = ShowUIAlert(title, message, GotoLobby);
            }
        }

        /// <summary>
        /// Shows a UI alert and even allows an action argument for when the window is closed.
        /// </summary>
        /// <param name="title">Title of the alert window</param>
        /// <param name="message">The body of the window</param>
        /// <param name="action">What to do upon clicking OK</param>
        /// <returns></returns>
        private UIAlert ShowUIAlert(string title, string message, Action action)
        {
            UIAlert alert = null;
            alert = UIScreen.GlobalShowAlert(new UIAlertOptions()
            {
                TextSize = 12,
                Title = title,
                Message = message,
                Alignment = TextAlignment.Center,
                TextEntry = false,
                Buttons = UIAlertButton.Ok((btn) =>
                {
                    UIScreen.RemoveDialog(alert);
                    ResultsScreen = null;
                    action?.Invoke();
                    AlertClosed.Invoke();
                }),
            }, true);
            return alert;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="soundString"></param>
        private void PlaySound(string soundString) => HIT.HITVM.Get().PlaySoundEvent(soundString);
    }
    public enum UIMazeTwoEODControls
    {
        None = 10,
        Left = -1,
        Up = 0,
        Right = 1,
        Down = 2,
        DeadEnd = 5
    }
    public enum UIMazeTwoEODDirections
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }
}