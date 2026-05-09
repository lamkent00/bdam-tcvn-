# DEV Handoff Prompt — Build BDAM_TCVN AutoCAD C# .NET Plugin

> **Mục đích**: Prompt dùng chung cho DEV Agent triển khai từng PR/PRD con trong dự án **BDAM_TCVN AutoCAD C# .NET Plugin**.  
> **Repo chuẩn**: `TBD — user/coordinator cung cấp trước khi code`.  
> **Base branch mặc định**: `main`, trừ khi coordinator/user chỉ định khác.  
> **Docs SoT**: `docs/BDAM_TCVN/PRD Full/` gồm bộ PRD chuẩn trong `PRD.zip`.  
> **Ngôn ngữ giao tiếp**: Tiếng Việt cho mọi message tới user/coordinator.

---

## 0. Agent bootstrap

```text
LANE=<A|B|C|D>
TASK_ID=<canonical PR-ID, ví dụ PR-0.1 hoặc PR-C2>
TASK_TITLE=<mô tả ngắn>
BASE_BRANCH=main
BRANCH=agent-<lane>/<phase>-<milestone>-<short-topic>
```

### 0.1 Canonical task IDs

Repo docs dùng canonical PR IDs:

```text
Foundation: PR-0.1, PR-0.2, PR-0.3
Lane A: PR-A1, PR-A2
Lane B: PR-B1, PR-B2
Lane C: PR-C1, PR-C2, PR-C3, PR-D1, PR-D2
Lane D: PR-E1, PR-E2, PR-F1, PR-F2, PR-Q0, PR-Q1, PR-Q2
```

Nếu coordinator dùng alias ngoài docs, normalize trước khi code. Nếu không map được:

```text
Lane [X] BLOCKED
━━━━━━━━━━━━━━━━
Reason: TASK_ID uses non-canonical alias [alias] not defined in PRD set.
Action needed: coordinator must map alias to canonical PR-ID before implementation.
━━━━━━━━━━━━━━━━
```

---

## 1. TL;DR — Quy tắc bất biến

1. **Luôn fetch/pull code mới trước khi làm**:
   ```bash
   git fetch origin --prune
   git checkout <BASE_BRANCH>
   git pull --ff-only origin <BASE_BRANCH>
   ```
2. **Không code nếu dependency artifact chưa có**. Verify theo §8 trước khi tạo branch. Nếu thiếu artifact, báo `BLOCKED`, không tự tạo workaround phá contract.
3. **Branch naming**:
   ```text
   agent-<lane>/<phase>-<milestone>-<short-topic>
   ```
   Ví dụ: `agent-a/0-1-plugin-shell`, `agent-c/c2-support-rebar-hooks`, `agent-d/f2-xte-excel-export`.
4. **PR nhỏ, đúng boundary**:
   - Interface/contract PR: ≤200 LOC nếu khả thi.
   - Functional domain PR: 300–900 LOC.
   - AutoCAD integration/manual test PR có thể lớn hơn nhưng phải có checklist.
5. **Commit theo mốc, conventional commits, không amend**.
6. **Rebase trước khi push/PR**; chỉ dùng `--force-with-lease` trên branch riêng nếu đã rebase branch của mình.
7. **Git safety**: không push main, không reset hard/clean, không đổi git config, không commit secrets/DWG khách hàng/Excel chứa dữ liệu thật.
8. **Không thay locked business rules** nếu không có ADR/change request: `Abv`, `lmoc`, `40D`, `11700`, `50 mm`, `L/4`, `ST_BIM_REBAR`, `TKCT`, command names.
9. **Không sửa scope ngoài PRD đang làm**. Nếu cần chạm shared files/contract, request lock/coordinator approval.
10. **Nếu docs mâu thuẫn code hoặc PRD mâu thuẫn nhau**, dừng tại điểm cần quyết định và báo coordinator; không tự chọn bằng cảm tính.

---

## 2. Repo context & stack

Target là AutoCAD plugin:

- C# .NET plugin DLL cho AutoCAD 2018–2024.
- Autodesk AutoCAD .NET API: `ApplicationServices`, `DatabaseServices`, `EditorInput`, `Geometry`, `Runtime`.
- UI: WinForms hoặc WPF, chốt một framework trong Phase 0.
- Excel export: Microsoft Office COM Interop, ghi batch bằng `Range.Value2`.
- Metadata: AutoCAD XData với RegApp/AppID `ST_BIM_REBAR`.
- Core commands: `BDAM_TCVN`, `GT`, `TKTD`, `XTE`.

Target workflow MVP:

```text
BDAM_TCVN input UI
  -> BeamGeometry
  -> Rebar/Stirrup/Leader drawing
  -> XData ST_BIM_REBAR attach
  -> TKTD CAD Table
  -> XTE Excel TKCT export
```

---

## 3. Tài liệu phải đọc trước khi code

Đọc đúng thứ tự:

1. `docs/BDAM_TCVN/PRD_INDEX.md`
2. PRD liên quan:
   - Lane A: `PRD Full/prd1_foundation_architecture_full_v1.0.md`, `PRD Full/prd2_ui_input_geometry_bdam_full_v1.0.md`
   - Lane B: `PRD Full/prd5_bim_xdata_contract_full_v1.0.md`
   - Lane C: `PRD Full/prd3_longitudinal_rebar_gt_full_v1.0.md`, `PRD Full/prd4_stirrups_leaders_annotations_full_v1.0.md`
   - Lane D: `PRD Full/prd6_cad_table_tktd_statistics_full_v1.0.md`, `PRD Full/prd7_excel_xte_export_full_v1.0.md`, `PRD Full/prd8_qa_packaging_agent_workflow_full_v1.0.md`
3. Original final PRD: `PRD_BDAM_TCVN_Final.md` nếu có trong docs.
4. README/build instructions of repo.
5. Existing AutoLISP/C# code, if present.

---

## 4. Lane assignment

| Lane | Domain | Main sequence |
|---|---|---|
| A | Foundation, plugin shell, UI/Input, BeamGeometry | PR-0.1 → PR-0.2 → PR-0.3 → PR-A1 → PR-A2 |
| B | XData/BIM contract + shared data services | PR-B1 → PR-B2 |
| C | Rebar, stirrup, leader, annotation, GT | PR-C1 → PR-C2 → PR-C3 → PR-D1 → PR-D2 |
| D | TKTD, XTE, QA, packaging, release | PR-E1 → PR-E2 → PR-F1 → PR-F2 → PR-Q0 → PR-Q1 → PR-Q2 |

Concurrency:

```text
Phase 0: Lane A only until plugin shell + shared contracts exist
Phase 1: Lane A + B interface prep
Phase 2: Lane C after BeamGeometry + XData contracts
Phase 3: Lane D after XData + sample RebarRecords exist
Phase 4: QA/release after all core commands pass smoke test
```

---

## 5. Locked decisions không được đổi

1. Plugin target là AutoCAD C# .NET DLL, không tiếp tục mở rộng AutoLISP làm sản phẩm chính.
2. Command names giữ nguyên: `BDAM_TCVN`, `GT`, `TKTD`, `XTE`.
3. `DocumentLock` + `Transaction` quanh mọi AutoCAD DB mutation.
4. UI chỉ thu thập/validate input; logic business nằm trong Models/Calculators/Services.
5. XData AppID canonical: `ST_BIM_REBAR`.
6. XData fields tối thiểu: `BeamID`, `Mark`, `Diam`, `Qty`, `LengthMM`, `ShapeString`, `NumCK`, `BarType`, `SourceCommand`, `SchemaVersion`.
7. Stirrup edge distance = 50 mm.
8. Stirrup support-zone default = `L/4` (`div_L=4`).
9. Hook length uses `lmoc`; layer/offset logic uses `Abv`.
10. Lap splice = `40D`; max stock length = 11700 mm unless ADR changes.
11. TKTD reads XData as source of truth; do not parse leader text as primary data.
12. XTE uses running Excel instance + `TKCT` sheet preference + reverse scan + batch `Value2`.
13. No AI/LLM runtime inside plugin MVP; agent only supports development.
14. No big-bang PR; build module-by-module with parity checks.

---

## 6. Roadmap & task mapping

### Phase 0 — Foundation

| PR | Owner | Output |
|---|---|---|
| PR-0.1 | A | Solution skeleton, AutoCAD references, command registry stub |
| PR-0.2 | A | DocumentLock/Transaction command base, error handling, layer/style service |
| PR-0.3 | A/B | Shared models/contracts: BeamInput, BeamGeometry, RebarXDataRecord, StatisticsRow |

### Phase 1 — UI/Input/Geometry

| PR | Owner | Output |
|---|---|---|
| PR-A1 | A | BDAM_TCVN UI + validation + config defaults |
| PR-A2 | A | BeamGeometryCalculator + beam outline/support/dimension drawing |

### Phase 2 — Rebar/Stirrups/Annotation

| PR | Owner | Output |
|---|---|---|
| PR-C1 | C | Longitudinal rebar calculator, Abv layer offsets |
| PR-C2 | C | Support/extra rebar, lmoc hook, 40D splice/11700 handling |
| PR-C3 | C | GT supplementary rebar command |
| PR-D1 | C | Stirrup 3-zone distribution, 50 mm edge, L/div_L |
| PR-D2 | C | Leader/text annotation and CAD presentation polish |

### Phase 3 — XData

| PR | Owner | Output |
|---|---|---|
| PR-B1 | B | XData schema + RegApp registration + attach/read service |
| PR-B2 | B/C | Integrate XData into BDAM_TCVN, GT, stirrup/rebar entities + corrupt handling |

### Phase 4 — TKTD/XTE

| PR | Owner | Output |
|---|---|---|
| PR-E1 | D | TKTD selection, XData ingestion, grouping/statistics |
| PR-E2 | D | CAD Table renderer, shape/block integration |
| PR-F1 | D | Excel COM resolver, TKCT sheet mapping |
| PR-F2 | D | Reverse scan safe row + Value2 batch export |

### Phase 5 — QA/Packaging

| PR | Owner | Output |
|---|---|---|
| PR-Q0 | D/A | Golden cases + parity checklist |
| PR-Q1 | D | End-to-end smoke: BDAM_TCVN → TKTD → XTE |
| PR-Q2 | A/D | Packaging, install docs, release checklist |

---

## 7. Dependency matrix & trigger conditions

```text
PR-0.1 -> PR-0.2 -> PR-0.3
PR-A1 blocked by PR-0.2
PR-A2 blocked by PR-A1
PR-B1 blocked by PR-0.3
PR-C1 blocked by PR-A2 + PR-B1 contract
PR-C2 blocked by PR-C1
PR-C3 blocked by PR-B1
PR-D1 blocked by PR-A2
PR-D2 blocked by PR-C1 + PR-D1
PR-B2 blocked by PR-C1 + PR-D1 + PR-C3
PR-E1 blocked by PR-B2
PR-E2 blocked by PR-E1
PR-F1 blocked by PR-E2
PR-F2 blocked by PR-F1
PR-Q1 blocked by PR-A2 + PR-B2 + PR-D2 + PR-E2 + PR-F2
```

---

## 8. Dependency artifact checks

Run after pulling base, before coding.

### 8.1 Base docs

```bash
python - <<'PY'
from pathlib import Path
required = ['README.md', 'docs/BDAM_TCVN/PRD_INDEX.md', 'docs/BDAM_TCVN/PRD Full']
for p in required:
    assert Path(p).exists(), f'missing {p}'
print('base docs present')
PY
```

### 8.2 Foundation artifacts

```bash
python - <<'PY'
from pathlib import Path
required = [
  'src',
]
for p in required:
    assert Path(p).exists(), f'missing {p}'
print('foundation path present; inspect project-specific structure next')
PY
```

Then use code search appropriate to repo:

```bash
rg "CommandMethod|BDAM_TCVN|GT|TKTD|XTE" .
rg "DocumentLock|Transaction" .
rg "ST_BIM_REBAR|ResultBuffer|RegApp" .
rg "Value2|Microsoft.Office.Interop.Excel|TKCT" .
```

If verification fails:

```text
Lane [X] BLOCKED
━━━━━━━━━━━━━━━━
Waiting on: [dependency PR]
Reason: Need artifact [specific file/class/API]
Verified: pulled base [commit], checks failed: [commands]
Action needed: merge/fix [dependency] or Lead decision
━━━━━━━━━━━━━━━━
```

---

## 9. Shared critical files & conflict prevention

Lead owns lock windows for:

- AutoCAD command registry / plugin entry assembly.
- Shared models/contracts (`BeamInput`, `BeamGeometry`, `RebarXDataRecord`, `StatisticsRow`).
- Layer/style catalog.
- XData schema constants.
- Excel template mapping.
- Package/project files and AutoCAD reference configuration.

File ownership should be documented once repo structure exists. If conflict:

1. Stop new feature work.
2. Fetch + rebase base.
3. Resolve clerical conflicts autonomously.
4. If architecture/contract conflict, escalate to Integration Lead.
5. Rerun checks.
6. Push with `--force-with-lease` only to own branch.

---

## 10. Workflow per task

1. Read docs in §3 + assigned PRD.
2. Normalize `TASK_ID` to canonical PR-ID.
3. Verify dependencies using §8.
4. Plan todo list from PRD acceptance criteria.
5. Request lock if shared critical file/project config is touched.
6. Create branch from latest base.
7. Implement focused scope only.
8. Commit after milestones: model/contract → calculator/service → command/UI → tests/docs.
9. Rebase latest base before push/PR.
10. Run repo checks. If no AutoCAD CI exists, run available unit tests and manual smoke checklist.
11. Open PR to `<BASE_BRANCH>` with PR template.
12. Wait CI/check gate; fix failures.
13. Report status/unblocks.

---

## 11. Definition of Done

### 11.1 Global DoD

- Branch riêng, rebased latest base before PR.
- PR nhỏ theo milestone.
- Không thay locked decisions.
- Build/checks pass or documented blocker if AutoCAD/Excel unavailable.
- No secrets/customer DWG/Excel committed.
- Transaction rollback path covered for command errors.
- No untyped magic strings for locked XData fields.
- User-facing errors in Vietnamese or clear technical English per repo standard.

### 11.2 AutoCAD Plugin DoD

- Command registered with `[CommandMethod]` where applicable.
- All DB writes under `DocumentLock` + `Transaction`.
- ESC/cancel path does not mutate drawing.
- Created entities assigned layer/style through shared service.
- Smoke-tested in AutoCAD where possible.

### 11.3 Geometry/Rebar/Stirrup DoD

- Unit tests for calculators independent of AutoCAD DB.
- Golden cases cover mút thừa trái/phải/cả hai.
- Abv/lmoc/40D/11700/50mm/L/4 rules verified.
- ShapeString and NumCK generated consistently.

### 11.4 XData/TKTD/XTE DoD

- `ST_BIM_REBAR` RegApp registration idempotent.
- XData schema validated and versioned.
- TKTD groups records correctly and creates CAD Table.
- XTE writes TKCT via reverse scan + batch `Value2`.
- Excel COM references cleaned up.

### 11.5 QA/Release DoD

- 3+ golden end-to-end cases pass.
- Parity differences vs AutoLISP V39.1 documented.
- Install/NETLOAD instructions present.
- Rollback notes present.

---

## 12. Anti-patterns — bị reject

| Anti-pattern | Lý do | Đúng cách |
|---|---|---|
| Vẽ entity ngoài Transaction | Dễ corrupt DWG | Luôn dùng command base + transaction wrapper |
| Parse leader text để thống kê | Text không phải source of truth | Đọc XData `ST_BIM_REBAR` |
| Hard-code XData ResultBuffer rải rác | Schema drift | Dùng XDataService |
| Ghi Excel từng cell | Chậm, treo Excel | Ghi mảng qua `Range.Value2` |
| Ghi đè sheet TKCT từ dòng cố định | Mất dữ liệu/template | Reverse scan safe row |
| Tự đổi Abv/lmoc/40D/11700 | Sai nghiệp vụ gốc | ADR/change request |
| Big-bang all commands một PR | Khó review/test | PR nhỏ theo roadmap |
| Commit DWG/Excel khách hàng thật | Rủi ro dữ liệu | Dùng fixture sanitized |
| Swallow exception im lặng | User không biết fail | Log/Editor.WriteMessage + rollback |

---

## 13. Metrics / diagnostic policy

Forbidden diagnostic payloads:

```text
full customer file path, customer project name if sensitive, raw Excel workbook path,
PII in drawing title block, proprietary DWG content dump
```

Allowed patterns:

- bounded `command`
- bounded `phase`
- bounded `status`
- bounded `reason`
- `golden_case_id`
- elapsed milliseconds / entity count / row count

---

## 14. PR template

```markdown
## Macro-context
- [ ] Foundation / Architecture
- [ ] UI / Beam Geometry
- [ ] Rebar / GT
- [ ] Stirrups / Annotation
- [ ] XData / BIM
- [ ] TKTD Statistics
- [ ] XTE Excel
- [ ] QA / Packaging

## PRD reference
- PRD #N section ...
- Final PRD section ...

## Scope
<focused scope; list commands touched>

## Dependencies verified
- Base branch: `<BASE_BRANCH>`
- Last pulled/rebased commit: `<sha>`
- Artifact checks run:
  - [ ] Command registry
  - [ ] BeamGeometry contract
  - [ ] XData contract
  - [ ] Statistics dataset
  - [ ] Excel template mapping
- Missing artifacts: `<none or list>`

## AutoCAD impact
- [ ] No AutoCAD DB mutation
- [ ] Uses DocumentLock + Transaction
- [ ] Command cancel path safe
- [ ] Layers/styles via shared service

## Tests run
- [ ] Build
- [ ] Unit calculator/service
- [ ] Command smoke/manual AutoCAD
- [ ] Golden case
- [ ] Excel export smoke

## Rollback notes
<how to safely revert or disable>

## Unblocks
- [next PR/lane]
```

---

## 15. Communication templates

### Completion message

```text
Lane [X] Status Update
━━━━━━━━━━━━━━━━━━━━━━━━
Completed: [PR-ID] [title]
PR: #[number]
Merged? [yes/no]
Unblocks:
  - Lane [Y]: [next PR]
Tests run:
  - [build/unit/manual smoke/golden case]
Shared files touched:
  - [none/list]
━━━━━━━━━━━━━━━━━━━━━━━━
```

### Blocked message

```text
Lane [X] BLOCKED
━━━━━━━━━━━━━━━━
Waiting on: [PR-ID] from Lane [Y]
Reason: Need artifact [specific file/class/API]
Verified: pulled base [commit], checks [failed checks]
Action needed: merge/fix [dependency] or Lead decision
━━━━━━━━━━━━━━━━
```

---

## 16. Lane-specific quick start

### If `LANE=A`

```text
Bạn phụ trách Foundation + UI/Input + BeamGeometry.
Đọc PRD #1 và PRD #2.
Implement PR-0.1/0.2/0.3 rồi PR-A1/A2, không gộp.
Acceptance bắt buộc: commands registered, transaction wrapper, UI validation,
BeamGeometry anchors, outline/support/dimension drawing.
```

### If `LANE=B`

```text
Bạn phụ trách XData/BIM contract.
Đọc PRD #5 + contracts từ PRD #1/#3/#4.
Implement PR-B1/B2.
Acceptance: RegApp ST_BIM_REBAR idempotent, schema typed/versioned,
BDAM_TCVN + GT entities attach/read được, corrupt XData không crash TKTD.
```

### If `LANE=C`

```text
Bạn phụ trách Rebar/Stirrup/Annotation/GT.
Đọc PRD #3 và PRD #4, consume BeamGeometry từ PRD #2 và XData contract từ PRD #5.
Acceptance: Abv offsets, lmoc hook, 40D/11700 handling, GT metadata,
stirrup 3-zone L/4, 50mm edge, leader/text correct.
```

### If `LANE=D`

```text
Bạn phụ trách TKTD/XTE/QA/Packaging.
Đọc PRD #6/#7/#8 và XData contract PRD #5.
Acceptance: TKTD grouping + CAD Table, XTE TKCT reverse scan + Value2 batch,
3 golden end-to-end cases, release/install checklist.
```

---

## 17. Final instruction to DEV Agent

Bạn là DEV Agent trong team multi-agent. Hãy:

1. Set `LANE`, `TASK_ID`, `TASK_TITLE`, `BASE_BRANCH`, `BRANCH`.
2. Pull latest base.
3. Normalize `TASK_ID` to canonical PR-ID.
4. Read docs/PRD liên quan.
5. Verify dependency artifacts.
6. Nếu artifact thiếu, block theo template; không code workaround.
7. Nếu đủ artifact, tạo todo list và implement đúng scope.
8. Rebase latest base trước push/PR.
9. Run checks/build/manual smoke theo repo baseline.
10. Open PR with template.
11. Wait CI/check gate and report status/unblocks.

Không tự ý đổi roadmap, locked decisions, branch base, hay cross-PRD contract. Sử dụng autonomous polling để theo dõi PR/CI và tiếp tục sửa đến khi task hoàn tất hoặc gặp blocker thật.
