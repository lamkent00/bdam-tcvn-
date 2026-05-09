# PRD #2 — UI, Input Model & BDAM_TCVN Beam Geometry (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 2/8 — UI/Input + Beam Geometry  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: UI Owner + Geometry Calculator Owner  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own toàn bộ workflow nhập liệu và hình học dầm cho command BDAM_TCVN trước khi vẽ thép.
2. Bao gồm WinForms/WPF UI, validation tham số, danh sách nhịp/gối, mút thừa trái/phải, kích thước dầm/cột, scale, cover, default spacing và preview/execute flow.
3. Không own logic chi tiết thép; chỉ xuất BeamInput/BeamGeometry contract cho PRD #3/#4.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C2.1 | BDAM_TCVN input form | S | UI owner | Input thiếu/sai kiểu → validation inline, không vẽ |
| C2.2 | Span/support/cantilever model | S | Geometry owner | Tổng chiều dài sai → fail validation |
| C2.3 | Beam outline + columns/supports drawing | S | Drawing owner | Mút thừa/gối sai → parity test fail |
| C2.4 | Dimension/breakline geometry anchors | C | CAD presentation | Anchor sai → leader/dim lệch |
| C2.5 | Input persistence/default profile | O | UX owner | Không lưu được config → fallback default |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-GEO-001 | BDAM_TCVN hiển thị UI nhập tham số dầm nhiều nhịp, chiều dài từng nhịp, b/h, gối/cột, mút thừa trái/phải, scale, cover, Abv, lmoc, div_L. | P0 | BDAM_TCVN UI | Phase 1 |
| FR-GEO-002 | Validation bắt buộc: số nhịp >=1, chiều dài >0, b/h >0, cover hợp lệ, spacing >0, div_L >0, đường kính thép trong danh sách cho phép. | P0 | Input model | Phase 1 |
| FR-GEO-003 | BeamGeometryCalculator tạo tọa độ chuẩn cho trục dầm, mép bê tông, gối/cột, mút thừa, vùng nhịp, điểm neo leader/dimension. | P0 | Geometry core | Phase 1 |
| FR-GEO-004 | Vẽ đường bao bê tông, cột/gối, breakline, dimension tổng/nhịp theo scale và layer chuẩn. | P0 | BDAM_TCVN drawing | Phase 1 |
| FR-GEO-005 | Workflow command phải cho phép Cancel trước khi mutate CAD; chỉ commit transaction sau khi toàn bộ drawing pipeline thành công. | P0 | Command UX | Phase 1 |

**FR-GEO-002 validation — Acceptance criteria (P0)**:
```
Given user mở form BDAM_TCVN
When nhập số nhịp = 0 hoặc chiều dài nhịp <= 0
Then nút Execute bị disable hoặc form báo lỗi rõ trường sai
And không tạo entity CAD nào
And input hợp lệ mới được chuyển sang BeamGeometryCalculator
```

**FR-GEO-004 beam outline — Acceptance criteria (P0)**:
```
Given BeamInput có 3 nhịp và mút thừa trái/phải
When user xác nhận vẽ
Then plugin vẽ đúng outline dầm, gối/cột, mút thừa, breakline và dimension
And tọa độ đầu/cuối từng span khớp chiều dài input
And các anchor được expose cho PRD #3/#4 dùng tiếp
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| BeamInput | PRD #2 | command-session | Span lengths, beam b/h, supports, cantilever flags/lengths, cover, scale, text height |
| BeamGeometry | PRD #2 | calculated | Coordinate anchors: start/end/span/support/top/bottom/leader/dim points |
| UiDraftState | PRD #2 | user profile | Last-used values, validation state |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| BDAM_TCVN UI open | Collect and validate input | Dialog cancel → no transaction |
| BDAM_TCVN Execute | Calculate geometry then call drawing/rebar pipeline | Calculation exception → rollback |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-GEO-001 | P0 guard | BDAM_TCVN UI | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-GEO-002 | P0 guard | Input model | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-GEO-003 | P0 guard | Geometry core | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-GEO-004 | P0 guard | BDAM_TCVN drawing | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-GEO-005 | P0 guard | Command UX | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_ui_input_geometry_bdam_command_success_total | counter | command, status | Command reliability |
| bdam_ui_input_geometry_bdam_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_ui_input_geometry_bdam_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_ui_input_geometry_bdam_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #2 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- UI nên chia tab/GroupBox: Geometry, Reinforcement, Stirrups, Annotation/Output, Advanced.
- Các field có tooltip tiếng Việt; default hợp lý lấy từ AutoLISP và PRD Final.
- Nên có nút Reset default và Save last profile.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | WinForms hoặc WPF đều được, nhưng phải chọn một framework thống nhất cho MVP. | PRD_BDAM_TCVN_Final.md | PRD #2 section reflection |
| 2 | BeamInput là contract bất biến sau validation; downstream không đọc trực tiếp control UI. | PRD_BDAM_TCVN_Final.md | PRD #2 section reflection |
| 3 | Mút thừa là first-class input, không xử lý bằng hack span âm. | PRD_BDAM_TCVN_Final.md | PRD #2 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Chốt UI framework cuối cùng WinForms hay WPF theo repo hiện có.
2. Có cần preview canvas trước khi vẽ CAD hay MVP chỉ nhập rồi execute.

## Assumptions

1. Kỹ sư nhập số liệu đã kiểm tra thiết kế chịu lực; plugin không tự tính As trong MVP.
2. Geometry dùng hệ tọa độ phẳng 2D trong ModelSpace.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-GEO-1 | BDAM input UI + validation | PR-A1 | P0 |
| EP-GEO-2 | BeamGeometryCalculator + outline drawer | PR-A2 | P0 |

Story samples:

- **EP-GEO-1-ST-1**: Implement BDAM input UI + validation; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-GEO-2-ST-1**: Implement BeamGeometryCalculator + outline drawer; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Provides BeamInput/BeamGeometry to PRD #3 longitudinal rebar and PRD #4 stirrup/annotation.

**End of PRD #2 v1.0.** Awaiting review/lock before implementation PRs.
