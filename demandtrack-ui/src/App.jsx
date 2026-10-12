import { useEffect, useState } from 'react'
import { getDemands, createDemand, updateDemand, deleteDemand, deleteDemands, getReport, getStatusHistory } from './api'

import './App.css'

function App() {
  const [demands, setDemands] = useState([])
  const [error, setError] = useState('')
//yeni talep ekleme formu
  const bosForm = {
  başlık: '',
  açıklama: '',
  oluşturan_Kişi: '',
  durum: 'Yeni',
  oluşturma_Tarihi: new Date().toISOString().slice(0, 10),
}
const [yeni, setYeni] = useState(bosForm)

function onYeniChange(e) {
  setYeni({ ...yeni, [e.target.name]: e.target.value })
}

async function ekle(e) {
  e.preventDefault()
  try {
    // Oluşturma tarihi her zaman bugünün tarihi olarak gönderilir
    await createDemand({ ...yeni, oluşturma_Tarihi: new Date().toLocaleDateString('en-CA') })
    setYeni(bosForm)
    load()
  } catch (err) {
    setError(err.message)
  }
}

const [rapor, setRapor] = useState(null)

// Report.py çıktısındaki "Etiket: değer" satırlarını ayırır ("Onaylanan talep sayısı" -> "Onaylanan")
function raporuAyristir(metin) {
  return metin.split('\n')
    .map(satir => satir.trim())
    .filter(satir => satir.includes(':'))
    .map(satir => {
      const i = satir.lastIndexOf(':')
      return { etiket: satir.slice(0, i).replace('talep sayısı', '').trim(), deger: satir.slice(i + 1).trim() }
    })
}

async function raporuGetir() {
  try {
    const metin = await getReport()
    setRapor(raporuAyristir(metin))
    setError('')
  } catch (err) {
    setError(err.message)
  }
}

//durum geçmişi
const [statusHistory, setStatusHistory] = useState(null)

async function showStatusHistory(talepNo) {
  try {
    const entries = await getStatusHistory(talepNo)
    setStatusHistory({ talepNo, entries })
    setError('')
  } catch (err) {
    setError(err.message)
  }
}

function formatDateTime(value) {
  return new Date(value).toLocaleString('tr-TR', { dateStyle: 'short', timeStyle: 'short' })
}

async function sil(talepNo) {
  if (!confirm(`${talepNo} numaralı talebi silmek istediğinize emin misiniz?`)) return
  try {
    await deleteDemand(talepNo)
    setSelectedNos(selectedNos.filter(no => no !== talepNo))
    if (statusHistory?.talepNo === talepNo) setStatusHistory(null)
    load()
  } catch (err) {
    await load()
    setError(err.message)
  }
}

//toplu silme
const [selectedNos, setSelectedNos] = useState([])

function toggleSelection(talepNo) {
  setSelectedNos(selectedNos.includes(talepNo)
    ? selectedNos.filter(no => no !== talepNo)
    : [...selectedNos, talepNo])
}

async function deleteSelected() {
  if (!confirm(`Seçilen ${selectedNos.length} talebi silmek istediğinize emin misiniz?`)) return
  try {
    await deleteDemands(selectedNos)
    if (selectedNos.includes(statusHistory?.talepNo)) setStatusHistory(null)
    setSelectedNos([])
    load()
  } catch (err) {
    setSelectedNos([])
    await load()
    setError(err.message)
  }
}

const [duzenlenenNo, setDuzenlenenNo] = useState(null)
const [duzenle, setDuzenle] = useState(bosForm)

function duzenlemeyeBasla(d) {
  setDuzenlenenNo(d.talepNo)
  setDuzenle({
    başlık: d.başlık,
    açıklama: d.açıklama,
    oluşturan_Kişi: d.oluşturan_Kişi,
    durum: d.durum,
    oluşturma_Tarihi: d.oluşturma_Tarihi,
  })
}

function duzenlemeyiIptalEt() {
  setDuzenlenenNo(null)
}

function onDuzenleChange(e) {
  setDuzenle({ ...duzenle, [e.target.name]: e.target.value })
}

async function kaydet(talepNo) {
  try {
    await updateDemand(talepNo, { talepNo, ...duzenle })
    setDuzenlenenNo(null)
    // Açık olan geçmiş bu talebe aitse yeni durum değişikliği görünsün
    if (statusHistory?.talepNo === talepNo) showStatusHistory(talepNo)
    load()
  } catch (err) {
    setDuzenlenenNo(null)
    await load()
    setError(err.message)
  }
}

//sayfalama ve sıralama
const pageSizeOptions = [20, 50, 100]
const sortableColumns = [
  { key: 'demandNo', label: 'No', width: '100px' },
  { key: 'title', label: 'Başlık', width: '150px' },
  { key: 'description', label: 'Açıklama', width: '200px' },
  { key: 'createdBy', label: 'Oluşturan', width: '120px' },
  { key: 'status', label: 'Durum', width: '140px' },
  { key: 'date', label: 'Tarih', width: '110px' },
]
const [page, setPage] = useState(1)
const [pageSize, setPageSize] = useState(20)
const [sortBy, setSortBy] = useState('')
const [sortDirection, setSortDirection] = useState('asc')
const [totalCount, setTotalCount] = useState(0)
const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

// Aynı sütuna tıklandıkça: A-Z (eskiden yeniye) -> Z-A (yeniden eskiye) -> sıralama yok (eklenme sırası)
function changeSort(column) {
  if (sortBy === column && sortDirection === 'asc') {
    setSortDirection('desc')
  } else if (sortBy === column) {
    setSortBy('')
    setSortDirection('asc')
  } else {
    setSortBy(column)
    setSortDirection('asc')
  }
  setPage(1)
  setSelectedNos([])
}

function changePageSize(size) {
  setPageSize(size)
  setPage(1)
  setSelectedNos([])
}

function goToPage(newPage) {
  setPage(newPage)
  setSelectedNos([])
}

///
  async function load() {
    try {
      const data = await getDemands({ page, pageSize, sortBy, sortDirection })
      // Son sayfadaki tüm talepler silindiyse bir önceki sayfaya geçilir
      if (data.items.length === 0 && page > 1) {
        setPage(page - 1)
        return
      }
      setDemands(data.items)
      setTotalCount(data.totalCount)
      setError('')
    } catch (e) {
      setError(e.message)
    }
  }

  useEffect(() => {
    load()
  }, [page, pageSize, sortBy, sortDirection])

  return (
    <div>
      <button className="rapor-buton" onClick={raporuGetir}>Rapor</button>
      <h1>Talepler</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {rapor && (
        <div className="rapor">
          {rapor.map(s => (
            <span key={s.etiket}>{s.etiket}: <strong>{s.deger}</strong></span>
          ))}
          <button className="rapor-kapat" title="Kapat" aria-label="Raporu kapat" onClick={() => setRapor(null)}>×</button>
        </div>
      )}
      {statusHistory && (
        <div className="status-history">
          <div className="status-history-title">
            <strong>{statusHistory.talepNo} – Durum geçmişi</strong>
            <button className="rapor-kapat" title="Kapat" aria-label="Durum geçmişini kapat" onClick={() => setStatusHistory(null)}>×</button>
          </div>
          {statusHistory.entries.length === 0
            ? <p>Bu talebin durumu henüz değiştirilmedi.</p>
            : (
              <ul>
                {statusHistory.entries.map((entry, i) => (
                  <li key={i}>{formatDateTime(entry.changedAt)}: {entry.oldStatus} → {entry.newStatus}</li>
                ))}
              </ul>
            )}
        </div>
      )}
      <form className="talep-formu" onSubmit={ekle}>
        <label>
          Başlık
          <input name="başlık" value={yeni.başlık} onChange={onYeniChange} required maxLength={20} />
        </label>
        <label className="genis">
          Açıklama
          <input name="açıklama" value={yeni.açıklama} onChange={onYeniChange} required maxLength={200} />
        </label>
        <label>
          Oluşturan
          <input name="oluşturan_Kişi" value={yeni.oluşturan_Kişi} onChange={onYeniChange} required maxLength={20} />
        </label>
        <label>
          Durum
          <select name="durum" value={yeni.durum} onChange={onYeniChange}>
            <option>Yeni</option>
            <option>Onaylandı</option>
            <option>Reddedildi</option>
            <option>Revizyon Bekliyor</option>
          </select>
        </label>
        <button type="submit">Ekle</button>
      </form>

      <button className="toplu-sil" onClick={deleteSelected} disabled={selectedNos.length === 0}>
        Seçilenleri Sil ({selectedNos.length})
      </button>

      <table border="1">
        <thead>
          <tr>
               <th style={{ width: '40px' }}></th>
               {sortableColumns.map(column => (
                 <th key={column.key} style={{ width: column.width }}
                   aria-sort={sortBy === column.key ? (sortDirection === 'asc' ? 'ascending' : 'descending') : 'none'}>
                   <button type="button" className="sort-button" title="Sırala" onClick={() => changeSort(column.key)}>
                     {column.label}
                     {sortBy === column.key && <span className="sort-arrow">{sortDirection === 'asc' ? '▲' : '▼'}</span>}
                   </button>
                 </th>
               ))}
               <th style={{ width: '150px' }}></th>
          </tr>
        </thead>
        <tbody>
          {demands.map(d => (
            d.talepNo === duzenlenenNo ? (
              <tr key={d.talepNo}>
                <td><input type="checkbox" checked={selectedNos.includes(d.talepNo)} onChange={() => toggleSelection(d.talepNo)} /></td>
                <td>{d.talepNo}</td>
                <td><input form="duzenleForm" name="başlık" value={duzenle.başlık} onChange={onDuzenleChange} required maxLength={20} /></td>
                <td><textarea form="duzenleForm" name="açıklama" rows={3} className="aciklama" value={duzenle.açıklama} onChange={onDuzenleChange} required maxLength={200} /></td>
                <td><input form="duzenleForm" name="oluşturan_Kişi" value={duzenle.oluşturan_Kişi} onChange={onDuzenleChange} required maxLength={20} /></td>
                <td>
                  <select form="duzenleForm" name="durum" value={duzenle.durum} onChange={onDuzenleChange}>
                    <option>Yeni</option>
                    <option>Onaylandı</option>
                    <option>Reddedildi</option>
                    <option>Revizyon Bekliyor</option>
                  </select>
                </td>
                     <td><input form="duzenleForm" type="date" name="oluşturma_Tarihi" min="2020-01-01" max={new Date().toISOString().slice(0, 10)} value={duzenle.oluşturma_Tarihi} onChange={onDuzenleChange} required /></td>

                <td>
                  <form id="duzenleForm" onSubmit={e => { e.preventDefault(); kaydet(d.talepNo) }}>
                      <button type="submit">Kaydet</button>
                      <button type="button" onClick={duzenlemeyiIptalEt}>İptal</button>
                  </form>
                </td>
              </tr>
            ) : (
              <tr key={d.talepNo} className={selectedNos.includes(d.talepNo) ? 'secili' : undefined}>
                <td><input type="checkbox" checked={selectedNos.includes(d.talepNo)} onChange={() => toggleSelection(d.talepNo)} /></td>
                <td>{d.talepNo}</td>
                <td>{d.başlık}</td>
                <td className="aciklama">{d.açıklama}</td>
                <td>{d.oluşturan_Kişi}</td>
                <td>{d.durum}</td>
                <td>{d.oluşturma_Tarihi}</td>
                <td className="islemler">
                  <button className="ikon-buton degistir" title="Değiştir" aria-label="Değiştir" onClick={() => duzenlemeyeBasla(d)}>
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M12 20h9" />
                      <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z" />
                    </svg>
                  </button>
                  <button className="ikon-buton history" title="Durum geçmişi" aria-label="Durum geçmişi" onClick={() => showStatusHistory(d.talepNo)}>
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <circle cx="12" cy="12" r="9" />
                      <path d="M12 7v5l3 2" />
                    </svg>
                  </button>
                  <button className="ikon-buton sil" title="Sil" aria-label="Sil" onClick={() => sil(d.talepNo)}>
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M3 6h18" />
                      <path d="M8 6V4h8v2" />
                      <path d="M19 6l-1 14H6L5 6" />
                      <path d="M10 11v6M14 11v6" />
                    </svg>
                  </button>
                </td>
              </tr>
            )
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <label>
          Sayfa başına
          <select value={pageSize} onChange={e => changePageSize(Number(e.target.value))}>
            {pageSizeOptions.map(size => <option key={size} value={size}>{size}</option>)}
          </select>
        </label>
        <span className="pagination-total">Toplam {totalCount} talep</span>
        <button type="button" onClick={() => goToPage(page - 1)} disabled={page <= 1}>‹ Önceki</button>
        <span>Sayfa {page} / {totalPages}</span>
        <button type="button" onClick={() => goToPage(page + 1)} disabled={page >= totalPages}>Sonraki ›</button>
      </div>
    </div>
  )
}

export default App