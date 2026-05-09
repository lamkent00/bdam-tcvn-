# PRD #5 — BIM/XData Entity Metadata Contract (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 5/8 — BIM/XData Contract  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Data Contract Owner + AutoCAD API Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own contract dữ liệu gắn vào CAD entity bằng XData AppID ST_BIM_REBAR để các command BDAM_TCVN, GT, TKTD, XTE liên thông.
2. Định nghĩa schema, versioning, attach/read/update API, validation và compatibility với entity do AutoLISP/GT tạo.
3. Không own tính toán số lượng; PRD #6 consume XData để thống kê.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C5.1 | Register AppID ST_BIM_REBAR | S | XData owner | AppID thiếu → XData attach fail |
| C5.2 | Rebar XData schema | S | Data owner | Field thiếu → TKTD/XTE mismatch |
| C5.3 | Attach/update/read service | S | AutoCAD API owner | Corrupt XData → reject + diagnostic |
| C5.4 | Schema versioning/backward compatibility | C | Data owner | Old drawing không thống kê được |
| C5.5 | Selection filters for BIM entities | C | Stats owner | TKTD chọn sai entity |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-XD-001 | Plugin phải đăng ký RegAppTableRecord `ST_BIM_REBAR` idempotent trước khi gắn XData. | P0 | All rebar entities | Phase 3 |
| FR-XD-002 | Schema tối thiểu gồm BeamID, Mark, Diam, Qty, LengthMM, ShapeString, NumCK, BarType, SourceCommand, SchemaVersion. | P0 | XData schema | Phase 3 |
| FR-XD-003 | XDataService.Attach phải validate type/range, không gắn thiếu Mark/Diam/Qty/LengthMM cho entity P0. | P0 | Attach API | Phase 3 |
| FR-XD-004 | XDataService.Read phải bỏ qua entity không có AppID và trả lỗi có phân loại cho entity có XData corrupt. | P0 | Read API | Phase 3 |
| FR-XD-005 | GT và BDAM_TCVN đều dùng cùng XDataService, không tự build ResultBuffer rời rạc trong command. | P0 | Command integration | Phase 3 |

**FR-XD-002 schema — Acceptance criteria (P0)**:
```
Given một polyline thép được tạo bởi BDAM_TCVN
When XDataService.Attach chạy
Then entity có XData dưới AppID ST_BIM_REBAR
And fields BeamID, Mark, Diam, Qty, LengthMM, ShapeString, NumCK tồn tại đúng kiểu
And TKTD đọc lại được RebarRecord tương đương input
```

**FR-XD-004 corrupt XData — Acceptance criteria (P0)**:
```
Given selection chứa entity có ST_BIM_REBAR nhưng thiếu Diam
When TKTD gọi XDataService.Read
Then service trả validation error có EntityId
And TKTD báo danh sách entity lỗi thay vì crash
And các entity hợp lệ khác vẫn được thống kê nếu user chọn tiếp tục
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| RebarXDataRecord | PRD #5 | entity XData | BeamID, Mark, Diam, Qty, LengthMM, ShapeString, NumCK, BarType, SourceCommand, SchemaVersion |
| XDataValidationError | PRD #5 | runtime | EntityId, field, reason, severity |
| ShapeStringCatalog | PRD #5 | shared | Canonical shape identifiers consumed by TKTD |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| BDAM_TCVN XData attach | Attach metadata to generated bars/stirrups | Missing field → rollback P0 |
| GT XData attach | Attach metadata to selected manual bars | Invalid selection/input → reject |
| TKTD read | Read all selected rebar records | Corrupt records reported |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-XD-001 | P0 guard | All rebar entities | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XD-002 | P0 guard | XData schema | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XD-003 | P0 guard | Attach API | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XD-004 | P0 guard | Read API | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-XD-005 | P0 guard | Command integration | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_bim_xdata_contract_command_success_total | counter | command, status | Command reliability |
| bdam_bim_xdata_contract_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_bim_xdata_contract_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_bim_xdata_contract_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #5 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- XData lỗi cần báo bằng Editor.WriteMessage và có thể highlight entity lỗi.
- Không yêu cầu user thấy XData trong UI thường ngày; dữ liệu phục vụ automation.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | ST_BIM_REBAR là AppID canonical. | PRD_BDAM_TCVN_Final.md | PRD #5 section reflection |
| 2 | LengthMM là chiều dài tính bằng mm; Excel/CAD table không tự suy diễn lại nếu XData đã chuẩn. | PRD_BDAM_TCVN_Final.md | PRD #5 section reflection |
| 3 | SchemaVersion bắt buộc để nâng cấp sau này. | PRD_BDAM_TCVN_Final.md | PRD #5 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Có cần tool migrate XData từ bản AutoLISP cũ nếu field name khác không.
2. Chốt BarType enum đầy đủ: main, support, extra, stirrup, manual.

## Assumptions

1. AutoCAD XData giới hạn kích thước đủ cho metadata nhỏ của mỗi thanh.
2. Các entity thống kê là Line/Polyline/BlockReference có thể mang XData.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-XD-1 | XData schema + service | PR-B1 | P0 |
| EP-XD-2 | Command integration + corrupt handling | PR-B2 | P0 |

Story samples:

- **EP-XD-1-ST-1**: Implement XData schema + service; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-XD-2-ST-1**: Implement Command integration + corrupt handling; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Consumed by PRD #6 TKTD and PRD #7 XTE; produced by PRD #3/#4 and GT.

**End of PRD #5 v1.0.** Awaiting review/lock before implementation PRs.
