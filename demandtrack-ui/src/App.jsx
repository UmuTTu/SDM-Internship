import { useEffect, useState } from 'react'
import { getDemands, createDemand, updateDemand, deleteDemand, getReport } from './api'

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
    await createDemand(yeni)
    setYeni(bosForm)
    load()
  } catch (err) {
    setError(err.message)
  }
}

async function raporuGetir() {
  try {
    alert(await getReport())
    setError('')
  } catch (err) {
    setError(err.message)
  }
}

async function sil(talepNo) {
  try {
    await deleteDemand(talepNo)
    load()
  } catch (err) { 
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
    load()
  } catch (err) {
    setDuzenlenenNo(null)
    await load()
    setError(err.message)
  }
}

///
  async function load() {
    try {
      const data = await getDemands()
      setDemands(data)
      setError('')
    } catch (e) {
      setError(e.message)
    }
  }

  useEffect(() => {
    load()
  }, [])

  return (
    <div>
      <button onClick={raporuGetir} style={{ position: 'fixed', top: '16px', right: '16px' }}>Rapor</button>
      <h1>Talepler</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
            <form onSubmit={ekle}>
  <input name="başlık" placeholder="Başlık" value={yeni.başlık} onChange={onYeniChange} required maxLength={20} />
  <input name="açıklama" placeholder="Açıklama" value={yeni.açıklama} onChange={onYeniChange} required maxLength={200} />
  <input name="oluşturan_Kişi" placeholder="Oluşturan" value={yeni.oluşturan_Kişi} onChange={onYeniChange} required maxLength={20} />
  <select name="durum" value={yeni.durum} onChange={onYeniChange}>
    <option>Yeni</option>
    <option>Onaylandı</option>
    <option>Reddedildi</option>
    <option>Revizyon Bekliyor</option>
  </select>
      <input type="date" name="oluşturma_Tarihi" min="2020-01-01" max={new Date().toISOString().slice(0, 10)} value={yeni.oluşturma_Tarihi} onChange={onYeniChange} />
      <button type="submit">Ekle</button>
</form>


      <table border="1">
        <thead>
          <tr>
               <th style={{ width: '50px' }}>No</th>
               <th style={{ width: '150px' }}>Başlık</th>
               <th style={{ width: '200px' }}>Açıklama</th>
               <th style={{ width: '120px' }}>Oluşturan</th>
               <th style={{ width: '140px' }}>Durum</th>
               <th style={{ width: '110px' }}>Tarih</th>
               <th style={{ width: '150px' }}></th>
          </tr>
        </thead>
        <tbody>
          {demands.map(d => (
            d.talepNo === duzenlenenNo ? (
              <tr key={d.talepNo}>
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
              <tr key={d.talepNo}>
                <td>{d.talepNo}</td>
                <td>{d.başlık}</td>
                <td className="aciklama">{d.açıklama}</td>
                <td>{d.oluşturan_Kişi}</td>
                <td>{d.durum}</td>
                <td>{d.oluşturma_Tarihi}</td>
                <td>
                  <button onClick={() => duzenlemeyeBasla(d)}>Değiştir</button>
                  <button onClick={() => sil(d.talepNo)}>Sil</button>
                </td>
              </tr>
            )
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default App