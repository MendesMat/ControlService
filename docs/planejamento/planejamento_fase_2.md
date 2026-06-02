# Planejamento: Fase 2 (Tático e Comportamento) - Itens 2.3 e 2.4

## Visão Geral
Atendendo aos requisitos 2.3 e 2.4 mapeados no arquivo `task.md`, este planejamento descreve a implementação de um **Domain Service** e do padrão **Specification** no contexto do módulo Comercial. 

Após a varredura no backend (`ControlService.Commercial`), constatamos que não há código para estes padrões ainda, e que o agregado de `Order` não existe no momento. Para não desviar o foco da tarefa apenas para criar um agregado novo inteiro, focaremos o Domain Service e a Specification no agregado **`Customer`**, que é a base do nosso Core Domain e já está implementado.

---

## Fase 2.4: Implementação do Padrão Specification / Policy

### Objetivo
O padrão *Specification* será utilizado para encapsular regras de negócio complexas de validação, isolando-as das entidades e permitindo reutilização limpa.

### Proposta de Implementação
**`CustomerEligibleForServiceSpecification`**
- **Onde:** `ControlService.Commercial.Domain.Customers.Specifications`
- **Contrato Base:** Criação da interface genérica `ISpecification<T>` no diretório `SharedKernel/SeedWork` com o método `IsSatisfiedBy(T entity)`.
- **Regra de Negócio (Exemplo):** Um cliente só é elegível para ter um serviço agendado se satisfizer regras comerciais rígidas:
  - Possuir um `Document` (CPF/CNPJ) válido cadastrado.
- **Por que usar Specification?** 
Essa regra blindará a geração de Ordens de Serviço (futuras) e a aceitação de propostas comerciais. Isolar essa regra complexa em uma Specification permite que ela seja testada unitariamente de forma isolada e reaproveitada em diversos fluxos de uso (Application Services) sem sujar o agregado com regras que variam muito.

---

## Fase 2.3: Implementação de um Domain Service

### Objetivo
Criar um serviço de domínio puro para orquestrar comportamentos que requerem acesso a estado fora do escopo de um único agregado (por exemplo, verificações no banco de dados) e que, portanto, não cabem dentro do próprio Agregado `Customer` (já que uma entidade não pode injetar repositórios).

### Proposta de Implementação
**`CustomerUniquenessDomainService`** (Serviço de Domínio de Unicidade de Documento)
- **Onde:** `ControlService.Commercial.Domain.Customers.Services`
- **Regra de Negócio:** Garantir que o `Document` (CPF/CNPJ) de um novo cliente seja **único** em toda a base de dados. Um CPF/CNPJ duplicado fere a integridade do CRM e do faturamento.
- **Como Funciona:** 
O Agregado `Customer` não tem como saber se o seu CPF/CNPJ já existe na base de dados. O Domain Service fará essa ponte: ele recebe o `ICustomerRepository` em seu construtor e possui um método assíncrono `VerifyUniquenessAsync(Customer newCustomer)`. Caso exista duplicação, ele lança uma `DomainException`.
- **Uso no Fluxo:** 
Será invocado pela camada de *Application* (durante o *Command* de criação ou atualização de cliente) antes de persistir o cliente.

---

## Próximos Passos
Verifique este planejamento. Se o escopo dessas duas classes fizer sentido para o contexto do ERP e atender aos requisitos acadêmicos da tarefa 2.3 e 2.4, podemos prosseguir com a implementação do código C# correspondente e então documentar o resultado no seu documento principal do DDD.
