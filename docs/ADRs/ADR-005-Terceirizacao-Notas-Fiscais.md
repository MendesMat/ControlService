# ADR 05: Terceirização da Emissão de Notas Fiscais

## Status:
Aceito

## Contexto:
O faturamento da empresa exige a emissão de notas fiscais de serviço e/ou produto. A integração direta com as prefeituras brasileiras para emissão de NFS-e é caótica e instável. Além disso, as regras de negócio variam atreladas às alíquotas que nossos diferentes CNPJs (e municípios de atuação) possuem.

## Decisão:
Decidi que o meu ERP apenas enviará as informações do cliente faturado e os dados básicos (alíquotas já pré-configuradas no nosso perfil CNPJ) para um sistema SaaS terceiro especializado em notas (ex: eNotas, Focus NFe, PlugNotas). Esse terceiro assumirá a responsabilidade pela mensageria com a SEFAZ/Prefeitura e me disponibilizará o link do PDF/XML de retorno no final do fluxo.

## Alternativas consideradas:
Desenvolvimento interno do emissor fiscal: Descartei sumariamente. Sendo eu o único desenvolvedor criando esse ERP do zero, abraçar o escopo de comunicação direta e atualização constante dos webservices de dezenas de prefeituras elevaria minha complexidade arquitetural a níveis absurdos e arruinaria a data de entrega do projeto.

## Trade-offs, Riscos e Impactos:
Garanto a entrega técnica temporal do projeto (*time-to-market*). O *trade-off* para a empresa é a inclusão de um fornecedor crítico ao fluxo de caixa (*Vendor Lock-in*) e o meu compromisso com a disponibilidade dessa infraestrutura terceira para a concretização operacional (Conascência Dinâmica de serviço externo).
