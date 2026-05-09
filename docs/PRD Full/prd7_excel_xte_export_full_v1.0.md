# PRD #7 — XTE Excel Export & Template Interop (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 7/8 — Excel Export / XTE  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Excel Interop Owner + Statistics Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own command XTE: xuất bảng thống kê từ CAD sang Excel đang mở, ưu tiên sheet TKCT, không phá template/công thức/dữ liệu cũ.
2. Bảo toàn logic AutoLISP: reverse scan tìm dòng an toàn, ghi batch qua Value2 để tăng tốc, mapping cột template.
3. Không own tính toán thống kê; consume StatisticsDataset/CAD Table từ PRD #6.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C7.1 | Excel COM attach to running instance | S | Excel owner | Excel chưa mở → hướng dẫn user mở file |
| C7.2 | Sheet TKCT resolver | S | Excel owner | Không có TKCT → prompt chọn sheet hoặc fail rõ |
| C7.3 | Safe row detection reverse scan | S | Excel owner | Ghi đè dữ liệu → P0 bug |
| C7.4 | Batch Value2 write | S | Performance owner | Ghi từng cell chậm/treo Excel |
| C7.5 | Template formula preservation | C | QA owner | Phá công thức → release block |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-XL-001 | XTE kết nối Excel instance đang mở bằng COM; nếu không có Excel workbook thì báo lỗi thân thiện và không crash AutoCAD. | P0 | XTE command | Phase 4 |
| FR-XL-002 | Ưu tiên sheet `TKCT`; nếu không tồn tại, xử lý theo rule: prompt chọn sheet hoặc báo thiếu template. | P0 | Sheet resolver | Phase 4 |
| FR-XL-003 | Tìm dòng ghi an toàn bằng thuật toán quét ngược theo vùng/cột khóa, tránh ghi đè dữ liệu cũ. | P0 | Row detection | Phase 4 |
| FR-XL-004 | Ghi dữ liệu bằng mảng 2D qua Range.Value2 batch, không loop cell-by-cell cho dataset lớn. | P0 | Performance | Phase 4 |
| FR-XL-005 | Không ghi đè công thức/template ngoài vùng output; có dry-run summary trước khi ghi nếu rủi ro. | P0 | Template safety | Phase 4 |

**FR-XL-003 safe row — Acceptance criteria (P0)**:
```
Given workbook TKCT đã có dữ liệu thống kê cũ
When XTE export dataset mới
Then plugin quét ngược để tìm dòng trống/an toàn tiếp theo
And dữ liệu cũ không bị ghi đè
And user nhận thông báo số dòng đã xuất và vị trí bắt đầu
```

**FR-XL-004 Value2 batch — Acceptance criteria (P0)**:
```
Given StatisticsDataset có nhiều dòng
When XTE ghi sang Excel
Then plugin tạo mảng object[,] và gán một lần vào Range.Value2
And thời gian export nằm trong ngưỡng KPI
And Excel vẫn responsive sau khi COM object được release
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| ExcelExportRow | PRD #7 | from StatisticsDataset | Column-mapped values for TKCT |
| ExcelTemplateMap | PRD #7 | config | Sheet name, start row, key columns, output columns |
| ExportResult | PRD #7 | runtime | Workbook, sheet, start row, rows written, warnings |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| XTE | Export current/selected CAD table statistics to Excel | No workbook/sheet/data → clear error |
| Excel COM cleanup | Release COM references after write | COM exception → cleanup finally |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-XL-001 | P0 guard | XTE command | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XL-002 | P0 guard | Sheet resolver | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XL-003 | P0 guard | Row detection | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XL-004 | P0 guard | Performance | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XL-005 | P0 guard | Template safety | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_excel_xte_export_command_success_total | counter | command, status | Command reliability |
| bdam_excel_xte_export_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_excel_xte_export_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_excel_xte_export_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #7 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- XTE prompt nên cho user chọn CAD table hoặc dùng dataset cache mới nhất từ TKTD.
- Thông báo sau export: workbook/sheet/start row/row count/warnings.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | COM Interop là P0 vì cần ghi vào Excel đang mở. | PRD_BDAM_TCVN_Final.md | PRD #7 section reflection |
| 2 | Value2 batch write là mandatory performance rule. | PRD_BDAM_TCVN_Final.md | PRD #7 section reflection |
| 3 | TKCT là sheet ưu tiên theo tài liệu nguồn. | PRD_BDAM_TCVN_Final.md | PRD #7 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Chốt mapping cột Excel cuối cùng theo template thực tế.
2. Có cần export khi Excel chưa mở bằng cách tự mở file template trong MVP không.

## Assumptions

1. Máy user có Microsoft Excel cài đặt.
2. Template TKCT có vùng output ổn định và không bảo vệ sheet bằng password không biết.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-XL-1 | Excel COM resolver + sheet TKCT | PR-F1 | P0 |
| EP-XL-2 | Safe row + Value2 batch export | PR-F2 | P0 |

Story samples:

- **EP-XL-1-ST-1**: Implement Excel COM resolver + sheet TKCT; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-XL-2-ST-1**: Implement Safe row + Value2 batch export; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Consumes StatisticsDataset/CAD Table from PRD #6; QA in PRD #8 validates template preservation.

**End of PRD #7 v1.0.** Awaiting review/lock before implementation PRs.
