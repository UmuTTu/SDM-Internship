async function request(url, options = {}) {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },//Tür
    ...options,
  });
  if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
  return res.status === 204 ? null : res.json();
}
export async function getReport() {
  const res = await fetch('/demands/report');
  const text = await res.text();
  if (!res.ok) throw new Error('Rapor oluşturulamadı: ' + text);
  return text;
}

export const getDemands   = ()      => request('/demands');
export const createDemand = (d)     => request('/demands', { method: 'POST', body: JSON.stringify(d) });
export const updateDemand = (id, d) => request(`/demands/${id}`, { method: 'PUT', body: JSON.stringify(d) });
export const deleteDemand = (id)    => request(`/demands/${id}`, { method: 'DELETE' });