# Failed testcase log

Thoi diem ghi nhan: 12/06/2026 09:38:14 +0700
Thoi diem xu ly xong: 12/06/2026 09:40:56 +0700

## Lenh test bi fail

```text
npx tsc --noEmit
```

## Ket qua fail ban dau

```text
src/services/securityService.ts(1,24): error TS7016: Could not find a declaration file for module 'node-forge'. '/home/benhv1en/Documents/an_ninh_thong_tin/node_modules/node-forge/lib/index.js' implicitly has an 'any' type.
  Try `npm i --save-dev @types/node-forge` if it exists or add a new declaration (.d.ts) file containing `declare module 'node-forge';`
```

## Nguyen nhan

Sau khi bo `expo-crypto` va dung `node-forge` truc tiep de ho tro Expo Go, package type `@types/node-forge` chua duoc khai bao trong `devDependencies`, nen TypeScript strict mode khong co declaration cho module `node-forge`.

## Cach da sua

- Chay `npm install --save-dev @types/node-forge` de khai bao type cho `node-forge`.
- Chay lai `npx tsc --noEmit` thanh cong, exit code 0.

## Trang thai sau fix

```text
npx tsc --noEmit
# pass, exit code 0
```
