# ADR 04: Motor Interno de Templates e Parse Dinâmico

- **Status:** Aceito
- **Contexto:** Nossa empresa precisa emitir documentos operacionais, contratos e certificados de garantia que são específicos e personalizados para cada perfil de CNPJ que usamos. Estes templates contratuais sofrem alterações jurídicas constantes que fogem da minha alçada.
- **Decisão:** Tomei a decisão de embutir na nossa aplicação um motor interno que lê templates (textos longos com marcações do tipo `[[NOME_CLIENTE]]`) salvos no nosso banco de dados e os renderiza diretamente no meu servidor na hora em que o cliente solicita a geração do PDF.
- **Alternativas consideradas:** 
  - *SaaS de terceiros focados em PDF*: Aumentariam demasiadamente as despesas fixas (OPEX) do nosso negócio corporativo para algo trivial.
  - *Hardcode de layouts em código HTML/PDF*: Exigiria um *deploy* meu a todo e qualquer momento que o setor jurídico quisesse alterar uma vírgula de contrato.
- **Trade-offs, Riscos e Impactos:** Elimino a minha necessidade técnica de intervir em pequenas atualizações textuais (liberando a agilidade comercial da empresa). O meu *trade-off* é a injeção de uma complexidade estrutural pesada no meu código base para desenvolver esse interpretador (parse seguro, falhas de renderização de fontes nativas no servidor, etc).
