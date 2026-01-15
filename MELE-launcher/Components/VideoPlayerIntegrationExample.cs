using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Example showing how to integrate the IntroPlayer with embedded video support.
    /// This demonstrates both fullscreen and embedded video playback modes.
    /// </summary>
    public partial class VideoPlayerIntegrationExample : Form
    {
        private IntroPlayer _introPlayer;
        private Panel _videoPanel;
        private Button _playFullscreenButton;
        private Button _playEmbeddedButton;
        private Button _stopButton;
        private TextBox _gamePathTextBox;
        private Label _statusLabel;

        public VideoPlayerIntegrationExample()
        {
            InitializeComponent();
            _introPlayer = new IntroPlayer();
        }

        private void InitializeComponent()
        {
            this.Text = "Video Player Integration Example";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Game path input
            var gamePathLabel = new Label
            {
                Text = "Game Path:",
                Location = new Point(10, 10),
                Size = new Size(100, 23)
            };
            this.Controls.Add(gamePathLabel);

            _gamePathTextBox = new TextBox
            {
                Location = new Point(120, 10),
                Size = new Size(400, 23),
                Text = @"C:\Program Files (x86)\Steam\steamapps\common\Mass Effect Legendary Edition"
            };
            this.Controls.Add(_gamePathTextBox);

            // Buttons
            _playFullscreenButton = new Button
            {
                Text = "Play Fullscreen",
                Location = new Point(10, 50),
                Size = new Size(120, 30)
            };
            _playFullscreenButton.Click += PlayFullscreenButton_Click;
            this.Controls.Add(_playFullscreenButton);

            _playEmbeddedButton = new Button
            {
                Text = "Play Embedded",
                Location = new Point(140, 50),
                Size = new Size(120, 30)
            };
            _playEmbeddedButton.Click += PlayEmbeddedButton_Click;
            this.Controls.Add(_playEmbeddedButton);

            _stopButton = new Button
            {
                Text = "Stop",
                Location = new Point(270, 50),
                Size = new Size(80, 30),
                Enabled = false
            };
            _stopButton.Click += StopButton_Click;
            this.Controls.Add(_stopButton);

            // Status label
            _statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(10, 90),
                Size = new Size(500, 23),
                ForeColor = Color.Blue
            };
            this.Controls.Add(_statusLabel);

            // Video panel for embedded playback
            _videoPanel = new Panel
            {
                Location = new Point(10, 120),
                Size = new Size(760, 430),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(_videoPanel);

            // Add instructions label
            var instructionsLabel = new Label
            {
                Text = "Instructions:\n" +
                       "• Play Fullscreen: Opens video in fullscreen mode\n" +
                       "• Play Embedded: Embeds video player in the black panel below\n" +
                       "• Press ESC during playback to skip the intro\n" +
                       "• Requires RAD Video Tools installed or will attempt to download",
                Location = new Point(530, 10),
                Size = new Size(250, 100),
                ForeColor = Color.DarkGreen
            };
            this.Controls.Add(instructionsLabel);
        }

        private async void PlayFullscreenButton_Click(object sender, EventArgs e)
        {
            await PlayVideoAsync(embedded: false);
        }

        private async void PlayEmbeddedButton_Click(object sender, EventArgs e)
        {
            await PlayVideoAsync(embedded: true);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _introPlayer.StopIntro();
            UpdateUI(playing: false);
            _statusLabel.Text = "Stopped";
            _statusLabel.ForeColor = Color.Red;
        }

        private async Task PlayVideoAsync(bool embedded)
        {
            try
            {
                UpdateUI(playing: true);
                _statusLabel.Text = embedded ? "Playing embedded video..." : "Playing fullscreen video...";
                _statusLabel.ForeColor = Color.Green;

                string gamePath = _gamePathTextBox.Text.Trim();
                if (string.IsNullOrEmpty(gamePath))
                {
                    MessageBox.Show("Please enter a valid game path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateUI(playing: false);
                    return;
                }

                Control parentControl = embedded ? _videoPanel : null;
                bool success = await _introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true, parentControl);

                if (success)
                {
                    _statusLabel.Text = "Video completed successfully";
                    _statusLabel.ForeColor = Color.Blue;
                }
                else
                {
                    _statusLabel.Text = "Video playback failed";
                    _statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
                MessageBox.Show($"Error playing video: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateUI(playing: false);
            }
        }

        private void UpdateUI(bool playing)
        {
            _playFullscreenButton.Enabled = !playing;
            _playEmbeddedButton.Enabled = !playing;
            _stopButton.Enabled = playing;
            _gamePathTextBox.Enabled = !playing;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Ensure video player is stopped when form closes
            _introPlayer?.StopIntro();
            base.OnFormClosed(e);
        }
    }

    /// <summary>
    /// Program entry point for testing the video player integration.
    /// </summary>
    public static class VideoPlayerTestProgram
    {
        [STAThread]
        public static void RunExample()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VideoPlayerIntegrationExample());
        }
    }
}