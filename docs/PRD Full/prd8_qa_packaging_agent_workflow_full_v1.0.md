# PRD #8 — QA, Packaging, Release Roadmap & Agent Workflow (FULL)

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Repo**: `TBD / AutoCAD plugin repository`  
**Macro-context**: 8/8 — QA/Release + Dev Agent Workflow  
**Status**: Draft v1.0 — split from Final PRD on 2026-05-09  
**Blueprint SoT**: `PRD_BDAM_TCVN_Final.md` v2.0  
**ADR contracts**: ADR-BDAM-001 AutoCAD Transaction Boundary; ADR-BDAM-002 XData Schema; ADR-BDAM-003 Excel COM Interop; ADR-BDAM-004 Agent PR Workflow  
**Owner**: Integration Lead + QA Owner + Agent Coordinator  
**Changelog**: v1.0 generated from Final PRD, original Word requirements and AutoLISP V39.1 prompt/source synthesis.

> Mọi requirement có scope rõ ràng, P0 có acceptance criteria dạng Given-When-Then. Các nội dung agent/phối hợp nằm ở PRD #8 và prompt Dev Agent, không làm loãng yêu cầu sản phẩm.

---

# PART A — PRD BODY

## 1. Macro-context summary

1. Own test matrix, parity validation với AutoLISP V39.1, packaging/release checklist, agent coordination và roadmap triển khai.
2. Chuyển nội dung phối hợp/agent/prompt từ tài liệu nguồn vào phụ lục vận hành, đồng thời định nghĩa DoD cho từng PRD con.
3. Không own logic domain riêng; đảm bảo tất cả PRD #1-#7 tích hợp thành workflow hoàn chỉnh.

Macro **không own** các phần ngoài boundary: các PRD con khác consume/produce contract qua bảng handoff cuối tài liệu.

---

## 2. Capability inventory

| ID | Capability | Label (S/C/O) | Owner role family | Failure mode (high level) |
|---|---|---|---|---|
| C8.1 | Golden sample drawing parity | S | QA owner | Output lệch LISP → block release |
| C8.2 | Command integration test matrix | S | Integration lead | Command riêng lẻ pass nhưng pipeline fail |
| C8.3 | Packaging + install guide | C | Release owner | User không load được DLL |
| C8.4 | Agent lane/task decomposition | O | Coordinator | Agent làm chồng scope/conflict |
| C8.5 | Acceptance sign-off checklist | S | PM/QA | Thiếu sign-off → no release |

Legend: S = system/core capability, C = cross-context contract, O = operational/support capability.

---

## 3. Functional requirements

> ID format theo macro: `FR-*`. Priority P0 = bắt buộc cho MVP.

| FR ID | Description | Priority | Applies to | Track / Phase |
|---|---|---|---|---|
| FR-QA-001 | Có bộ test mẫu cho dầm 1 nhịp, nhiều nhịp, mút thừa trái/phải/cả hai, nhiều lớp thép, đai biến đổi spacing, GT, TKTD, XTE. | P0 | QA | All phases |
| FR-QA-002 | Mỗi PR domain phải có unit test calculator và integration/smoke test command tương ứng nếu môi trường AutoCAD test cho phép. | P0 | Dev workflow | All phases |
| FR-QA-003 | Release MVP chỉ đạt Go khi workflow BDAM_TCVN → GT optional → TKTD → XTE chạy end-to-end trên ít nhất 3 case vàng. | P0 | Release gate | Final phase |
| FR-QA-004 | Bộ tài liệu agent/dev phải nêu rõ lane, dependency, artifact checks, shared files, anti-patterns và prompt handoff. | P0 | Agent workflow | Phase 0 |
| FR-QA-005 | Packaging gồm DLL, dependencies, hướng dẫn NETLOAD/install, version/changelog và rollback nếu plugin gây lỗi. | P1 | Release | Final phase |

**FR-QA-003 end-to-end gate — Acceptance criteria (P0)**:
```
Given plugin build release candidate
When QA chạy 3 golden cases từ nhập BDAM_TCVN đến xuất Excel XTE
Then CAD output có đủ hình học, thép, đai, leader, XData, CAD Table
And Excel TKCT có dữ liệu đúng, không phá template
And mọi sai lệch so với expected được ghi vào release checklist trước Go/No-Go
```

**FR-QA-004 agent workflow — Acceptance criteria (P0)**:
```
Given Dev Agent nhận một TASK_ID canonical
When bắt đầu làm
Then agent đọc đúng PRD liên quan và dependency PRD
And verify artifact trước khi code
And nếu thiếu contract thì báo BLOCKED thay vì tự workaround
```

---

## 4. Data ownership / interface matrix

| Entity / Contract | Owner macro | Scope | Fields / Meaning |
|---|---|---|---|
| GoldenCase | PRD #8 | test fixture | Input parameters, expected entity counts, expected table/export rows |
| ReleaseChecklist | PRD #8 | release artifact | Build, tests, parity, packaging, rollback |
| AgentTaskMap | PRD #8 | coordination | PR-ID, lane, dependencies, shared files, DoD |

---

## 5. Entry point / command context

| Entry point | Responsibility | Failure mode |
|---|---|---|
| QA runbook | Execute golden cases and collect evidence | Any P0 mismatch → no release |
| Agent handoff | Start implementation from PRD set + prompt | Dependency missing → BLOCKED template |

**Command context invariant**: mọi AutoCAD write phải chạy dưới `DocumentLock` + `Transaction`; nếu fail phải rollback và không để DWG ở trạng thái bán cập nhật.

---

## 6. Command / policy contract list

| Contract | Guard | Scope | Rationale |
|---|---|---|---|
| FR-QA-001 | P0 guard | QA | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-QA-002 | P0 guard | Dev workflow | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-QA-003 | P0 guard | Release gate | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-QA-004 | P0 guard | Agent workflow | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |
| FR-QA-005 | P1 guard | Release | Validate input → calculate → draw/attach/export → commit; fail closed on invalid state |

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
| bdam_qa_packaging_agent_workflow_command_success_total | counter | command, status | Command reliability |
| bdam_qa_packaging_agent_workflow_validation_fail_total | counter | field, reason | UX/input quality |
| bdam_qa_packaging_agent_workflow_runtime_seconds | histogram | command, phase | Performance baseline |
| bdam_qa_packaging_agent_workflow_parity_fail_total | counter | golden_case, reason | AutoLISP V39.1 parity tracking |

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

PRD #8 consumes `PRD_BDAM_TCVN_Final.md` and source Word docs; standard structure adapted from `prd1_identity_tenancy_full_v2.1.md`.

Compatibility requirements:

- Preserve command names where applicable.
- Preserve business rules called out in Final PRD: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT` when in scope.
- Do not replace deterministic CAD/detailing logic with AI-generated runtime behavior. AI/agent is only implementation support.
- Any deliberate behavior change from AutoLISP must be recorded as Open Question or ADR change.

---

## 11. UX/UI section

- QA evidence nên gồm DWG sample, screenshot CAD table, Excel output snapshot nếu có.
- Agent prompt giao tiếp tiếng Việt với coordinator/user.

---

# PART B — Decision trace table

| # | Locked decision | Source | PRD section reflection |
|---|---|---|---|
| 1 | MVP Go only after BDAM_TCVN → TKTD → XTE end-to-end. | PRD_BDAM_TCVN_Final.md | PRD #8 section reflection |
| 2 | Agent content là phụ lục/handoff, không thay PRD product logic. | PRD_BDAM_TCVN_Final.md | PRD #8 section reflection |
| 3 | Không big-bang; implement theo PR nhỏ và dependency rõ. | PRD_BDAM_TCVN_Final.md | PRD #8 section reflection |

---

# PART C — Open questions / Data gaps / Assumptions

## Open Questions

1. Có repo cụ thể chưa; nếu chưa, Dev Agent phải hỏi/nhận repo trước khi code.
2. Có môi trường AutoCAD automation test/headless không; nếu không, dùng manual smoke checklist.

## Assumptions

1. Có thể kiểm thử thủ công trong AutoCAD nếu CI không chạy được AutoCAD.
2. Người dùng/QA có Excel template TKCT thực tế để validate XTE.

## Data gaps

1. Repo thực tế, version AutoCAD target và CAD/Excel template chuẩn cần được xác nhận trước khi code.
2. Bộ DWG/Excel golden case thực tế cần được lưu trong repo hoặc test artifact trước release.


---

# PART D — Backlog seed (epic + story)

| EP ID | Epic | Stories / PR IDs | Priority |
|---|---|---|---|
| EP-QA-1 | Golden case fixture set | PR-Q1 | P0 |
| EP-QA-2 | Packaging + install docs | PR-Q2 | P1 |
| EP-QA-3 | Agent coordination docs | PR-Q0 | P0 |

Story samples:

- **EP-QA-1-ST-1**: Implement Golden case fixture set; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.
- **EP-QA-2-ST-1**: Implement Packaging + install docs; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P1.
- **EP-QA-3-ST-1**: Implement Agent coordination docs; AC: P0 FR trong PRD này pass, unit/integration checks có evidence; Priority P0.

---

# Cross-PRD handoff notes

- Depends on all PRD #1-#7; produces Dev Agent prompt and release/governance process.

**End of PRD #8 v1.0.** Awaiting review/lock before implementation PRs.
