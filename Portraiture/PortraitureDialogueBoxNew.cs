using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace Portraiture
{
    public class PortraitureDialogueBoxNew : IClickableMenu
    {
        private List<string> dialogues = new List<string>();
        private Stack<string> characterDialoguesBrokenUp = new Stack<string>();
        private List<Response> responses = new List<Response>();
        private Rectangle friendshipJewel = Rectangle.Empty;
        private int transitionX = -1;
        private int safetyTimer = 750;
        private int selectedResponse = -1;
        private bool transitioning = true;
        private bool transitioningBigger = true;
        private string hoverText = "";
        private Dialogue characterDialogue;
        public const int portraitBoxSize = 74;
        public const int nameTagWidth = 102;
        public const int nameTagHeight = 18;
        public const int portraitPlateWidth = 115;
        public const int nameTagSideMargin = 5;
        public const float transitionRate = 3f;
        public const int characterAdvanceDelay = 30;
        public const int safetyDelay = 750;
        private int questionFinishPauseTimer;
        public List<ClickableComponent> responseCC;
        private bool activatedByGamePad;
        private int x;
        private int y;
        private int transitionY;
        private int transitionWidth;
        private int transitionHeight;
        private int characterAdvanceTimer;
        private int characterIndexInDialogue;
        private int heightForQuestions;
        private int newPortaitShakeTimer;
        private int gamePadIntroTimer;
        private bool dialogueContinuedOnNextPage;
        private bool dialogueFinished;
        private bool isQuestion;
        private TemporaryAnimatedSprite dialogueIcon;


        //Begin Changes
        public int frame = 0;
        public bool animationFinished = false;
        public int portraitIndex = 0;
        public static IMonitor Monitor;
        public int nextTick = 0;
        public static int totalTick;
       //End Changes


            

        public PortraitureDialogueBoxNew(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.activatedByGamePad = Game1.isAnyGamePadButtonBeingPressed();
            this.gamePadIntroTimer = 1000;
        }

        public PortraitureDialogueBoxNew(string dialogue)
        {
            this.activatedByGamePad = Game1.isAnyGamePadButtonBeingPressed();
            if (this.activatedByGamePad)
            {
                Game1.mouseCursorTransparency = 0.0f;
                this.gamePadIntroTimer = 1000;
            }
            this.dialogues.AddRange((IEnumerable<string>)dialogue.Split('#'));
            this.width = Math.Min(1240, SpriteText.getWidthOfString(dialogue) + Game1.tileSize);
            this.height = SpriteText.getHeightOfString(dialogue, this.width - Game1.pixelZoom * 5) + Game1.pixelZoom;
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - Game1.tileSize;
            this.setUpIcons();
        }

        public PortraitureDialogueBoxNew(string dialogue, List<Response> responses, int width = 1200)
        {
            this.activatedByGamePad = Game1.isAnyGamePadButtonBeingPressed();
            this.gamePadIntroTimer = 1000;
            this.dialogues.Add(dialogue);
            this.responses = responses;
            this.isQuestion = true;
            this.width = width;
            this.setUpQuestions();
            this.height = this.heightForQuestions;
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - Game1.tileSize;
            this.setUpIcons();
            this.characterIndexInDialogue = dialogue.Length - 1;
            if (responses == null || !Game1.options.SnappyMenus)
                return;
            this.responseCC = new List<ClickableComponent>();
            int y = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
            for (int index = 0; index < responses.Count; ++index)
            {
                List<ClickableComponent> responseCc = this.responseCC;
                ClickableComponent clickableComponent = new ClickableComponent(new Rectangle(this.x + Game1.pixelZoom * 2, y, width - Game1.pixelZoom * 2, SpriteText.getHeightOfString(responses[index].responseText, width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4), "");
                clickableComponent.myID = index;
                int num1 = index < responses.Count - 1 ? index + 1 : -1;
                clickableComponent.downNeighborID = num1;
                int num2 = index > 0 ? index - 1 : -1;
                clickableComponent.upNeighborID = num2;
                responseCc.Add(clickableComponent);
                y += SpriteText.getHeightOfString(responses[index].responseText, width) + Game1.pixelZoom * 4;
            }
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        public PortraitureDialogueBoxNew(Dialogue dialogue)
        {
            this.characterDialogue = dialogue;
            this.activatedByGamePad = Game1.isAnyGamePadButtonBeingPressed();
            if (this.activatedByGamePad)
            {
                Game1.mouseCursorTransparency = 0.0f;
                this.gamePadIntroTimer = 1000;
            }
            this.width = 1200;
            this.height = 6 * Game1.tileSize;
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - Game1.tileSize;
            this.friendshipJewel = new Rectangle(this.x + this.width - Game1.tileSize, this.y + Game1.tileSize * 4, 11 * Game1.pixelZoom, 11 * Game1.pixelZoom);
            this.characterDialoguesBrokenUp.Push(dialogue.getCurrentDialogue());
            this.checkDialogue(dialogue);
            this.newPortaitShakeTimer = this.characterDialogue.getPortraitIndex() == 1 ? 250 : 0;
            this.setUpForGamePadMode();
        }

        public PortraitureDialogueBoxNew(List<string> dialogues)
        {
            this.activatedByGamePad = Game1.isAnyGamePadButtonBeingPressed();
            if (this.activatedByGamePad)
            {
                Game1.mouseCursorTransparency = 0.0f;
                this.gamePadIntroTimer = 1000;
            }
            this.dialogues = dialogues;
            this.width = Math.Min(1200, SpriteText.getWidthOfString(dialogues[0]) + Game1.tileSize);
            this.height = SpriteText.getHeightOfString(dialogues[0], this.width - Game1.pixelZoom * 4);
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - Game1.tileSize;
            this.setUpIcons();
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override bool autoCenterMouseCursorForGamepad()
        {
            return false;
        }

        private void playOpeningSound()
        {
            Game1.playSound("breathin");
        }

        public override void setUpForGamePadMode()
        {
            if (!Game1.options.gamepadControls || !this.activatedByGamePad && Game1.lastCursorMotionWasMouse)
                return;
            this.gamePadControlsImplemented = true;
            if (this.isQuestion)
            {
                int num = 0;
                string currentString = this.getCurrentString();
                if (currentString != null && currentString.Length > 0)
                    num = SpriteText.getHeightOfString(currentString, 999999);
                if (Game1.options.snappyMenus)
                    return;
                Game1.setMousePosition(this.x + this.width - Game1.tileSize * 2, this.y + num + Game1.tileSize);
            }
            else
                Game1.mouseCursorTransparency = 0.0f;
        }

        public void closeDialogue()
        {
            if (Game1.activeClickableMenu.Equals((object)this))
            {
                Game1.exitActiveMenu();
                Game1.dialogueUp = false;
                if (this.characterDialogue != null && this.characterDialogue.speaker != null && (this.characterDialogue.speaker.CurrentDialogue.Count > 0 && this.dialogueFinished) && this.characterDialogue.speaker.CurrentDialogue.Count > 0)
                    this.characterDialogue.speaker.CurrentDialogue.Pop();
                if (Game1.messagePause)
                    Game1.pauseTime = 500f;
                if (Game1.currentObjectDialogue.Count > 0)
                    Game1.currentObjectDialogue.Dequeue();
                Game1.currentDialogueCharacterIndex = 0;
                if (Game1.currentObjectDialogue.Count > 0)
                {
                    Game1.dialogueUp = true;
                    Game1.questionChoices.Clear();
                    Game1.dialogueTyping = true;
                }
                Game1.tvStation = -1;
                if (this.characterDialogue != null && this.characterDialogue.speaker != null && (!this.characterDialogue.speaker.name.Equals("Gunther") && !Game1.eventUp) && !this.characterDialogue.speaker.doingEndOfRouteAnimation)
                    this.characterDialogue.speaker.doneFacingPlayer(Game1.player);
                Game1.currentSpeaker = (NPC)null;
                if (!Game1.eventUp)
                {
                    Game1.player.CanMove = true;
                    Game1.player.movementDirections.Clear();
                }
                else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
                {
                    if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
                        ++Game1.currentLocation.currentEvent.CurrentCommand;
                    else
                        Game1.player.CanMove = true;
                }
                Game1.questionChoices.Clear();
            }
            if (Game1.afterDialogues == null)
                return;
            Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
            Game1.afterDialogues = (Game1.afterFadeFunction)null;
            afterDialogues();
        }

        public void finishTyping()
        {
            this.characterIndexInDialogue = this.getCurrentString().Length - 1;
        }

        public void beginOutro()
        {
            this.transitioning = true;
            this.transitioningBigger = false;
            Game1.playSound("breathout");
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.receiveLeftClick(x, y, playSound);
        }

        private void tryOutro()
        {
            if (Game1.activeClickableMenu == null || !Game1.activeClickableMenu.Equals((object)this))
                return;
            this.beginOutro();
        }

        public override void receiveKeyPress(Keys key)
        {
            if ((!Game1.options.SnappyMenus || !this.isQuestion) && (Game1.options.doesInputListContain(Game1.options.actionButton, key) || Game1.options.doesInputListContain(Game1.options.menuButton, key)))
            {
                if (!Game1.options.SnappyMenus || Game1.options.doesInputListContain(Game1.options.actionButton, key))
                    return;
                this.receiveLeftClick(0, 0, true);
            }
            else if (this.isQuestion && !Game1.eventUp && this.characterDialogue == null)
            {
                if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
                {
                    if (this.responses != null && this.responses.Count > 0 && Game1.currentLocation.answerDialogue(this.responses[this.responses.Count - 1]))
                        Game1.playSound("smallSelect");
                    this.selectedResponse = -1;
                    this.tryOutro();
                }
                else if (Game1.options.SnappyMenus)
                {
                    base.receiveKeyPress(key);
                }
                else
                {
                    if (key != Keys.Y || this.responses == null || (this.responses.Count <= 0 || !this.responses[0].responseKey.Equals("Yes")) || !Game1.currentLocation.answerDialogue(this.responses[0]))
                        return;
                    Game1.playSound("smallSelect");
                    this.selectedResponse = -1;
                    this.tryOutro();
                }
            }
            else
            {
                if (!Game1.options.SnappyMenus || !this.isQuestion || Game1.options.doesInputListContain(Game1.options.menuButton, key))
                    return;
                base.receiveKeyPress(key);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.transitioning)
                return;
            if (this.characterIndexInDialogue < this.getCurrentString().Length - 1)
            {
                this.characterIndexInDialogue = this.getCurrentString().Length - 1;
            }
            else
            {
                if (this.safetyTimer > 0)
                    return;
                if (this.isQuestion)
                {
                    if (this.selectedResponse == -1)
                        return;
                    this.questionFinishPauseTimer = Game1.eventUp ? 600 : 200;
                    this.transitioning = true;
                    this.transitionX = -1;
                    this.transitioningBigger = true;
                    if (this.characterDialogue != null)
                    {
                        this.characterDialoguesBrokenUp.Pop();
                        this.characterDialogue.chooseResponse(this.responses[this.selectedResponse]);
                        this.characterDialoguesBrokenUp.Push("");
                        Game1.playSound("smallSelect");
                    }
                    else
                    {
                        Game1.dialogueUp = false;
                        if (Game1.eventUp)
                        {
                            Game1.playSound("smallSelect");
                            Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, this.selectedResponse);
                            this.selectedResponse = -1;
                            this.tryOutro();
                            return;
                        }
                        if (Game1.currentLocation.answerDialogue(this.responses[this.selectedResponse]))
                            Game1.playSound("smallSelect");
                        this.selectedResponse = -1;
                        this.tryOutro();
                        return;
                    }
                }
                else if (this.characterDialogue == null)
                {
                    this.dialogues.RemoveAt(0);
                    if (this.dialogues.Count == 0)
                    {
                        this.closeDialogue();
                    }
                    else
                    {
                        this.width = Math.Min(1200, SpriteText.getWidthOfString(this.dialogues[0]) + Game1.tileSize);
                        this.height = SpriteText.getHeightOfString(this.dialogues[0], this.width - Game1.pixelZoom * 4);
                        this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
                        this.y = Game1.viewport.Height - this.height - Game1.tileSize * 2;
                        this.xPositionOnScreen = x;
                        this.yPositionOnScreen = y;
                        this.setUpIcons();
                    }
                }
                this.characterIndexInDialogue = 0;
                if (this.characterDialogue != null)
                {
                    int portraitIndex = this.characterDialogue.getPortraitIndex();
                    if (this.characterDialoguesBrokenUp.Count == 0)
                    {
                        this.beginOutro();
                        return;
                    }
                    this.characterDialoguesBrokenUp.Pop();
                    if (this.characterDialoguesBrokenUp.Count == 0)
                    {
                        if (!this.characterDialogue.isCurrentStringContinuedOnNextScreen)
                            this.beginOutro();
                        this.characterDialogue.exitCurrentDialogue();
                    }
                    if (!this.characterDialogue.isDialogueFinished() && this.characterDialogue.getCurrentDialogue().Length > 0 && this.characterDialoguesBrokenUp.Count == 0)
                        this.characterDialoguesBrokenUp.Push(this.characterDialogue.getCurrentDialogue());
                    this.checkDialogue(this.characterDialogue);
                    if (this.characterDialogue.getPortraitIndex() != portraitIndex)
                        this.newPortaitShakeTimer = this.characterDialogue.getPortraitIndex() == 1 ? 250 : 50;
                }
                if (!this.transitioning)
                    Game1.playSound("smallSelect");
                this.setUpIcons();
                this.safetyTimer = 750;
                if (this.getCurrentString() == null || this.getCurrentString().Length > 20)
                    return;
                this.safetyTimer = this.safetyTimer - 200;
            }
        }

        private void setUpIcons()
        {
            this.dialogueIcon = (TemporaryAnimatedSprite)null;
            if (this.isQuestion)
                this.setUpQuestionIcon();
            else if (this.characterDialogue != null && (this.characterDialogue.isCurrentStringContinuedOnNextScreen || this.characterDialoguesBrokenUp.Count > 1))
                this.setUpNextPageIcon();
            else if (this.dialogues != null && this.dialogues.Count > 1)
                this.setUpNextPageIcon();
            else
                this.setUpCloseDialogueIcon();
            this.setUpForGamePadMode();
            if (this.getCurrentString() == null || this.getCurrentString().Length > 20)
                return;
            this.safetyTimer = this.safetyTimer - 200;
        }

        public override void performHoverAction(int mouseX, int mouseY)
        {
            this.hoverText = "";
            if (!this.transitioning && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
            {
                base.performHoverAction(mouseX, mouseY);
                if (this.isQuestion)
                {
                    int selectedResponse = this.selectedResponse;
                    int num = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width) + Game1.pixelZoom * 12;
                    for (int index = 0; index < this.responses.Count; ++index)
                    {
                        SpriteText.getHeightOfString(this.responses[index].responseText, this.width);
                        if (mouseY >= num && mouseY < num + SpriteText.getHeightOfString(this.responses[index].responseText, this.width))
                        {
                            this.selectedResponse = index;
                            break;
                        }
                        num += SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + Game1.pixelZoom * 4;
                    }
                    if (this.selectedResponse != selectedResponse)
                        Game1.playSound("Cowboy_gunshot");
                }
            }
            if (!Game1.eventUp && !this.friendshipJewel.Equals(Rectangle.Empty) && (this.friendshipJewel.Contains(mouseX, mouseY) && this.characterDialogue != null) && (this.characterDialogue.speaker != null && Game1.player.friendships.ContainsKey(this.characterDialogue.speaker.name)))
                this.hoverText = Game1.player.getFriendshipHeartLevelForNPC(this.characterDialogue.speaker.name).ToString() + "/" + (this.characterDialogue.speaker.name.Equals(Game1.player.spouse) ? "12" : "10") + "<";
            if (!Game1.options.SnappyMenus || this.currentlySnappedComponent == null)
                return;
            this.selectedResponse = this.currentlySnappedComponent.myID;
        }

        private void setUpQuestionIcon()
        {
            Vector2 position = new Vector2((float)(this.x + this.width - 10 * Game1.pixelZoom), (float)(this.y + this.height - 11 * Game1.pixelZoom));
            TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(330, 357, 7, 13), 100f, 6, 999999, position, false, false, 0.89f, 0.0f, Color.White, (float)Game1.pixelZoom, 0.0f, 0.0f, 0.0f, true);
            temporaryAnimatedSprite.yPeriodic = true;
            temporaryAnimatedSprite.yPeriodicLoopTime = 1500f;
            double num = (double)(Game1.tileSize / 8);
            temporaryAnimatedSprite.yPeriodicRange = (float)num;
            this.dialogueIcon = temporaryAnimatedSprite;
        }

        private void setUpCloseDialogueIcon()
        {
            Vector2 position = new Vector2((float)(this.x + this.width - 10 * Game1.pixelZoom), (float)(this.y + this.height - 11 * Game1.pixelZoom));
            if (this.isPortraitBox())
                position.X -= (float)(115 * Game1.pixelZoom + 8 * Game1.pixelZoom);
            this.dialogueIcon = new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(289, 342, 11, 12), 80f, 11, 999999, position, false, false, 0.89f, 0.0f, Color.White, (float)Game1.pixelZoom, 0.0f, 0.0f, 0.0f, true);
        }

        private void setUpNextPageIcon()
        {
            Vector2 position = new Vector2((float)(this.x + this.width - 10 * Game1.pixelZoom), (float)(this.y + this.height - 10 * Game1.pixelZoom));
            if (this.isPortraitBox())
                position.X -= (float)(115 * Game1.pixelZoom + 8 * Game1.pixelZoom);
            TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(232, 346, 9, 9), 90f, 6, 999999, position, false, false, 0.89f, 0.0f, Color.White, (float)Game1.pixelZoom, 0.0f, 0.0f, 0.0f, true);
            temporaryAnimatedSprite.yPeriodic = true;
            temporaryAnimatedSprite.yPeriodicLoopTime = 1500f;
            double num = (double)(Game1.tileSize / 8);
            temporaryAnimatedSprite.yPeriodicRange = (float)num;
            this.dialogueIcon = temporaryAnimatedSprite;
        }

        private void checkDialogue(Dialogue d)
        {
            this.isQuestion = false;
            string str1 = "";
            if (this.characterDialoguesBrokenUp.Count == 1)
                str1 = SpriteText.getSubstringBeyondHeight(this.characterDialoguesBrokenUp.Peek(), this.width - 115 * Game1.pixelZoom - 5 * Game1.pixelZoom, this.height - Game1.pixelZoom * 4);
            if (str1.Length > 0)
            {
                string str2 = this.characterDialoguesBrokenUp.Pop().Replace(Environment.NewLine, "");
                this.characterDialoguesBrokenUp.Push(str1.Trim());
                this.characterDialoguesBrokenUp.Push(str2.Substring(0, str2.Length - str1.Length + 1).Trim());
            }
            if (d.getCurrentDialogue().Length == 0)
                this.dialogueFinished = true;
            if (d.isCurrentStringContinuedOnNextScreen || this.characterDialoguesBrokenUp.Count > 1)
                this.dialogueContinuedOnNextPage = true;
            else if (d.getCurrentDialogue().Length == 0)
                this.beginOutro();
            if (!d.isCurrentDialogueAQuestion())
                return;
            this.responses = d.getResponseOptions();
            this.isQuestion = true;
            this.setUpQuestions();
        }

        private void setUpQuestions()
        {
            int widthConstraint = this.width - Game1.pixelZoom * 4;
            this.heightForQuestions = SpriteText.getHeightOfString(this.getCurrentString(), widthConstraint);
            foreach (Response response in this.responses)
                this.heightForQuestions = this.heightForQuestions + (SpriteText.getHeightOfString(response.responseText, widthConstraint) + Game1.pixelZoom * 4);
            this.heightForQuestions = this.heightForQuestions + Game1.pixelZoom * 10;
            if (this.responses == null || !Game1.options.SnappyMenus)
                return;
            this.responseCC = new List<ClickableComponent>();
            int y = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
            for (int index = 0; index < this.responses.Count; ++index)
            {
                List<ClickableComponent> responseCc = this.responseCC;
                ClickableComponent clickableComponent = new ClickableComponent(new Rectangle(this.x + Game1.pixelZoom * 2, y, this.width - Game1.pixelZoom * 2, SpriteText.getHeightOfString(this.responses[index].responseText, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4), "");
                clickableComponent.myID = index;
                int num1 = index < this.responses.Count - 1 ? index + 1 : -1;
                clickableComponent.downNeighborID = num1;
                int num2 = index > 0 ? index - 1 : -1;
                clickableComponent.upNeighborID = num2;
                responseCc.Add(clickableComponent);
                y += SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + Game1.pixelZoom * 4;
            }
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        public bool isPortraitBox()
        {
            if (this.characterDialogue != null && this.characterDialogue.speaker != null && (this.characterDialogue.speaker.Portrait != null && this.characterDialogue.showPortrait))
                return Game1.options.showPortraits;
            return false;
        }

        public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            if (xPos <= 0)
                return;
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle?(new Rectangle(306, 320, 16, 16)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 5 * Game1.pixelZoom, boxWidth, 6 * Game1.pixelZoom), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + 3 * Game1.pixelZoom, yPos + boxHeight, boxWidth - 5 * Game1.pixelZoom, 8 * Game1.pixelZoom), new Rectangle?(new Rectangle(275, 328, 1, 8)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos - 8 * Game1.pixelZoom, yPos + 6 * Game1.pixelZoom, 8 * Game1.pixelZoom, boxHeight - 7 * Game1.pixelZoom), new Rectangle?(new Rectangle(264, 325, 8, 1)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 7 * Game1.pixelZoom, boxHeight), new Rectangle?(new Rectangle(293, 324, 7, 1)), Color.White);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 11 * Game1.pixelZoom), (float)(yPos - 7 * Game1.pixelZoom)), new Rectangle?(new Rectangle(261, 311, 14, 13)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - Game1.pixelZoom * 2), (float)(yPos - 7 * Game1.pixelZoom)), new Rectangle?(new Rectangle(291, 311, 12, 11)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - Game1.pixelZoom * 2), (float)(yPos + boxHeight - 2 * Game1.pixelZoom)), new Rectangle?(new Rectangle(291, 326, 12, 12)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 11 * Game1.pixelZoom), (float)(yPos + boxHeight - Game1.pixelZoom)), new Rectangle?(new Rectangle(261, 327, 14, 11)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
        }

        private bool shouldPortraitShake(Dialogue d)
        {
            int portraitIndex = d.getPortraitIndex();
            if (d.speaker.name.Equals("Pam") && portraitIndex == 3 || d.speaker.name.Equals("Abigail") && portraitIndex == 7 || (d.speaker.name.Equals("Haley") && portraitIndex == 5 || d.speaker.name.Equals("Maru") && portraitIndex == 9))
                return true;
            return this.newPortaitShakeTimer > 0;
        }
//Begin changes
        public bool setTexture(string characterName, Texture2D fallBack)
        {

            if (ImageHelper.pTextures.ContainsKey(characterName))
            {
                if (ImageHelper.pTextures[characterName] == fallBack)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


            if (ImageHelper.doesImageFileExist(characterName + ".png"))
            {
                ImageHelper.pTextures.Add(characterName, ImageHelper.loadTextureFromModFolder(characterName + ".png"));
                return true;
            }
            else
            {
                ImageHelper.pTextures.Add(characterName, fallBack);
            }

            return false;
        }

        public bool setXNBTexture(string characterName, Texture2D fallBack)
        {
            string cName = characterName + "XNB";

            if (ImageHelper.pTextures.ContainsKey(cName))
            {
                if (ImageHelper.pTextures[cName] == fallBack)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


            if (ImageHelper.doesImageFileExist(characterName + ".xnb"))
            {
                Texture2D getXNB = ImageHelper.loadXNBFromModFolder(characterName);
                ImageHelper.pTextures.Add(cName, getXNB);

                return true;
            }
            else
            {
                ImageHelper.pTextures.Add(cName, fallBack);
            }

            return false;
        }

        public void drawPortrait(SpriteBatch b)
        {
            string characterName = this.characterDialogue.speaker.name;
            Texture2D texture = this.characterDialogue.speaker.Portrait;
            Texture2D animTexture = this.characterDialogue.speaker.Portrait;
            bool hasAnimation = false;
            string picked = characterName;
            bool isVanillaType = true;
            int index = this.characterDialogue.getPortraitIndex();
            List<string> variations = new List<string>();
            bool loop = false;

            string[] characterParts = characterName.Split('_');
            if (characterParts.Length > 1)
            {
                if (!ImageHelper.doesImageFileExist(characterName + ".xnb") && !ImageHelper.doesImageFileExist(characterName + ".png"))
                {
                    characterName = characterParts[0];
                }
            }

            if (Game1.isRaining && Game1.currentLocation.isOutdoors)
            {
                variations.Add(characterName + "_" + "rain" + "_" + Game1.currentSeason + "_year" + Game1.year);
                variations.Add(characterName + "_" + "rain" + "_year" + Game1.year);
                variations.Add(characterName + "_" + "rain" + "_" + Game1.currentSeason);
                variations.Add(characterName + "_" + "rain");
            }

            variations.Add(characterName + "_" + Game1.currentSeason + "_year" + Game1.year);
            variations.Add(characterName + "_year" + Game1.year);
            variations.Add(characterName + "_" + Game1.currentSeason);
            variations.Add(characterName);

            if (setXNBTexture(characterName, texture))
            {
                string cName = characterName + "XNB";
                texture = ImageHelper.pTextures[cName];
                isVanillaType = true;
            }
            else
            {
                foreach (string s in variations)
                {

                    if (setTexture(s, texture))
                    {
                        texture = ImageHelper.pTextures[s];
                        isVanillaType = false;
                        picked = s;
                        break;
                    }


                }
            }


            int textureSize = 64;
            if (!isVanillaType && texture.Width > 64)
            {
                textureSize = texture.Width / 2;
            }

            int frames = 0;


            if (!isVanillaType && setTexture(picked+"_"+index, texture))
            {
                animTexture = ImageHelper.pTextures[picked + "_" + index];
                if (animTexture != texture)
                {
                    
                    frames = (animTexture.Width / textureSize) * (animTexture.Height / textureSize);
                    if(frames % 2 == 0)
                    {
                        loop = true;
                    }
                    hasAnimation = true;
                }
            }

            

            if (!hasAnimation)
            {
                this.frame = 0;
            }

            if (index != portraitIndex)
            {
                animationFinished = false;
                this.frame = 0;
                portraitIndex = index;
            }

            int useIndex = index + frame;
            Texture2D useTexture = texture;
            if (hasAnimation && (!animationFinished || loop))
            {
                useTexture = animTexture;
            }

       

            //Begin Vanilla
            if (this.width < 107 * Game1.pixelZoom * 3 / 2)
                return;
            int num1 = this.x + this.width - 112 * Game1.pixelZoom + Game1.pixelZoom;
            int num2 = this.x + this.width - num1;
            b.Draw(Game1.mouseCursors, new Rectangle(num1 - 10 * Game1.pixelZoom, this.y, 9 * Game1.pixelZoom, this.height), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
            b.Draw(Game1.mouseCursors, new Vector2((float)(num1 - 10 * Game1.pixelZoom), (float)(this.y - 5 * Game1.pixelZoom)), new Rectangle?(new Rectangle(278, 313, 10, 7)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(num1 - 10 * Game1.pixelZoom), (float)(this.y + this.height)), new Rectangle?(new Rectangle(278, 328, 10, 8)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.88f);
            int num3 = num1 + Game1.pixelZoom * 19;
            int num4 = this.y + this.height / 2 - 74 * Game1.pixelZoom / 2 - 18 * Game1.pixelZoom / 2;
            b.Draw(Game1.mouseCursors, new Vector2((float)(num1 - 2 * Game1.pixelZoom), (float)this.y), new Rectangle?(new Rectangle(583, 411, 115, 97)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.88f);
            Rectangle rectangle = Game1.getSourceRectForStandardTileSheet(this.characterDialogue.speaker.Portrait, this.characterDialogue.getPortraitIndex(), 64, 64);
            if (!this.characterDialogue.speaker.Portrait.Bounds.Contains(rectangle))
                rectangle = new Rectangle(0, 0, 64, 64);
            int num5 = this.shouldPortraitShake(this.characterDialogue) ? Game1.random.Next(-1, 2) : 0;
            //End Vanilla

            rectangle = Game1.getSourceRectForStandardTileSheet(useTexture, useIndex, textureSize, textureSize);


            // Replace b.Draw(this.characterDialogue.speaker.Portrait, new Vector2((float)(num3 + 4 * Game1.pixelZoom + num5), (float)(num4 + 6 * Game1.pixelZoom)), new Rectangle?(rectangle), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.88f);
            b.Draw(useTexture, new Rectangle(num3 + 4 * Game1.pixelZoom + num5, num4 + 6 * Game1.pixelZoom, 64 * Game1.pixelZoom, 64 * Game1.pixelZoom), new Rectangle?(rectangle), Color.White);

            //Next Frame
            
            if (hasAnimation && (!animationFinished || loop) && totalTick >= nextTick) { 
            this.frame++;
                nextTick = totalTick + 1;
            }
            
            if (this.frame >= frames)
            {
                animationFinished = true;
                this.frame = 0;
            }

            

            //Begin Vanilla
            SpriteText.drawStringHorizontallyCenteredAt(b, this.characterDialogue.speaker.getName(), num1 + num2 / 2, num4 + 74 * Game1.pixelZoom + 4 * Game1.pixelZoom, 999999, -1, 999999, 1f, 0.88f, false, -1);
            if (Game1.eventUp || this.friendshipJewel.Equals(Rectangle.Empty) || (this.characterDialogue == null || this.characterDialogue.speaker == null) || !Game1.player.friendships.ContainsKey(this.characterDialogue.speaker.name))
                return;
            b.Draw(Game1.mouseCursors, new Vector2((float)this.friendshipJewel.X, (float)this.friendshipJewel.Y), new Rectangle?(Game1.player.getFriendshipHeartLevelForNPC(this.characterDialogue.speaker.name) >= 10 ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(this.characterDialogue.speaker.name) / 2 * 11), 11, 11)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.88f);
            //End Vanilla
        }

        //End Changes

        public string getCurrentString()
        {
            if (this.characterDialogue != null)
            {
                string str = this.characterDialoguesBrokenUp.Count <= 0 ? this.characterDialogue.getCurrentDialogue().Trim().Replace(Environment.NewLine, "") : this.characterDialoguesBrokenUp.Peek().Trim().Replace(Environment.NewLine, "");
                if (!Game1.options.showPortraits)
                    str = this.characterDialogue.speaker.getName() + ": " + str;
                return str;
            }
            if (this.dialogues.Count > 0)
                return this.dialogues[0].Trim().Replace(Environment.NewLine, "");
            return "";
        }

        public override void update(GameTime time)
        {
            base.update(time);
            Game1.mouseCursorTransparency = Game1.lastCursorMotionWasMouse || this.isQuestion ? 1f : 0.0f;
            if (this.gamePadIntroTimer > 0 && !this.isQuestion)
            {
                Game1.mouseCursorTransparency = 0.0f;
                this.gamePadIntroTimer = this.gamePadIntroTimer - time.ElapsedGameTime.Milliseconds;
            }
            if (this.safetyTimer > 0)
                this.safetyTimer = this.safetyTimer - time.ElapsedGameTime.Milliseconds;
            if (this.questionFinishPauseTimer > 0)
            {
                this.questionFinishPauseTimer = this.questionFinishPauseTimer - time.ElapsedGameTime.Milliseconds;
            }
            else
            {
                TimeSpan elapsedGameTime;
                if (this.transitioning)
                {
                    if (this.transitionX == -1)
                    {
                        this.transitionX = this.x + this.width / 2;
                        this.transitionY = this.y + this.height / 2;
                        this.transitionWidth = 0;
                        this.transitionHeight = 0;
                    }
                    if (this.transitioningBigger)
                    {
                        int transitionWidth1 = this.transitionWidth;
                        this.transitionX = this.transitionX - (int)((double)time.ElapsedGameTime.Milliseconds * 3.0);
                        int transitionY = this.transitionY;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num1 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * ((this.isQuestion ? (double)this.heightForQuestions : (double)this.height) / (double)this.width));
                        this.transitionY = transitionY - num1;
                        this.transitionX = Math.Max(this.x, this.transitionX);
                        this.transitionY = Math.Max(this.isQuestion ? this.y + this.height - this.heightForQuestions : this.y, this.transitionY);
                        int transitionWidth2 = this.transitionWidth;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num2 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * 2.0);
                        this.transitionWidth = transitionWidth2 + num2;
                        int transitionHeight = this.transitionHeight;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num3 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * ((this.isQuestion ? (double)this.heightForQuestions : (double)this.height) / (double)this.width) * 2.0);
                        this.transitionHeight = transitionHeight + num3;
                        this.transitionWidth = Math.Min(this.width, this.transitionWidth);
                        this.transitionHeight = Math.Min(this.isQuestion ? this.heightForQuestions : this.height, this.transitionHeight);
                        if (transitionWidth1 == 0 && this.transitionWidth > 0)
                            this.playOpeningSound();
                        if (this.transitionX == this.x && this.transitionY == (this.isQuestion ? this.y + this.height - this.heightForQuestions : this.y))
                        {
                            this.transitioning = false;
                            this.characterAdvanceTimer = 90;
                            this.setUpIcons();
                            this.transitionX = this.x;
                            this.transitionY = this.y;
                            this.transitionWidth = this.width;
                            this.transitionHeight = this.height;
                        }
                    }
                    else
                    {
                        this.transitionX = this.transitionX + (int)((double)time.ElapsedGameTime.Milliseconds * 3.0);
                        this.transitionY = this.transitionY + (int)((double)time.ElapsedGameTime.Milliseconds * 3.0 * ((double)this.height / (double)this.width));
                        this.transitionX = Math.Min(this.x + this.width / 2, this.transitionX);
                        this.transitionY = Math.Min(this.y + this.height / 2, this.transitionY);
                        this.transitionWidth = this.transitionWidth - (int)((double)time.ElapsedGameTime.Milliseconds * 3.0 * 2.0);
                        this.transitionHeight = this.transitionHeight - (int)((double)time.ElapsedGameTime.Milliseconds * 3.0 * ((double)this.height / (double)this.width) * 2.0);
                        this.transitionWidth = Math.Max(0, this.transitionWidth);
                        this.transitionHeight = Math.Max(0, this.transitionHeight);
                        if (this.transitionWidth == 0 && this.transitionHeight == 0)
                            this.closeDialogue();
                    }
                }
                if (!this.transitioning && this.characterIndexInDialogue < this.getCurrentString().Length)
                {
                    int characterAdvanceTimer = this.characterAdvanceTimer;
                    elapsedGameTime = time.ElapsedGameTime;
                    int milliseconds = elapsedGameTime.Milliseconds;
                    this.characterAdvanceTimer = characterAdvanceTimer - milliseconds;
                    if (this.characterAdvanceTimer <= 0)
                    {
                        this.characterAdvanceTimer = 30;
                        int characterIndexInDialogue = this.characterIndexInDialogue;
                        this.characterIndexInDialogue = Math.Min(this.characterIndexInDialogue + 1, this.getCurrentString().Length);
                        if (this.characterIndexInDialogue != characterIndexInDialogue && this.characterIndexInDialogue == this.getCurrentString().Length)
                            Game1.playSound("dialogueCharacterClose");
                        if (this.characterIndexInDialogue > 1 && this.characterIndexInDialogue < this.getCurrentString().Length && Game1.options.dialogueTyping)
                            Game1.playSound("dialogueCharacter");
                    }
                }
                if (!this.transitioning && this.dialogueIcon != null)
                    this.dialogueIcon.update(time);
                if (this.transitioning || this.newPortaitShakeTimer <= 0)
                    return;
                int portaitShakeTimer = this.newPortaitShakeTimer;
                elapsedGameTime = time.ElapsedGameTime;
                int milliseconds1 = elapsedGameTime.Milliseconds;
                this.newPortaitShakeTimer = portaitShakeTimer - milliseconds1;
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.width = 1200;
            this.height = 6 * Game1.tileSize;
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - Game1.tileSize;
            this.friendshipJewel = new Rectangle(this.x + this.width - Game1.tileSize, this.y + Game1.tileSize * 4, 11 * Game1.pixelZoom, 11 * Game1.pixelZoom);
            this.setUpIcons();
        }

        public override void draw(SpriteBatch b)
        {
            if (this.width < Game1.tileSize / 4 || this.height < Game1.tileSize / 4)
                return;
            if (this.transitioning)
            {
                this.drawBox(b, this.transitionX, this.transitionY, this.transitionWidth, this.transitionHeight);
                if (this.activatedByGamePad && !Game1.lastCursorMotionWasMouse && (!this.isQuestion && !Game1.isGamePadThumbstickInMotion()) || Game1.getMouseX() == 0 && Game1.getMouseY() == 0)
                    return;
                this.drawMouse(b);
            }
            else
            {
                if (this.isQuestion)
                {
                    this.drawBox(b, this.x, this.y - (this.heightForQuestions - this.height), this.width, this.heightForQuestions);
                    SpriteText.drawString(b, this.getCurrentString(), this.x + Game1.pixelZoom * 2, this.y + Game1.pixelZoom * 3 - (this.heightForQuestions - this.height), this.characterIndexInDialogue, this.width - Game1.pixelZoom * 4, 999999, 1f, 0.88f, false, -1, "", -1);
                    if (this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
                    {
                        int y = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
                        for (int index = 0; index < this.responses.Count; ++index)
                        {
                            if (index == this.selectedResponse)
                                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.x + Game1.pixelZoom, y - Game1.pixelZoom * 2, this.width - Game1.pixelZoom * 2, SpriteText.getHeightOfString(this.responses[index].responseText, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4, Color.White, (float)Game1.pixelZoom, false);
                            SpriteText.drawString(b, this.responses[index].responseText, this.x + Game1.pixelZoom * 2, y, 999999, this.width, 999999, this.selectedResponse == index ? 1f : 0.6f, 0.88f, false, -1, "", -1);
                            y += SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + Game1.pixelZoom * 4;
                        }
                    }
                }
                else
                {
                    this.drawBox(b, this.x, this.y, this.width, this.height);
                    if (!this.isPortraitBox() && !this.isQuestion)
                        SpriteText.drawString(b, this.getCurrentString(), this.x + Game1.pixelZoom * 2, this.y + Game1.pixelZoom * 2, this.characterIndexInDialogue, this.width, 999999, 1f, 0.88f, false, -1, "", -1);
                }
                if (this.isPortraitBox() && !this.isQuestion)
                {
                    this.drawPortrait(b);
                    if (!this.isQuestion)
                        SpriteText.drawString(b, this.getCurrentString(), this.x + Game1.pixelZoom * 2, this.y + Game1.pixelZoom * 2, this.characterIndexInDialogue, this.width - 115 * Game1.pixelZoom - 5 * Game1.pixelZoom, 999999, 1f, 0.88f, false, -1, "", -1);
                }
                if (this.dialogueIcon != null && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
                    this.dialogueIcon.draw(b, true, 0, 0);
                if ((!this.activatedByGamePad || Game1.lastCursorMotionWasMouse || (this.isQuestion || Game1.isGamePadThumbstickInMotion())) && (Game1.getMouseX() != 0 || Game1.getMouseY() != 0))
                    this.drawMouse(b);
                if (this.hoverText.Length <= 0)
                    return;
                SpriteText.drawStringWithScrollBackground(b, this.hoverText, this.friendshipJewel.Center.X - SpriteText.getWidthOfString(this.hoverText) / 2, this.friendshipJewel.Y - Game1.tileSize, "", 1f, -1);
            }
        }
    }
}
