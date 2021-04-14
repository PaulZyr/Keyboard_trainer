using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Home0416_KeyboardTrainer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Label> _labels = new List<Label>();

        bool _isPlaying = false;
        string _goal;
        string _goalBase;
        int _maxWordLength = 0;
        int _maxGoalLength = 0;
        int _difficulty = 0;
        int _fails = 0;

        DispatcherTimer timer = new DispatcherTimer();
        int _timeGone = 0;

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0,0,0,0,1000);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement elem in (this.Content as Grid).Children)
            {
                if (elem is Grid)
                {
                    foreach (var item in (elem as Grid).Children)
                    {
                        if (item != null && item is Label)
                        {
                            _labels.Add((item as Label));
                        }
                    }
                }
            }
        }

        #region Start Stop
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            ReadyToPlay();
            CreateGoalBase();
            GenerateGoal();
            ShowGoal();
            _isPlaying = true;
            timer.Start();
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            difficultySlider.IsEnabled = false;
            caseSensCheckBox.IsEnabled = false;
        }
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            _isPlaying = false;
            GameOver();
        }
        
        #endregion

        #region GamePlay
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeGone++;
            CountSpeed();
        }
        
        private void CountSpeed()
        {
            if (_timeGone != 0)
            {
                speedLabel.Content = (Math.Round((double)(resultStackPanel.Children.Count * (60 / _timeGone)))).ToString();
            }
            else speedLabel.Content = "0";
        }
        private void ReadyToPlay()
        {
            _goal = "";
            _goalBase = "";
            _timeGone = 0;
            _fails = 0;
            goalStackPanel.Children.Clear();
            resultStackPanel.Children.Clear();
            speedLabel.Content = "0";
            failsLabel.Content = "0";
            endLabel.Content = "playing...";
        }
        private void GameOver()
        {
            timer.Stop();
            CountSpeed();
            endLabel.Content = "GAME OVER! Mark = " + CountMark();
            _isPlaying = false;
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            difficultySlider.IsEnabled = true;
            caseSensCheckBox.IsEnabled = true;
        }
        private string CountMark()
        { 
            int mark = 0;
            if (_goal.Length == resultStackPanel.Children.Count)
            {
                if (_difficulty == 4 && _fails < 10) return "Gold Medal!";
                else if (_difficulty == 4 && _fails < 20) mark = 12;
                else if (_difficulty > 2 && _fails < 3) mark = 12;
                else if (_difficulty > 1 && _fails < 3) mark = 11;
                else if (_difficulty > 1 && _fails < 4) mark = 10;
                else if (_difficulty > 1 && _fails < 6) mark = 9;
                else if (_fails < 3) mark = 8;
                else if (_fails < 4) mark = 7;
                else if (_fails < 6) mark = 6;
                else if (_fails < 8) mark = 5;
                else if (_fails < 10) mark = 4;
                else mark = 2;
            }
            return mark.ToString();
        }
        #endregion

        #region Events
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key != Key.Capital)
            {
                LightingKey(e.Key.ToString());
            }  
            else
            {
                if (Keyboard.IsKeyToggled(Key.CapsLock))
                {
                    SetKeys(CAPSKeys);
                    LightingCapsLock();
                }
                else
                {
                    SetKeys(LowerKeys);
                    LightingCapsLock();
                }
            }

            if(e.Key == Key.Back && resultStackPanel.Children.Count > 0)
            {
                int n = resultStackPanel.Children.Count;
                resultStackPanel.Children.RemoveAt(n - 1);
                (goalStackPanel.Children[n - 1] as Label).Background = Brushes.LightBlue;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                SetKeys(ShiftKeys);
            }
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Capital) UnLightingKey(e.Key.ToString());
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                if (!Keyboard.IsKeyToggled(Key.CapsLock))
                {
                    SetKeys(LowerKeys);
                }
                else SetKeys(CAPSKeys);
            }
        }
        private void Window_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (_isPlaying && e.Text != "\b")
            {
                Label tmp = new Label();
                tmp.Content = e.Text;
                tmp.FontSize = 30;
                int n = resultStackPanel.Children.Count;

                string str = "" + (goalStackPanel.Children[n] as Label).Content;

                if (str == e.Text.ToString())
                {
                    tmp.Background = Brushes.LightGreen;
                    (goalStackPanel.Children[n] as Label).Background = Brushes.LightGreen;
                }
                else
                {
                    tmp.Background = Brushes.LightPink;
                    (goalStackPanel.Children[n] as Label).Background = Brushes.LightPink;
                    _fails++;
                    failsLabel.Content = _fails.ToString();
                }
                resultStackPanel.Children.Add(tmp);

                if (n + 1 == goalStackPanel.Children.Count) GameOver();
            }

        }
        #endregion

        #region LightingKeys
        private void LightingKey(string name)
        {
            foreach (var label in _labels)
            {
                if (label.Name == name)
                {
                    label.Opacity = 0.5;
                }
                    
            }
        }
        private void UnLightingKey(string name)
        {
            foreach (var label in _labels)
            {
                if (label.Name == name) label.Opacity = 1;
            }
        }
        private void LightingCapsLock()
        {
            if (Keyboard.IsKeyToggled(Key.CapsLock)) LightingKey("Capital");
            else UnLightingKey("Capital");
        }

        #endregion

        #region Goal text
        private void CreateGoalBase()
        {
            _difficulty = (int)difficultySlider.Value;
            _goalBase += letters;

            if (_difficulty >= 2)
            {
                _goalBase += lettersDiff;
            }   
            if (_difficulty >= 3)
            {
                _goalBase += symbols;
            }  
            if (_difficulty == 4)
            {
                _goalBase += shiftSymbols;
            }
            if (caseSensCheckBox.IsChecked ?? false)
            {
                _goalBase += letters.ToUpper();
                _goalBase += lettersDiff.ToUpper();
            }
            _maxWordLength = _difficulty * 2;
            _maxGoalLength = _difficulty * 18;
        }

        private void GenerateGoal()
        {
            Random rand = new Random();

            while(_goal.Length < _maxGoalLength)
            {
                var n = rand.Next(1, _maxWordLength + 2);

                for (var i = 0; i < n; ++i)
                {
                    var j = rand.Next(0, _goalBase.Length);
                    _goal += _goalBase[j];
                }
                _goal += " ";
            }
            _goal = _goal.Substring(0, _maxGoalLength - 1);
            _goal += ".";
        }

        private void ShowGoal()
        {
            foreach (var ch in _goal)
            {
                Label tmp = new Label();
                tmp.Content = ch;
                tmp.FontSize = 30;
                tmp.Background = Brushes.LightBlue;
                goalStackPanel.Children.Add(tmp);
            }
        }

        #endregion

        #region Keys Strings
        private void SetKeys(string[] str)
        {
            Oem3.Content = str[0];
            D1.Content = str[1];
            D2.Content = str[2];
            D3.Content = str[3];
            D4.Content = str[4];
            D5.Content = str[5];
            D6.Content = str[6];
            D7.Content = str[7];
            D8.Content = str[8];
            D9.Content = str[9];
            D0.Content = str[10];
            OemMinus.Content = str[11];
            OemPlus.Content = str[12];
            Q.Content = str[13];
            W.Content = str[14];
            E.Content = str[15];
            R.Content = str[16];
            T.Content = str[17];
            Y.Content = str[18];
            U.Content = str[19];
            I.Content = str[20];
            O.Content = str[21];
            P.Content = str[22];
            OemOpenBrackets.Content = str[23];
            Oem6.Content = str[24];
            Oem5.Content = str[25];
            A.Content = str[26];
            S.Content = str[27];
            D.Content = str[28];
            F.Content = str[29];
            G.Content = str[30];
            H.Content = str[31];
            J.Content = str[32];
            K.Content = str[33];
            L.Content = str[34];
            Oem1.Content = str[35];
            OemQuotes.Content = str[36];
            Z.Content = str[37];
            X.Content = str[38];
            C.Content = str[39];
            V.Content = str[40];
            B.Content = str[41];
            N.Content = str[42];
            M.Content = str[43];
            OemComma.Content = str[44];
            OemPeriod.Content = str[45];
            OemQuestion.Content = str[46];
        }

        private string[] LowerKeys = new string[]
        {
            "`","1","2","3","4",  "5","6","7","8","9",  "0","-","=",
            "q","w","e","r","t",  "y","u","i","o","p",  "[","]","\\",
            "a","s","d","f","g",  "h","j","k","l",";",  "'",
            "z","x","c","v","b",  "n","m",",",".","/",
            
        };
        private string[] ShiftKeys = new string[]
        {
            "~","!","@","#","$",  "%","^","&","*","(",  ")","_","+",
            "Q","W","E","R","T",  "Y","U","I","O","P",  "{","}","|",
            "A","S","D","F","G",  "H","J","K","L",":",  "\"",
            "Z","X","C","V","B",  "N","M","<",">","?",
        };
        private string[] CAPSKeys = new string[]
        {
            "`","1","2","3","4",  "5","6","7","8","9",  "0","-","=",
            "Q","W","E","R","T",  "Y","U","I","O","P",  "[","]","\\",
            "A","S","D","F","G",  "H","J","K","L",";",  "'",
            "Z","X","C","V","B",  "N","M",",",".","/",
        };

        private string letters = "rtyufghjvbn";
        private string lettersDiff = "qweiopasdklzxcm0123456789";
        private string symbols = "`-=[]\\;',./";
        private string shiftSymbols = "~!@#$%^&*()_+{}|:\"<>?";
        #endregion

        
    }
}
