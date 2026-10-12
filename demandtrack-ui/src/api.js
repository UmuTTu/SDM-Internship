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
  if (!res.ok) {
    // API hatayı ProblemDetails JSON'u olarak döner; sadece açıklamayı göster
    let detay = text || `Sunucu ${res.status} hatası döndü.`;
    try { detay = JSON.parse(text).detail ?? detay; } catch { /* JSON olmayan yanıt */ }
    throw new Error('Rapor oluşturulamadı: ' + detay);
  }
  return text;
}

// params: { page, pageSize, sortBy, sortDirection } -> { items, totalCount, page, pageSize }
export const getDemands   = (params) => request(`/demands?${new URLSearchParams(params)}`);
export const getStatusHistory = (demandNo) => request(`/demands/${demandNo}/status-history`);
export const createDemand = (d)     => request('/demands', { method: 'POST', body: JSON.stringify(d) });
export const updateDemand  = (demandNo, d) => request(`/demands/${demandNo}`, { method: 'PUT', body: JSON.stringify(d) });
export const deleteDemand  = (demandNo)    => request(`/demands/${demandNo}`, { method: 'DELETE' });
export const deleteDemands = (demandNos)   => request('/demands/bulk-delete', { method: 'POST', body: JSON.stringify(demandNos) });