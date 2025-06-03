using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ProjectForLanguage
{
    public partial class FrmWordle : Form
    {
        const int MaxRows = 6;
        const int WordLength = 5;
        Label[,] boxes = new Label[MaxRows, WordLength];
        int currentRow = 0;
        int currentColumn = 0;
        string targetWord;
        List<string> wordList = new List<string>();
        int wins = 0, losses = 0;
        Label scoreLabel;
        Button restartButton;

        string connectionString = "Data Source=DESKTOP-DUEUI74;Initial Catalog=LanguageApp;Integrated Security=True;TrustServerCertificate=True"; // 🔁 DEĞİŞTİR

        public FrmWordle()
        {
            InitializeComponent();
            this.Size = new Size(400, 500);
            this.Load += Form1_Load;
            this.KeyPreview = true;
            this.Shown += Form1_Shown;
            this.KeyPress += Form1_KeyPress;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateGrid();
            CreateRestartButton();
            CreateScoreLabel();
            SelectRandomWord();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
            this.Focus();
            this.Select();
        }

        private void CreateGrid()
        {
            int startX = 50, startY = 50;
            for (int row = 0; row < MaxRows; row++)
            {
                for (int col = 0; col < WordLength; col++)
                {
                    Label box = new Label
                    {
                        Size = new Size(50, 50),
                        Location = new Point(startX + col * 55, startY + row * 55),
                        BorderStyle = BorderStyle.FixedSingle,
                        Font = new Font("Segoe UI", 18, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.White
                    };
                    this.Controls.Add(box);
                    boxes[row, col] = box;
                }
            }
        }

        private void CreateRestartButton()
        {
            restartButton = new Button
            {
                Text = "Restart",
                Size = new Size(100, 30),
                Location = new Point(50, 400)
            };
            restartButton.Click += RestartButton_Click;
            this.Controls.Add(restartButton);
        }

        private void CreateScoreLabel()
        {
            scoreLabel = new Label
            {
                Location = new Point(170, 400),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = $"Wins: {wins} | Losses: {losses}"
            };
            this.Controls.Add(scoreLabel);
        }

        private void SelectRandomWord()
        {
            wordList.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT EnglishWord 
                    FROM Words 
                    WHERE LEN(EnglishWord) = 5 
                      AND CorrectCount >= 6
                ", connection);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        wordList.Add(reader.GetString(0).ToUpper());
                    }
                }
            }

            if (wordList.Count == 0)
            {
                MessageBox.Show("Uygun kelime bulunamadı.");
                targetWord = "EMPTY";
                return;
            }

            Random rnd = new Random();
            targetWord = wordList[rnd.Next(wordList.Count)];
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Tekrar oynamak ister misiniz?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                FrmMainPage mainPage = new FrmMainPage();
                this.Close();
                mainPage.Show();
                return;
            }

            currentRow = 0;
            currentColumn = 0;

            foreach (var box in boxes)
            {
                box.Text = "";
                box.BackColor = Color.White;
            }

            this.ActiveControl = null;
            this.Focus();
            SelectRandomWord();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (currentRow >= MaxRows) return;

            char upperChar = char.ToUpper(e.KeyChar);

            if (char.IsLetter(upperChar))
            {
                if (currentColumn < WordLength)
                {
                    boxes[currentRow, currentColumn].Text = upperChar.ToString();
                    currentColumn++;
                }
                e.Handled = true;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentRow >= MaxRows) return;

            if (e.KeyCode == Keys.Back && currentColumn > 0)
            {
                currentColumn--;
                boxes[currentRow, currentColumn].Text = "";
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left && currentColumn > 0)
            {
                currentColumn--;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right && currentColumn < WordLength)
            {
                currentColumn++;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && currentColumn == WordLength)
            {
                CheckGuess();
                e.Handled = true;
            }
        }

        private void CheckGuess()
        {
            string guess = GetGuess();
            if (guess.Length != WordLength)
            {
                MessageBox.Show("Lütfen 5 harfli bir kelime girin.");
                return;
            }

            ApplyColorFeedback(guess);

            if (guess == targetWord)
            {
                MessageBox.Show("🎉 TEBRİKLER!");
                wins++;
                UpdateScore();
                currentRow = MaxRows;
                return;
            }

            currentRow++;
            currentColumn = 0;

            if (currentRow == MaxRows)
            {
                MessageBox.Show($"❌ OYUN BİTTİ! Kelime: {targetWord}");
                losses++;
                UpdateScore();
            }
        }

        private string GetGuess()
        {
            string guess = "";
            for (int i = 0; i < WordLength; i++)
                guess += boxes[currentRow, i].Text.ToUpper();
            return guess;
        }

        private void ApplyColorFeedback(string guess)
        {
            bool[] matched = new bool[WordLength];
            bool[] used = new bool[WordLength];
            char[] guessArr = guess.ToCharArray();
            char[] targetArr = targetWord.ToCharArray();

            for (int i = 0; i < WordLength; i++)
            {
                if (guessArr[i] == targetArr[i])
                {
                    boxes[currentRow, i].BackColor = Color.Green;
                    matched[i] = true;
                    used[i] = true;
                }
            }

            for (int i = 0; i < WordLength; i++)
            {
                if (boxes[currentRow, i].BackColor == Color.Green) continue;

                for (int j = 0; j < WordLength; j++)
                {
                    if (!matched[j] && guessArr[i] == targetArr[j])
                    {
                        boxes[currentRow, i].BackColor = Color.Goldenrod;
                        matched[j] = true;
                        used[i] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < WordLength; i++)
            {
                if (!used[i])
                    boxes[currentRow, i].BackColor = Color.Gray;
            }
        }

        private void FrmWordle_Load(object sender, EventArgs e)
        {

        }

        private void UpdateScore()
        {
            scoreLabel.Text = $"Wins: {wins} | Losses: {losses}";
        }
    }
}
