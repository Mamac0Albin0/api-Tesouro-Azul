
<h1 align="center">Tesouro Azul API</h1>

<p align="center">
  <em>API de gerenciamento de estoque desenvolvida em C# (.NET 8) e MySQL.</em>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/eaacb096-6b22-462a-800d-ca9d0f0f4a36" alt="Tesouro Azul" width="300"/>
</p>

---

##  Sobre o projeto

A **Tesouro Azul API** é meu primeiro grande projeto back-end, focado em oferecer uma solução para gerenciamento de estoque com funcionalidades administrativas. Ele foi desenvolvido como parte do meu aprendizado em desenvolvimento com **C# (.NET 8)** e **MySQL**.

---

##  Funcionalidades atuais

- ✅ Controle de produtos no estoque (registrar, excluir, desativar)
- ✅ Gerenciamento básico de usuários

---

## 🛠️ Tecnologias e dependências

### Tecnologias principais:
- [C# (.NET 8)](https://dotnet.microsoft.com/en-us/download)
- [MySQL Server 8.x+](https://dev.mysql.com/downloads/mysql/)

### Principais dependências NuGet:
- `Microsoft.EntityFrameworkCore`
- `Pomelo.EntityFrameworkCore.MySql`
- `Swashbuckle.AspNetCore` (Swagger)

---

## 📌 Requisitos de ambiente

- .NET SDK 8.0 instalado
- Servidor MySQL (recomenda-se versão 8 ou superior)

---

## ⚙️ Como usar

1. **Banco de Dados**:
   - Utilize os scripts SQL disponíveis na pasta `Scripts` do repositório para criar o banco.

2. **Configuração do Projeto**:
   - Altere os arquivos `appsettings.json` e `appsettings.Development.json` com as informações do seu banco de dados.
   - Se estiver integrando com uma aplicação web, edite o `Program.cs` conforme necessário.

3. **Rodando o projeto**:
   - Execute o comando:
     ```bash
     dotnet run
     ```
   - A API será inicializada e você poderá acessá-la via Swagger em `http://localhost:{porta}/swagger`.

---

## 🔮 Futuras implementações

-  Controle e metas de vendas
-  Exportação para Excel
-  Monitoramento de validade de produtos
-  Comandos automáticos de rotina no back-end
-  Integração com envio de e-mails

---

## 📚 Mais informações

Esse projeto é parte do sistema **Tesouro Azul**, um gerenciador de estoque com interface amigável e voltado para facilitar o controle de produtos e finanças em pequenos negócios.

🔗 Repositório principal: [Planejamento e versões do TCC](https://github.com/BEaew0/Versoes-pro-TCC-planejamento-)

---

## 🤝 Contribuição

Sinta-se à vontade para abrir **Issues** ou enviar **Pull Requests** com sugestões, correções ou melhorias!

---

## 🤡💻 Autor

**Miguel Fonseca**  
Desenvolvedor Back-End | Tecnico de Desenvolvimento de Sistemas

[GitHub](https://github.com/Mamac0Albin0)
