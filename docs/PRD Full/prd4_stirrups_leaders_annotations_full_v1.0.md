# PRD #4 — Stirrups, Leaders, Annotation & CAD Presentation (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 4/8 — Stirrups & Annotation  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Stirrup Calculator Owner + CAD Presentation Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own rải thép đai, leader, text, block ký hiệu, dimension phụ và presentation rules cho bản vẽ dầm.
2. Logic trọng tâm: rải đai cách mép 50 mm, chia 3 vùng theo L/div_L mặc định L/4, khoảng cách vùng gối/giữa nhịp, ShapeString/NumCK cho đai.
3. Đảm bảo output CAD dễ đọc, đúng layer, không chồng leader/text không cần thiết.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C4.1 | 3-zone stirrup distribution | S | Stirrup owner | Sai số lượng/spacing → quantity wrong |
| C4.2 | Edge offset 50mm | S | Stirrup owner | Đai sát mép sai → shopdrawing error |
| C4.3 | Stirrup shape/block RBSH_DAI | C | CAD owner | TKTD không nhận dạng hình dạng |
| C4.4 | Leader/text annotation | S | Presentation owner | Leader thiếu/sai mark → mismatch |
| C4.5 | Dimension/label cleanup | O | Presentation owner | Bản vẽ khó đọc |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-ST-001 | Rải đai từng nhịp theo 3 vùng: vùng gối trái, vùng giữa, vùng gối phải; chiều dài vùng gối = L/div_L mặc định L/4. | P0 | BDAM_TCVN | Phase 2 |
| FR-ST-002 | Đai đầu tiên/cuối cùng cách mép vùng/đầu cấu kiện 50 mm theo logic AutoLISP. | P0 | Stirrup distribution | Phase 2 |
| FR-ST-003 | Hỗ trợ input đường kính đai, số nhánh, spacing vùng gối/giữa, ký hiệu/Mark và NumCK. | P0 | UI + calculator | Phase 2 |
| FR-ST-004 | Leader/text cho thép chủ, thép tăng cường, thép đai phải đồng bộ Mark/Diam/Qty/spacing và đọc được bởi người dùng CAD. | P0 | Annotation | Phase 2 |
| FR-ST-005 | Mọi stirrup/leader cần XData-ready metadata để TKTD/XTE thống kê chính xác. | P0 | XData handoff | Phase 3 |

**FR-ST-001 3-zone distribution — Acceptance criteria (P0)**:
```
Given một nhịp dài L=6000 và div_L=4
When rải đai với spacing gối/giữa được nhập
Then vùng gối trái dài 1500, vùng giữa dài 3000, vùng gối phải dài 1500
And vị trí đai tuân thủ spacing từng vùng
And tổng số đai được lưu vào metadata Qty/NumCK để thống kê
```

**FR-ST-004 leader — Acceptance criteria (P0)**:
```
Given các entity rebar/stirrup đã tạo
When annotation stage chạy
Then leader hiển thị đúng mark, đường kính, số lượng/khoảng cách
And text nằm trên layer/style chuẩn
And không thiếu leader cho nhóm thép P0
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| StirrupSpec | PRD #4 | input | Diam, branches, support spacing, mid spacing, div_L, edge offset |
| StirrupRun | PRD #4 | calculated | Zone boundaries, count, positions, NumCK |
| LeaderDraft | PRD #4 | drawing | Target entity, text, arrow points, style |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| BDAM_TCVN stirrup stage | Generate stirrup runs per span | Invalid spacing → validation fail |
| Annotation stage | Draw leaders/text for rebar groups | Missing metadata → skip with warning/error by priority |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-ST-001 | P0 guard | BDAM_TCVN | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-ST-002 | P0 guard | Stirrup distribution | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-ST-003 | P0 guard | UI + calculator | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-ST-004 | P0 guard | Annotation | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-ST-005 | P0 guard | XData handoff | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_stirrups_leaders_annotations_command_success_total | counter | command, status | Command reliability |
| bdam_stirrups_leaders_annotations_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_stirrups_leaders_annotations_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_stirrups_leaders_annotations_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #4 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- UI cần field spacing vùng gối/giữa, div_L, edge offset hiển thị default 50 mm.
- Có thể gom leader theo nhóm để giảm rối bản vẽ nhưng không được làm mất dữ liệu thống kê.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | div_L default = 4 (L/4). | PRD_BDAM_TCVN_Final.md | PRD #4 section reflection |
| 2 | Edge distance stirrup = 50 mm là P0 locked rule. | PRD_BDAM_TCVN_Final.md | PRD #4 section reflection |
| 3 | Leader là presentation nhưng dữ liệu thật vẫn nằm trong XData. | PRD_BDAM_TCVN_Final.md | PRD #4 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Có cần layout tránh chồng text tự động nâng cao trong MVP không.
2. Chốt format text leader cuối cùng theo office standard.

## Assumptions

1. Đai được vẽ 2D bằng block/polyline representative đủ cho shopdrawing.
2. RBSH_DAI hoặc ShapeString tương đương là contract với TKTD.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-ST-1 | Stirrup distribution calculator | PR-D1 | P0 |
| EP-ST-2 | Leader/text annotation engine | PR-D2 | P0 |

Story samples:

- **EP-ST-1-ST-1**: Implement Stirrup distribution calculator; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-ST-2-ST-1**: Implement Leader/text annotation engine; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Consumes geometry from PRD #2 and rebar groups from PRD #3; sends metadata to PRD #5 and quantities to PRD #6.

**End of PRD #4 v1.0.** Awaiting review/lock before implementation PRs.
