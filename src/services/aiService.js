import { api } from './api';

export function aiAnalyze({ orderId } = {}) {
  return api.post('/api/ai/analyze', {
    mode: 'risk_analysis',
    order_id: orderId ?? null,
  });
}

