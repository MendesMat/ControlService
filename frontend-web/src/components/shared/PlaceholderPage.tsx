interface PlaceholderPageProps {
  title: string;
  description?: string;
}

/**
 * Componente temporário para páginas ainda não implementadas.
 * Substitua por componentes reais conforme cada módulo for desenvolvido.
 */
export default function PlaceholderPage({
  title,
  description = "Esta página está em desenvolvimento.",
}: PlaceholderPageProps) {
  return (
    <div className="p-8 max-w-5xl mx-auto">
      <h1 className="mb-1">{title}</h1>
      <p className="text-content-secondary text-sm">{description}</p>
    </div>
  );
}
