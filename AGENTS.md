# AGENTS.md

## Pham vi

File nay ap dung cho toan bo repository.

Day la du an CashTrack: frontend hien tai la React Native/Expo viet bang TypeScript voi `strict: true`. Neu them backend, backend phai dung C# ASP.NET Core, EF Core code-first va SQLite.

## Quy tac lam viec

- Truoc khi sua code, phai trinh bay mot ke hoach ngan, ro cac file/vung se tac dong.
- Moi thay doi phai nho, co giai thich ly do va khong tron refactor ngoai pham vi.
- Khong tu y sua UI. Khong sua man hinh, component, theme, layout, text hien thi, navigation hoac style neu nguoi dung khong yeu cau ro.
- Neu can sua frontend de ket noi backend, chi sua lop API adapter/service va cac kieu du lieu lien quan den contract. Khong sua UI de "cho tien".
- Khong revert hoac xoa thay doi san co cua nguoi dung neu khong duoc yeu cau ro.
- Sau khi sửa code, hãy lưu lại hết tất cả các thay đổi bằng cách CẬP NHẬT VÀO ĐẦU FILE (KHÔNG PHẢI LÀ VIẾT MỚI, cũng không phải là cập nhật vào cuối file) vào file `CHANGELOG.md`. Cập nhật ấy phải ghi rõ: ngày tháng năm giờ phút giây, thứ gì đã thay đổi, lý do mục đích thay đổi thứ ấy,...
- Trong quá trình testing, nếu test lỗi, bắt buộc phải lưu log báo lỗi của failed tastcase đó bằng cách VIẾT MỚI lại file `harness-engineering/FAILED_TESTCASE.md`
- Khi app chạy lỗi trên điện thoại hoặc chạy lỗi qua Expo Go, bắt buộc phải lưu log báo lỗi của lỗi đó bằng cách VIẾT MỚI lại file `harness-engineering/ERROR_LOG.md`

## Harness engineering

- `AGENTS.md` va `CHANGELOG.md` duoc giu o root repository.
- Tat ca markdown phuc vu harness engineering phai nam trong thu muc `harness-engineering/`.
- Khong tao them markdown harness engineering moi o root repository. Neu nguoi dung yeu cau luu file `.md` cho ke hoach, contract, entity list, thieu sot, testcase, log loi, project definition hoac tai lieu thiet ke ky thuat, mac dinh luu vao `harness-engineering/<ten-file>.md`.
- File project definition dung cho ghi chep sau moi prompt la `harness-engineering/project_definition.md`.
- Sau moi prompt, ghi tiep vao `harness-engineering/project_definition.md` bang tieng Viet gom timestamp, noi dung da thay doi, project definition day du va flow he thong sau thay doi.
- Sau moi prompt, viet lai cach chay code vao `build.sh`.
- File ke hoach ngan neu can luu la `harness-engineering/my_plan.md`.
- File entity list la `harness-engineering/my_entity_list.md`.
- File REST API contract la `harness-engineering/my_api_contract.md`.
- File danh sach thieu sot la `harness-engineering/missing.md`.
- File testcase la `harness-engineering/testcases.md`.
- Khi testing fail, viet moi log vao `harness-engineering/FAILED_TESTCASE.md`.
- Khi app loi tren dien thoai hoac Expo Go, viet moi log vao `harness-engineering/ERROR_LOG.md`.
- README, CONTRIBUTING, CODE_OF_CONDUCT va GitHub issue template khong duoc xem la harness engineering docs neu nguoi dung khong noi ro.

## API contract

- Frontend React Native/Expo/TypeScript strict mode la source of truth ve API contract.
- Cac type/interface trong `src/types` va cac API adapter/service frontend la chuan de backend bam theo.
- Backend DTO, request, response va validation phai tuong thich voi frontend contract hien co.
- Khi contract thay doi, uu tien cap nhat contract o frontend truoc, sau do moi dong bo backend theo contract do.
- Khong doi ten field, enum value, dinh dang ngay gio, hoac y nghia du lieu neu khong neu ro migration/compatibility.

## Backend

- Backend phai dung C# ASP.NET Core.
- ORM phai la EF Core theo huong code-first.
- Database mac dinh la SQLite.
- Khong dung database-first, khong tu y doi sang PostgreSQL/MySQL/MongoDB/Firebase/Supabase neu khong duoc yeu cau.
- Entity, DbContext, migration va API endpoint phai duoc giu don gian, bam dung contract frontend.
- Du lieu tai chinh va xac thuc la nhay cam; khong log plaintext secret, password, token, API key hoac noi dung giao dich nhay cam.

## Frontend

- Giu TypeScript strict mode.
- Neu sua frontend API adapter/service, phai giu contract typed ro rang, tranh `any` neu co the.
- Khong goi backend truc tiep tu UI component neu da co hoac nen co lop service/adapter.
- Khong thay doi state store, navigation hoac UI flow neu thay doi backend/API adapter da du de giai quyet yeu cau.

## Kiem chung

- Voi thay doi backend, phai chay `dotnet build`.
- Neu repository co test backend, phai chay `dotnet test`.
- Neu sua TypeScript/frontend service hoac contract, nen chay `npx tsc --noEmit` khi kha thi.
- Trong phan tra loi cuoi, phai neu ro cac lenh da chay va ket qua. Neu khong chay duoc lenh nao, noi ro ly do.

## Bao cao ket qua

- Tra loi ngan gon bang tieng Viet.
- Neu co thay doi file, tom tat file nao da sua va noi dung chinh.
- Neu con rui ro hoac viec chua kiem chung, neu ro de nguoi dung biet.
