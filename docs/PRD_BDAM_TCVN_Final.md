# PRD hoàn chỉnh – BDAM_TCVN AutoCAD C# .NET Plugin

## 1. Thông tin tài liệu

- Tên sản phẩm: **BDAM_TCVN C# Plugin**
- Loại tài liệu: Product Requirements Document (PRD)
- Phiên bản: **2.0 – bản hoàn thiện từ tài liệu Word gốc**
- Ngày tổng hợp: 2026-05-08
- Mục tiêu tài liệu: chuẩn hóa toàn bộ yêu cầu nghiệp vụ, kỹ thuật, UI/UX, dữ liệu BIM/XData, thống kê CAD, xuất Excel, kiến trúc triển khai và phạm vi MVP cho việc chuyển đổi hệ sinh thái AutoLISP BDAM_TCVN sang plugin AutoCAD C# .NET.

## 2. Nguồn tài liệu đã rà soát

| STT | Nguồn | Vai trò trong PRD |
|---:|---|---|
| 1 | `prd-bdam-tcvn.md` | Bản PRD tổng hợp ban đầu, dùng làm nền cấu trúc. |
| 2 | `DÙNG PROMPT này.docx` | Project brief, 8 prompt/module triển khai, quy tắc AutoCAD API, XData, UI, BOM/Excel. |
| 3 | `Đây là cấu trúc chi tiết cho các file cấu hình tác tử...docx` | Agent rules, skills/workflows, task groups, quy trình tự động hóa và UI specification chi tiết. |
| 4 | `CODE và Prompt trong gemini.docx` | Nguồn quan trọng nhất: chứa AutoLISP V39.1 hoàn chỉnh, các lệnh `BDAM_TCVN`, `GT`, `TKTD`, `XTE`, logic rải đai, móc L, XData, thống kê CAD, xuất Excel và prompt tạo code C#. |
| 5 | `CHITIET LAM GG.docx` | Cấu hình dự án, core rules, WPF/UI, phân vai PM/UI/Core/Excel/QA, yêu cầu XData/Excel. |
| 6 | `Luồng thực thi GG anigravity.docx` | Luồng phối hợp agent, cách dùng YAML/JSON task, yêu cầu approval UI trước khi triển khai backend. |

Phần lõi PRD dưới đây ưu tiên yêu cầu sản phẩm/plugin AutoCAD. Các nội dung phối hợp AI agent, prompt và workflow được đưa vào phụ lục để không làm loãng yêu cầu sản phẩm.

## 3. Executive Summary

BDAM_TCVN là dự án chuyển đổi hệ sinh thái AutoLISP shopdrawing dầm bê tông cốt thép liên tục nhiều nhịp sang plugin AutoCAD dạng DLL viết bằng C# .NET. Sản phẩm mục tiêu phải chạy trực tiếp trong AutoCAD 2018–2024, cho phép kỹ sư nhập tham số qua UI, tự động vẽ dầm BTCT nhiều nhịp có/không có mút thừa, bố trí thép chủ, thép tăng cường, thép đai, leader, dimension, gắn dữ liệu BIM/XData vào entity, tạo bảng thống kê trong CAD và xuất dữ liệu sang Excel template hiện hữu.

Yêu cầu cốt lõi không chỉ là “vẽ hình học dầm”, mà là bảo toàn logic nghiệp vụ đã được tích lũy trong AutoLISP V39.1:

- Lệnh `BDAM_TCVN` vẽ dầm, gối/cột, mút thừa, thép, leader và dữ liệu BIM.
- Lệnh `GT` gán thép bổ sung và leader cho các polyline/line đã chọn.
- Lệnh `TKTD` đọc XData `ST_BIM_REBAR`, phân tích hình dạng thép, gom nhóm, tính chiều dài/khối lượng và sinh CAD Table.
- Lệnh `XTE` xuất bảng thống kê từ CAD sang Excel đang mở, ưu tiên sheet `TKCT`, bảo toàn dữ liệu cũ và không phá công thức template.
- Logic quan trọng: rải đai cách mép 50 mm, chia 3 vùng theo `L/div_L` mặc định `L/4`, móc L theo `lmoc`, offset thép theo `Abv`, nối chồng 40D trong vùng an toàn, chiều dài phôi tối đa 11.7 m, XData schema thống nhất.

Khuyến nghị sản phẩm: **Go theo MVP có kiểm soát**, ưu tiên workflow “nhập liệu -> vẽ CAD -> gắn XData -> thống kê CAD -> xuất Excel” trước khi mở rộng sang tự tính As, kiểm tra chịu lực, dynamic block hay cloud library.

## 4. Bối cảnh và vấn đề

### 4.1 Hiện trạng

Hệ thống hiện hữu đang ở dạng AutoLISP, phiên bản logic được mô tả là “Shop Drawing Dầm V39.1 – Perfect Detailing”. Hệ thống đã có khả năng dùng thực tế trong AutoCAD với các lệnh:

- `BDAM_TCVN`: vẽ dầm bê tông cốt thép liên tục nhiều nhịp, hỗ trợ mút thừa trái/phải, thép chủ, thép tăng cường, thép đai, leader.
- `GT`: gán dữ liệu BIM/XData cho thép bổ sung và vẽ leader.
- `TKTD`: thống kê thép dầm từ XData, tự động gom nhóm và tính trọng lượng.
- `XTE`: xuất thống kê sang Excel bằng thuật toán quét ngược, ghi dữ liệu an toàn.

AutoLISP hiện tại chứa nhiều “rule ngầm” có giá trị nghiệp vụ cao, bao gồm offset theo `Abv`, móc L, rải đai 50 mm, XData `ST_BIM_REBAR`, block hình dạng thép `RBSHxx`, bảng thống kê 10 cột và mapping Excel template.

### 4.2 Vấn đề cần giải quyết

AutoLISP phù hợp để phát triển nhanh nhưng gặp hạn chế khi muốn thương mại hóa hoặc mở rộng dài hạn:

- Khó bảo trì khi code và logic ngày càng lớn.
- Khó kiểm thử từng module, khó viết unit/integration test.
- UI DCL hạn chế validation, usability và khả năng lưu cấu hình.
- Khó quản lý lỗi, memory leak, transaction và rollback ở quy mô lớn.
- Khó đóng gói thành sản phẩm DLL chuyên nghiệp.
- Khó mở rộng sang kiến trúc BIM/BOM/Excel chuẩn hóa.

### 4.3 Cơ hội

Chuyển sang C# .NET AutoCAD API giúp:

- Tách module rõ ràng: command, model, calculator, drawer, XData, BOM, Excel.
- Có UI WinForms/WPF chuyên nghiệp, validation tốt, ít nhập sai.
- Quản lý `DocumentLock`, `Transaction`, exception và COM interop chặt chẽ.
- Dễ đóng gói DLL, versioning, QA và triển khai nội bộ/thương mại.
- Có nền tảng mở rộng cho tính toán TCVN, thư viện dự án và workflow BIM.

## 5. Mục tiêu sản phẩm

### 5.1 Mục tiêu chính

Xây dựng plugin AutoCAD bằng C# .NET thay thế lõi AutoLISP BDAM_TCVN, cho phép kỹ sư kết cấu/shopdrawing nhập tham số dầm BTCT nhiều nhịp và tự động tạo bản vẽ, dữ liệu thống kê và Excel output theo logic TCVN và workflow thực tế hiện có.

### 5.2 Mục tiêu cụ thể

- Vẽ dầm BTCT liên tục nhiều nhịp.
- Hỗ trợ mút thừa trái, mút thừa phải, cả hai hoặc không có mút thừa.
- Vẽ gối/cột, breakline, đường bao bê tông, thép dọc, thép tăng cường, thép đai, leader, dimension.
- Bảo toàn các logic kỹ thuật đã được nhấn mạnh trong AutoLISP V39.1.
- Gắn metadata BIM/XData cho từng thanh thép hoặc đối tượng đại diện thống kê.
- Sinh bảng thống kê thép trong CAD.
- Xuất bảng thống kê sang Excel đang mở, ưu tiên sheet `TKCT`, không ghi đè dữ liệu cũ.
- Có UI nhập liệu tham số hóa 100%, validation rõ ràng, có default hợp lý và có thể lưu cấu hình gần nhất.
- Tách code đủ sạch để AI agent/dev team có thể phát triển module-by-module nhưng vẫn cần checkpoint kiểm duyệt.

### 5.3 Không phải mục tiêu của MVP

- Không tự tính thiết kế chịu lực đầy đủ.
- Không thay thế kỹ sư kiểm tra TCVN.
- Không cố gắng “AI tự viết một phát xong hết” mà không qua review UI, CAD output và Excel output.
- Không mở rộng thành multi-agent platform riêng trong release đầu.

## 6. Người dùng mục tiêu

### 6.1 Primary users

- Kỹ sư shopdrawing kết cấu BTCT.
- Kỹ sư kết cấu triển khai bản vẽ dầm khung dân dụng/công nghiệp.
- Người dùng AutoCAD cần tự động hóa vẽ và thống kê cốt thép dầm.

### 6.2 Secondary users

- PM kỹ thuật/team lead quản lý việc chuyển đổi từ LISP sang C#.
- CAD/Core developer phát triển plugin AutoCAD .NET.
- QA kiểm tra output bản vẽ, dữ liệu thống kê và Excel.
- Người quản lý template CAD/Excel/BIM trong doanh nghiệp.

### 6.3 Jobs-to-be-done

- Tôi muốn nhập nhanh thông số dầm nhiều nhịp để tool tự vẽ đúng logic shopdrawing.
- Tôi muốn xử lý mút thừa trái/phải mà không phải vẽ tay nhiều lần.
- Tôi muốn thép, leader, bảng thống kê và Excel luôn đồng bộ dữ liệu.
- Tôi muốn giảm lỗi mismatch giữa hình vẽ và bảng thống kê.
- Tôi muốn tool dễ bảo trì và mở rộng hơn AutoLISP cũ.

## 7. Phạm vi sản phẩm

### 7.1 In-scope cho MVP/Release 1

1. Command `BDAM_TCVN`
   - UI nhập liệu.
   - Tính hình học dầm/gối/mút thừa.
   - Vẽ đường bao bê tông và breakline.
   - Vẽ thép chủ trên/dưới lớp 1/lớp 2.
   - Vẽ thép tăng cường gối trên và nhịp dưới.
   - Vẽ thép đai theo vùng.
   - Vẽ leader/thông tin thép.
   - Tạo dimension cơ bản.
   - Gắn XData cho thép phục vụ thống kê.

2. Command `GT`
   - Chọn line/polyline thép bổ sung.
   - Nhập thông tin cấu kiện, mark, đường kính, số lượng, số cấu kiện.
   - Tính chiều dài polyline/line.
   - Gắn XData và vẽ leader.

3. Command `TKTD`
   - Quét đối tượng có XData `ST_BIM_REBAR`.
   - Parse dữ liệu thép.
   - Phân tích hình dạng thép.
   - Gom nhóm, tính tổng chiều dài và khối lượng.
   - Tạo CAD Table 10 cột theo format hiện hữu.

4. Command `XTE`
   - Chọn CAD Table thống kê.
   - Kết nối Excel đang mở.
   - Tìm sheet `TKCT` theo Display Name.
   - Quét ngược tìm vùng ghi an toàn.
   - Ghi dữ liệu sang Excel theo mapping template.

5. UI nhập liệu
   - Hình học.
   - Thép chủ.
   - Thép tăng cường.
   - Thép đai.
   - Cài đặt bản vẽ.
   - Validation và default values.

### 7.2 Out-of-scope cho MVP nhưng cần thiết kế mở rộng

- Tự động tính As theo TCVN.
- Kiểm tra điều kiện chịu lực.
- Vẽ mặt cắt ngang nâng cao.
- Dynamic block phức tạp cho thép.
- Thay thế Excel Interop bằng OpenXML/EPPlus ở release đầu.
- Cloud/project library.
- Multi-standard ngoài TCVN.
- Multi-agent orchestration như một sản phẩm độc lập.

## 8. Workflow sản phẩm lõi

### 8.1 Workflow chính

1. Người dùng gọi lệnh `BDAM_TCVN` trong AutoCAD.
2. Plugin mở form nhập liệu.
3. Người dùng nhập/chỉnh thông số và chọn “Vẽ Dầm”.
4. Plugin validate input, map dữ liệu sang DTO.
5. Người dùng chọn điểm chèn trong bản vẽ.
6. Plugin tính hình học, tạo layer, vẽ bê tông, gối, mút thừa, thép, đai, leader, dimension.
7. Plugin gắn XData `ST_BIM_REBAR` cho các entity/thép cần thống kê.
8. Người dùng gọi `TKTD`, chọn vùng/đối tượng cần thống kê.
9. Plugin sinh CAD Table.
10. Người dùng mở Excel template và gọi `XTE`.
11. Plugin xuất dữ liệu từ CAD Table sang sheet `TKCT` mà không ghi đè dữ liệu cũ.

### 8.2 Workflow bổ sung

- Với thép bổ sung không sinh tự động, người dùng vẽ line/polyline, gọi `GT`, nhập thông tin và để plugin gắn XData + leader.
- CAD Table có thể gồm nhiều bảng; `XTE` phải xử lý nhiều bảng được chọn, sắp xếp từ trên xuống dưới theo vị trí chèn để ghi Excel có trật tự.

## 9. Input model và tham số bắt buộc

### 9.1 Thông tin chung

- `BeamID`: tên cấu kiện/dầm, ví dụ `D1`.
- `NumCK`: số cấu kiện cùng loại, mặc định 1.
- `Scale`: tỷ lệ bản vẽ. Bản LISP dùng mặc định 20; UI có thể cung cấp preset 1:20, 1:25, 1:50, 1:100.
- `TextHeight`: có thể tính từ scale theo rule hiện hữu `txt_h = 2.4 * Scale` hoặc cho phép override.
- `InsertionPoint`: điểm chèn do người dùng chọn trong AutoCAD.

### 9.2 Hình học dầm

MVP cần hỗ trợ cả cách nhập chuỗi tương thích LISP và UI bảng dễ dùng:

- `SpanCount N >= 1`.
- Danh sách nhịp, mỗi nhịp gồm `(L, B, H)`.
- Danh sách bề rộng cột/gối `Bc`, số lượng phải bằng `N + 1`.
- Mút thừa trái `(L_mt, B_mt, H_mt)`, nhập `0 0 0` hoặc bỏ trống nếu không có.
- Mút thừa phải `(L_mp, B_mp, H_mp)`, nhập `0 0 0` hoặc bỏ trống nếu không có.
- `Abv`/cover: lớp bảo vệ bê tông, mặc định 25 mm.
- `lmoc`: chiều dài móc U/L, mặc định 200 mm.

Validation bắt buộc:

- Danh sách nhịp phải là bội số của 3 nếu dùng chuỗi `L B H`.
- Số lượng cột phải bằng `N + 1`.
- Chiều dài, bề rộng, chiều cao, cover, scale phải là số dương hợp lệ.
- Nếu có mút thừa, chiều dài mút thừa phải > 0 và có đủ B/H tương ứng.

### 9.3 Thông tin cốt thép

- Thép trên lớp 1 `T1`, ví dụ `2D20`.
- Thép trên lớp 2 `T2`, ví dụ `2D18`, có thể bỏ trống.
- Thép dưới lớp 1 `B1`, ví dụ `3D20`.
- Thép dưới lớp 2 `B2`, ví dụ `2D18`, có thể bỏ trống.
- Thép tăng cường gối trên `TC_Goi`, nhập theo chuỗi từ trái sang phải, ví dụ `2D20 3D22 2D20`.
- Thép tăng cường nhịp dưới `TC_Nhip`, nhập theo chuỗi từ trái sang phải, ví dụ `2D18 2D20`.
- Đường kính đai `DDai`, mặc định 8.
- Bước đai vùng gối `a1`, mặc định 100.
- Bước đai giữa nhịp `a2`, mặc định 200.
- Hệ số chia vùng đai `div_L`, mặc định 4, tương ứng vùng gối `L/4`.

Parsing rule tương thích LISP:

- Token dạng `qDdiam`, ví dụ `2D20`, được parse thành số lượng `q=2`, đường kính `diam=20`.
- Nếu số lượng hoặc đường kính không hợp lệ, không nên âm thầm lấy default trong UI mới; phải cảnh báo người dùng. Logic fallback chỉ dùng để tương thích migration.

### 9.4 Cài đặt layer

Plugin phải tự tạo layer nếu chưa có. Bộ layer lõi theo LISP hiện hữu:

| Layer | Mục đích | Màu gợi ý | Lineweight gợi ý |
|---|---|---:|---:|
| `RB_CONC` | Bê tông, gối, breakline | 8 | 20 |
| `RB_MAIN` | Thép chủ | 1 | 40 |
| `RB_STIR` | Thép đai | 3 | 15 |
| `RB_TEXT` | Text, leader, mark | 7 | -3 |
| `RB_TC` | Thép tăng cường | 4 | 30 |

Có thể map alias tương thích với cấu hình cũ: `DAM`, `THEP_DOC`, `THEP_DAI`, `DIM`, `TEXT`.

## 10. Yêu cầu chức năng chi tiết

### 10.1 Command `BDAM_TCVN`

Hệ thống phải:

- Mở UI nhập liệu hoặc command-driven input fallback.
- Validate toàn bộ dữ liệu trước khi vẽ.
- Cho người dùng chọn điểm chèn là mép trái cột đầu tiên.
- Tắt/bảo toàn OSNAP/layer cũ trong quá trình vẽ và khôi phục sau khi hoàn tất.
- Bọc toàn bộ thao tác ghi CAD bằng `DocumentLock` và `Transaction` trong C#.
- Khi lỗi, rollback transaction, khôi phục trạng thái CAD và báo lỗi rõ ràng.
- Thứ tự vẽ: tạo layer -> vẽ cột/gối -> vẽ mút thừa -> vẽ nhịp bê tông -> rải đai -> vẽ thép chủ -> vẽ thép tăng cường -> leader -> dimension -> XData.

### 10.2 Command `GT`

Hệ thống phải:

- Cho phép chọn `Line` hoặc `Polyline` đại diện thép bổ sung.
- Nếu chọn sai entity, cảnh báo và cho chọn lại.
- Nhập `BeamID`, `Mark`, đường kính, số lượng trong 1 cấu kiện, số cấu kiện.
- Tính chiều dài thật của line/polyline.
- Gắn XData `ST_BIM_REBAR`.
- Đổi màu/layer phù hợp cho thép bổ sung.
- Cho người dùng chọn vị trí leader.
- Vẽ leader gồm đường dẫn, vòng tròn mark và text thông tin, ví dụ `2%%C20`.

### 10.3 Command `TKTD`

Hệ thống phải:

- Cho người dùng chọn vùng/đối tượng có XData `ST_BIM_REBAR`.
- Nếu không có dữ liệu, cảnh báo rõ: không tìm thấy thép có dữ liệu BIM.
- Đọc đủ các trường XData.
- Phân tích hình dạng thép để map block hình dạng và các kích thước L1/L2/L3.
- Gom nhóm thép đai có cùng cấu kiện, shape, đường kính, chiều dài và số cấu kiện.
- Tính:
  - `QtyTotal = QtyPerCK * NumCK`.
  - `LengthMeter = LengthMM / 1000`.
  - `TotalLength = LengthMeter * QtyTotal`.
  - `WeightKg = LengthMeter * (Diam^2 / 162) * QtyTotal`.
- Sinh CAD Table 10 cột.
- Tạo dòng tổng hợp theo từng đường kính.
- Tạo dòng tổng trọng lượng toàn bộ.

### 10.4 Command `XTE`

Hệ thống phải:

- Kết nối Excel đang mở; nếu không có Excel/Workbook, báo lỗi dễ hiểu.
- Tìm worksheet theo Display Name chứa `TKCT`; nếu không có thì chỉ fallback ActiveSheet khi người dùng xác nhận hoặc cấu hình cho phép.
- Kiểm tra sheet protected; nếu bị khóa, dừng và hướng dẫn mở khóa.
- Quét ngược từ UsedRange hoặc tối đa dòng 2000 để tìm dòng dữ liệu cuối, tối thiểu từ dòng 4.
- Bắt đầu ghi cách dòng cuối ít nhất 1 dòng trống, theo logic hiện hữu `ex_r = last_data_r + 2`.
- Cho phép chọn một hoặc nhiều CAD Table.
- Sắp xếp table theo tọa độ Y từ trên xuống trước khi xuất.
- Không ghi đè ô Excel đã có dữ liệu nếu template yêu cầu bảo toàn dữ liệu.
- Ưu tiên ghi batch bằng `Value2` để giảm nghẽn COM; nếu phải ghi từng ô để bảo toàn công thức thì cần giới hạn số lần COM call và release object đúng cách.
- Release toàn bộ COM object sau khi hoàn tất.

## 11. Logic hình học và vẽ dầm

### 11.1 Tính tọa độ tổng quát

- Tổng chiều dài dầm:

```text
L_total = L_mút_trái + Σ(L_nhịp) + L_mút_phải
```

- Điểm chèn là `x_start`, `y_top` tại mép trái cột đầu tiên.
- `H_max` là chiều cao lớn nhất trong các nhịp và mút thừa, dùng làm mốc đáy tổng quát.
- Danh sách trục/mép cột `lst_x_col` được tính tuần tự:

```text
x_col[0] = x_start
x_col[i+1] = x_col[i] + Bc[i] + L_span[i]
```

- `X_total_start = x_start`.
- `X_total_end = x_col cuối + Bc cuối`.
- Không tạo gối tại đầu tự do của mút thừa.

### 11.2 Vẽ gối/cột và breakline

- Mỗi gối/cột được vẽ bằng line/outline tại vị trí `x_col[i]` đến `x_col[i] + Bc[i]`.
- Cột kéo dài lên/xuống so với dầm để thể hiện gối.
- Breakline zigzag đặt trên/dưới gối theo logic `ST_DrawBreakLine`.
- Nếu mút thừa thấp hơn `H_max`, cần vẽ đoạn đứng chuyển tiếp hợp lý để bản vẽ sạch.

### 11.3 Vẽ mút thừa

- Mút thừa trái bắt đầu từ `X_total_start - L_mt` đến `X_total_start`.
- Mút thừa phải bắt đầu từ `X_total_end` đến `X_total_end + L_mp`.
- Đai mút thừa rải từ mép ngoài +50 mm đến mép cột -50 mm.
- Leader đai mút thừa hiển thị tổng số đai và bước đai, ví dụ `n%%C8a100`.

## 12. Logic cốt thép chủ

### 12.1 Nguyên tắc chung

- Thép dưới chạy liên tục qua các nhịp.
- Thép trên có vùng tăng cường/gối và cắt giảm ở giữa nhịp theo logic TCVN/shopdrawing.
- Móc L/U ở đầu thanh theo biến `lmoc`.
- Chiều dài phôi tối đa mặc định: `11700 mm`.
- Chiều dài nối chồng: `40D`.
- Điểm cắt/nối phải nằm trong vùng an toàn, không đặt tại vị trí nguy hiểm.

### 12.2 Offset theo `Abv`

Các rule offset là yêu cầu bắt buộc, đã được nhấn mạnh nhiều lần trong tài liệu nguồn:

- Thép chủ lớp 1: cách mép bê tông `1 * Abv`.
- Thép chủ lớp 2: cách mép bê tông `2 * Abv` theo phương đặt thanh; phương đứng có thể cộng khoảng hở cấu tạo 25 mm như LISP hiện hữu.
- Thép tăng cường gối/nhịp: dùng mức `2.5 * Abv`, `3 * Abv` hoặc `3.5 * Abv` tùy ngữ cảnh lớp thép hiện hữu; MVP cần bảo toàn hành vi LISP:
  - Nếu có thép lớp 2 thì thép tăng cường lùi sâu hơn (`3.5 * Abv`).
  - Nếu không có lớp 2 thì dùng mức thấp hơn (`2.5 * Abv`).
  - Tại đầu mút có móc, điểm neo/móc phải lùi vào đúng logic khoảng `3 * Abv`.

### 12.3 Cắt thép và nối chồng

- Mỗi thanh được chia đoạn nếu vượt `L_max = 11700 mm`.
- `L_lap = 40 * Diam`.
- Với thép trên, vùng cắt ưu tiên nằm ở vùng giữa nhịp an toàn theo khoảng 0.25L–0.75L.
- Với thép dưới, vùng nối ưu tiên quanh gối theo rule hiện hữu.
- Nếu không tìm được vùng cắt an toàn, MVP có thể fallback theo reach nhưng phải log/cảnh báo để QA kiểm tra.

### 12.4 Shape string cho thống kê

- Thanh thẳng: shape string là chiều dài, ví dụ `5000`.
- Thanh có một móc: `lmoc U L` hoặc `L U lmoc`.
- Thanh có hai móc: `lmoc U L U lmoc`.
- Shape string phải đủ để `TKTD` phân tích lại hình dạng và điền L1/L2/L3.

## 13. Logic thép tăng cường

### 13.1 Thép tăng cường gối trên

- Nhập theo chuỗi từ trái sang phải; mỗi token ứng với một vị trí gối/cột.
- Với gối đầu trái:
  - Nếu có mút thừa trái, chiều neo vào nhịp lấy `max(L_right/4, 1.5 * L_mt)`.
  - Điểm bắt đầu tại đầu mút thừa lùi vào khoảng `3 * Abv`.
  - Có móc xuống với chiều dài `lmoc`.
- Với gối đầu phải:
  - Nếu có mút thừa phải, chiều neo lấy `max(L_left/4, 1.5 * L_mp)`.
  - Điểm kết thúc tại đầu mút phải lùi vào khoảng `3 * Abv`.
  - Có móc xuống với chiều dài `lmoc`.
- Với gối giữa:
  - Thanh đi từ phía trái gối `L_left/4` sang phía phải gối `L_right/4`.
  - Không cần móc nếu là thanh tăng cường giữa.
- Gắn XData với mark dạng `TC#`.

### 13.2 Thép tăng cường nhịp dưới

- Nhập theo chuỗi từ trái sang phải; mỗi token ứng với một nhịp.
- Thanh đặt ở giữa nhịp dưới.
- Điểm bắt đầu: mép trong gối trái + `L_span/6`.
- Điểm kết thúc: mép trong gối phải - `L_span/6`.
- Offset theo `2.5 * Abv` hoặc `3.5 * Abv` tùy có lớp dưới thứ hai hay không.
- Gắn XData với mark dạng `TC#`.

## 14. Logic thép đai

### 14.1 Rải đai trong nhịp

- Mỗi nhịp được chia 3 vùng:
  - Vùng gối trái: bước `a1`.
  - Vùng giữa nhịp: bước `a2`.
  - Vùng gối phải: bước `a1`.
- Chiều dài vùng gối:

```text
L_zone = L_span / div_L
```

- Mặc định `div_L = 4`, tương ứng `L/4` mỗi bên.
- Đai đầu tiên cách mép cột 50 mm.
- Đai cuối vùng gối phải cách mép cột 50 mm.
- Vùng giữa bắt đầu sau vùng gối trái theo bước `a2` và kết thúc trước vùng gối phải.
- Nếu vùng giữa không đủ chiều dài thì không rải vùng giữa, nhưng không crash.

### 14.2 Rải đai mút thừa

- Mút thừa trái/phải dùng bước `a1`.
- Đai cách mép ngoài và mép cột 50 mm.
- Leader hiển thị tổng số đai trong mút thừa.

### 14.3 Chiều dài và shape đai

- Chiều dài đai theo rule LISP:

```text
len_stir = 2 * (B - 2*Abv) + 2 * (H - 2*Abv) + 100
```

- Shape string dạng:

```text
[ (B - 2*Abv) x (H - 2*Abv) ]
```

- Shape này được `TKTD` nhận diện là đai và map block `RBSH_DAI`.

## 15. Leader, mark và annotation

### 15.1 Leader bắt buộc

Plugin cần sinh leader cho:

- Thép chủ.
- Thép tăng cường.
- Thép đai từng nhịp/mút thừa.
- Thép bổ sung qua command `GT`.

### 15.2 Thành phần leader

Leader theo logic hiện hữu gồm:

- Đường leader từ thanh thép/đai đến điểm gấp.
- Landing line.
- Circle bao quanh mark.
- Text mark nằm giữa circle.
- Text thông tin thép, ví dụ `2%%C20`, `12%%C8a150`.

Trong C#, ưu tiên dùng `MLeader` hoặc tổ hợp `Leader`/`Polyline`/`Circle`/`DBText` qua DatabaseServices, tránh `Editor.Command`.

## 16. BIM/XData và schema dữ liệu

### 16.1 AppID

- AppID bắt buộc: `ST_BIM_REBAR`.
- Nếu AppID chưa tồn tại, plugin phải đăng ký trong `RegAppTable` trước khi gắn XData.

### 16.2 Schema XData tối thiểu

| Thứ tự | Field | Kiểu logic | TypedValue gợi ý |
|---:|---|---|---|
| 1 | `BeamID` | string | 1000 |
| 2 | `Mark` | string | 1000 |
| 3 | `Diam` | number | 1040 |
| 4 | `Qty` | number | 1040 |
| 5 | `LengthMM` | number | 1040 |
| 6 | `ShapeString` | string | 1000 |
| 7 | `NumCK` | number | 1040 |

### 16.3 Yêu cầu triển khai C#

- Dùng `ResultBuffer` và `TypedValue` để set/get XData.
- Không dùng string serialization tùy tiện làm mất khả năng parse.
- Gắn XData vào entity thép chính; với thép đai có thể gắn vào leader/text đại diện nếu không tạo từng đai như object thống kê riêng, nhưng phải nhất quán để `TKTD` đọc được.
- `GetRebarXData` phải tolerant với dữ liệu thiếu field nhưng không được crash; các trường thiếu phải báo cảnh báo hoặc dùng fallback được kiểm soát.

## 17. Thống kê CAD Table

### 17.1 Cột bảng thống kê

CAD Table MVP dùng 10 cột như logic hiện hữu:

| Cột | Tên | Ý nghĩa |
|---:|---|---|
| 0 | `Cau Kien` | Tên dầm/cấu kiện |
| 1 | `So CK` | Số cấu kiện |
| 2 | `SH` | Số hiệu/mark |
| 3 | `HINH DANG` | Block hình dạng thép + L1/L2/L3 |
| 4 | `DK (D)` | Đường kính |
| 5 | `SL/1 Ck` | Số lượng trong 1 cấu kiện |
| 6 | `SL Tong` | Số lượng tổng |
| 7 | `C.DAI (mm)` | Chiều dài 1 thanh |
| 8 | `T.DAI (m)` | Tổng chiều dài |
| 9 | `K.LUONG (kg)` | Khối lượng |

### 17.2 Shape analysis

Plugin cần phân tích `ShapeString` và/hoặc geometry:

- Shape chứa `[` là đai, dùng block `RBSH_DAI` và hai kích thước cạnh.
- Shape có 1 số: thanh thẳng, block `RBSH01`.
- Shape có 2 số: thanh có một móc, dùng `RBSH04` hoặc `RBSH05` tùy móc trái/phải.
- Shape có 3 số: thanh có hai móc, dùng `RBSH02` hoặc `RBSH03` tùy hướng móc.
- `L1` luôn đại diện đoạn móc/ngắn hơn khi phân tích hook.
- `L2` là đoạn thẳng/chính.
- `L3` là móc còn lại nếu có.

### 17.3 Gom nhóm

- Thép đai được gom nếu cùng `BeamID`, `ShapeString`, `Diam`, `LengthMM`, `NumCK`.
- Nếu gom nhiều mark, mark có thể hiển thị dạng `mark1+mark2`.
- Thép thanh không nên gom nếu khác mark/shape quan trọng.
- Tổng hợp theo đường kính phải hiển thị sau danh sách chi tiết.

## 18. Xuất Excel

### 18.1 Excel target

- Excel phải đang mở trước khi chạy `XTE`.
- Workbook active phải là workbook đích.
- Worksheet ưu tiên: Display Name chứa hoặc bằng `TKCT`.
- Không được dùng CodeName như `Sheet9` làm mặc định vì có thể sai giữa các workbook.

### 18.2 Mapping dữ liệu

Mapping theo logic hiện hữu:

| Excel column | Dữ liệu |
|---:|---|
| 1 | Tên cấu kiện |
| 2 | Số hiệu/mark |
| 3 | Đường kính |
| 4 | L1 |
| 5 | L2 |
| 6 | L3 |
| 7 | Chiều dài/C.DAI hoặc dữ liệu hình học theo template |
| 8 | Số lượng/1 cấu kiện nếu có |
| 9 | Số cấu kiện |
| 10 | Số lượng tổng |
| 11 | Tổng chiều dài |
| 12 | Khối lượng |
| 13 | Tên block hình dạng, nếu cần debug/template |

Lưu ý quan trọng từ tài liệu nguồn: với thép dạng L-shape hoặc shape đặc biệt, các giá trị hình học trái/phải phải đi đúng cột template quy định, đặc biệt không đẩy sai sang cột làm hỏng công thức có sẵn. PRD khuyến nghị khóa mapping theo template Excel thực tế và đưa vào test case bắt buộc.

### 18.3 An toàn dữ liệu Excel

- Quét ngược để tìm dòng cuối có dữ liệu, tránh ghi đè.
- Không ghi vào ô đã có dữ liệu nếu rule bảo toàn template đang bật.
- Parse số học bằng `double.TryParse`/culture-safe parser trước khi ghi.
- Dùng `Value2`.
- Release COM object đúng thứ tự.
- Báo lỗi rõ khi Excel không mở, workbook không có, sheet khóa, sheet `TKCT` không tồn tại hoặc người dùng chọn sai table.

## 19. Yêu cầu UX/UI

### 19.1 Nguyên tắc UX

- UI dành cho kỹ sư CAD/shopdrawing, ưu tiên tốc độ nhập và giảm lỗi.
- Ít click, ít nhập lặp lại.
- Có default theo workflow hiện hữu.
- Có validation trước khi vẽ.
- Có preview/tóm tắt dữ liệu quan trọng trước khi commit nếu có thể.
- Cho phép lưu cấu hình gần nhất.
- Không để người dùng nhập chữ vào ô số.

### 19.2 Framework UI

- MVP khuyến nghị: **WinForms** nếu ưu tiên tương thích AutoCAD cũ và tốc độ triển khai.
- WPF phù hợp nếu team muốn MVVM, XAML, maintainability và UI scale dài hạn.
- Nếu chọn WPF, phải kiểm tra kỹ hosting/modal behavior trong AutoCAD.

### 19.3 Layout tối thiểu

1. Group/Tab “Hình học & tỷ lệ”
   - Scale.
   - BeamID.
   - NumCK.
   - Số nhịp.
   - DataGrid danh sách nhịp `(L, B, H)`.
   - Danh sách bề rộng cột/gối.
   - Mút thừa trái/phải.
   - `Abv`, `lmoc`.

2. Group/Tab “Thép chủ & đai”
   - T1, T2.
   - B1, B2.
   - Đường kính đai.
   - `a1`, `a2`, `div_L`.

3. Group/Tab “Thép tăng cường”
   - `TC_Goi` từ trái sang phải.
   - `TC_Nhip` từ trái sang phải.
   - Có hướng dẫn format ví dụ `2D20 3D22`.

4. Group/Tab “Cài đặt bản vẽ”
   - Layer preset.
   - Text height.
   - Dimstyle hiện hành.
   - Tùy chọn giữ form mở sau khi vẽ.

5. Action buttons
   - `Vẽ Dầm`.
   - `Thống Kê`.
   - `Xuất Excel`.
   - `Thoát`.

### 19.4 Behavior bắt buộc

- Khi form load, đọc cấu hình lần trước hoặc hiển thị default.
- `Vẽ Dầm` chỉ bật khi dữ liệu tối thiểu hợp lệ.
- Khi bấm `Vẽ Dầm`, form hide/submit DTO và command thực thi trong AutoCAD.
- Validation phải chỉ rõ trường sai và cách sửa.
- Nếu người dùng hủy chọn điểm chèn, không thay đổi bản vẽ.

## 20. Kiến trúc kỹ thuật đề xuất

### 20.1 Công nghệ

- C#: .NET Framework tương thích AutoCAD target. Với AutoCAD 2020–2024 có thể dùng .NET Framework 4.8; nếu phải hỗ trợ AutoCAD 2018 cần xác nhận framework/API tương ứng.
- AutoCAD .NET API:
  - `Autodesk.AutoCAD.Runtime`
  - `Autodesk.AutoCAD.ApplicationServices`
  - `Autodesk.AutoCAD.DatabaseServices`
  - `Autodesk.AutoCAD.EditorInput`
  - `Autodesk.AutoCAD.Geometry`
- Excel Interop cho MVP nếu cần tương thích workflow Excel đang mở.

### 20.2 Module chính

- `Commands`
  - `BdamCommand`
  - `GtCommand`
  - `TktdCommand`
  - `XteCommand`
- `Models / DTOs`
  - `BeamInputModel`
  - `SpanInfo`
  - `CantileverInfo`
  - `RebarSpec`
  - `StirrupSpec`
  - `RebarItem`
- `UI`
  - `InputForm` hoặc WPF Window/ViewModel.
- `Services`
  - `InputValidationService`
  - `LayerService`
  - `DimensionService`
  - `SettingsService`
- `Calculators`
  - `BeamGeometryCalculator`
  - `MainRebarCalculator`
  - `ExtraRebarCalculator`
  - `StirrupCalculator`
  - `ShapeAnalyzer`
- `Drawers`
  - `ConcreteDrawer`
  - `MainRebarDrawer`
  - `ExtraRebarDrawer`
  - `StirrupDrawer`
  - `LeaderDrawer`
  - `TableDrawer`
- `Data`
  - `XDataHelper`
  - `BimDataMapper`
  - `BomGenerator`
  - `ExcelExporter`
- `Utils`
  - Parsing `2D20`.
  - Unit conversion.
  - COM cleanup.
  - Error handling.

### 20.3 Quy tắc code bắt buộc

- Mọi thao tác ghi database phải nằm trong `using (Transaction tr = db.TransactionManager.StartTransaction())`.
- Dùng `DocumentLock` khi thao tác với document hiện hành.
- Ưu tiên `DatabaseServices`; không dùng `Editor.Command` hoặc `SendStringToExecute` trừ trường hợp migration bất khả kháng.
- Không hard-code kích thước nếu đó là rule nghiệp vụ có thể cấu hình; các default phải nằm trong settings.
- Dùng tên biến rõ nghĩa.
- Comment tiếng Anh chuyên ngành, ngắn gọn.
- Bắt exception đầy đủ, rollback khi lỗi.
- Dispose/release đúng với Entity, Transaction, COM Excel object.
- Không để plugin làm crash AutoCAD.

### 20.4 AutoLISP bridging

AutoLISP chỉ là tùy chọn chuyển tiếp, không phải lõi sản phẩm. Chỉ dùng khi:

- Cần reuse block/hàm LISP cũ trong giai đoạn migration.
- Chưa kịp chuyển một module phụ sang C#.
- Cần hỗ trợ file `.lsp` load plugin tạm thời.

Nếu sinh AutoLISP bổ trợ xử lý XData, tài liệu nguồn nhấn mạnh phải dùng `cons` để ghép cặp dữ liệu, không dùng `list` tùy tiện gây mất ổn định database.

## 21. Yêu cầu phi chức năng

- Tương thích AutoCAD 2018–2024 theo target đã chốt.
- Build thành DLL load bằng `NETLOAD`.
- Không crash AutoCAD trong các case hợp lệ.
- Không làm mất dữ liệu CAD/Excel hiện hữu.
- Hiệu năng chấp nhận được với dầm nhiều nhịp và nhiều đối tượng thép.
- Dễ test theo từng command.
- Dễ mở rộng thêm module tính toán, mặt cắt, Excel nâng cao.
- Có logging/cảnh báo đủ để QA debug các lỗi tọa độ, XData, Excel.

## 22. Acceptance Criteria cho MVP

### 22.1 `BDAM_TCVN`

- Vẽ đúng dầm 1 nhịp không mút thừa.
- Vẽ đúng dầm nhiều nhịp không mút thừa.
- Vẽ đúng dầm có mút thừa trái.
- Vẽ đúng dầm có mút thừa phải.
- Vẽ đúng dầm có cả hai mút thừa.
- Đai luôn cách mép cột/mép ngoài 50 mm.
- Thép lớp 1/lớp 2 đúng offset theo `Abv`.
- Thép tăng cường gối/nhịp đúng vị trí `L/4`, `L/6`, `max(L/4, 1.5*mút thừa)`.
- Móc L đúng chiều dài `lmoc` và đúng hướng.
- XData gắn đủ schema cho các thép cần thống kê.

### 22.2 `GT`

- Chọn line/polyline hợp lệ và gắn XData thành công.
- Chọn sai entity không crash.
- Leader hiển thị đúng mark và thông tin thép.

### 22.3 `TKTD`

- Đọc được các object có XData `ST_BIM_REBAR`.
- Gom nhóm đai đúng.
- Tính chiều dài, tổng chiều dài, khối lượng đúng công thức.
- CAD Table đủ 10 cột và dòng tổng theo đường kính.
- Shape block/L1/L2/L3 đúng với thanh thẳng, thanh 1 móc, thanh 2 móc và đai.

### 22.4 `XTE`

- Báo lỗi nếu Excel chưa mở.
- Tìm đúng sheet `TKCT` theo Display Name.
- Không ghi vào sheet protected.
- Ghi sau dòng dữ liệu cuối, không ghi đè dữ liệu cũ.
- Mapping L1/L2/L3/C.DAI đúng template.
- Xuất nhiều bảng theo thứ tự từ trên xuống.
- Release COM object và không treo Excel/AutoCAD.

## 23. Test matrix khuyến nghị

| Nhóm test | Case |
|---|---|
| Hình học | 1 nhịp, 2 nhịp, 5 nhịp, nhịp khác chiều cao, cột khác bề rộng. |
| Mút thừa | Không mút, mút trái, mút phải, hai mút, mút thấp hơn `H_max`. |
| Thép chủ | Chỉ lớp 1, có lớp 2, thanh vượt 11.7 m, nối chồng 40D. |
| Thép tăng cường | Gối đầu có mút, gối đầu không mút, gối giữa, nhịp dưới L/6. |
| Đai | a1/a2 chuẩn, nhịp ngắn không đủ vùng giữa, div_L khác 4. |
| XData | Đủ field, thiếu field, sai kiểu, nhiều BeamID. |
| TKTD | Thẳng, 1 móc, 2 móc, đai, gom nhóm đai, tổng đường kính. |
| XTE | Không mở Excel, không có workbook, không có TKCT, sheet protected, nhiều CAD Table, ô Excel đã có dữ liệu. |
| UI | Nhập chữ vào ô số, thiếu cột, sai số lượng cột, lưu default, hủy thao tác. |

## 24. Rủi ro, giả định và câu hỏi mở

### 24.1 Rủi ro

- Logic AutoLISP có nhiều rule ngầm chưa được tài liệu hóa đầy đủ.
- Một số rule TCVN trong code hiện tại là thực hành shopdrawing, cần domain expert xác nhận trước khi freeze.
- Hệ block hình dạng thép `RBSH01`, `RBSH02`, `RBSH03`, `RBSH04`, `RBSH05`, `RBSH_DAI` có thể phụ thuộc template CAD hiện hữu.
- Excel template `TKCT` có công thức/mapping đặc thù; nếu chưa có file mẫu, dễ map sai cột.
- AutoCAD version khác nhau có khác biệt API/.NET Framework.
- Excel Interop dễ lỗi nếu máy người dùng không có Excel, workbook khóa hoặc COM bị treo.
- Nếu giao AI sinh quá nhiều module cùng lúc, dễ sai API AutoCAD hoặc sai logic nghiệp vụ.

### 24.2 Giả định

- Người dùng có bản vẽ/template CAD chứa block shape cần thiết hoặc chấp nhận plugin tự tạo block thay thế.
- Người dùng có Excel template sheet `TKCT` đang dùng thực tế.
- AutoLISP V39.1 là reference truth cho MVP migration.
- Domain expert sẽ duyệt output bản vẽ ở từng phase.

### 24.3 Câu hỏi cần chốt trước build production

1. AutoCAD target chính xác là 2018–2024 hay 2020–2025?
2. Framework build chốt là .NET Framework 4.8 hay cần bản thấp hơn để hỗ trợ AutoCAD 2018?
3. UI MVP chọn WinForms hay WPF?
4. Template Excel `TKCT` có mapping cột chính thức không?
5. Các block `RBSHxx` sẽ được cung cấp từ template CAD hay plugin phải tự sinh?
6. Dimension chi tiết cần đến mức nào trong MVP?
7. Có cần lưu cấu hình input theo user/project không?
8. Rule nối chồng 40D và vùng an toàn cần domain expert duyệt theo từng loại thép hay dùng logic hiện hữu trước?

## 25. Lộ trình triển khai khuyến nghị

### Phase 1 – Foundation

- Setup class library AutoCAD .NET.
- Tạo command skeleton.
- Tạo model/DTO.
- Tạo `LayerService`, `XDataHelper`, parser `2D20`.
- Test NETLOAD và command đơn giản.

### Phase 2 – UI gate

- Dựng form nhập liệu.
- Validation.
- Settings/default values.
- Map DTO.
- Approval gate: người dùng/domain expert duyệt UI trước khi code CAD backend.

### Phase 3 – Core CAD drawing

- Geometry calculator.
- Concrete/gối/mút thừa/breakline.
- Main rebar.
- Extra rebar.
- Stirrups.
- Leader.
- Dimension cơ bản.

### Phase 4 – Data/BOM/Excel

- XData end-to-end.
- `GT`.
- `TKTD` CAD Table.
- `XTE` Excel export.
- Mapping Excel template.

### Phase 5 – Hardening

- Regression test với bản vẽ reference từ LISP.
- Fix crash/transaction/COM issues.
- Tối ưu hiệu năng.
- Đóng gói DLL, hướng dẫn NETLOAD, version release.

## 26. KPI thành công

### 26.1 KPI sản phẩm

- Giảm thời gian vẽ và thống kê dầm so với workflow thủ công/LISP cũ.
- Giảm lỗi mismatch giữa hình vẽ, CAD Table và Excel.
- Người dùng có thể hoàn thành workflow lõi không cần sửa tay nhiều.

### 26.2 KPI kỹ thuật

- DLL build ổn định.
- Không crash AutoCAD trong test matrix MVP.
- XData đọc/ghi ổn định.
- Excel export không ghi đè dữ liệu cũ.
- Module hóa đủ để bảo trì và mở rộng.

## 27. Go / No-Go Recommendation

### Nên Go nếu

- Mục tiêu là sản phẩm hóa dài hạn thay vì chỉ vá LISP.
- Có thể dùng AutoLISP V39.1 và bản vẽ mẫu làm reference truth.
- Có domain expert duyệt các rule TCVN/shopdrawing.
- Chấp nhận triển khai theo MVP có approval gate.

### Chưa nên Go full-scope nếu

- Chưa chốt Excel template và block shape.
- Chưa có người duyệt logic TCVN cuối cùng.
- Muốn AI tự sinh toàn bộ sản phẩm không qua review.
- Chưa xác định AutoCAD/.NET target chính xác.

### Khuyến nghị cuối

**Go theo MVP có kiểm soát.** Ưu tiên hoàn thành core workflow:

1. UI + DTO.
2. Geometry + rebar + stirrup + leader.
3. XData schema.
4. CAD Table `TKTD`.
5. Excel export `XTE`.

Sau khi workflow này ổn định mới mở rộng sang tính As, kiểm tra chịu lực, mặt cắt ngang, dynamic block và thương mại hóa.

# Phụ lục A – Mapping AutoLISP sang C# module

| AutoLISP | C# module đề xuất | Ghi chú |
|---|---|---|
| `ST_SetXData` | `XDataHelper.SetRebarXData` | Dùng `RegAppTable`, `ResultBuffer`, `TypedValue`. |
| `ST_GetXData` | `XDataHelper.GetRebarXData` | Parse về `RebarItem`. |
| `ParseNumbers` | `InputParser.ParseNumbers` | Dùng culture-safe parsing. |
| `ParseStrings` | `InputParser.ParseTokens` | Dùng cho chuỗi thép tăng cường. |
| `GetDia` | `RebarSpec.Parse` | Parse `2D20`. |
| `RaiDai_V21` | `StirrupCalculator` + `StirrupDrawer` | Rải đai theo step và trả số lượng. |
| `ST_MakeLayer` | `LayerService.EnsureLayer` | Tạo layer/lw/color. |
| `ST_DrawBreakLine` | `ConcreteDrawer.DrawBreakLine` | Polyline zigzag. |
| `ST_DrawProLeader` | `LeaderDrawer.DrawRebarLeader` | Dùng MLeader/Leader/Circle/Text. |
| `c:BDAM_TCVN` | `BdamCommand` | Command chính. |
| `DrawMain` | `MainRebarCalculator/Drawer` | Chia thanh, móc, nối chồng. |
| `c:GT` | `GtCommand` | Gán thép bổ sung. |
| `ST_AnalyzeShape` | `ShapeAnalyzer` | Map RBSH block và L1/L2/L3. |
| `c:TKTD` | `TktdCommand` + `BomGenerator` | CAD Table và tổng hợp. |
| `c:XTE` | `XteCommand` + `ExcelExporter` | Excel Interop. |

# Phụ lục B – Agent/workflow dùng để hỗ trợ triển khai

Nội dung này lấy từ các tài liệu prompt/agent, chỉ đóng vai trò hỗ trợ triển khai, không phải yêu cầu sản phẩm lõi.

## B.1 Vai trò đề xuất

- PM/Architect: chốt scope, đọc PRD, duyệt UI, duyệt output CAD/Excel.
- UI Developer: WinForms/WPF, validation, DTO mapping.
- Core CAD Developer: AutoCAD .NET API, geometry, rebar, stirrup, leader, dimension.
- Excel/Data Developer: XData, BOM, CAD Table, Excel export.
- QA: so sánh output C# với AutoLISP reference, test regression.

## B.2 Task breakdown agent-friendly

1. T1 – Core & Utils: project setup, commands, layers, XData helper.
2. T2 – UI & Models: input model, form, validation.
3. T3 – Concrete: beam outline, columns, cantilevers, breakline.
4. T4 – MainRebar: continuous top/bottom rebar, hooks, lap logic.
5. T5 – ExtraRebar: support/span extra rebars.
6. T6 – Stirrups: 3-zone stirrup logic, leader generation.
7. T7 – BOM_CAD: read XData, group, draw CAD Table.
8. T8 – Excel_Export: export CAD Table to Excel `TKCT`.

## B.3 Approval gates

- Gate 1: duyệt UI và DTO trước khi vẽ CAD.
- Gate 2: duyệt output CAD cho các case dầm chuẩn.
- Gate 3: duyệt CAD Table và Excel mapping.
- Gate 4: duyệt release DLL sau regression test.

## B.4 Nguyên tắc dùng AI agent

- Cung cấp PRD, AutoLISP reference, Excel template và CAD template cho agent.
- Chia nhỏ theo task, không yêu cầu sinh toàn bộ code một lần.
- Bắt buộc agent tuân thủ AutoCAD API rules: Transaction, DocumentLock, DatabaseServices, ResultBuffer.
- Sau mỗi module phải build/test trước khi chuyển module tiếp theo.
- Exception từ AutoCAD phải copy lại đầy đủ để sửa đúng lỗi transaction/entity/API.

# Phụ lục C – Prompt triển khai rút gọn

Có thể dùng prompt tổng lực sau cho agent/dev AI sau khi đã cung cấp PRD và code AutoLISP reference:

```text
Chúng ta xây dựng plugin AutoCAD BDAM_TCVN bằng C# .NET.
Hãy đọc kỹ PRD, AutoLISP V39.1 reference, Excel template TKCT và CAD template block RBSH.
Tuân thủ tuyệt đối:
- AutoCAD .NET API, DocumentLock, Transaction, DatabaseServices.
- Không dùng Editor.Command trừ khi bất khả kháng.
- XData AppID ST_BIM_REBAR với schema BeamID, Mark, Diam, Qty, LengthMM, ShapeString, NumCK.
- Offset thép theo Abv, rải đai cách mép 50 mm, vùng đai L/4, móc lmoc, lap 40D, phôi tối đa 11.7 m.
Triển khai tuần tự T1 -> T8, dừng để tôi approve sau UI, sau CAD output và sau Excel output.
```

# Phụ lục D – Checklist tài liệu nguồn đã đưa vào PRD

| Nhóm nội dung | Đã phản ánh trong PRD |
|---|---|
| Chuyển AutoLISP sang C# DLL AutoCAD | Có |
| AutoCAD 2018–2024 | Có |
| `BDAM_TCVN`, `GT`, `TKTD`, `XTE` | Có |
| XData `ST_BIM_REBAR` | Có |
| `BeamID`, `Mark`, `Diam`, `Qty`, `Length`, `ShapeString`, `NumCK` | Có |
| Offset `Abv` lớp 1/lớp 2/tăng cường | Có |
| Móc L/U theo `lmoc` | Có |
| Rải đai cách mép 50 mm | Có |
| Vùng đai `L/4`, `a1`, `a2`, `div_L` | Có |
| Nối chồng 40D, phôi 11.7 m | Có |
| Thép tăng cường gối `max(L/4, 1.5*mút thừa)` | Có |
| Thép tăng cường nhịp cắt tại `L/6` | Có |
| Leader có circle mark + info text | Có |
| CAD Table 10 cột | Có |
| Shape blocks `RBSHxx` | Có |
| Công thức khối lượng `D^2/162` | Có |
| Excel sheet `TKCT`, quét ngược, `Value2` | Có |
| UI WinForms/WPF, GroupBox, validation, default | Có |
| Kiến trúc module, Transaction, không `Editor.Command` | Có |
| Phân vai PM/DEV/QA/Agent | Có, trong phụ lục |
| Task/prompt/Antigravity workflow | Có, trong phụ lục |
