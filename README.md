# UsersAPI (Fase 3 - Tech Challenge)

Este repositório contém o microsserviço de Gestão de Usuários e Autenticação do ecossistema FiapCloudGames.

## 🚀 Novidades da Fase 3
- **Integração com Kong API Gateway:** A API agora opera sob o guarda-chuva do Kong. As rotas são expostas de forma unificada e a emissão de JWT foi otimizada para trabalhar com a segurança de borda.
- **Observabilidade (OpenTelemetry):** O código foi totalmente instrumentado com OpenTelemetry em C# (.NET 8). A API agora expõe dados de saúde, consumo de CPU/Memória (Kestrel) e latência através da rota /metrics para consumo do **Prometheus** e **Grafana**.

## Execução Local
A orquestração completa dos serviços de infraestrutura (bancos, mensageria e observabilidade) está no repositório FiapCloudGames-Infra.
