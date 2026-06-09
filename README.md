# 👤 UsersAPI

Este microsserviço é responsável pela **Autenticação e Gestão de Usuários** dentro do ecossistema Fiap Cloud Games (FCG).

Foi extraído do projeto monolítico original durante a Fase 2 do Tech Challenge para adotar uma arquitetura escalável orientada a eventos.

## Funcionalidades
- **Autenticação:** Geração de Tokens JWT para usuários e administradores.
- **Gestão:** Cadastro, atualização e deleção de usuários.
- **Eventos:** Publica o evento `UserCreatedEvent` no RabbitMQ quando um novo usuário se registra com sucesso, notificando outros serviços interessados.

## Tecnologias Utilizadas
- C# .NET 8 Web API
- Entity Framework Core (SQLite)
- Autenticação via JWT (JSON Web Tokens)
- MassTransit + RabbitMQ
- Docker (Multi-stage build)

## Como Executar
O ideal é executar todos os microsserviços juntos pelo Orquestrador, mas se precisar rodar este microsserviço isoladamente:

1. Certifique-se de que possui uma instância local do RabbitMQ rodando na porta padrão (ou configure no `appsettings.json`).
2. Execute o projeto usando a CLI do .NET:
```bash
dotnet build
dotnet run
```
3. Acesse o Swagger gerado para explorar os endpoints da API.
