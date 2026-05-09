# PRD #1 — Foundation, Standards & AutoCAD .NET Architecture (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 1/8 — Foundation & Architecture  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Technical Lead + AutoCAD Core Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Định nghĩa nền tảng kỹ thuật cho việc chuyển đổi AutoLISP BDAM_TCVN V39.1 sang plugin AutoCAD C# .NET dạng DLL.
2. Own kiến trúc solution, command registry, transaction/document lock, layer/text/dimension standard, cấu hình mặc định, nguyên tắc chuyển đổi module-by-module.
3. Không own chi tiết vẽ thép, XData, thống kê hay Excel; các PRD sau consume nền tảng này.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C1.1 | AutoCAD .NET plugin shell + DLL packaging | S | Core owner | DLL không load được trong AutoCAD → fail install, log rõ lỗi dependency |
| C1.2 | Command registry BDAM_TCVN/GT/TKTD/XTE | S | Core owner | Command trùng tên hoặc thiếu command → reject build smoke test |
| C1.3 | DocumentLock + Transaction boundary | S | All devs | Entity ghi ngoài transaction → rollback, audit debug log |
| C1.4 | Layer/style/dim/text standard service | C | CAD presentation owner | Thiếu layer/style → auto-create idempotent, không phá layer có sẵn |
| C1.5 | Config/default resolver | C | UI/Core owner | Config lỗi → fallback default có cảnh báo |
| C1.6 | AutoLISP parity governance | O | QA owner | Output lệch V39.1 → block release theo parity checklist |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-FD-001 | Plugin phải build thành DLL load qua NETLOAD/loader nội bộ trên AutoCAD 2018–2024, ưu tiên .NET Framework tương thích AutoCAD target. | P0 | All commands | Phase 0 |
| FR-FD-002 | Khai báo đủ 4 command public: BDAM_TCVN, GT, TKTD, XTE; mỗi command có guard DocumentLock, Transaction và error handling tập trung. | P0 | Command layer | Phase 0 |
| FR-FD-003 | Tất cả entity tạo mới phải đi qua factory/service để gán layer, linetype, color, text style, dimension style nhất quán. | P0 | Drawing layer | Phase 0-1 |
| FR-FD-004 | Không hard-code magic number nghiệp vụ nếu giá trị phải cấu hình: cover, Abv, lmoc, div_L, default spacing, max stock length 11700. | P0 | Core calculators | Phase 1 |
| FR-FD-005 | Tách module: Commands, Models, Calculators, Drawers, XData, Statistics, Excel, UI, Tests. | P0 | Solution architecture | Phase 0 |
| FR-FD-006 | Mọi lỗi command phải rollback transaction, không để bản vẽ ở trạng thái nửa vời. | P0 | All commands | Phase 0 |

**FR-FD-002 command registry — Acceptance criteria (P0)**:
```
Given plugin DLL đã được load trong AutoCAD
When người dùng gõ BDAM_TCVN, GT, TKTD hoặc XTE
Then command tương ứng được kích hoạt
And nếu command cần selection/input thì hiển thị prompt/UI đúng scope
And lỗi runtime được bắt, transaction rollback, Editor.WriteMessage hiển thị thông báo dễ hiểu
```

**FR-FD-003 drawing standards — Acceptance criteria (P0)**:
```
Given bản vẽ chưa có layer/style chuẩn BDAM_TCVN
When command đầu tiên tạo entity CAD
Then LayerStyleService tạo layer/style/dim style idempotent
And entity mới được gán đúng layer theo loại: beam, rebar, stirrup, leader, text, dimension
And command chạy lại không tạo duplicate style rác
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| PluginConfig | Foundation | global/user profile | Default cover, Abv, lmoc, div_L, spacing, text height, scale, maxStockLength |
| CommandContext | Foundation | per command | Document, Database, Editor, Transaction, scale, unit assumptions |
| LayerStyleCatalog | Foundation | drawing | Layer names, text style, dim style, color/linetype defaults |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| NETLOAD / Plugin loader | Load DLL and register commands | Missing AutoCAD API references → load failure with diagnostic |
| BDAM_TCVN | Open UI and execute full beam drawing workflow | Invalid input → no drawing mutation |
| GT/TKTD/XTE | Reuse shared services and command context | Selection/Excel unavailable → graceful cancel |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-FD-001 | P0 guard | All commands | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-FD-002 | P0 guard | Command layer | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-FD-003 | P0 guard | Drawing layer | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-FD-004 | P0 guard | Core calculators | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-FD-005 | P0 guard | Solution architecture | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-FD-006 | P0 guard | All commands | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_foundation_architecture_command_success_total | counter | command, status | Command reliability |
| bdam_foundation_architecture_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_foundation_architecture_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_foundation_architecture_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #1 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- Không yêu cầu UI lớn trong PRD #1 ngoài thông báo lỗi chuẩn, progress/status message và setting defaults dùng chung.
- Mọi command phải có cancel path rõ ràng, không để AutoCAD treo khi user ESC.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | C# .NET plugin là target chính, AutoLISP chỉ là nguồn parity. | PRD_BDAM_TCVN_Final.md | PRD #1 section reflection |
| 2 | Majestic monolith plugin assembly trong MVP; chưa tách nhiều DLL nếu chưa cần. | PRD_BDAM_TCVN_Final.md | PRD #1 section reflection |
| 3 | Transaction-per-command là default; nested transaction chỉ khi có lý do kỹ thuật. | PRD_BDAM_TCVN_Final.md | PRD #1 section reflection |
| 4 | Excel export dùng COM interop vì yêu cầu ghi vào Excel đang mở. | PRD_BDAM_TCVN_Final.md | PRD #1 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Chốt chính xác version .NET Framework theo AutoCAD target thấp nhất.
2. Chốt naming layer/style cuối cùng theo template CAD nội bộ nếu có.

## Assumptions

1. Đơn vị bản vẽ là mm.
2. AutoCAD chạy trên Windows có quyền COM automation với Excel khi dùng XTE.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-FD-1 | Plugin shell + command registry | PR-0.1, PR-0.2 | P0 |
| EP-FD-2 | Shared CAD services | PR-0.3 | P0 |

Story samples:

- **EP-FD-1-ST-1**: Implement Plugin shell + command registry; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-FD-2-ST-1**: Implement Shared CAD services; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Unblocks PRD #2 UI/Geometry, PRD #5 XData, PRD #6 TKTD, PRD #7 XTE.

**End of PRD #1 v1.0.** Awaiting review/lock before implementation PRs.
