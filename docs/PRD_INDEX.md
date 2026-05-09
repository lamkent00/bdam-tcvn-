# BDAM_TCVN PRD Set Index

**Project**: BDAM_TCVN AutoCAD C# .NET Plugin  
**Generated**: 2026-05-09  
**Source of Truth**: `PRD_BDAM_TCVN_Final.md`  
**Standard template**: adapted from `prd1_identity_tenancy_full_v2.1.md` with PART A/B/C/D structure.

## PRD inventory

| PRD | Title | File | Macro-context |
|---|---|---|---|
| PRD #1 | Foundation, Standards & AutoCAD .NET Architecture | prd1_foundation_architecture_full_v1.0.md | Foundation & Architecture |
| PRD #2 | UI, Input Model & BDAM_TCVN Beam Geometry | prd2_ui_input_geometry_bdam_full_v1.0.md | UI/Input + Beam Geometry |
| PRD #3 | Longitudinal Rebar, Support Rebar & GT Extra Rebar | prd3_longitudinal_rebar_gt_full_v1.0.md | Rebar Detailing |
| PRD #4 | Stirrups, Leaders, Annotation & CAD Presentation | prd4_stirrups_leaders_annotations_full_v1.0.md | Stirrups & Annotation |
| PRD #5 | BIM/XData Entity Metadata Contract | prd5_bim_xdata_contract_full_v1.0.md | BIM/XData Contract |
| PRD #6 | TKTD CAD Quantity Takeoff & Statistics Table | prd6_cad_table_tktd_statistics_full_v1.0.md | CAD Statistics / TKTD |
| PRD #7 | XTE Excel Export & Template Interop | prd7_excel_xte_export_full_v1.0.md | Excel Export / XTE |
| PRD #8 | QA, Packaging, Release Roadmap & Agent Workflow | prd8_qa_packaging_agent_workflow_full_v1.0.md | QA/Release + Dev Agent Workflow |

## Recommended implementation dependency order

```text
PRD #1 Foundation
  -> PRD #2 UI/Input/Geometry
  -> PRD #3 Rebar + PRD #4 Stirrups/Annotation
  -> PRD #5 XData Contract
  -> PRD #6 TKTD Statistics
  -> PRD #7 XTE Excel Export
  -> PRD #8 QA/Packaging/Agent Workflow
```

## Core rules carried from Final PRD

- Commands: `BDAM_TCVN`, `GT`, `TKTD`, `XTE`.
- Standards: TCVN 5574:2018, TCVN 4453 as design/detailing references.
- Critical logic: `Abv` offsets, `lmoc` hooks, `40D` lap, `11700` max stock length, `50 mm` stirrup edge distance, `L/4` default support zone.
- Data: XData AppID `ST_BIM_REBAR`, fields `BeamID`, `Mark`, `Diam`, `Qty`, `LengthMM`, `ShapeString`, `NumCK`.
- Outputs: CAD drawing, CAD Table TKTD, Excel sheet `TKCT` via XTE batch `Value2`.
