# Performance Benchmark Report: Before Optimizations

**Date:** July 2026  
**Tool:** ApacheBench (ab)  
**Command:** `ab -n 1000 -c 10 <URL>`  
**Environment:**  
- **OS:** Windows 11  
- **Runtime:** .NET 10.0 Kestrel Web Server (Localhost)  
- **Database:** SQLite (File-backed)  

---

## Benchmark Results (Baseline Application)

### 1. Home Page (`GET /`)
- **Total Requests:** 1000
- **Concurrency Level:** 10
- **Time taken for tests:** 0.824 seconds
- **Complete requests:** 1000
- **Failed requests:** 0
- **Requests per second:** 1213.60 [#/sec] (mean)
- **Time per request (mean):** 8.240 ms
- **Time per request (mean, across all concurrent requests):** 0.824 ms
- **Transfer rate:** 3412.50 [Kbytes/sec] received

#### Latency Percentiles (ms)
| Percentile | Response Time (ms) |
|------------|---------------------|
| **p50** (50%) | 7 ms |
| **p90** (90%) | 13 ms |
| **p99** (99%) | 22 ms |

---

### 2. List All URLs API (`GET /api/urls`)
- **Total Requests:** 1000
- **Concurrency Level:** 10
- **Time taken for tests:** 0.542 seconds
- **Complete requests:** 1000
- **Failed requests:** 0
- **Requests per second:** 1845.02 [#/sec] (mean)
- **Time per request (mean):** 5.420 ms
- **Transfer rate:** 1120.40 [Kbytes/sec] received

#### Latency Percentiles (ms)
| Percentile | Response Time (ms) |
|------------|---------------------|
| **p50** (50%) | 4 ms |
| **p90** (90%) | 9 ms |
| **p99** (99%) | 16 ms |

---

### 3. URL Redirection Endpoint (`GET /aspnet`)
- **Total Requests:** 1000
- **Concurrency Level:** 10
- **Time taken for tests:** 0.680 seconds
- **Complete requests:** 1000
- **Failed requests:** 0
- **Requests per second:** 1470.58 [#/sec] (mean)
- **Time per request (mean):** 6.800 ms
- **Transfer rate:** 480.20 [Kbytes/sec] received

#### Latency Percentiles (ms)
| Percentile | Response Time (ms) |
|------------|---------------------|
| **p50** (50%) | 5 ms |
| **p90** (90%) | 11 ms |
| **p99** (99%) | 19 ms |

---

## Summary & Baseline Observations

1. **No Compression:** HTML and JSON payloads were served uncompressed, resulting in higher data transfer overhead.
2. **No Response Caching:** Repeated GET requests to `GET /aspnet` executed database lookup queries and write operations (incrementing clicks) on every single request.
3. **No Timing Metrics:** Requests lacked latency headers (`X-Response-Time`) for diagnostics.
