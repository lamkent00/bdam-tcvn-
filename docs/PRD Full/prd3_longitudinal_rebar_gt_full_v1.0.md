# PRD #3 — Longitudinal Rebar, Support Rebar & GT Extra Rebar (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 3/8 — Rebar Detailing  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Rebar Calculator Owner + CAD Drawing Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own logic thép chủ, thép tăng cường trên/dưới, thép gối, móc L, nối chồng, cắt thép theo phôi và command GT.
2. Phải bảo toàn rule AutoLISP V39.1: offset theo Abv, lớp 1/2/3, móc theo lmoc, nối chồng 40D, chiều dài phôi tối đa 11700 mm, ShapeString thống nhất.
3. Xuất RebarEntityDraft kèm metadata cho PRD #5 gắn XData.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C3.1 | Main bottom/top rebar generation | S | Rebar owner | Sai offset/layer → parity fail |
| C3.2 | Support/top extra rebar generation | S | Rebar owner | Sai chiều dài neo/móc → QA block |
| C3.3 | Hook L + lmoc logic | S | Rebar owner | Hook sai hướng/độ dài → shopdrawing error |
| C3.4 | Splice 40D + max stock 11700 | C | Calculator owner | Thanh quá dài → auto split/cảnh báo |
| C3.5 | GT supplementary rebar command | S | Command owner | Không gắn XData/leader → TKTD thiếu |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-RB-001 | Tạo thép chủ theo BeamGeometry và input đường kính/số lượng/lớp; offset lớp theo Abv: lớp 1 = 1x, lớp 2 = 2x, lớp 3 = 3x tùy vị trí và logic V39.1. | P0 | BDAM_TCVN | Phase 2 |
| FR-RB-002 | Tạo thép gối/tăng cường với chiều dài vươn vào nhịp, móc L nếu cần, offset hook có xét 3*Abv theo logic AutoLISP. | P0 | BDAM_TCVN | Phase 2 |
| FR-RB-003 | Áp dụng lmoc cho chiều dài móc; ShapeString phải phản ánh đúng đoạn thẳng/móc để TKTD nhận dạng. | P0 | Rebar geometry | Phase 2 |
| FR-RB-004 | Nếu thanh vượt chiều dài phôi 11700 mm, phải split/nối chồng trong vùng an toàn với lap length 40D hoặc cảnh báo theo rule được chốt. | P0 | Rebar calculator | Phase 2 |
| FR-RB-005 | Command GT cho phép chọn line/polyline thép bổ sung, nhập/gán Mark/Diam/Qty/BeamID/NumCK/ShapeString và vẽ leader. | P0 | GT command | Phase 3 |

**FR-RB-001 Abv offsets — Acceptance criteria (P0)**:
```
Given BeamGeometry hợp lệ và input thép có nhiều lớp
When RebarCalculator tạo polyline thép
Then các lớp thép được đặt offset theo bội số Abv đúng quy tắc lớp
And không giao cắt mép bê tông/cover
And entity draft có Mark, Diam, Qty, LengthMM, ShapeString trước khi gắn XData
```

**FR-RB-005 GT command — Acceptance criteria (P0)**:
```
Given user chọn line/polyline thép bổ sung đã vẽ tay
When chạy GT và nhập thông tin thép
Then plugin gắn XData ST_BIM_REBAR cho entity được chọn
And vẽ leader theo điểm user chọn
And TKTD đọc được thanh này trong thống kê
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| RebarSpec | PRD #3 | input/calculated | Mark, Diam, Qty, layer, position, hook flags, lmoc |
| RebarEntityDraft | PRD #3 | pre-XData | Polyline geometry + metadata fields for XData |
| GtInput | PRD #3 | command-session | Selected entity ids and user-supplied metadata |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| BDAM_TCVN rebar stage | Generate all longitudinal/support bars from BeamGeometry | Invalid rebar spec → validation fail |
| GT | Annotate supplementary bars | Selection empty/non-curve → reject |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-RB-001 | P0 guard | BDAM_TCVN | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-RB-002 | P0 guard | BDAM_TCVN | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-RB-003 | P0 guard | Rebar geometry | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-RB-004 | P0 guard | Rebar calculator | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-RB-005 | P0 guard | GT command | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_longitudinal_rebar_gt_command_success_total | counter | command, status | Command reliability |
| bdam_longitudinal_rebar_gt_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_longitudinal_rebar_gt_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_longitudinal_rebar_gt_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #3 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- Rebar inputs trong UI cần nhóm theo thép dưới, thép trên, thép gối, thép tăng cường.
- GT phải prompt rõ thứ tự chọn đối tượng, nhập mark/diam/qty, chọn điểm leader.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | Abv/lmoc/40D/11700 là locked business rules cho MVP. | PRD_BDAM_TCVN_Final.md | PRD #3 section reflection |
| 2 | GT là command bắt buộc để thống kê cả thép vẽ bổ sung ngoài BDAM_TCVN. | PRD_BDAM_TCVN_Final.md | PRD #3 section reflection |
| 3 | Không tự tính thiết kế chịu lực; chỉ triển khai detailing theo input. | PRD_BDAM_TCVN_Final.md | PRD #3 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Chốt danh sách diameter allowed và mapping trọng lượng kg/m nếu chưa có trong template.
2. Chốt behavior split thanh dài: auto split hay prompt user trong MVP.

## Assumptions

1. ShapeString đủ mô tả hình dạng phục vụ TKTD, không cần block dynamic 3D.
2. Layer thép chủ/tăng cường được cung cấp bởi PRD #1.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-RB-1 | Longitudinal rebar calculator | PR-C1 | P0 |
| EP-RB-2 | Support/extra rebar + hook/splice | PR-C2 | P0 |
| EP-RB-3 | GT supplementary command | PR-C3 | P0 |

Story samples:

- **EP-RB-1-ST-1**: Implement Longitudinal rebar calculator; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-RB-2-ST-1**: Implement Support/extra rebar + hook/splice; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-RB-3-ST-1**: Implement GT supplementary command; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Consumes BeamGeometry from PRD #2; sends RebarEntityDraft to PRD #5; visual style consumed by PRD #4.

**End of PRD #3 v1.0.** Awaiting review/lock before implementation PRs.
