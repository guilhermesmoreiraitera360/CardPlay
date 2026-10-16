export function formatarMoeda(valor: number): string {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(valor);
}

export function formatarDataHora(iso: string): string {
  const comFuso = /(?:[zZ]|[+-]\d{2}:\d{2})$/.test(iso) ? iso : `${iso}Z`;
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(comFuso));
}
