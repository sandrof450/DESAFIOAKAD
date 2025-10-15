🛒 Desafio AKAD – Sistema de Pedidos com Microserviços

Este projeto implementa uma arquitetura de microserviços para um sistema de pedidos de compra, com autenticação, controle de estoque e comunicação via API Gateway.
Todas as requisições devem obrigatoriamente passar pelo Gateway para autenticação e roteamento entre os serviços.

🚀 Estrutura de Microserviços
Serviço	Descrição	URL Pública
🧩 API Gateway	Roteia requisições e gerencia autenticação	https://akad-gateway.onrender.com

🧮 Serviço de Vendas	Gerencia pedidos de compra	https://akad-vendas.onrender.com

📦 Serviço de Estoque	Gerencia produtos e quantidades	https://desafioakad.onrender.com

🔐 Serviço de Autenticação	Gerencia login e perfis de usuário	https://akad-authentication.onrender.com

⚠️ Todas as chamadas devem ser feitas através do Gateway.
Chamadas diretas aos microserviços não funcionarão sem autenticação via Gateway.

🧰 Pré-requisitos

Antes de realizar um pedido, siga as etapas abaixo na ordem indicada.

🧑‍💼 1. Criar um usuário administrador

Endpoint:
POST https://akad-gateway.onrender.com/api/Login/IncluirAdministrador

Body:

{
  "email": "teste@gmail.com",
  "senha": "123456",
  "perfil": "Admin"
}


Retorno esperado:
<img width="727" height="650" alt="image" src="https://github.com/user-attachments/assets/fc556eeb-47ff-4188-94be-f72e7a4be72e" />

🔑 2. Fazer login com o usuário criado

Endpoint:
POST https://akad-gateway.onrender.com/api/login

Body:

{
  "EMAIL": "teste@gmail.com",
  "SENHA": "123456"
}


Retorno esperado (contendo o token JWT):
<img width="729" height="633" alt="image" src="https://github.com/user-attachments/assets/e6c31d83-e054-461e-9300-eb3ef321f262" />

💡 Guarde o token JWT retornado — ele será necessário para as próximas requisições (autorização).

📦 3. Cadastrar um produto no estoque

Antes de realizar um pedido, o produto precisa existir no estoque.

Endpoint:
POST https://akad-gateway.onrender.com/api/produto

Body:

{
  "nomeProduto": "Mouse",
  "quantidadeDisponivel": 100,
  "preco": 500
}


Retorno esperado:
<img width="702" height="606" alt="image" src="https://github.com/user-attachments/assets/ebe2b245-e243-4b3c-8766-0b8ec3fd316e" />

🧾 4. Realizar um pedido

Com o produto já cadastrado, é possível criar um pedido.

Endpoint:
POST https://akad-gateway.onrender.com/api/pedido

Body:

{ 
  "NOMEPRODUTO": "Mouse",
  "PRECO": 500.0,
  "QUANTIDADE": 1
}


Retorno esperado:
<img width="708" height="649" alt="image" src="https://github.com/user-attachments/assets/443a08c7-dc85-4c42-842a-30f6c804a7f9" />

🔒 Autenticação

Todas as requisições (exceto criação e login) exigem JWT Token.

Inclua o token retornado no login no header Authorization das requisições:

Authorization: Bearer <seu_token_aqui>

🧩 Observações Importantes

Todas as requisições devem ser feitas via Gateway.

Requisições diretas aos microserviços (como /akad-vendas ou /akad-authentication) não funcionarão sem autenticação.

Cada microserviço está hospedado separadamente no Render e se comunica internamente via HTTP.

📚 Exemplos de URLs dos serviços
Serviço	URL
Gateway	https://akad-gateway.onrender.com

Vendas	https://akad-vendas.onrender.com

Estoque	https://desafioakad.onrender.com

Autenticação	https://akad-authentication.onrender.com
🧠 Tecnologias Utilizadas

.NET 9

Entity Framework Core (PostgreSQL)

C# ASP.NET Web API

Docker

Render Cloud Deployment
