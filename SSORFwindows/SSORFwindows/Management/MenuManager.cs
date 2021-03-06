﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SSORFlibrary;

//MenuManager will need functionality added to allow player to go 
//back and forth between different menus by setting the currentMenu

namespace SSORF.Management
{
    
    class MenuManager
    {

        //Which menu is currently being viewed?
        enum Menu : int
        {
            Main = 0,
            VehicleSelect,
            Dealership,
            Missions,
            TuneShop,
            Options,
            Credits,
            NumMenus,
        }
        int offset = SSORF.Management.StateManager.bounds.Height / 20;
        int offsetW = SSORF.Management.StateManager.bounds.Width / 20;

        #region declarations

        //if a mission other than zero is selected, state is switched
        public short selectedMission = 0;

        private ContentManager GameContent;

        private SpriteFont menuFont;
        private Objects.MessageBox messageBox = new Objects.MessageBox();
        private string message;

        private Menu CurrentMenu;
        private States.SubMenu[] Menus;
 
        private Texture2D CursorImage;
        private Texture2D fixedImage;
        private Texture2D SoundOption;
        private Texture2D DifficultyOption;
        //used to display upgrade data
        private UpgradeData[] upgrades;
        private ScooterData[] scooters;

        //used to display scooter models in vehicle select
        Matrix view, proj;
        private SSORF.Objects.SimpleModel[] scooterModels = new Objects.SimpleModel[8];
        private float scooterYaw = 0.0f;
        private short[] scooterIDnums = new short[8];
        private short VSBackButton;

        GraphicsDevice graphics;

        public Matrix scale;
        public bool easyMode;

        #endregion

        public MenuManager(ContentManager content, Objects.Player player, GraphicsDevice graphicsDevice)
        {
            Rectangle screen = SSORF.Management.StateManager.bounds;
            graphics = graphicsDevice;
            GameContent = content;
            //font for displaying upgrade data
            menuFont = content.Load<SpriteFont>("menuFont");

            //Load UpgradeData
            upgrades = content.Load<UpgradeData[]>("upgrades");
            //Load ScooterData
            scooters = content.Load<ScooterData[]>("scooters");

            //load scooter models
            for (int i = 0; i < 8; i++)
            {
                scooterModels[i] = new Objects.SimpleModel();
                scooterModels[i].Mesh = content.Load<Model>("Models\\scooter" + i.ToString());
            }

            Menus = new States.SubMenu[(int)Menu.NumMenus];

            #region Load Main Menu
            Menus[(int)Menu.Main] = new States.SubMenu(6); //main menu has 7 buttons
            Menus[(int)Menu.Main].BackGround = content.Load<Texture2D>("Images\\game menu");
            Menus[(int)Menu.Main].ButtonImage[0] = content.Load<Texture2D>("Images\\Missions");
            Menus[(int)Menu.Main].ButtonPosition[0] = new Vector2(screen.Left + offset, screen.Top + ((offset / 2) * 10));
            Menus[(int)Menu.Main].ButtonImage[1] = content.Load<Texture2D>("Images\\Dealership");
            Menus[(int)Menu.Main].ButtonPosition[1] = new Vector2(screen.Left + offset, screen.Top + ((offset / 2) * 13));
            Menus[(int)Menu.Main].ButtonImage[2] = content.Load<Texture2D>("Images\\Garage");
            Menus[(int)Menu.Main].ButtonPosition[2] = new Vector2(screen.Left + offset, screen.Top + ((offset / 2) * 16));
            Menus[(int)Menu.Main].ButtonImage[3] = content.Load<Texture2D>("Images\\Versus");
            Menus[(int)Menu.Main].ButtonPosition[3] = new Vector2(screen.Left + offset, screen.Top + ((offset / 2) * 19));
            Menus[(int)Menu.Main].ButtonImage[4] = content.Load<Texture2D>("Images\\Options");
            Menus[(int)Menu.Main].ButtonPosition[4] = new Vector2(screen.Left + offset, screen.Top + ((offset / 2) * 22));
            Menus[(int)Menu.Main].ButtonImage[5] = content.Load<Texture2D>("Images\\Credits");
            Menus[(int)Menu.Main].ButtonPosition[5] = new Vector2(screen.Left + offset, screen.Top + ((offset / 2) * 25));
            #endregion

            #region Load VehicleSelect (select owned vehicles)

            loadVehicleSelect(content, player.ScootersOwned);

            #endregion

            #region Load Dealership (buy new vehicles)

            Menus[(int)Menu.Dealership] = new States.SubMenu(10);
            Menus[(int)Menu.Dealership].BackGround = content.Load<Texture2D>("Images\\Dealership1");

            int y = offset * 4;
            for (int i = 0; i < 8; i++)
            {
                Menus[(int)Menu.Dealership].ButtonImage[i] = content.Load<Texture2D>("Images\\vehicle" + i.ToString());
                Menus[(int)Menu.Dealership].ButtonPosition[i] = new Vector2(screen.Left + offset, screen.Top + y);
                y += offset;
            }
            Menus[(int)Menu.Dealership].ButtonImage[8] = content.Load<Texture2D>("Images\\TuneShopButton");
            Menus[(int)Menu.Dealership].ButtonPosition[8] = new Vector2(screen.Right - (Menus[(int)Menu.Dealership].ButtonImage[8].Bounds.Right), screen.Top);
            Menus[(int)Menu.Dealership].ButtonImage[9] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Dealership].ButtonPosition[9] = new Vector2(screen.Left + (offset * 2), screen.Bottom - (Menus[(int)Menu.Dealership].ButtonImage[9].Bounds.Height + (offset * 3)));
            

            #endregion

            #region Load TuneShop (buy upgrades)

            Menus[(int)Menu.TuneShop] = new States.SubMenu(13); //tune shop has 4 buttons
            Menus[(int)Menu.TuneShop].BackGround = content.Load<Texture2D>("Images\\TuneShopTest");
            for (int i = 0; i < 12; i++)
            {
                Menus[(int)Menu.TuneShop].ButtonImage[i] = content.Load<Texture2D>("Images\\" + upgrades[i].button);
            }
            //row 1
            int row = 1;
            int column = 1;
            Menus[(int)Menu.TuneShop].ButtonPosition[0] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[1] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[2] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[3] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column = 1;
            row++;
            Menus[(int)Menu.TuneShop].ButtonPosition[4] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[5] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[6] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[7] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column = 1;
            row++;
            Menus[(int)Menu.TuneShop].ButtonPosition[8] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[9] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[10] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.TuneShop].ButtonPosition[11] = new Vector2(screen.Left + (offset * 3) + (offset * column * 3), screen.Top + (offset * 6) + (offset * row * 2)); column = 1;

            Menus[(int)Menu.TuneShop].ButtonImage[12] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.TuneShop].ButtonPosition[12] = new Vector2(screen.Left + offset, screen.Top + (offset * 2));
            #endregion

            #region Load MissionsMenu
            Menus[(int)Menu.Missions] = new States.SubMenu(17); //mission menu has 2 buttons
            Menus[(int)Menu.Missions].BackGround = content.Load<Texture2D>("Images\\MissionForm");
            for (int j = 0; j <= 15; j++)
                Menus[(int)Menu.Missions].ButtonImage[j] = content.Load<Texture2D>("Images\\mission" + (j+1).ToString());
            //this is ugly as fuck i know, for loops were just pissing me off
            row = 1;
            column = 1;
            Menus[(int)Menu.Missions].ButtonPosition[0] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[1] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[2] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[3] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[4] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[5] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column = 1;
            row++;
            Menus[(int)Menu.Missions].ButtonPosition[6] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[7] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[8] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[9] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[10] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column = 1;
            row++;
            Menus[(int)Menu.Missions].ButtonPosition[11] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[12] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[13] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[14] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column++;
            Menus[(int)Menu.Missions].ButtonPosition[15] = new Vector2(screen.Left + (screen.Right / 8) + (offsetW * column * 2), screen.Top + (screen.Bottom / 6) + (offset * row * 2)); column = 1;
            //Menus[(int)Menu.Missions].ButtonImage[0] = content.Load<Texture2D>("Images\\button1");
            //Menus[(int)Menu.Missions].ButtonPosition[0] = new Vector2(screen.Left + 100, 250);
            //Menus[(int)Menu.Missions].ButtonImage[1] = content.Load<Texture2D>("Images\\button2");
            //Menus[(int)Menu.Missions].ButtonPosition[1] = new Vector2(screen.Left + 450, 300);
            Menus[(int)Menu.Missions].ButtonImage[16] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Missions].ButtonPosition[16] = new Vector2(screen.Left + ((screen.Right / 2f) - (Menus[(int)Menu.Missions].ButtonImage[16].Bounds.X /2)), screen.Bottom - (Menus[(int)Menu.Missions].ButtonImage[16].Bounds.Bottom + (offset * 3)));
            #endregion

            #region Load Credits
            Menus[(int)Menu.Credits] = new States.SubMenu(1); //mission menu has 2 buttons
            Menus[(int)Menu.Credits].BackGround = content.Load<Texture2D>("Images\\Credits Form");
            Menus[(int)Menu.Credits].ButtonImage[0] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Credits].ButtonPosition[0] = new Vector2(screen.Left + offset, screen.Bottom - (offset * 2));
            #endregion

            #region Load Options
            
            Menus[(int)Menu.Options] = new States.SubMenu(7); 
            Menus[(int)Menu.Options].BackGround = content.Load<Texture2D>("Images\\Options Form");
            //Menus[(int)Menu.Options].ButtonImage[0] = content.Load<Texture2D>("Images\\Music Button");
            //Menus[(int)Menu.Options].ButtonPosition[0] = new Vector2(screen.Left + 75, 150);
            Menus[(int)Menu.Options].ButtonImage[0] = content.Load<Texture2D>("Images\\On Button");
            Menus[(int)Menu.Options].ButtonPosition[0] = new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 3.5f));
            Menus[(int)Menu.Options].ButtonImage[1] = content.Load<Texture2D>("Images\\Off Button");
            Menus[(int)Menu.Options].ButtonPosition[1] = new Vector2(screen.Left + (offset * 10), screen.Top + (offset * 3.5f));
            Menus[(int)Menu.Options].ButtonImage[2] = content.Load<Texture2D>("Images\\On Button");
            Menus[(int)Menu.Options].ButtonPosition[2] = new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 6.5f));
            Menus[(int)Menu.Options].ButtonImage[3] = content.Load<Texture2D>("Images\\Off Button");
            Menus[(int)Menu.Options].ButtonPosition[3] = new Vector2(screen.Left + (offset * 10), screen.Top + (offset * 6.5f));
            Menus[(int)Menu.Options].ButtonImage[4] = content.Load<Texture2D>("Images\\easy");
            Menus[(int)Menu.Options].ButtonPosition[4] = new Vector2(screen.Left + offset * 7, screen.Bottom - (offset * 9.5f));
            Menus[(int)Menu.Options].ButtonImage[5] = content.Load<Texture2D>("Images\\hard");
            Menus[(int)Menu.Options].ButtonPosition[5] = new Vector2(screen.Left + offset * 10, screen.Bottom - (offset * 9.5f));
            Menus[(int)Menu.Options].ButtonImage[6] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Options].ButtonPosition[6] = new Vector2(screen.Left + offset, screen.Bottom - (offset * 2));
            #endregion

            //load messageBox background
            messageBox.Background = content.Load<Texture2D>("Images\\messagebox");
            fixedImage = content.Load<Texture2D>("Images\\Music Button");
            SoundOption = content.Load<Texture2D>("Images\\Sound");
            DifficultyOption = content.Load<Texture2D>("Images\\difficulty");
            //load cursor image and set current menu to main menu
            CursorImage = content.Load<Texture2D>("Images\\cursor");
            CurrentMenu = Menu.Main;

            //for 3D camera
            proj = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(45.0f),
                            1.33f, 1.0f, 1000.0f);
            view = Matrix.CreateLookAt(Vector3.Zero, new Vector3(0, 0, -20), Vector3.Up);


            Menus[(int)Menu.Options].buttonHighlight[1] = true;
            Menus[(int)Menu.Options].buttonHighlight[3] = true;
            Menus[(int)Menu.Options].buttonHighlight[4] = true;
        }

        public void update(GameTime gameTime, Objects.Player player)
        {
            if (!messageBox.Active)
            {

                Menus[(int)CurrentMenu].update(gameTime);


#if XBOX
                    if (gamePadState.current.Buttons.Y == ButtonState.Pressed &&
                        gamePadState.previous.Buttons.Y == ButtonState.Released)
                    {
                        Menus[(int)CurrentMenu].SelectedButton = 1;
                        CurrentMenu = Menu.Main;
                        AudioManager.playSound(AudioManager.CLICK_CUE);
                    }
#else
                if (keyBoardState.current.IsKeyDown(Keys.Back) &&
                    keyBoardState.previous.IsKeyUp(Keys.Back))
                {
                    Menus[(int)CurrentMenu].SelectedButton = 1;
                    CurrentMenu = Menu.Main;
                    AudioManager.playSound(AudioManager.CLICK_CUE);
                }
#endif
                switch (CurrentMenu)
                {

                    #region update MainMenu
                    case Menu.Main:
#if XBOX
                        
                        if ((gamePadState.current.Buttons.B == ButtonState.Pressed && gamePadState.current.Buttons.X == ButtonState.Pressed) &&
                            (gamePadState.previous.Buttons.B == ButtonState.Released && gamePadState.previous.Buttons.X == ButtonState.Released))
                            player.Money += 1000;
#else
                        if ((keyBoardState.current.IsKeyDown(Keys.OemPlus) && keyBoardState.current.IsKeyDown(Keys.M)) && 
                            (keyBoardState.previous.IsKeyUp(Keys.OemPlus) && keyBoardState.previous.IsKeyUp(Keys.M)))
                            player.Money += 1000;
#endif

                        if (Menus[(int)Menu.Main].buttonPressed == 1)
                        {
                            CurrentMenu = Menu.Missions;
                            Menus[(int)Menu.Missions].updateCursor();
                        }
                        else if (Menus[(int)Menu.Main].buttonPressed == 2)
                        {
                            CurrentMenu = Menu.Dealership;
                            Menus[(int)Menu.Dealership].updateCursor();
                        }
                        else if (Menus[(int)Menu.Main].buttonPressed == 3)
                        {
                            loadVehicleSelect(GameContent, player.ScootersOwned);
                            CurrentMenu = Menu.VehicleSelect;
                            Menus[(int)Menu.VehicleSelect].updateCursor();
                        }
                        //else if (Menus[(int)Menu.Main].buttonPressed == 4)
                        //    CurrentMenu = Menu.VehicleSelect;
                        else if (Menus[(int)Menu.Main].buttonPressed == 5)
                            CurrentMenu = Menu.Options;
                        else if (Menus[(int)Menu.Main].buttonPressed == 6)
                            CurrentMenu = Menu.Credits;
                        Menus[(int)Menu.Main].buttonPressed = 0;

                        break;
                    #endregion

                    #region update MissionsMenu
                    case Menu.Missions:

                        //Note: buttonPressed = 0 means no button has been pressed
                        //If we are in the missions menu and a button is pressed...
                        if (Menus[(int)Menu.Missions].buttonPressed == 17)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.Missions].selectedButton = 1;
                        }
                        else if (Menus[(int)Menu.Missions].buttonPressed != 0)
                        {
                            //Set selected mission to a value other than zero to deactivate the menu
                            selectedMission = Menus[(int)Menu.Missions].buttonPressed;
                            //reset buttonPressed for when we return to the menu

                        }

                        Menus[(int)Menu.Missions].buttonPressed = 0;
                        break;
                    #endregion

                    #region update Dealership

                    case Menu.Dealership:
                        if (Menus[(int)Menu.Dealership].buttonPressed == 10)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.Dealership].selectedButton = 1;
                        }
                        if (Menus[(int)Menu.Dealership].buttonPressed == 9)
                        {
                            CurrentMenu = Menu.TuneShop;
                            Menus[(int)Menu.Dealership].selectedButton = 1;
                        }

                        // < 3 should get changed to < 9 when we have rest of scooters 
                        if (Menus[(int)Menu.Dealership].buttonPressed > 0 &&
                           Menus[(int)Menu.Dealership].buttonPressed < 9) 
                        {
                            message = player.PurchaseScooter(
                                scooters[Menus[(int)Menu.Dealership].buttonPressed - 1]);
                            messageBox.setMessage(message);
                            messageBox.Active = true;
                        }
                        
                        Menus[(int)Menu.Dealership].buttonPressed = 0;
                        break;

                    #endregion

                    #region update TuneShop
                    case Menu.TuneShop:
                        if (Menus[(int)Menu.TuneShop].buttonPressed == 13)
                        {
                            CurrentMenu = Menu.Dealership;
                            Menus[(int)Menu.TuneShop].selectedButton = 1;
                        }

                        if (Menus[(int)Menu.TuneShop].buttonPressed > 0 &&
                            Menus[(int)Menu.TuneShop].buttonPressed < 13)
                        {
                            message = player.PurchaseUpgrade(scooters[player.SelectedScooter],
                                upgrades[Menus[(int)Menu.TuneShop].buttonPressed - 1]);
                            messageBox.setMessage(message);
                            messageBox.Active = true;
                        }

                        Menus[(int)Menu.TuneShop].buttonPressed = 0;
                        break;
                    #endregion

                    #region update VehicleSelect

                    case Menu.VehicleSelect:

                        if (Menus[(int)Menu.VehicleSelect].buttonPressed == VSBackButton)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.VehicleSelect].selectedButton = 1;
                        }
                        if (Menus[(int)Menu.VehicleSelect].buttonPressed > 0 &&
                            Menus[(int)Menu.VehicleSelect].buttonPressed < VSBackButton)
                        {
                            player.SelectedScooter = scooterIDnums[Menus[(int)Menu.VehicleSelect].buttonPressed - 1];
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.VehicleSelect].selectedButton = 1;
                        }

                        Menus[(int)Menu.VehicleSelect].buttonPressed = 0;

                        break;
                    #endregion

                    #region update Credits
                    case Menu.Credits:
                         if (Menus[(int)Menu.Credits].buttonPressed == 1)
                            CurrentMenu = Menu.Main;
                        Menus[(int)Menu.Credits].buttonPressed = 0;

                        break;
                    //etc.
                    #endregion

                    #region update Options
                    case Menu.Options:
                        if (Menus[(int)Menu.Options].buttonPressed == 1)
                        {
                            AudioManager.setMusicPlaying(true);
                            Menus[(int)Menu.Options].buttonHighlight[0] = false;
                            Menus[(int)Menu.Options].buttonHighlight[1] = true;
                        }
                        if (Menus[(int)Menu.Options].buttonPressed == 2)
                        {
                            AudioManager.setMusicPlaying(false);
                            Menus[(int)Menu.Options].buttonHighlight[0] = true;
                            Menus[(int)Menu.Options].buttonHighlight[1] = false;
                        }
                        if (Menus[(int)Menu.Options].buttonPressed == 3)
                        {
                            AudioManager.setSoundPlaying(true);
                            Menus[(int)Menu.Options].buttonHighlight[2] = false;
                            Menus[(int)Menu.Options].buttonHighlight[3] = true;
                        }
                        if (Menus[(int)Menu.Options].buttonPressed == 4)
                        {
                            AudioManager.setSoundPlaying(false);
                            Menus[(int)Menu.Options].buttonHighlight[2] = true;
                            Menus[(int)Menu.Options].buttonHighlight[3] = false;
                        }
                        if (Menus[(int)Menu.Options].buttonPressed == 5)
                        {
                            easyMode = true;
                            Menus[(int)Menu.Options].buttonHighlight[4] = false;
                            Menus[(int)Menu.Options].buttonHighlight[5] = true;
                        }
                        if (Menus[(int)Menu.Options].buttonPressed == 6)
                        {
                            easyMode = false;
                            Menus[(int)Menu.Options].buttonHighlight[4] = true;
                            Menus[(int)Menu.Options].buttonHighlight[5] = false;
                        }
                        if (Menus[(int)Menu.Options].buttonPressed == 7)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.Options].selectedButton = 1;
                        }
                        Menus[(int)Menu.Options].buttonPressed = 0;

                        break;
                    //etc.
                    #endregion
                }


            }
            else
                messageBox.update();

        }
        //draw the current menu and the cursor
        public void draw(SpriteBatch spriteBatch, Objects.Player player)
        {
            Rectangle screen = graphics.Viewport.Bounds;

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, scale);
            if (CurrentMenu == Menu.Options)
                Menus[(int)CurrentMenu].drawWithHighlights(spriteBatch, scale);
            else
            Menus[(int)CurrentMenu].draw(spriteBatch, scale);
            spriteBatch.Begin();



            #region draw menu messages and vehicle specs

            string backButton;
            string selectButton;
#if XBOX
            backButton = "    Y";
            selectButton = "  [A]   ";
#else
            backButton = "BackSpace";
            selectButton = "SpaceBar ";
#endif


            switch (CurrentMenu)
            {
                case Menu.Main:
                    spriteBatch.DrawString(menuFont, "Current Vehicle:  " + scooters[player.SelectedScooter].name,
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 1)), Color.Black);
                    spriteBatch.DrawString(menuFont, "Press " + selectButton + "to make a selection",
                        new Vector2(screen.Left + offset * 10, screen.Bottom - (offset * 16)), Color.Black);
                    break;

                //display upgrade data
                case Menu.TuneShop:
                    spriteBatch.DrawString(menuFont, "Current Vehicle:  " + scooters[player.SelectedScooter].name,
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 2)), Color.Tan);
                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 1)), Color.Tan);

                    if (Menus[(int)Menu.TuneShop].SelectedButton != 13)
                    {
                        drawUpgradeSpecs(spriteBatch);
                    }

                    spriteBatch.DrawString(menuFont, "Press " + backButton,
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 2)), Color.Tan);
                    spriteBatch.DrawString(menuFont, "to return to Main Menu",
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 1)), Color.Tan);
                    break;

                case Menu.Dealership:
                    spriteBatch.DrawString(menuFont, "Current Vehicle:  " + scooters[player.SelectedScooter].name,
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 1)), Color.Black);

                    if (Menus[(int)Menu.Dealership].SelectedButton < 9)
                    {
                        drawVehicleSpecs(spriteBatch, new Vector2(screen.Left + (offset * 16), screen.Top + (screen.Bottom / 4)),
                            Menus[(int)Menu.Dealership].SelectedButton - 1);
                    }
                    spriteBatch.DrawString(menuFont, "Press " + backButton,
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "to return to Main Menu",
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 1)), Color.Black);
                    break;


                case Menu.VehicleSelect:

                    if (Menus[(int)Menu.VehicleSelect].SelectedButton != VSBackButton)
                        drawVehicleSpecs(spriteBatch, new Vector2(screen.Left + (offset * 13), screen.Top + (screen.Bottom / 2) - offset), scooterIDnums[Menus[(int)Menu.VehicleSelect].SelectedButton - 1],
                            player.UpgradeTotals[scooterIDnums[Menus[(int)Menu.VehicleSelect].SelectedButton - 1]]);

                    spriteBatch.DrawString(menuFont, "Press " + backButton,
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "to return to Main Menu",
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 1)), Color.Black);

                    break;

                case Menu.Options:
                    string forward, reverse, steering, pause;
#if XBOX
                    forward = "Right Trigger";
                    reverse = "Left Trigger";
                    steering = "Left Thumbstick";
                    pause = "Start button";
#else
                    forward = "Up";
                    reverse = "Down";
                    steering = "Left/Right";
                    pause = "Enter";
#endif
                    string controls = "DRIVING CONTROLS \n\nForward:  \nReverse:  \nSteering:  \nPause:  ";
                    string buttons = "\n\n" + forward + "\n" + reverse + "\n" + steering + "\n" + pause;
                    spriteBatch.DrawString(menuFont, controls, new Vector2(screen.Left + (offset * 15), screen.Top + (screen.Bottom / 2) - (offset * 6)), Color.Black);
                    spriteBatch.DrawString(menuFont, buttons, new Vector2(screen.Left + (offset * 18), screen.Top + (screen.Bottom / 2) - (offset * 6)), Color.Black);

                    if (Menus[(int)Menu.Options].SelectedButton == 5)
                        spriteBatch.DrawString(menuFont, "More time, less prize money", new Vector2(screen.Left + (offset * 15), screen.Top + (offset * 11)), Color.Black);
                    else if (Menus[(int)Menu.Options].SelectedButton == 6)
                        spriteBatch.DrawString(menuFont, "Less time, more prize money", new Vector2(screen.Left + (offset * 15), screen.Top + (offset * 11)), Color.Black);

                    spriteBatch.Draw(fixedImage, new Vector2(screen.Left + (offset * 2), screen.Top + (offset * 3)), Color.White);
                    spriteBatch.Draw(SoundOption, new Vector2(screen.Left + (offset * 2), screen.Top + (offset * 6)), Color.White);
                    spriteBatch.Draw(DifficultyOption, new Vector2(screen.Left + (offset * 2), screen.Top + (offset * 9)), Color.White);

                    spriteBatch.DrawString(menuFont, "Press " + backButton,
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "to return to Main Menu",
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 1)), Color.Black);

                    break;

                case Menu.Missions:
                    spriteBatch.DrawString(menuFont, "Current Vehicle:  " + scooters[player.SelectedScooter].name,
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(screen.Left + offset, screen.Bottom - (offset * 1)), Color.Black);

                    spriteBatch.DrawString(menuFont, "Press " + backButton,
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 2)), Color.Black);
                    spriteBatch.DrawString(menuFont, "to return to Main Menu",
                        new Vector2(screen.Left + (screen.Right / 2) - (offset * 2), screen.Bottom - (offset * 1)), Color.Black);

                    break;


            }

            #endregion


            spriteBatch.Draw(CursorImage, Menus[(int)CurrentMenu].CursorPosition, Color.White);

            if (messageBox.Active)
                messageBox.draw(spriteBatch, menuFont, Color.DimGray, Color.Black);

            spriteBatch.End();

            #region draw scooter model for Garage and Dealership
            DepthStencilState newDepthStencilState = new DepthStencilState();
            DepthStencilState oldDepthStencilState = graphics.DepthStencilState;

            newDepthStencilState.DepthBufferFunction = CompareFunction.Less;
            graphics.DepthStencilState = newDepthStencilState;
            //graphics.ReferenceStencil = 1;

            if (CurrentMenu == Menu.VehicleSelect &&
                Menus[(int)CurrentMenu].SelectedButton != VSBackButton)
            {
                scooterYaw += 0.02f;
                scooterModels[scooterIDnums[Menus[(int)CurrentMenu].SelectedButton - 1]].rotate(scooterYaw);
                scooterModels[scooterIDnums[Menus[(int)CurrentMenu].SelectedButton - 1]].draw(view, proj, new Vector3(35,-30,-100));
            }

            else if (CurrentMenu == Menu.Dealership && Menus[(int)CurrentMenu].SelectedButton < 9)
            {
                scooterYaw += 0.02f;
                scooterModels[Menus[(int)CurrentMenu].SelectedButton - 1].rotate(scooterYaw);
                scooterModels[Menus[(int)CurrentMenu].SelectedButton - 1].draw(view, proj, new Vector3(4, -30, -100)); 
            }

            graphics.DepthStencilState = oldDepthStencilState;
            #endregion

        }

        public void loadVehicleSelect(ContentManager content, bool[] ScootersOwned)
        {
            Rectangle screen = SSORF.Management.StateManager.bounds;
            short totalVehiclesOwned = 0;

            for (int i = 0; i < 8; i++)
            {
                if (ScootersOwned[i] == true)
                {
                    scooterIDnums[totalVehiclesOwned] = (short)i;
                    totalVehiclesOwned += 1;
                }
            }

            VSBackButton = (short)(totalVehiclesOwned + 1);
            Menus[(int)Menu.VehicleSelect] = new States.SubMenu((short)(totalVehiclesOwned + 1));
            Menus[(int)Menu.VehicleSelect].BackGround = content.Load<Texture2D>("Images\\Garage1");

            int y = offset * 4;
            for (int i = 0; i < totalVehiclesOwned; i++)
            {

                Menus[(int)Menu.VehicleSelect].ButtonImage[i] = content.Load<Texture2D>("Images\\vehicle" + scooterIDnums[i].ToString());
                Menus[(int)Menu.VehicleSelect].ButtonPosition[i] = new Vector2(screen.Left + offset, screen.Top + y);
                y += offset;
            }

            Menus[(int)Menu.VehicleSelect].ButtonImage[VSBackButton - 1] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[VSBackButton - 1] = new Vector2(screen.Left + offset, screen.Bottom - (offset * 4));
        }

        public void drawUpgradeSpecs(SpriteBatch spriteBatch)
        {
            Rectangle screen = SSORF.Management.StateManager.bounds;

            spriteBatch.DrawString(menuFont,
                upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].name,
                new Vector2(screen.Left + (offset * 10), screen.Top + (offset * 2.5f)), Color.Tan);

            spriteBatch.DrawString(menuFont,
                upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].description1,
                new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 3.5f)), Color.Tan);

            spriteBatch.DrawString(menuFont,
                upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].description2,
                new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 4.1f)), Color.Tan);

            spriteBatch.DrawString(menuFont,
                "Power:  " + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].power.ToString(),
                new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 4.9f)), Color.Tan);

            spriteBatch.DrawString(menuFont,
                "Weight:  " + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].weight.ToString("+#;-#;0") + "%",
                new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 5.5f)), Color.Tan);

            spriteBatch.DrawString(menuFont,
                "Grip:  +" + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].grip.ToString(),
                new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 6.1f)), Color.Tan);

            spriteBatch.DrawString(menuFont,
                "Cost:  $" + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].cost.ToString(),
                new Vector2(screen.Left + (offset * 7), screen.Top + (offset * 6.9f)), Color.Tan);
        }

        public void drawVehicleSpecs(SpriteBatch spriteBatch, Vector2 location, int ID)
        {
            spriteBatch.DrawString(menuFont, scooters[ID].name,
                location, Color.Black);

            spriteBatch.DrawString(menuFont, scooters[ID].description1,
                new Vector2(location.X + (offsetW * 3.3f), location.Y), Color.Black);

            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Power:  " + scooters[ID].outputPower.ToString(),
                location, Color.Black);

            spriteBatch.DrawString(menuFont, scooters[ID].description2,
                new Vector2(location.X + (offsetW * 3.3f), location.Y), Color.Black);

            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Weight:  " + scooters[ID].weight.ToString() + "kg",
                location, Color.Black);

            spriteBatch.DrawString(menuFont, scooters[ID].description3,
                 new Vector2(location.X + (offsetW * 3.3f), location.Y), Color.Black);

            location.Y += offset;


            spriteBatch.DrawString(menuFont, "Grip Rating:  " + scooters[ID].gripRating.ToString(),
                location, Color.Black);

            spriteBatch.DrawString(menuFont, scooters[ID].description4,
                new Vector2(location.X + (offsetW * 3.3f), location.Y), Color.Black);

            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Cost:  $" + scooters[ID].cost.ToString(),
                location, Color.Black);
        }

        public void drawVehicleSpecs(SpriteBatch spriteBatch, Vector2 location,
            int ID, Objects.upgradeSpecs upgradeTotals)
        {
            spriteBatch.DrawString(menuFont, scooters[ID].name,
                location, Color.Black);
            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Power:  " + (scooters[ID].outputPower + upgradeTotals.power).ToString(),
                location, Color.Black);
            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Weight:  " + (scooters[ID].weight + upgradeTotals.weight).ToString() + "kg",
                location, Color.Black);
            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Grip Rating:  " + (scooters[ID].gripRating + upgradeTotals.grip).ToString(),
                location, Color.Black);
            location.Y += offset;

            spriteBatch.DrawString(menuFont, "Cost:  $" + scooters[ID].cost.ToString(),
                location, Color.Black);
        }

        public ScooterData[] ScooterSpecs
        {
            get { return scooters; }
        }
    }
}
