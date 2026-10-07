# Workflow: record a decision

Architecture decisions live in [`docs/decisoes-de-arquitetura.md`](../../decisoes-de-arquitetura.md): one document, in Portuguese, because it is also the owner's study material. The owner decides; an agent proposes and writes.

## When a decision belongs there

- A new technology, library or external service with architectural impact.
- A new pattern or convention that all features must follow.
- A change to an existing decision, including a small one.
- An answer from the owner that settles a technical trade-off.

Implementation details that do not constrain other code (a private helper, a method name) are not decisions. Business rules are not either: they go to `docs/product/`.

## Propose and write a decision

1. Explain it to the owner first, in Portuguese, in the six points of [AGENTS.md](../../../AGENTS.md#the-owner): what is proposed, which problem it solves, what happens without it, what it costs, the simpler alternatives, and what you recommend and why.
2. Only after the owner decides, write it in the document, in the format of the others: the six numbered points, then **Onde ver no código** and **Em uma frase**. Put it in the block it belongs to, with the next free number. Never renumber: other documents cite decisions by number.
3. If the owner decides **not** to adopt something, add a row to "O que ficou de fora" with the reason.
4. If the code does not do it yet, say so in the text, as the document does with "Ainda não está construído", and list any pending cleanup in the table at the start of the block.
5. If the decision answers an item in `docs/product/open-questions.md`, move the item to "Resolved".

## Change a decision

Edit it in place and keep the six points true. Record when and why it changed in the sixth point ("Reavaliada em AAAA-MM-DD: ..."), as the document already does. Update every document in `docs/product/` and `docs/api/` that described the old behavior.

## Deliver

A change goes through a pull request like any other ([git and pull requests](git-and-pull-requests.md)), with the `docs` type: `docs: record the decision on signature storage`.
