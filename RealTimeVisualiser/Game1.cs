﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RealTimeVisualiser
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont SourceHeavy;
        private Cursor _cursor;
        private Color _backColor = Color.Black;

        private Texture2D _boxes;
        private Texture2D _labels;
        private Rectangle _screenPos;

        private List<Component> _gameComponent;
        private List<TextBox> _textBoxs;
        private List<Slide> _sliders;

        private Vector2 FPSDisplayPos;
        private Vector2 _screenDimensions;

        public static MouseState CURRENTMOUSE;
        public static bool GAMESTATE;

        private string _pausePlay;
        private int _pausePLayTxtX;
        private int _pausePLayTxtY;

        private int currentTextBox;

        private audioIn _audioIn;


        private Single inputLen;
        private int FFTLen;

        private List<Single> _audioData;
        

        private inputGraph _inputGraph;
        private FastFourierTransform _FFTDisplay;
        //private bool _drawGraphs;

        private int bitDepth = 32;
        private int sampleRate = 48000; //38400;

        private KeyboardState _currentKeyboard;
        private KeyboardState _oldKeyboard;
        private bool isTyping;
        private char keyInpt;

        public static bool HOVERING;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// initializes graphics and input 
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            IsMouseVisible = false;
            GAMESTATE = false;

            _pausePlay = "paused";
            _pausePLayTxtX = Convert.ToInt16(39 * GraphicsDevice.DisplayMode.Width / 1920); //- (SourceHeavy.MeasureString(_pausePlay).X / 2
            _pausePLayTxtY = 31 * GraphicsDevice.DisplayMode.Height / 1080;

            _currentKeyboard = Keyboard.GetState();

            base.Initialize();
            _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            _graphics.ApplyChanges();

        }

        /// <summary>
        /// loads all content to be used during runtime and creates relevant objects
        /// </summary>
        protected override void LoadContent()
        {
            
            _screenDimensions = new Vector2(Convert.ToSingle(GraphicsDevice.DisplayMode.Width), Convert.ToSingle(GraphicsDevice.DisplayMode.Height));
            _screenPos = new Rectangle(new Point(0, 0), new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height));
            FPSDisplayPos = new Vector2(GraphicsDevice.DisplayMode.Width - GraphicsDevice.DisplayMode.Width / 16, GraphicsDevice.DisplayMode.Height / 40);

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //loading textures
            SourceHeavy = Content.Load<SpriteFont>("Source");
            _boxes = Content.Load<Texture2D>("boxes");
            _labels = Content.Load<Texture2D>("labels");
            var _buttonTexture = Content.Load<Texture2D>("button texture");
            var _textboxTexture = Content.Load<Texture2D>("textbox texture");
            var whitePixel = Content.Load<Texture2D>("whitePix");

            //creating controls
            _cursor = new Cursor(Content.Load<Texture2D>("cursorsml"), Content.Load<Texture2D>("cursorUsesml"));

            var fourierInputText = new TextBox(_textboxTexture, SourceHeavy)
            {
                position = new Vector2(Convert.ToInt32(39 * GraphicsDevice.DisplayMode.Width / 1920), Convert.ToInt32(140 * GraphicsDevice.DisplayMode.Height / 1080)),
                text = "",
                backupText = "samples to analyse",
                penColour = Color.White
            };
            fourierInputText.click += fourierInputText_Click;

            var timeScaleText = new TextBox(_textboxTexture, SourceHeavy)
            {
                position = new Vector2(Convert.ToInt32(39 * GraphicsDevice.DisplayMode.Width / 1920), Convert.ToInt32(80 * GraphicsDevice.DisplayMode.Height / 1080)),
                text = "",
                backupText = "enter buffer length",
                penColour = Color.White
            };

            timeScaleText.click += timeScaleText_Click;

            var randomButton = new Button(_buttonTexture, SourceHeavy) // need to add button texture
            {
                position = new Vector2(39 * GraphicsDevice.DisplayMode.Width / 1920, 31 * GraphicsDevice.DisplayMode.Height / 1080),
                text = "",
                penColour = Color.White

            };

            randomButton.click += randomButton_Click;

            var quitButton = new Button(_buttonTexture, SourceHeavy) // need to add button texture
            {
                position = new Vector2(39 * GraphicsDevice.DisplayMode.Width / 1920, 772 * GraphicsDevice.DisplayMode.Height / 1080),
                text = "quit",
                penColour = Color.White

            };

            quitButton.click += quitButton_Click;

            var moreSamplesButton = new Button(_buttonTexture, SourceHeavy)
            {
                position = new Vector2(258 * GraphicsDevice.DisplayMode.Width / 1920, 189 * GraphicsDevice.DisplayMode.Height / 1080),
                text = ">",
                penColour = Color.White

            };

            moreSamplesButton.click += moreSamplesButton_Click;

            var lessSamplesButton = new Button(_buttonTexture, SourceHeavy)
            {
                position = new Vector2(39 * GraphicsDevice.DisplayMode.Width / 1920, 189 * GraphicsDevice.DisplayMode.Height / 1080),
                text = "<",
                penColour = Color.White

            };

            lessSamplesButton.click += lessSamplesButton_Click;

            var amplitudeSlide = new Slide(whitePixel, _buttonTexture, SourceHeavy)
            {
                position = new Vector2(53 * GraphicsDevice.DisplayMode.Width / 1920, 360 * GraphicsDevice.DisplayMode.Height / 1080),
                dimensions = new Vector2(371 * GraphicsDevice.DisplayMode.Width / 1920, 45 * GraphicsDevice.DisplayMode.Height / 1080),
                slitWidth = 4,
                handleWidth = 27,
                distance = 0.1f,
                penColour = Color.White,
                text = "output amplitude: ",
            };

            var biasSlide = new Slide(whitePixel, _buttonTexture, SourceHeavy)
            {
                position = new Vector2(53 * GraphicsDevice.DisplayMode.Width / 1920, 542 * GraphicsDevice.DisplayMode.Height / 1080),
                dimensions = new Vector2(371 * GraphicsDevice.DisplayMode.Width / 1920, 45 * GraphicsDevice.DisplayMode.Height / 1080),
                slitWidth = 4,
                handleWidth = 27,
                distance = 0.1f,
                penColour = Color.White,
                text = "balance: ",
            };


            //making lists of different component types
            _gameComponent = new List<Component>()
            {
                fourierInputText,
                timeScaleText,
                randomButton,
                quitButton,
                moreSamplesButton,
                lessSamplesButton,              
            };

            _textBoxs = new List<TextBox>()
            {
                timeScaleText,
                fourierInputText,
            };

            _sliders = new List<Slide>()
            {
                amplitudeSlide,
                biasSlide,
            };

            //initializing audio capture and graphs
            _audioIn = new audioIn(bitDepth);
            _inputGraph = new inputGraph(sampleRate,_screenDimensions, whitePixel);
            _FFTDisplay = new FastFourierTransform(_screenDimensions, whitePixel);
        }

     
        /// <summary>
        /// increases the samples analyzed by power of 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moreSamplesButton_Click(object sender, EventArgs e)
        {
            if (GAMESTATE == false)
            {
                int n;
                if (!int.TryParse(_textBoxs[1].text, out n)) _textBoxs[1].text = "512";
                if (n == 0) n = 512;
                _textBoxs[1].text = Convert.ToString(n * 2);
            }
        }

        /// <summary>
        /// decreases the number of samples analyzed by power of 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lessSamplesButton_Click(object sender, EventArgs e)
        {
            if (GAMESTATE == false)
            {
                int n;
                if (!int.TryParse(_textBoxs[1].text, out n)) _textBoxs[1].text = "512";
                if (n == 0) n = 512;
                if (n > 1) _textBoxs[1].text = Convert.ToString(n / 2);
            }
        }

        /// <summary>
        /// not needed as value is controlled by buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fourierInputText_Click(object sender, EventArgs e)
        {
            //if (GAMESTATE == false) currentTextBox = 2;
        }

        /// <summary>
        /// selects the current text box to write in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeScaleText_Click(object sender, EventArgs e)
        {
            if(GAMESTATE == false) currentTextBox = 1;
        }


        /// <summary>
        /// exits game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitButton_Click(object sender, EventArgs e)
        {
            /*
            _inputGraph._stop();
            string[] strData = new string[data_to_FFT.Count()];

            for(int i = 0; i <data_to_FFT.Count(); i++)
            {
                strData[i] = Convert.ToString(data_to_FFT[i]);
            }

            System.IO.File.WriteAllLines("testOutpt.txt",  strData);
            */
         
            _audioIn.dispose();
            Exit();
            
        }


        /// <summary>
        /// changes game state, 
        /// if changing to playing, all options from textboxes parsed to required data types and objects
        /// if changing to paused, input and input graph are stopped, and audiodata re initialized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void randomButton_Click(object sender, EventArgs e)
        {
            
            GAMESTATE = !GAMESTATE;
            _pausePlay = (GAMESTATE) ? "playing" : "paused";
            _pausePLayTxtX = Convert.ToInt16((39 * GraphicsDevice.DisplayMode.Width / 1920));
            _pausePLayTxtY = 31 * GraphicsDevice.DisplayMode.Height / 1080;
            if (GAMESTATE == true)
            {
                if (_textBoxs[0].text == "") _textBoxs[0].text = "1.0";
                else if (!Single.TryParse(_textBoxs[0].text, out _)) _textBoxs[0].text += ".0";

                if (_textBoxs[1].text == "") _textBoxs[1].text = "1024";

                

                inputLen = Convert.ToSingle(_textBoxs[0].text);

                FFTLen = Convert.ToInt32(_textBoxs[1].text);

                currentTextBox = 0;

                _FFTDisplay.SetInputLength(Convert.ToInt32(_textBoxs[1].text));
                _inputGraph.setInputData(inputLen);
                _audioIn._start();                
            }
            else
            {
                _audioIn._stop();
                _inputGraph._stop();
                _audioData = new List<float> { };
            }
        }

        /// <summary>
        /// updates all controls and graphs
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {

                _audioIn.dispose();
                Exit();
                
            }


            HOVERING = false;
            CURRENTMOUSE = Mouse.GetState();

            _oldKeyboard = _currentKeyboard;
            _currentKeyboard = Keyboard.GetState();

            isTyping = TryConvertKeyboardInput(_currentKeyboard, _oldKeyboard,out keyInpt);

            //tries to get audio data if playing
            if (GAMESTATE == true)
            {

                var x = _audioIn.currentData;
                _audioIn.currentData = new List<Single>();
                _audioData = x;
                ////suspect this gap is whats causing grpah irrelugarities


                //draws input graph if there is any data
                if (_audioData != null && _audioData.Count != 0)
                {
                    //Debug.WriteLine("test2");
                    //input needs a decimal place bruh or big crash
                    var data = _inputGraph.Update(gameTime, _audioData, FFTLen, out var canFFT);

                    //_FFTDisplay.FFTForwardtoPoints(data);
                    //FFTs if long enough
                    if (canFFT)
                    {
                        _FFTDisplay.FFTForwardtoPoints(data,_sliders[0].distance,_sliders[1].distance);
                        //_drawGraphs = true;                       
                    }
                    
                }
            }


            //updates textboxes
            if (currentTextBox != 0)
            {
                if (isTyping)
                {
                    _textBoxs[currentTextBox - 1].updateTxt(keyInpt);
                    //Debug.WriteLine(keyInpt);
                }


                else if (_currentKeyboard.IsKeyDown(Keys.Back) && !(_oldKeyboard.IsKeyDown(Keys.Back)))
                {
                    _textBoxs[currentTextBox - 1].deleteTxt();
                }
            }

            // TODO: Add your update logic here

            
            _cursor.Update(gameTime);
            foreach(var component in _gameComponent)
            {
                component.Update(gameTime);
            }

            foreach(var slide in _sliders)
            {
                slide.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// draws all controls and graphs
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backColor);
            _spriteBatch.Begin();

            
           
            foreach (var component in _gameComponent)
            {
                component.Draw(gameTime, _spriteBatch);
            }

            foreach (var slide in _sliders)
            {
                slide.Draw(gameTime,_spriteBatch);
            }

            if (GAMESTATE == true)
            {
                _inputGraph.Draw(gameTime, _spriteBatch);
                
                _FFTDisplay.Draw(_spriteBatch);
                
            }
            

                    

            _spriteBatch.DrawString(SourceHeavy, (Convert.ToString(1 / gameTime.ElapsedGameTime.TotalSeconds)).Substring(0,5), FPSDisplayPos, Color.White);
            
            _spriteBatch.DrawString(SourceHeavy, _pausePlay, new Vector2(_pausePLayTxtX, _pausePLayTxtY), Color.White);
            _cursor.Draw(gameTime, _spriteBatch);
            _spriteBatch.Draw(_boxes, _screenPos, Color.White);
            _spriteBatch.Draw(_labels, _screenPos, Color.White);
            _spriteBatch.End();
            // TODO: Add your drawing code here
            //to make "sliding" button banks swap buttons for an image
            base.Draw(gameTime);
        }

        /// <summary>
        /// Tries to convert keyboard input to characters and prevents repeatedly returning the 
        /// same character if a key was pressed last frame, but not yet unpressed this frame.
        /// </summary>
        /// <param name="keyboard">The current KeyboardState</param>
        /// <param name="oldKeyboard">The KeyboardState of the previous frame</param>
        /// <param name="key">When this method returns, contains the correct character if conversion succeeded.
        /// Else contains the null, (000), character.</param>
        /// <returns>True if conversion was successful</returns>
        public static bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            if (keys.Length > 0 && !oldKeyboard.IsKeyDown(keys[0]))
            {
                switch (keys[0])
                {
                    //Alphabet keys
                    case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                    case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                    case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                    case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                    case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                    case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                    case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                    case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                    case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                    case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                    case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                    case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                    case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                    case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                    case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                    case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                    case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                    case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                    case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                    case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                    case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                    case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                    case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                    case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                    case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                    case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                    //Decimal keys
                    case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                    case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                    case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                    case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                    case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                    case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                    case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                    case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                    case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                    case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                    //Decimal numpad keys
                    case Keys.NumPad0: key = '0'; return true;
                    case Keys.NumPad1: key = '1'; return true;
                    case Keys.NumPad2: key = '2'; return true;
                    case Keys.NumPad3: key = '3'; return true;
                    case Keys.NumPad4: key = '4'; return true;
                    case Keys.NumPad5: key = '5'; return true;
                    case Keys.NumPad6: key = '6'; return true;
                    case Keys.NumPad7: key = '7'; return true;
                    case Keys.NumPad8: key = '8'; return true;
                    case Keys.NumPad9: key = '9'; return true;

                    //Special keys
                    case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                    case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                    case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                    case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                    case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                    case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                    case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                    case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                    case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                    case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                    case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                    case Keys.Space: key = ' '; return true;
                }
            }

            key = (char)0;
            return false;
        }

       
    }
}
