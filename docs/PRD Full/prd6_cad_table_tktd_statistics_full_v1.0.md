# PRD #6 — TKTD CAD Quantity Takeoff & Statistics Table (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 6/8 — CAD Statistics / TKTD  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Statistics Owner + CAD Table Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own command TKTD: đọc XData ST_BIM_REBAR, phân tích hình dạng, gom nhóm, tính chiều dài/khối lượng và sinh CAD Table.
2. Bảo toàn logic AutoLISP V39.1: shape analysis, block RBSHxx/RBSH_DAI, bảng thống kê 10 cột, gom nhóm theo Mark/Diam/ShapeString/Length/NumCK.
3. Xuất dataset chuẩn để PRD #7 ghi Excel.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C6.1 | Selection and XData ingestion | S | Stats owner | Selection không có XData → warning |
| C6.2 | Rebar grouping and shape analysis | S | Stats owner | Gom sai → khối lượng sai |
| C6.3 | Weight/length calculation | S | Stats owner | Đơn vị sai → BOM sai |
| C6.4 | CAD Table rendering | S | CAD Table owner | Bảng lỗi format → user khó dùng |
| C6.5 | Statistics dataset handoff to XTE | C | Excel owner | Excel export thiếu dữ liệu |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-TK-001 | TKTD cho phép user chọn vùng/entity, lọc các entity có XData ST_BIM_REBAR hợp lệ. | P0 | TKTD command | Phase 4 |
| FR-TK-002 | Gom nhóm theo BeamID/Mark/Diam/ShapeString/LengthMM/NumCK hoặc rule đã chốt để thống kê không double-count. | P0 | Stats engine | Phase 4 |
| FR-TK-003 | Tính tổng số lượng, chiều dài, khối lượng theo đường kính và hệ số kg/m chuẩn. | P0 | Quantity calculation | Phase 4 |
| FR-TK-004 | Sinh CAD Table với schema 10 cột tương thích AutoLISP/Excel template, gồm hình dạng/ký hiệu, đường kính, số lượng, chiều dài, tổng dài, trọng lượng. | P0 | CAD Table | Phase 4 |
| FR-TK-005 | Nếu gặp XData lỗi, TKTD phải báo entity lỗi và không crash; cho phép thống kê phần hợp lệ theo lựa chọn user. | P0 | Error handling | Phase 4 |

**FR-TK-002 grouping — Acceptance criteria (P0)**:
```
Given selection có nhiều thanh cùng Mark, Diam, ShapeString và LengthMM
When TKTD thống kê
Then các thanh cùng key được gom thành một dòng
And Qty/NumCK/tổng chiều dài được cộng đúng
And thanh khác ShapeString hoặc Diam nằm ở dòng riêng
```

**FR-TK-004 CAD Table — Acceptance criteria (P0)**:
```
Given StatisticsDataset đã tính xong
When user chọn điểm đặt bảng
Then plugin tạo CAD Table đúng 10 cột theo template
And các dòng có số liệu chiều dài/khối lượng format nhất quán
And table có metadata/cache để XTE có thể export nếu cần
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| RebarRecord | PRD #6 | read from XData | Canonical typed metadata |
| StatisticsRow | PRD #6 | calculated | Group key, count, length, total length, weight, shape block |
| CadTableLayout | PRD #6 | drawing | Column definitions, title, style, insertion point |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| TKTD | Select entities and create CAD table | No valid records → no table |
| Dataset cache | Expose rows for XTE | Cache missing → XTE asks user to select table/rerun TKTD |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-TK-001 | P0 guard | TKTD command | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-TK-002 | P0 guard | Stats engine | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-TK-003 | P0 guard | Quantity calculation | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-TK-004 | P0 guard | CAD Table | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-TK-005 | P0 guard | Error handling | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

---

## 7. Audit / diagnostic tier mapping

| Event | Tier | Rationale |
|---|---|---|
| Command start/end | Diagnostic | Trace command runtime and user cancel path |
| Validation reject | User-visible warning | Prevent bad CAD mutations |
| Transaction rollback | Error diagnostic | Root-cause debugging without corrupting DWG |
| XData/stat/export mismatch | P0 QA defect | Downstream BOM/Excel correctness risk |

---

## 8. Metrics / quality signals

| Metric | Type | Labels | Use |
|---|---|---|---|
| bdam_cad_table_tktd_statistics_command_success_total | counter | command, status | Command reliability |
| bdam_cad_table_tktd_statistics_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_cad_table_tktd_statistics_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_cad_table_tktd_statistics_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

Forbidden labels: raw user name, file path chứa thông tin nhạy cảm, full DWG path, Excel workbook full path nếu có dữ liệu khách hàng.

---

## 9. Failure mode + recovery

| Failure case | Detection | Recovery | Idempotency key |
|---|---|---|---|
| Invalid user input | UI validator / command guard | Reject before transaction; show field-level message | input hash/session |
| AutoCAD API exception | catch exception in command boundary | Rollback transaction; Editor.WriteMessage diagnostic | command invocation id |
| Missing downstream contract | artifact check / null service | Block implementation or command path until dependency exists | PR dependency id |
| Partial output risk | transaction abort | Abort commit; user reruns after fixing input | transaction id |

---

## 10. Compatibility with AutoLISP V39.1 / Final PRD

PRD #6 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- TKTD prompt rõ: chọn thép hoặc vùng bản vẽ, sau đó chọn điểm đặt bảng.
- Khi có entity lỗi, hiển thị số lượng hợp lệ/lỗi và option tiếp tục/hủy nếu feasible.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | CAD Table là output P0, không chỉ export Excel. | PRD_BDAM_TCVN_Final.md | PRD #6 section reflection |
| 2 | XData là source of truth; không parse text leader để thống kê. | PRD_BDAM_TCVN_Final.md | PRD #6 section reflection |
| 3 | Bảng 10 cột tương thích AutoLISP là acceptance bắt buộc. | PRD_BDAM_TCVN_Final.md | PRD #6 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Chốt chính xác thứ tự/tên 10 cột nếu template nội bộ có khác.
2. Chốt bảng tra khối lượng đường kính thép theo TCVN/office standard.

## Assumptions

1. RebarRecord LengthMM đã tính đúng bởi upstream; TKTD không sửa hình học.
2. Shape block RBSHxx có sẵn hoặc được tạo bởi plugin.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-TK-1 | TKTD ingestion + grouping | PR-E1 | P0 |
| EP-TK-2 | CAD Table renderer + shape block integration | PR-E2 | P0 |

Story samples:

- **EP-TK-1-ST-1**: Implement TKTD ingestion + grouping; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-TK-2-ST-1**: Implement CAD Table renderer + shape block integration; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Consumes XData from PRD #5; provides StatisticsDataset/CAD Table for PRD #7 XTE.

**End of PRD #6 v1.0.** Awaiting review/lock before implementation PRs.
