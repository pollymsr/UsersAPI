# UsersAPI — FIAP Cloud Games (Fase 4 - Tech Challenge)

Microsserviço responsável pela Gestão de Usuários, Autenticação e Emissão de Tokens JWT da plataforma **FIAP Cloud Games (FCG)**.

---

## 🚀 Entregas e Arquitetura da Fase 4

1. **Persistência Relacional (Poliglota no Ecossistema):**
   - Utiliza Entity Framework Core + SQLite para dados transacionais e estruturados de contas de usuários e credenciais (hash com `BCrypt.Net-Next`), compondo a arquitetura de persistência poliglota em conjunto com o MongoDB/Redis/Elasticsearch da `CatalogAPI`.

2. **Automação de CI/CD e GitOps (GitHub Actions + AWS ECR/EKS):**
   - Pipeline completa em [`.github/workflows/ci-cd.yml`](./.github/workflows/ci-cd.yml):
     - **Build & Test:** Compilação .NET 8 e execução automatizada de testes unitários (`UsersAPI.Tests` com `xUnit` e `Moq`).
     - **Containerização:** Build da imagem Docker versionada com tags `${GITHUB_SHA}` e `latest`.
     - **Security Scan:** Análise de vulnerabilidades da imagem com **Trivy** (`aquasecurity/trivy-action`).
     - **Push & Deploy:** Publicação no **Amazon ECR** e deploy com **Rolling Update** (`maxUnavailable: 0`) no cluster **Amazon EKS**.

3. **Segurança e Gestão de Segredos (Zero Hardcoded Credentials):**
   - Nenhuma chave JWT, senha de seed, string de conexão ou credencial do RabbitMQ está fixa no código-fonte, `appsettings.json` ou arquivos YAML.
   - Todas as variáveis sensíveis (`JWT_SECRET_KEY`, `ConnectionStrings__DefaultConnection`, `RabbitMQ__Username`, `RabbitMQ__Password`, `SEED_ADMIN_PASSWORD`, `SEED_USER_PASSWORD`) são injetadas em tempo de execução via **Kubernetes Secrets** (`app-secrets`) / **AWS Secrets Manager**.

4. **Observabilidade:**
   - Instrumentação nativa com **OpenTelemetry** expondo métricas HTTP e Kestrel no endpoint `/metrics` para coleta pelo Prometheus e visualização no Grafana.

---

## 📌 Principais Endpoints

| Método | Rota | Autenticação | Descrição |
| :--- | :--- | :---: | :--- |
| `POST` | `/api/auth/register` | Público | Registra novo usuário e publica `UserCreatedEvent` |
| `POST` | `/api/auth/login` | Público | Autentica usuário e retorna token JWT assinado (HS256) |
| `GET` | `/api/users` | Admin (JWT) | Lista todos os usuários cadastrados |
| `GET` | `/api/users/{id}` | Autenticado | Obtém detalhes de um usuário por ID |
| `PUT` | `/api/users/{id}` | Autenticado | Atualiza dados cadastrais do usuário |
| `DELETE` | `/api/users/{id}` | Admin (JWT) | Remove um usuário |
| `PUT` | `/api/users/{id}/role` | Admin (JWT) | Altera perfil de acesso (`User` / `Admin`) |
| `GET` | `/metrics` | Público | Métricas OpenTelemetry para o Prometheus |

---

## 🧪 Executando os Testes Unitários

```bash
dotnet test UsersAPI.sln -c Release --verbosity normal
```
