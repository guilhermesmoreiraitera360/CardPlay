import { URL_API } from '../config/ambiente';

export class ErroApi extends Error {
  constructor(
    mensagem: string,
    readonly status: number,
  ) {
    super(mensagem);
    this.name = 'ErroApi';
  }
}

async function lerErro(resposta: Response): Promise<string> {
  try {
    const corpo = (await resposta.json()) as { detail?: string; title?: string };
    return corpo.detail ?? corpo.title ?? `Falha HTTP ${resposta.status}`;
  } catch {
    return `Falha HTTP ${resposta.status}`;
  }
}

async function tratar<T>(resposta: Response): Promise<T> {
  if (!resposta.ok) {
    throw new ErroApi(await lerErro(resposta), resposta.status);
  }

  return (await resposta.json()) as T;
}

export const clienteHttp = {
  get<T>(caminho: string): Promise<T> {
    return fetch(`${URL_API}${caminho}`).then((resposta) => tratar<T>(resposta));
  },

  post<T>(caminho: string, corpo: unknown): Promise<T> {
    return fetch(`${URL_API}${caminho}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(corpo),
    }).then((resposta) => tratar<T>(resposta));
  },

  postSemCorpo<T>(caminho: string): Promise<T> {
    return fetch(`${URL_API}${caminho}`, { method: 'POST' }).then((resposta) => tratar<T>(resposta));
  },
};
