/*
 * ========================================================
 * Projekt neve: Tsz+GYb-Kavegep-project
 * Készítők monogramja: Tsz + GYb
 * ========================================================
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tsz_GYb_kávé
{
    public class User
    {
        public string Name { get; set; }
        public string Pin { get; set; }
        public int Balance { get; set; }

        public User(string name, string pin, int balance)
        {
            Name = name;
            Pin = pin;
            Balance = balance;
        }
    }

    public partial class Form1 : Form
    {
        private List<User> users = new List<User>();
        private User loggedInUser = null;
        private Random rand = new Random();

        private Image gifImage;
        private int gifDurationMs = 0;
        private Bitmap firstFrame;
        private Bitmap lastFrame;
        private System.Windows.Forms.Timer animTimer = new System.Windows.Forms.Timer();

        private const int FeltoltesOsszeg = 1000;
        private const int GombSzel = 46;
        private const int GombMag = 42;
        private const int Res = 6;

        private Panel panelEgyenleg;
        private Label labelEgyenlegErtek;
        private Button buttonFeltoltes;
        private readonly Font egyenlegFont = new Font("Segoe UI", 20F, FontStyle.Bold);
        private readonly Font nincsFont = new Font("Segoe UI", 10F);

        private static readonly Color KekSotet = Color.FromArgb(13, 21, 58);
        private static readonly Color KekKozep = Color.FromArgb(29, 40, 119);
        private static readonly Color TealAlap = Color.FromArgb(14, 116, 125);
        private static readonly Color TealHover = Color.FromArgb(30, 145, 156);
        private static readonly Color TealSotet = Color.FromArgb(8, 78, 86);
        private static readonly Color HaloTeal = Color.FromArgb(226, 241, 243);
        private static readonly Color Narancs = Color.FromArgb(230, 126, 34);
        private static readonly Color NarancsHover = Color.FromArgb(243, 150, 62);
        private static readonly Color NarancsSotet = Color.FromArgb(190, 95, 20);

        public Form1()
        {
            InitializeComponent();

            InitializeUsers();

            gifImage = LoadGif();
            pictureBox7.Image = null;

            if (gifImage != null)
            {
                gifDurationMs = GetGifDuration(gifImage);
                int frames = gifImage.GetFrameCount(FrameDimension.Time);
                firstFrame = GetFrame(gifImage, 0);
                lastFrame = GetFrame(gifImage, Math.Max(frames - 1, 0));
            }
            else
            {
                MessageBox.Show("A GIF nem található itt:\n" +
                    Path.Combine(Application.StartupPath, "Resources", "kave_animacio_3x1.gif"),
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox7.Image = firstFrame;
            pictureBox7.Visible = true;

            animTimer.Tick += AnimTimer_Tick;

            BorderNelkul(this);
            WireUpEvents();
            FeluletStilus();
        }

        private void InitializeUsers()
        {
            users.Add(new User("Kovács Anna", "1234", rand.Next(0, 5601)));
            users.Add(new User("Nagy Béla", "2345", rand.Next(0, 5601)));
            users.Add(new User("Szabó Cecília", "3456", rand.Next(0, 5601)));
            users.Add(new User("Kiss Dénes", "4567", rand.Next(0, 5601)));
            users.Add(new User("Tóth Eszter", "5678", rand.Next(0, 5601)));
        }

        private void WireUpEvents()
        {
            Button[] numButtons = { button0, buttonegy, button2, buttonharom, buttonnegy, buttonot, buttonhat, button7, buttonnyoc, buttonkilenc };
            foreach (var btn in numButtons)
            {
                if (btn != null) btn.Click += NumericButton_Click;
            }

            if (button3 != null) button3.Click += ButtonTorles_Click;
            if (buttonok != null) buttonok.Click += ButtonOk_Click;

            if (buttonhanko != null) buttonhanko.Click += (s, e) => BuyDrink("Hankós kattogós", 160);
            if (buttonSzijjarto != null) buttonSzijjarto.Click += (s, e) => BuyDrink("Szijjártó szirupos", 180);
            if (buttonMeszaros != null) buttonMeszaros.Click += (s, e) => BuyDrink("Mészáros mokka", 280);
            if (buttonRogan != null) buttonRogan.Click += (s, e) => BuyDrink("Rogán ristretto", 200);
            if (buttonOrban != null) buttonOrban.Click += (s, e) => BuyDrink("Orban cappucino", 280);
            if (buttonLazar != null) buttonLazar.Click += (s, e) => BuyDrink("Lázár latte", 170);
        }

        private void NumericButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                if (textBoxpin.Text == "****" || textBoxpin.Text.Length >= 4)
                {
                    textBoxpin.Clear();
                }
                textBoxpin.Text += btn.Text;
            }
        }

        private void ButtonTorles_Click(object sender, EventArgs e)
        {
            if (textBoxpin.Text == "****")
            {
                textBoxpin.Clear();
            }
            else if (textBoxpin.Text.Length > 0)
            {
                textBoxpin.Text = textBoxpin.Text.Substring(0, textBoxpin.Text.Length - 1);
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            string enteredPin = textBoxpin.Text;
            string enteredName = textBoxname.Text.Trim();

            loggedInUser = users.FirstOrDefault(u => u.Pin == enteredPin ||
                (enteredName != "[Név megadása]" && u.Name.Equals(enteredName, StringComparison.OrdinalIgnoreCase)));

            FrissitEgyenleg();

            if (loggedInUser != null)
            {
                MessageBox.Show($"Sikeres azonosítás!\nÜdvözöljük, {loggedInUser.Name}!\nAktuális egyenleg: {loggedInUser.Balance} Ft", "Azonosítás", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxname.Text = loggedInUser.Name;
                textBoxpin.Text = loggedInUser.Pin;
            }
            else
            {
                MessageBox.Show("Hibás PIN-kód vagy név! Kérjük, próbálja újra.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonFeltoltes_Click(object sender, EventArgs e)
        {
            if (loggedInUser == null)
            {
                MessageBox.Show("Kérjük, előbb azonosítsa magát a PIN-kóddal vagy névvel!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dr = MessageBox.Show(
                $"Feltölti az egyenlegét {FeltoltesOsszeg} Ft-tal?\n\nEgyenleg: {loggedInUser.Balance} Ft  →  {loggedInUser.Balance + FeltoltesOsszeg} Ft",
                "Egyenleg feltöltése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            loggedInUser.Balance += FeltoltesOsszeg;
            FrissitEgyenleg();
            MessageBox.Show($"Sikeres feltöltés! Új egyenleg: {loggedInUser.Balance} Ft", "Feltöltés", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FrissitEgyenleg()
        {
            if (labelEgyenlegErtek == null) return;

            if (loggedInUser == null)
            {
                labelEgyenlegErtek.Font = nincsFont;
                labelEgyenlegErtek.ForeColor = Color.FromArgb(120, 130, 140);
                labelEgyenlegErtek.Text = "Nincs bejelentkezve";
            }
            else
            {
                labelEgyenlegErtek.Font = egyenlegFont;
                labelEgyenlegErtek.ForeColor = TealSotet;
                labelEgyenlegErtek.Text = loggedInUser.Balance + " Ft";
            }
        }

        private void BuyDrink(string drinkName, int price)
        {
            if (loggedInUser == null)
            {
                MessageBox.Show("Kérjük, előbb azonosítsa magát a PIN-kóddal vagy névvel!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (loggedInUser.Balance < price)
            {
                int missing = price - loggedInUser.Balance;
                DialogResult dr = MessageBox.Show($"Nincs elegendő fedezet!\nA hiányzó összeg: {missing} Ft.\nSzeretné feltölteni az egyenlegét?", "Alacsony egyenleg", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dr != DialogResult.Yes) return;

                loggedInUser.Balance += missing + 1000;
                FrissitEgyenleg();
                MessageBox.Show($"Sikeres feltöltés! Új egyenleg: {loggedInUser.Balance} Ft", "Feltöltés", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            DialogResult confirm = MessageBox.Show(
                $"Biztosan megveszi?\n\nItal: {drinkName}\nÁr: {price} Ft\nEgyenleg: {loggedInUser.Balance} Ft  →  {loggedInUser.Balance - price} Ft",
                "Rendelés megerősítése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            loggedInUser.Balance -= price;
            FrissitEgyenleg();
            PlayAnimation();

            MessageBox.Show($"Sikeres vásárlás!\nItal: {drinkName} ({price} Ft).\nMaradék egyenleg: {loggedInUser.Balance} Ft", "Kész", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ================= GIF kezelés =================

        private Image LoadGif()
        {
            if (pictureBox7.Image != null) return pictureBox7.Image;

            string path = Path.Combine(Application.StartupPath, "Resources", "kave_animacio_3x1.gif");
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                return Image.FromStream(new MemoryStream(bytes));
            }

            return null;
        }

        private void PlayAnimation()
        {
            if (gifImage == null) return;

            animTimer.Stop();
            pictureBox7.Image = null;

            gifImage.SelectActiveFrame(FrameDimension.Time, 0);
            pictureBox7.Image = gifImage;

            animTimer.Interval = gifDurationMs > 0 ? gifDurationMs : 3000;
            animTimer.Start();
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            animTimer.Stop();
            pictureBox7.Image = lastFrame;
        }

        private Bitmap GetFrame(Image img, int index)
        {
            img.SelectActiveFrame(FrameDimension.Time, index);
            return new Bitmap(img);
        }

        private int GetGifDuration(Image img)
        {
            if (img == null) return 0;
            try
            {
                int frames = img.GetFrameCount(FrameDimension.Time);
                if (frames <= 1) return 0;

                byte[] delays = img.GetPropertyItem(0x5100).Value;
                int total = 0;
                for (int i = 0; i < frames; i++)
                {
                    int d = BitConverter.ToInt32(delays, i * 4);
                    if (d <= 0) d = 10;
                    total += d * 10;
                }
                return total;
            }
            catch { return 0; }
        }

        // ================= Megjelenés =================

        private void BorderNelkul(Control szulo)
        {
            foreach (Control vezerlo in szulo.Controls)
            {
                if (vezerlo is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                }
                if (vezerlo.HasChildren)
                {
                    BorderNelkul(vezerlo);
                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                KekSotet,
                KekKozep,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void Lekerekit(Control cel, int sarok)
        {
            if (cel.Width <= 0 || cel.Height <= 0) return;
            int r = Math.Min(sarok, Math.Min(cel.Width, cel.Height) / 2);
            int d = r * 2;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(cel.Width - d, 0, d, d, 270, 90);
                path.AddArc(cel.Width - d, cel.Height - d, d, d, 0, 90);
                path.AddArc(0, cel.Height - d, d, d, 90, 90);
                path.CloseFigure();
                Region regi = cel.Region;
                cel.Region = new Region(path);
                regi?.Dispose();
            }
        }

        private void LekerekitFigyelve(Control cel, int sarok)
        {
            cel.Resize += (s, e) => Lekerekit(cel, sarok);
            Lekerekit(cel, sarok);
        }

        private void KeypadStilus(Button gomb, Color alap, Color hover, Color lenyomva, float betumeret)
        {
            gomb.UseVisualStyleBackColor = false;
            gomb.FlatStyle = FlatStyle.Flat;
            gomb.FlatAppearance.BorderSize = 0;
            gomb.BackColor = alap;
            gomb.FlatAppearance.MouseOverBackColor = hover;
            gomb.FlatAppearance.MouseDownBackColor = lenyomva;
            gomb.ForeColor = Color.White;
            gomb.Font = new Font("Segoe UI", betumeret, FontStyle.Bold);
            gomb.TextAlign = ContentAlignment.MiddleCenter;
            gomb.Cursor = Cursors.Hand;
            LekerekitFigyelve(gomb, 10);
        }

        private void KeypadHely(Button gomb, int oszlop, int sor, int szeles, int magas)
        {
            gomb.Location = new Point(oszlop * (GombSzel + Res), sor * (GombMag + Res));
            gomb.Size = new Size(szeles * GombSzel + (szeles - 1) * Res, magas * GombMag + (magas - 1) * Res);
        }

        // ================= Felület stílusa =================

        private void FeluletStilus()
        {
            panel2.BorderStyle = BorderStyle.None;
            panel2.BackColor = HaloTeal;
            panel2.Height = pictureBox7.Bottom - panel2.Top;

            pictureBox5.BackColor = TealAlap;
            pictureBox6.BackColor = TealAlap;

            labelazonositas.Text = "AZONOSÍTÁS ÉS EGYENLEG";
            labelazonositas.BackColor = TealAlap;
            labelazonositas.Left = (panel2.ClientSize.Width - labelazonositas.Width) / 2;
            labelazonositas.Top = 3;

            panel3.BackColor = Color.White;
            panel3.Width = panel2.ClientSize.Width - 16;
            panel3.Location = new Point(8, 32);

            label1.Text = "BELÉPÉS (PIN-kód alapú)";
            label1.BackColor = TealAlap;
            label1.Left = (panel3.Width - label1.Width) / 2;
            label1.Top = 2;

            labelnev.BackColor = Color.Transparent;
            labelpin.BackColor = Color.Transparent;
            labelnev.ForeColor = KekSotet;
            labelpin.ForeColor = KekSotet;

            textBoxname.BorderStyle = BorderStyle.FixedSingle;
            textBoxname.BackColor = Color.White;
            textBoxname.Font = new Font("Segoe UI", 9.5F);
            textBoxname.Width = 113;

            textBoxpin.BorderStyle = BorderStyle.FixedSingle;
            textBoxpin.BackColor = Color.White;
            textBoxpin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            textBoxpin.TextAlign = HorizontalAlignment.Center;
            textBoxpin.Width = 113;

            int mezoX = panel3.Width - textBoxname.Width - 10;
            textBoxname.Location = new Point(mezoX, 30);
            textBoxpin.Location = new Point(mezoX, textBoxname.Bottom + 6);
            labelnev.Location = new Point(10, textBoxname.Top + (textBoxname.Height - labelnev.Height) / 2);
            labelpin.Location = new Point(10, textBoxpin.Top + (textBoxpin.Height - labelpin.Height) / 2);

            panel4.Size = new Size(4 * GombSzel + 3 * Res, 4 * GombMag + 3 * Res);
            panel4.Location = new Point((panel3.Width - panel4.Width) / 2, textBoxpin.Bottom + 12);

            KeypadHely(button7, 0, 0, 1, 1);
            KeypadHely(buttonnyoc, 1, 0, 1, 1);
            KeypadHely(buttonkilenc, 2, 0, 1, 1);
            KeypadHely(buttonnegy, 0, 1, 1, 1);
            KeypadHely(buttonot, 1, 1, 1, 1);
            KeypadHely(buttonhat, 2, 1, 1, 1);
            KeypadHely(buttonegy, 0, 2, 1, 1);
            KeypadHely(button2, 1, 2, 1, 1);
            KeypadHely(buttonharom, 2, 2, 1, 1);
            KeypadHely(button0, 0, 3, 3, 1);
            KeypadHely(button3, 3, 0, 1, 2);
            KeypadHely(buttonok, 3, 2, 1, 2);

            foreach (Button b in new[] { button0, buttonegy, button2, buttonharom, buttonnegy, buttonot, buttonhat, button7, buttonnyoc, buttonkilenc })
            {
                KeypadStilus(b, TealAlap, TealHover, TealSotet, 12F);
            }

            KeypadStilus(button3, TealSotet, TealAlap, Color.FromArgb(5, 55, 62), 14F);
            KeypadStilus(buttonok, Narancs, NarancsHover, NarancsSotet, 13F);
            button3.Text = "←";
            buttonok.Text = "OK";
            buttonpont.Visible = false;

            panel3.Height = panel4.Bottom + 12;

            panelEgyenleg = new Panel();
            panelEgyenleg.BackColor = Color.White;
            panelEgyenleg.Size = new Size(panel3.Width, 120);
            panelEgyenleg.Location = new Point(panel3.Left, panel3.Bottom + 8);

            Label egyenlegFejlec = new Label();
            egyenlegFejlec.Text = "AKTUÁLIS EGYENLEG";
            egyenlegFejlec.Dock = DockStyle.Top;
            egyenlegFejlec.Height = 22;
            egyenlegFejlec.TextAlign = ContentAlignment.MiddleCenter;
            egyenlegFejlec.BackColor = TealAlap;
            egyenlegFejlec.ForeColor = Color.White;
            egyenlegFejlec.Font = new Font("Microsoft Tai Le", 9F, FontStyle.Bold);

            labelEgyenlegErtek = new Label();
            labelEgyenlegErtek.AutoSize = false;
            labelEgyenlegErtek.TextAlign = ContentAlignment.MiddleCenter;
            labelEgyenlegErtek.BackColor = Color.Transparent;
            labelEgyenlegErtek.Location = new Point(0, 30);
            labelEgyenlegErtek.Size = new Size(panelEgyenleg.Width, 36);

            buttonFeltoltes = new Button();
            buttonFeltoltes.Text = "FELTÖLTÉS (+" + FeltoltesOsszeg + " Ft)";
            buttonFeltoltes.Size = new Size(panelEgyenleg.Width - 20, 34);
            buttonFeltoltes.Location = new Point(10, 74);
            KeypadStilus(buttonFeltoltes, TealAlap, TealHover, TealSotet, 9.5F);
            buttonFeltoltes.Click += ButtonFeltoltes_Click;

            panelEgyenleg.Controls.Add(labelEgyenlegErtek);
            panelEgyenleg.Controls.Add(buttonFeltoltes);
            panelEgyenleg.Controls.Add(egyenlegFejlec);
            panel2.Controls.Add(panelEgyenleg);

            FrissitEgyenleg();

            foreach (Button b in new[] { buttonhanko, buttonSzijjarto, buttonMeszaros, buttonRogan, buttonOrban, buttonLazar })
            {
                b.Cursor = Cursors.Hand;
                LekerekitFigyelve(b, 14);
            }

            LekerekitFigyelve(pictureBox7, 14);
            LekerekitFigyelve(panel1, 16);
            LekerekitFigyelve(panel2, 16);
            LekerekitFigyelve(panel3, 14);
            LekerekitFigyelve(panelEgyenleg, 14);
        }

        private void textBoxname_Click(object sender, EventArgs e)
        {
            if (textBoxname.Text == "[Név megadása]")
            {
                textBoxname.Clear();
                textBoxname.ForeColor = KekSotet;
            }
        }

        private void textBoxpin_Click(object sender, EventArgs e)
        {
            if (textBoxpin.Text == "****")
            {
                textBoxpin.Clear();
            }
        }
    }
}