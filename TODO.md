# TODO

## 1. Implementação de Eventos de Domínio (Consistência Eventual)
**Motivação:** Escapar do acoplamento em Casos de Uso grandes e suportar efeitos colaterais (como e-mails, alertas e integrações) sem comprometer o limite transacional da raiz de agregação.
- [ ] Adicionar um mecanismo base para registro de eventos (ex: `IReadOnlyCollection<DomainEvent> DomainEvents` e um método `AddDomainEvent`) na classe base `Entity` ou `AggregateRoot`.
- [ ] Construir/Refatorar o processo de `SaveEntitiesAsync` no `UnitOfWork` (ou `DbContext` caso EF Core) para extrair e publicar os eventos de domínio via `IMediator` logo após a gravação no banco de dados.
- [ ] Adaptar o `Customer` (e outros *Aggregates*) para dispararem eventos expressivos do negócio dentro de seus próprios métodos mutadores (ex: disparar `CustomerCreatedEvent` ou `CustomerAddressUpdatedEvent`).

## 2. Refinamento de CQRS e Command Handlers
**Motivação:** Aumentar a aderência ao Command Query Separation (CQS), quebrando o vínculo de retorno excessivo de dados pós-gravacao para as camadas de rede.
- [ ] Analisar os comandos (ex: `CreateCustomerCommand`, `UpdateCustomerCommand`) que atualmente retornam DTOs inteiros (como `CustomerResponseDto`).
- [ ] Refatorar (se viável para o negócio/front-end) para que comandos retornem no máximo o ID (ex: `Guid`) do recurso criado/modificado, mantendo os handlers com retorno focado *apenas* no resultado da mutação.
- [ ] Mover as operações de exibição para `Queries` exclusivas de leitura.

## 3. Adoção de Factory Methods para Criação de Aggregates
**Motivação:** Controlar fortemente as regras invariantes de inicialização do domínio sem depender exclusivamente de construtores complexos e públicos.
- [ ] No `Customer.cs` (e em demais entidades chaves), mudar construtores públicos paramétricos para `protected` ou `private`.
- [ ] Adicionar métodos semânticos estáticos (ex: `public static Customer CreateNew(...)`) que encapsulem toda a lógica de negócio na "montagem" do objeto e permitam atrelar na mesma rotina o disparo de *Domain Events*.
- [ ] Refatorar os `CommandHandlers` atrelados para utilizar a nova porta de entrada (`Customer.CreateNew()`).
