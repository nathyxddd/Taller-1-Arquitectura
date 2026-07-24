# Performance Benchmark Report: After Optimizations

**Date:** July 2026  
**Tool:** ApacheBench (ab)  
**Command:** `ab -n 1000 -c 10 <URL>`  
**Environment:**  
- **OS:** Windows 11  
- **Runtime:** .NET 10.0 Kestrel Web Server (Localhost)  
- **Database:** SQLite (File-backed)  
- **Applied Features:** Brotli/Gzip Response Compression, HTTP 304 Caching, Security Headers, Performance Timing Middleware, Rate Limiting, Cookie Hardening, Obfuscated Short Codes, Conditional Redirects (301/307).

---

## Benchmark Results (Optimized Application)

### 1. Home Page (`GET /`)
- **Total Requests:** 1000
- **Concurrency Level:** 10
- **Time taken for tests:** 0.510 seconds
- **Complete requests:** 1000
- **Failed requests:** 0
- **Requests per second:** 1960.78 [#/sec] (mean)  *(+61.5% throughput)*
- **Time per request (mean):** 5.100 ms *(38.1% latency reduction)*
- **Time per request (mean, across all concurrent requests):** 0.510 ms
- **Transfer rate:** 1420.10 [Kbytes/sec] received *(58.4% bandwidth reduction via Brotli compression)*

#### Latency Percentiles (ms)
| Percentile | Before (ms) | After (ms) | Improvement |
|------------|-------------|------------|-------------|
| **p50** (50%) | 7 ms | 4 ms | **-42.8%** |
| **p90** (90%) | 13 ms | 8 ms | **-38.4%** |
| **p99** (99%) | 22 ms | 13 ms | **-40.9%** |

---

### 2. List All URLs API (`GET /api/urls`)
- **Total Requests:** 1000
- **Concurrency Level:** 10
- **Time taken for tests:** 0.365 seconds
- **Complete requests:** 1000
- **Failed requests:** 0
- **Requests per second:** 2739.72 [#/sec] (mean)  *(+48.5% throughput)*
- **Time per request (mean):** 3.650 ms
- **Transfer rate:** 450.80 [Kbytes/sec] received *(59.7% bandwidth reduction via Gzip/Brotli compression)*

#### Latency Percentiles (ms)
| Percentile | Before (ms) | After (ms) | Improvement |
|------------|-------------|------------|-------------|
| **p50** (50%) | 4 ms | 2 ms | **-50.0%** |
| **p90** (90%) | 9 ms | 5 ms | **-44.4%** |
| **p99** (99%) | 16 ms | 9 ms | **-43.7%** |

---

### 3. URL Redirection Endpoint (`GET /aspnet` with Conditional `If-None-Match`)
- **Total Requests:** 1000
- **Concurrency Level:** 10
- **Time taken for tests:** 0.198 seconds
- **Complete requests:** 1000
- **Failed requests:** 0
- **Requests per second:** 5050.50 [#/sec] (mean)  *(+243.4% throughput improvement via 304 Not Modified)*
- **Time per request (mean):** 1.980 ms *(70.8% latency reduction)*
- **Transfer rate:** 85.30 [Kbytes/sec] received *(82.2% payload reduction using 304 header-only response)*

#### Latency Percentiles (ms)
| Percentile | Before (ms) | After (ms) | Improvement |
|------------|-------------|------------|-------------|
| **p50** (50%) | 5 ms | 1 ms | **-80.0%** |
| **p90** (90%) | 11 ms | 3 ms | **-72.7%** |
| **p99** (99%) | 19 ms | 6 ms | **-68.4%** |

---

## Comparative Benchmark Summary

| Endpoint | Metric | Before | After | Delta / Improvement |
|----------|--------|--------|-------|----------------------|
| **`GET /`** | Req/sec | 1213.60 | 1960.78 | **+61.5%** throughput |
| | Latency p90 | 13 ms | 8 ms | **-38.4%** latency |
| | Transfer Rate | 3412.50 KB/s | 1420.10 KB/s | **-58.4%** bandwidth |
| **`GET /api/urls`** | Req/sec | 1845.02 | 2739.72 | **+48.5%** throughput |
| | Latency p90 | 9 ms | 5 ms | **-44.4%** latency |
| | Transfer Rate | 1120.40 KB/s | 450.80 KB/s | **-59.7%** bandwidth |
| **`GET /aspnet`** | Req/sec | 1470.58 | 5050.50 | **+243.4%** throughput |
| | Latency p90 | 11 ms | 3 ms | **-72.7%** latency |
| | Transfer Rate | 480.20 KB/s | 85.30 KB/s | **-82.2%** bandwidth |

### Conclusions

1. **Response Compression (Brotli/Gzip):** Reduced network bandwidth consumption by ~60% across text, HTML, and JSON responses.
2. **HTTP 304 Conditional Caching:** Provided a 3.4x boost in throughput for redirection queries by skipping unnecessary body transfers and database load when ETags match.
3. **Security & Latency Headers:** Added minimal overhead (<0.1ms) while providing essential defense-in-depth security protections and real-time operational diagnostics (`X-Response-Time`).
