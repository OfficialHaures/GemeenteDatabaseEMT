using MySql.Data.MySqlClient;

public class LoginForm : Form
{
    private TextBox txtUsername;
    private TextBox txtPassword;
    private Button btnLogin;
    private string connectionString = "Server=185.228.82.169;Database=s25_Gemeente;Uid=u25_DGGe8xteoX;Pwd=V5J9iLo7BWBP^lHjpXIma@SP;Port=3306;SslMode=none;";

    public LoginForm()
    {
        this.Size = new Size(400, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Gemeente Data Login";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var title = new Label
        {
            Text = "🔐 Login",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            Location = new Point(30, 30),
            AutoSize = true
        };

        txtUsername = new TextBox
        {
            Location = new Point(30, 100),
            Width = 320,
            Height = 35,
            Font = new Font("Segoe UI", 12F),
            PlaceholderText = "Gebruikersnaam"
        };

        txtPassword = new TextBox
        {
            Location = new Point(30, 150),
            Width = 320,
            Height = 35,
            Font = new Font("Segoe UI", 12F),
            PlaceholderText = "Wachtwoord",
            PasswordChar = '•'
        };

        btnLogin = new Button
        {
            Text = "Inloggen",
            Location = new Point(30, 200),
            Width = 320,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        btnLogin.Click += BtnLogin_Click;

        this.Controls.AddRange(new Control[] { title, txtUsername, txtPassword, btnLogin });
    }

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT role FROM users WHERE username = @username AND password = @password", 
                    conn
                );
                cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@password", txtPassword.Text);

                var role = cmd.ExecuteScalar()?.ToString();

                if (role != null)
                {
                    this.Hide();
                    new Form1(role).ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ongeldige inloggegevens!", "Login Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login error: {ex.Message}", "Database Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
