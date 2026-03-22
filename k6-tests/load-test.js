import http from 'k6/http';
import { sleep, check } from 'k6';

export const options = {
  vus: 10,
  duration: '10s',
  setupTimeout: '120s', // Seed işlemi uzun sürebilir, süreyi artırdık
  thresholds: {
    'http_req_duration': ['p(95)<400'],
    'http_req_failed': ['rate<0.1'],
  }
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export function setup() {
  console.log("API'nin hazır olması bekleniyor (10sn)...");
  sleep(10); 

  // 1. Önce veritabanında veri var mı kontrol edelim
  const allRes = http.get(`${BASE_URL}/api/url/all`);
  let codes = [];

  if (allRes.status === 200) {
    const existingData = allRes.json();
    if (existingData && existingData.length > 0) {
      console.log(`Veritabanında ${existingData.length} kayıt bulundu, seed atlanıyor.`);
      codes = existingData.map(item => item.shortCode);
    }
  }

  // 2. Eğer veri yoksa SEED endpoint'ini tetikleyelim
  if (codes.length === 0) {
    console.log("Veritabanı boş görünüyor. Seed işlemi başlatılıyor (10.000 kayıt)...");
    const seedRes = http.get(`${BASE_URL}/api/url/seed`);
    
    if (seedRes.status === 200) {
      console.log("Seed başarılı. Kodlar çekiliyor...");
      // Seed sonrası kodları tekrar çek
      const finalRes = http.get(`${BASE_URL}/api/url/all`);
      if (finalRes.status === 200) {
        codes = finalRes.json().map(item => item.shortCode);
      }
    } else {
      console.error(`Seed hatası! Durum: ${seedRes.status}`);
    }
  }

  console.log(`Setup tamamlandı. Test edilecek toplam kod sayısı: ${codes.length}`);
  return codes;
}

export default function (data) {
  if (!data || data.length === 0) {
    // Veri yoksa hata ver ve durdur
    console.error("Test için veri mevcut değil!");
    sleep(1);
    return;
  }

  // Rastgele bir kod seç ve redirect testini yap
  const randomCode = data[Math.floor(Math.random() * data.length)];
  const res = http.get(`${BASE_URL}/${randomCode}`, { redirects: 0 });

  check(res, {
    'status 302/301': (r) => r.status === 301 || r.status === 302,
    'is not 404': (r) => r.status !== 404,
  });

  // VU'lar arası nefes payı
  sleep(0.5);
}