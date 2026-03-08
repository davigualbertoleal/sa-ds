using MySql.Data.MySqlClient;
using revisao.Dados;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace sa_ds
{
    public partial class FrmEstoque : Form
    {
        private int _idUsuario;
        private string _nomeUsuario;
        private string _tipoUsuario;
        private bool _isAdmin;

        private Panel pnlContent;

        public FrmEstoque(int idUsuario, string nomeUsuario, string tipoUsuario)
        {
            InitializeComponent();

            _idUsuario = idUsuario;
            _nomeUsuario = nomeUsuario;
            _tipoUsuario = tipoUsuario;
            _isAdmin = tipoUsuario == "admin";

            lblSubtitle.Text = $"Bem-vindo(a), {nomeUsuario}  |  {(_isAdmin ? "Administrador" : "Usuário")}";

            pnlContent = new Panel();
            pnlContent.Location = new Point(245, 100);
            pnlContent.Size = new Size(645, 430);
            pnlContent.BackColor = Color.White;
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();

            dgvEstoque.Visible = false;

            btnProdutos.Click += (s, e) => CarregarProdutos();
            btnMovimentacoes.Click += (s, e) => CarregarMovimentacoes();
            btnEmprestimos.Click += (s, e) => CarregarEmprestimos();
            btnSair.Click += (s, e) => this.Close();

            CarregarProdutos();
            HighlightBtn(btnProdutos);
        }

        // ── Highlight do botão ativo na sidebar ──
        private void HighlightBtn(Button ativo)
        {
            foreach (Button b in new[] { btnProdutos, btnMovimentacoes, btnEmprestimos })
            {
                b.BackColor = Color.Transparent;
                b.ForeColor = Color.FromArgb(60, 60, 60);
            }
            ativo.BackColor = Color.FromArgb(45, 45, 45);
            ativo.ForeColor = Color.White;
        }

        // ── DataGridView com estilo padrão ──
        private DataGridView CriarGrid()
        {
            var dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 36;
            dgv.EnableHeadersVisualStyles = false;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgv.RowTemplate.Height = 30;
            return dgv;
        }

        // PRODUTOS — todos os usuários veem tudo
        private void CarregarProdutos()
        {
            HighlightBtn(btnProdutos);
            lblTitle.Text = "Produtos";
            pnlContent.Controls.Clear();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White };
            pnlTop.Controls.Add(new Label { Text = "Produtos Cadastrados", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 45, 45), Location = new Point(0, 5), AutoSize = true });
            pnlTop.Controls.Add(new Label { Text = "Visualização do inventário completo", Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray, Location = new Point(2, 33), AutoSize = true });

            var dgv = CriarGrid();
            pnlContent.Controls.Add(dgv);
            pnlContent.Controls.Add(pnlTop);

            try
            {
                using (MySqlConnection con = Db.GetConnection())
                {
                    string sql = @"SELECT p.codigoProduto AS 'Código', p.nome AS 'Nome',
                                          p.quantidade AS 'Qtd.', p.numeroPartimento AS 'Compartimento',
                                          p.estoqueMinimo AS 'Est. Mínimo',
                                          u.nome AS 'Cadastrado por'
                                   FROM produtos p
                                   LEFT JOIN usuarios u ON u.id = p.idUsuarioCadastro
                                   ORDER BY p.nome";

                    var adapter = new MySqlDataAdapter(sql, con);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgv.DataSource = dt;

                    // Destaca em vermelho produtos com estoque abaixo do mínimo
                    dgv.DataBindingComplete += (s, e) =>
                    {
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            int qtd = Convert.ToInt32(row.Cells["Qtd."].Value);
                            int min = Convert.ToInt32(row.Cells["Est. Mínimo"].Value);
                            if (qtd <= min)
                            {
                                row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 60, 60);
                                row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar produtos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MOVIMENTAÇÕES — admin vê tudo, usuário comum só as suas
        private void CarregarMovimentacoes()
        {
            HighlightBtn(btnMovimentacoes);
            lblTitle.Text = "Movimentações";
            pnlContent.Controls.Clear();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White };
            pnlTop.Controls.Add(new Label { Text = "Movimentações", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 45, 45), Location = new Point(0, 5), AutoSize = true });
            pnlTop.Controls.Add(new Label { Text = _isAdmin ? "Todas as movimentações do sistema" : "Suas movimentações", Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray, Location = new Point(2, 33), AutoSize = true });

            var dgv = CriarGrid();
            pnlContent.Controls.Add(dgv);
            pnlContent.Controls.Add(pnlTop);

            try
            {
                using (MySqlConnection con = Db.GetConnection())
                {
                    string filtroUsuario = _isAdmin ? "" : "WHERE m.idUsuario = @idUsuario";

                    string sql = $@"SELECT m.id AS 'ID',
                                          p.nome AS 'Produto',
                                          u.nome AS 'Usuário',
                                          m.tipoMovimentacao AS 'Tipo',
                                          m.quantidade AS 'Qtd.',
                                          DATE_FORMAT(m.dataMovimentacao, '%d/%m/%Y %H:%i') AS 'Data',
                                          m.observacoes AS 'Observações'
                                   FROM movimentacoes m
                                   LEFT JOIN produtos p ON p.id = m.idProduto
                                   LEFT JOIN usuarios u ON u.id = m.idUsuario
                                   {filtroUsuario}
                                   ORDER BY m.dataMovimentacao DESC";

                    var cmd = new MySqlCommand(sql, con);
                    if (!_isAdmin)
                        cmd.Parameters.AddWithValue("@idUsuario", _idUsuario);

                    var adapter = new MySqlDataAdapter(cmd);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgv.DataSource = dt;

                    // Colore cada linha pelo tipo de movimentação
                    dgv.DataBindingComplete += (s, e) =>
                    {
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            switch (row.Cells["Tipo"].Value?.ToString())
                            {
                                case "entrada": row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 130, 70); break;
                                case "saida": row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 60, 60); break;
                                case "emprestimo": row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 80, 180); break;
                                case "devolucao": row.DefaultCellStyle.ForeColor = Color.FromArgb(130, 80, 180); break;
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar movimentações: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // EMPRÉSTIMOS: admin vê tudo, usuário comum só os seus
        private void CarregarEmprestimos()
        {
            HighlightBtn(btnEmprestimos);
            lblTitle.Text = "Empréstimos";
            pnlContent.Controls.Clear();

            // PAINEL SUPERIOR: Formulário de novo empréstimo
            var pnlForm = new Panel { Dock = DockStyle.Top, Height = 200, BackColor = Color.FromArgb(248, 248, 248), Padding = new Padding(12) };

            var lblNovo = new Label { Text = "Novo Empréstimo", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 45, 45), Location = new Point(12, 10), AutoSize = true };

            // Produto
            var lblProduto = new Label { Text = "Produto:", Font = new Font("Segoe UI", 9F), Location = new Point(12, 45), AutoSize = true };
            var cbProduto = new ComboBox { Location = new Point(12, 63), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat };

            // Quantidade
            var lblQtd = new Label { Text = "Quantidade:", Font = new Font("Segoe UI", 9F), Location = new Point(285, 45), AutoSize = true };
            var nudQtd = new NumericUpDown { Location = new Point(285, 63), Width = 80, Minimum = 1, Maximum = 9999, Value = 1, Font = new Font("Segoe UI", 9F) };

            // Data de saída
            var lblSaida = new Label { Text = "Data de Saída:", Font = new Font("Segoe UI", 9F), Location = new Point(12, 100), AutoSize = true };
            var dtpSaida = new DateTimePicker { Location = new Point(12, 118), Width = 160, Font = new Font("Segoe UI", 9F), Value = DateTime.Today, Format = DateTimePickerFormat.Short };

            // Data de devolução prevista
            var lblDevolucao = new Label { Text = "Prev. Devolução:", Font = new Font("Segoe UI", 9F), Location = new Point(185, 100), AutoSize = true };
            var dtpDevolucao = new DateTimePicker { Location = new Point(185, 118), Width = 160, Font = new Font("Segoe UI", 9F), Value = DateTime.Today.AddDays(7), Format = DateTimePickerFormat.Short };

            // Descrição de uso
            var lblDesc = new Label { Text = "Descrição de uso:", Font = new Font("Segoe UI", 9F), Location = new Point(12, 152), AutoSize = true };
            var tbxDesc = new TextBox { Location = new Point(12, 170), Width = 450, Font = new Font("Segoe UI", 9F) };

            // Botão registrar
            var btnRegistrar = new Button
            {
                Text = "Registrar Empréstimo",
                Location = new Point(474, 163),
                Size = new Size(155, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegistrar.FlatAppearance.BorderSize = 0;

            pnlForm.Controls.AddRange(new Control[] { lblNovo, lblProduto, cbProduto, lblQtd, nudQtd, lblSaida, dtpSaida, lblDevolucao, dtpDevolucao, lblDesc, tbxDesc, btnRegistrar });

            // PAINEL INFERIOR: Lista de empréstimos
            var pnlLista = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 8, 0, 0) };

            var pnlFiltro = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.White };
            var lblSub = new Label { Text = _isAdmin ? "Todos os empréstimos" : "Seus empréstimos", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 45, 45), Location = new Point(0, 8), AutoSize = true };
            var lblFiltro = new Label { Text = "Status:", Font = new Font("Segoe UI", 9F), Location = new Point(300, 10), AutoSize = true };
            var cbFiltro = new ComboBox { Location = new Point(348, 7), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat };
            cbFiltro.Items.AddRange(new object[] { "Todos", "Pendentes", "Devolvidos" });
            cbFiltro.SelectedIndex = 0;
            pnlFiltro.Controls.AddRange(new Control[] { lblSub, lblFiltro, cbFiltro });

            var dgv = CriarGrid();
            pnlLista.Controls.Add(dgv);
            pnlLista.Controls.Add(pnlFiltro);

            pnlContent.Controls.Add(pnlLista);
            pnlContent.Controls.Add(pnlForm);

            // Carrega produtos no ComboBox
            try
            {
                using (MySqlConnection con = Db.GetConnection())
                {
                    string sql = "SELECT id, CONCAT(codigoProduto, ' — ', nome, ' (', quantidade, ' em estoque)') AS descricao FROM produtos WHERE quantidade > 0 ORDER BY nome";
                    var cmd = new MySqlCommand(sql, con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cbProduto.Items.Add(new { Id = reader.GetInt32("id"), Texto = reader.GetString("descricao") });
                    }
                    cbProduto.DisplayMember = "Texto";
                    cbProduto.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar produtos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Carrega a lista de empréstimos
            Action<string> carregarLista = (filtro) =>
            {
                try
                {
                    using (MySqlConnection con = Db.GetConnection())
                    {
                        string whereUsuario = _isAdmin ? "" : "AND e.idUsuario = @idUsuario";
                        string whereStatus = filtro == "Pendentes" ? "AND e.statusPendente = 1"
                                           : filtro == "Devolvidos" ? "AND e.statusDevolvido = 1"
                                           : "";

                        string sql = $@"SELECT e.id AS 'ID',
                                      u.nome AS 'Usuário',
                                      p.nome AS 'Produto',
                                      e.quantidade AS 'Qtd.',
                                      DATE_FORMAT(e.dataSaida, '%d/%m/%Y') AS 'Saída',
                                      DATE_FORMAT(e.dataDevolucaoPrevista, '%d/%m/%Y') AS 'Prev. Devolução',
                                      DATE_FORMAT(e.dataDevolucaoReal, '%d/%m/%Y') AS 'Dev. Real',
                                      CASE WHEN e.statusDevolvido = 1 THEN 'Devolvido'
                                           WHEN e.statusPendente  = 1 THEN 'Pendente'
                                           ELSE '-' END AS 'Status',
                                      e.observacoes AS 'Observações'
                               FROM emprestimos e
                               LEFT JOIN usuarios u ON u.id = e.idUsuario
                               LEFT JOIN produtos p ON p.id = e.idProduto
                               WHERE 1=1 {whereUsuario} {whereStatus}
                               ORDER BY e.dataSaida DESC";

                        var cmd = new MySqlCommand(sql, con);
                        if (!_isAdmin)
                            cmd.Parameters.AddWithValue("@idUsuario", _idUsuario);

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgv.DataSource = dt;

                        dgv.DataBindingComplete += (s, e) =>
                        {
                            foreach (DataGridViewRow row in dgv.Rows)
                            {
                                string status = row.Cells["Status"].Value?.ToString() ?? "";
                                if (status == "Devolvido")
                                {
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 130, 70);
                                }
                                else if (status == "Pendente")
                                {
                                    bool atrasado = DateTime.TryParseExact(
                                        row.Cells["Prev. Devolução"].Value?.ToString(), "dd/MM/yyyy",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out DateTime prev)
                                        && prev < DateTime.Today;

                                    row.DefaultCellStyle.ForeColor = atrasado ? Color.FromArgb(180, 60, 60) : Color.FromArgb(180, 120, 0);
                                    if (atrasado)
                                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                                }
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao carregar empréstimos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            cbFiltro.SelectedIndexChanged += (s, e) => carregarLista(cbFiltro.SelectedItem.ToString());
            carregarLista("Todos");

            // Registra novo empréstimo
            btnRegistrar.Click += (s, e) =>
            {
                if (cbProduto.SelectedItem == null)
                {
                    MessageBox.Show("Selecione um produto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (dtpDevolucao.Value.Date < dtpSaida.Value.Date)
                {
                    MessageBox.Show("A data de devolução não pode ser anterior à data de saída.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dynamic produtoSelecionado = cbProduto.SelectedItem;
                int idProduto = produtoSelecionado.Id;
                int quantidade = (int)nudQtd.Value;

                try
                {
                    using (MySqlConnection con = Db.GetConnection())
                    {
                        // Verifica estoque disponível
                        var cmdEstoque = new MySqlCommand("SELECT quantidade FROM produtos WHERE id = @id", con);
                        cmdEstoque.Parameters.AddWithValue("@id", idProduto);
                        int estoqueAtual = Convert.ToInt32(cmdEstoque.ExecuteScalar());

                        if (quantidade > estoqueAtual)
                        {
                            MessageBox.Show($"Estoque insuficiente. Disponível: {estoqueAtual}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Insere o empréstimo
                        string sqlEmprestimo = @"INSERT INTO emprestimos (idUsuario, idProduto, quantidade, dataSaida, dataDevolucaoPrevista, statusPendente, statusDevolvido, observacoes)
                                         VALUES (@idUsuario, @idProduto, @quantidade, @dataSaida, @dataDevolucao, 1, 0, @obs)";
                        var cmdEmp = new MySqlCommand(sqlEmprestimo, con);
                        cmdEmp.Parameters.AddWithValue("@idUsuario", _idUsuario);
                        cmdEmp.Parameters.AddWithValue("@idProduto", idProduto);
                        cmdEmp.Parameters.AddWithValue("@quantidade", quantidade);
                        cmdEmp.Parameters.AddWithValue("@dataSaida", dtpSaida.Value.Date);
                        cmdEmp.Parameters.AddWithValue("@dataDevolucao", dtpDevolucao.Value.Date);
                        cmdEmp.Parameters.AddWithValue("@obs", tbxDesc.Text.Trim());
                        cmdEmp.ExecuteNonQuery();

                        // Baixa o estoque do produto
                        var cmdUpdate = new MySqlCommand("UPDATE produtos SET quantidade = quantidade - @qtd WHERE id = @id", con);
                        cmdUpdate.Parameters.AddWithValue("@qtd", quantidade);
                        cmdUpdate.Parameters.AddWithValue("@id", idProduto);
                        cmdUpdate.ExecuteNonQuery();

                        // Registra a movimentação
                        string sqlMov = @"INSERT INTO movimentacoes (idProduto, idUsuario, tipoMovimentacao, quantidade, observacoes)
                                  VALUES (@idProduto, @idUsuario, 'emprestimo', @quantidade, @obs)";
                        var cmdMov = new MySqlCommand(sqlMov, con);
                        cmdMov.Parameters.AddWithValue("@idProduto", idProduto);
                        cmdMov.Parameters.AddWithValue("@idUsuario", _idUsuario);
                        cmdMov.Parameters.AddWithValue("@quantidade", quantidade);
                        cmdMov.Parameters.AddWithValue("@obs", tbxDesc.Text.Trim());
                        cmdMov.ExecuteNonQuery();

                        MessageBox.Show("Empréstimo registrado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Limpa o formulário e recarrega a lista
                        cbProduto.SelectedIndex = -1;
                        nudQtd.Value = 1;
                        dtpSaida.Value = DateTime.Today;
                        dtpDevolucao.Value = DateTime.Today.AddDays(7);
                        tbxDesc.Clear();
                        carregarLista(cbFiltro.SelectedItem.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao registrar empréstimo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }
    }
}