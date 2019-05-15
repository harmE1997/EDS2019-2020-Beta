﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDS_Poule
{
    public partial class PlayerForm : Form
    {
        public Player player;
        public PlayerManager manager;
        private NumericUpDown[] NUDs;
        private Week[] weeks;
        private Estimations estimations;
        int counter;
        bool ManualInput;
        public PlayerForm()
        {
            NUDs = new NumericUpDown[18];
            weeks = new Week[34];
            estimations = null;
            counter = 0;
            ManualInput = false;
            InitializeComponent();
            fillNudsArray();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Match[] matches = new Match[9];
            int Indicator = 0;
            while (Indicator < 17)
            {
                Match match = new Match(Convert.ToInt32(NUDs[Indicator].Value), Convert.ToInt32(NUDs[Indicator + 1].Value));
                matches[Indicator / 2] = match;
                Indicator += 2;
            }

            weeks[counter] = new Week((counter + 1), matches);
            if (counter == 33)
            {
                SavePlayer();
            }

            else if (player != null)
            {
                counter++;

                if (player.Weeks[counter] != null)
                {
                    gbWeeks.Text = "Week " + (counter + 1);
                    loadWeek(counter);
                }

                else
                    MessageBox.Show("Can't load week. It doesn't exist");
            }

            else
            {
                counter++;
                gbWeeks.Text = "Week " + (counter + 1);
                foreach (NumericUpDown nud in NUDs)
                    nud.Value = 99;
            }
        }

        private void btnReadFile_Click(object sender, EventArgs e)
        {
            string filename = tbFileName.Text;
            if (string.IsNullOrEmpty(filename) || !System.IO.File.Exists(filename) || ((!filename.EndsWith(".xls")) && (!filename.EndsWith(".xlsx"))))
            {
                MessageBox.Show("Invalid file");
                return;
            }

            ExcelManager em = new ExcelManager();
            if (cbSecondHalf.Checked)
            {
                weeks = em.readPredictions(filename, 1, new ExcelReadSettings(2, Convert.ToInt32(nudAfwijking.Value)), player.Weeks);
            }

            else if (cbFirstHalf.Checked)
            {
                weeks = em.readPredictions(filename, 1, new ExcelReadSettings(1, Convert.ToInt32(nudAfwijking.Value)));
            }

            else
            {
                weeks = em.readPredictions(filename, 1, new ExcelReadSettings(0, Convert.ToInt32(nudAfwijking.Value)));
            }
            estimations = em.ReadEstimations();
            SavePlayer();
            em.Clean();
            MessageBox.Show("Predictions succesfully loaded and saved!");
        }

        private void btnSwitchInput_Click(object sender, EventArgs e)
        {
            switchInput();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            if (ofdVoorspellingen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbFileName.Text = ofdVoorspellingen.FileName;
            }
        }
        private void cbSecondHalf_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSecondHalf.Checked)
            {
                cbFirstHalf.Checked = false;
            }
        }

        private void cbFirstHalf_CheckedChanged(object sender, EventArgs e)
        {
            if (cbFirstHalf.Checked)
            {
                cbSecondHalf.Checked = false;
            }
        }
        public void switchInput()
        {
            if (ManualInput)
            {
                gbInputFromFile.Visible = true;
                gbManualInput.Visible = false;
                btnSwitchInput.Text = "Switch to Manual Input";
                ManualInput = false;
            }

            else
            {
                gbInputFromFile.Visible = false;
                gbManualInput.Visible = true;
                btnSwitchInput.Text = "Switch to Input from Excel File";
                ManualInput = true;
            }
        }

        public void loadWeek(int round)
        {
            int loader = 0;
            while (loader < 17)
            {
                NUDs[loader].Value = player.Weeks[round].Matches[loader / 2].ResultA;
                NUDs[loader + 1].Value = player.Weeks[round].Matches[loader / 2].ResultB;
                loader += 2;
            }
        }

        public void loadBonus()
        {
            tbKampioen.Text = player.Questions.Answers["Kampioen"].Answer;
            tbDegradant.Text = player.Questions.Answers["Degradant"].Answer;
            tbDiv1Kampioen.Text = player.Questions.Answers["Kampioendivisie1"].Answer;
            tbFin1.Text = player.Questions.Answers["Finalisten"].AnswerArray[0];
            tbFin2.Text = player.Questions.Answers["Finalisten"].AnswerArray[1];
            tbRonde.Text = player.Questions.Answers["Championround"].Answer;
            tbTopscorer.Text = player.Questions.Answers["Topscorer"].Answer;
            tbTrainer.Text = player.Questions.Answers["Trainer"].Answer;
            tbWinterkampioen.Text = player.Questions.Answers["Winterkampioen"].Answer;
            tbMostRed.Text = player.Questions.Answers["Teamreds"].Answer;
            tbAssists.Text = player.Questions.Answers["Assists"].Answer;
            tbWorstDefence.Text = player.Questions.Answers["Worstdefence"].Answer;
            tbProdeg1.Text = player.Questions.Answers["Prodeg"].AnswerArray[0];
            tbProdeg2.Text = player.Questions.Answers["Prodeg"].AnswerArray[1];
        }

        public void loadEstimations()
        {
            nudGoals.Value = player.Estimations.Answers["Goals"].Answer;
            nudReds.Value = player.Estimations.Answers["Reds"].Answer;
        }
        public void loadPlayer(Player player)
        {
            tbName.Text = player.Name;
            tbAge.Text = player.Age;
            tbWoonplaats.Text = player.Woonplaats;
        }

        private void SavePlayer()
        {
            try
            {
                string[] finalists = { tbFin1.Text, tbFin2.Text };
                string[] prodeg = { tbProdeg1.Text, tbProdeg2.Text };
                BonusQuestions questions = new BonusQuestions(tbKampioen.Text, tbDegradant.Text, tbTopscorer.Text, tbTrainer.Text
                    , tbWinterkampioen.Text, tbRonde.Text, tbDiv1Kampioen.Text, finalists, tbMostRed.Text, tbAssists.Text, tbWorstDefence.Text, 
                     prodeg,new int[12] {99,99,99,99,99,99,99,99,99,99,99,99});

                if(estimations == null)
                estimations = new Estimations(Convert.ToInt32(nudReds.Value), Convert.ToInt32(nudGoals.Value));

                Player newplayer = new Player(tbName.Text, tbAge.Text, tbWoonplaats.Text, weeks, questions, estimations);
                if (player != null)
                {
                    manager.removePlayer(player.Name);
                }
                manager.AddPlayer(newplayer);
                manager.SavePlayers();
                this.Dispose();
                this.Close();
            }

            catch
            {
                MessageBox.Show("veld niet ingevuld");
            }
        }

        private void fillNudsArray()
        {
            NUDs[0] = numericUpDown1;
            NUDs[1] = numericUpDown2;
            NUDs[2] = numericUpDown3;
            NUDs[3] = numericUpDown4;
            NUDs[4] = numericUpDown5;
            NUDs[5] = numericUpDown6;
            NUDs[6] = numericUpDown7;
            NUDs[7] = numericUpDown8;
            NUDs[8] = numericUpDown9;
            NUDs[9] = numericUpDown10;
            NUDs[10] = numericUpDown11;
            NUDs[11] = numericUpDown12;
            NUDs[12] = numericUpDown13;
            NUDs[13] = numericUpDown14;
            NUDs[14] = numericUpDown15;
            NUDs[15] = numericUpDown16;
            NUDs[16] = numericUpDown17;
            NUDs[17] = numericUpDown18;
        }
    }
}
