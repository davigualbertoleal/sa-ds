using MySql.Data.MySqlClient;
using revisao.Dados;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    // Voltamos a buscar pela coluna EMAIL
                    string query = "SELECT nome, tipo FROM usuarios WHERE email = @email AND senha = @senha AND status = 'ativo'";

                    using (MySqlCommand comando = new MySqlCommand(query, conexao))
                    {
                        // Voltamos o parâmetro para @email, mas mantemos o .Trim() para evitar o erro do espaço fantasma
                        comando.Parameters.AddWithValue("@email", tbxUsuario.Text.Trim());
                        comando.Parameters.AddWithValue("@senha", tbxSenha.Text.Trim());

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                string nomeUsuario = leitor.GetString("nome");

                                MessageBox.Show($"Acesso liberado. Bem-vindo(a), {nomeUsuario}.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                this.Hide();
                            }
                            else
                            {
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
    }
}