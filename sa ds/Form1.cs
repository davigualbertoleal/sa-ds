using MySql.Data.MySqlClient;
using revisao.Dados;
using System;
using System.Windows.Forms;

namespace sa_ds
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxUsuario.Text) || string.IsNullOrWhiteSpace(tbxSenha.Text))
            {
                MessageBox.Show("Por favor, preencha o e-mail e a senha.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conexao = Db.GetConnection())
                {
                    string query = "SELECT id, nome, tipo FROM usuarios WHERE email = @email AND senha = @senha AND status = 'ativo'";

                    using (MySqlCommand comando = new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("@email", tbxUsuario.Text.Trim());
                        comando.Parameters.AddWithValue("@senha", tbxSenha.Text.Trim());

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                int idUsuario = leitor.GetInt32("id");
                                string nomeUsuario = leitor.GetString("nome");
                                string tipoUsuario = leitor.GetString("tipo");

                                leitor.Close();
                                RegistrarTentativa(conexao, idUsuario, true);

                                FrmEstoque frmEstoque = new FrmEstoque(idUsuario, nomeUsuario, tipoUsuario);
                                this.Hide();
                                frmEstoque.FormClosed += (s, args) => this.Close();
                                frmEstoque.Show();
                            }
                            else
                            {
                                leitor.Close();
                                int? idTentativa = BuscarIdPorEmail(conexao, tbxUsuario.Text.Trim());
                                RegistrarTentativa(conexao, idTentativa, false);

                                MessageBox.Show("E-mail ou Senha incorretos, ou usuário inativo.", "Acesso Negado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                tbxSenha.Clear();
                                tbxUsuario.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao se conectar ao banco de dados: " + ex.Message, "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RegistrarTentativa(MySqlConnection conexao, int? idUsuario, bool sucesso)
        {
            try
            {
                string sql = "INSERT INTO tentativasLogin (usuarioId, sucesso) VALUES (@id, @sucesso)";
                using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
                {
                    if (idUsuario.HasValue)
                        cmd.Parameters.AddWithValue("@id", idUsuario.Value);
                    else
                        cmd.Parameters.AddWithValue("@id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sucesso", sucesso ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private int? BuscarIdPorEmail(MySqlConnection conexao, string email)
        {
            try
            {
                string sql = "SELECT id FROM usuarios WHERE email = @email LIMIT 1";
                using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    object result = cmd.ExecuteScalar();
                    if (result != null) return Convert.ToInt32(result);
                }
            }
            catch { }
            return null;
        }
    }
}