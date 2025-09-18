# Deploy Esportify to Render

## Pré-requisitos

1. Conta no [Render.com](https://render.com)
2. Repositório no GitHub com o código da aplicação
3. Aplicação Esportify preparada com as configurações de produção

## Passo a Passo para Deploy

### 1. Preparar o Repositório GitHub

```bash
# Inicializar git (se ainda não estiver)
git init

# Adicionar todos os arquivos
git add .

# Commit das mudanças
git commit -m "Prepare Esportify for Render deployment"

# Adicionar origem remota (substitua pela sua URL do repositório)
git remote add origin https://github.com/SEU-USERNAME/esportify.git

# Push para GitHub
git push -u origin main
```

### 2. Criar Base de Dados PostgreSQL no Render

1. Aceda a [render.com](https://render.com) e faça login
2. Clique em **"New +"** → **"PostgreSQL"**
3. Configure:
   - **Name**: `esportify-db`
   - **Database**: `esportify`
   - **User**: `esportify`
   - **Region**: `Frankfurt` (ou `Oregon`)
   - **PostgreSQL Version**: `16`
   - **Plan**: `Free`
4. Clique **"Create Database"**
5. **Guardar a DATABASE_URL** que aparece nos detalhes da base de dados

### 3. Criar Web Service no Render

1. Clique em **"New +"** → **"Web Service"**
2. Conecte o seu repositório GitHub
3. Configure:
   - **Name**: `esportify-web`
   - **Region**: `Frankfurt` (mesmo da BD)
   - **Branch**: `main`
   - **Runtime**: `Docker`
   - **Build Command**: (deixar vazio)
   - **Start Command**: (deixar vazio)
   - **Plan**: `Free`

### 4. Configurar Variáveis de Ambiente

No Web Service criado, vá à aba **"Environment"** e adicione:

```
DATABASE_URL=postgresql://[copiar da base de dados criada]
ASPNETCORE_ENVIRONMENT=Production
INITIALIZE_DATABASE=true
```

**Nota**: `INITIALIZE_DATABASE=true` só é necessário no primeiro deploy para criar as tabelas e dados iniciais. Pode ser removido após o primeiro deploy bem-sucedido.

### 5. Deploy Automático

1. Clique **"Create Web Service"**
2. O Render irá automaticamente:
   - Fazer build da aplicação usando o Dockerfile
   - Configurar a base de dados PostgreSQL
   - Fazer deploy da aplicação

### 6. Verificar Deploy

1. Aguarde o build completar (5-10 minutos)
2. Aceda ao URL fornecido pelo Render (ex: `https://esportify-web.onrender.com`)
3. A aplicação deve estar online e funcionando!

## Funcionalidades em Produção

✅ **ASP.NET Core 8.0** executando em contentor Docker
✅ **PostgreSQL** como base de dados
✅ **Upload de imagens** (equipas, utilizadores, torneios)
✅ **Autenticação** com cookies seguros
✅ **SSL/HTTPS** automático fornecido pelo Render
✅ **Domínio personalizado** disponível (planos pagos)

## Monitorização

- **Logs**: Disponíveis no dashboard do Render
- **Métricas**: CPU, memória, requests no dashboard
- **Alertas**: Configuráveis para downtime

## Custos

- **Free Tier**: 
  - 750 horas/mês
  - Aplicação "hiberna" após 15 min de inatividade
  - Base de dados PostgreSQL 1GB free
- **Upgrade**: $7/mês para manter aplicação sempre ativa

## Troubleshooting

### Build Errors
- Verifique se o Dockerfile está correto
- Confirme se todas as dependências estão no .csproj

### Database Connection
- Verifique se DATABASE_URL está correta
- Confirme se PostgreSQL package está instalado

### Application Not Starting
- Verifique logs no dashboard do Render
- Confirme se ASPNETCORE_URLS está configurado corretamente

## URLs Importantes

- **Aplicação**: `https://[your-service-name].onrender.com`
- **Dashboard**: `https://dashboard.render.com`
- **Documentação**: `https://render.com/docs`