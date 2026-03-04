import http from 'k6/http';
import { sleep ,check} from 'k6';

export const options = {
  vus: 200,          // 50 users at the same time
  duration: '30s',  // for 30 seconds
  thresholds: {
    'http_req_duration' : ['p(95)<100'], // 95% of requests should be below 100ms
    'http_req_failed': ['rate<0.01'], // error rate should be less than 1%
  }
};


export function setup() {
  const generatedCodes = [];
  

  for(let i=0; i<10000; i++) {
    const payload = JSON.stringify({ originalUrl: `https://example.com/${i}` });
    const params = { headers: { 'Content-Type': 'application/json' } };
    const res = http.post('http://localhost:8080/api/shorten', payload, params);
    

    generatedCodes.push(res.json('shortCode')); 
  }
  
  return generatedCodes; 
}



export default function (data) {

    const randomCode = data[Math.floor(Math.random() * data.length)];
    
  const res = http.get(`http://localhost:8080/${randomCode}`, {
    redirects: 0,
  });

check(res, {
    // URL Shortener için beklenen kod 200 DEĞİL, 301 veya 302'dir.
    'is status 301/302': (r) => r.status === 301 || r.status === 302,
  });

  sleep(0.1);
}
