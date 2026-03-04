-- Criação do Banco de Dados
CREATE DATABASE IF NOT EXISTS controleEstoque;
USE controleEstoque;

-- 1. Tabela de Usuários
CREATE TABLE usuarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nome VARCHAR(100) NOT NULL,
  email VARCHAR(100) UNIQUE NOT NULL,
  senha VARCHAR(255) NOT NULL,
  tipo ENUM('admin','usuario') DEFAULT 'usuario',
  status ENUM('ativo','inativo') DEFAULT 'ativo'
);

-- 2. Tabela de Tentativas de Login
CREATE TABLE tentativasLogin (
  id INT AUTO_INCREMENT PRIMARY KEY,
  usuarioId INT DEFAULT NULL,
  dataTentativa DATETIME DEFAULT CURRENT_TIMESTAMP,
  sucesso TINYINT(1) DEFAULT NULL,
  FOREIGN KEY (usuarioId) REFERENCES usuarios(id)
);

-- 3. Tabela de Produtos 
CREATE TABLE produtos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  codigoProduto VARCHAR(255) UNIQUE NOT NULL,
  nome VARCHAR(255) NOT NULL,
  quantidade INT DEFAULT 0,
  numeroPartimento VARCHAR(255) DEFAULT NULL,
  estoqueMinimo INT DEFAULT 0,
  idUsuarioCadastro INT DEFAULT NULL
);

-- 4. Tabela de Empréstimos
CREATE TABLE emprestimos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  idUsuario INT DEFAULT NULL,
  idProduto INT DEFAULT NULL,
  quantidade INT NOT NULL,
  dataSaida DATETIME DEFAULT CURRENT_TIMESTAMP,
  dataDevolucaoPrevista DATE DEFAULT NULL,
  dataDevolucaoReal DATE DEFAULT NULL,
  statusPendente TINYINT(1) DEFAULT 1,
  statusDevolvido TINYINT(1) DEFAULT 0,
  observacoes TEXT DEFAULT NULL,
  FOREIGN KEY (idUsuario) REFERENCES usuarios(id),
  FOREIGN KEY (idProduto) REFERENCES produtos(id)
);

-- 5. Tabela de Movimentações
CREATE TABLE movimentacoes (
  id INT AUTO_INCREMENT PRIMARY KEY,
  idProduto INT DEFAULT NULL,
  idUsuario INT DEFAULT NULL,
  tipoMovimentacao ENUM('entrada','saida','emprestimo','devolucao') DEFAULT NULL,
  dataMovimentacao DATETIME DEFAULT CURRENT_TIMESTAMP,
  quantidade INT DEFAULT NULL,
  observacoes TEXT DEFAULT NULL,
  FOREIGN KEY (idUsuario) REFERENCES usuarios(id),
  FOREIGN KEY (idProduto) REFERENCES produtos(id)
);

-- 6. Tabela de Relatórios
CREATE TABLE relatorios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  tipoFiltro ENUM('entrada','saida','emprestimo','devolucao') DEFAULT NULL,
  dataGeracao DATETIME DEFAULT CURRENT_TIMESTAMP,
  nm VARCHAR(255) DEFAULT NULL
);

-- Inserindo uns usuários genéricos só para o idUsuarioCadastro não dar erro (caso precise)
INSERT INTO usuarios (nome, email, senha, tipo, status) VALUES 
('Admin', 'admin@empresa.com', '@dmin123', 'admin', 'ativo'),
('Tiago Souza Andrade', 'titi@empresa.com', 'titi123', 'usuario', 'ativo');

-- Inserindo os produtos com o padrão camelCase
INSERT INTO produtos (codigoProduto, nome, quantidade, numeroPartimento, estoqueMinimo, idUsuarioCadastro) VALUES
('PRD-001', 'Parafuso Allen 3mm', 260, 'A1', 50, 1),
('PRD-002', 'Porca Sextavada 3mm', 200, 'A1', 40, 1),
('PRD-003', 'Arruela Lisa de Pressão', 300, 'A2', 60, 1),
('PRD-004', 'Chave Phillips Média', 25, 'B1', 5, 2),
('PRD-005', 'Martelo de Borracha', 15, 'B2', 3, 2),
('PRD-006', 'Alicate Universal PRO', 34, 'B3', 5, 1),
('PRD-007', 'Fita Isolante 3M (Rolo)', 120, 'C1', 20, 2),
('PRD-008', 'Cabo Flexível 1,5mm (Rolo 100m)', 80, 'C2', 15, 1),
('PRD-009', 'Cabo Flexível 2,5mm (Rolo 100m)', 61, 'C2', 10, 1),
('PRD-010', 'Tomada Dupla 10A Padrão BR', 45, 'D1', 10, 2),
('LULA-13', 'Lulacate (Alicate de Bico Fino)', 80, 'A4', 20, 1);