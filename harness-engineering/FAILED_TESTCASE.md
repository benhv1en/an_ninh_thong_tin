# 12/06/2026 08:13:07 +0700

## Testcase da fix
- Command: `npx tsc --noEmit`
- Ket qua truoc do: fail voi exit code `2`.
- Ket qua sau khi sua: pass voi exit code `0`.
- Pham vi: TypeScript project-wide compile check.

## Loi cu da xu ly
```text
src/components/common/Card.tsx(45,17): error TS2769: LinearGradient colors nhan `string[]` thay vi tuple it nhat 2 mau.
src/store/authStore.ts(29,34): error TS2339: `lockInMemoryData` khong ton tai trong `TransactionState`.
src/theme/ThemeContext.tsx(75,41): error TS2322: `darkTheme` khong gan duoc vao `Theme` vi `Theme` chi la `typeof lightTheme`.
src/theme/index.ts(13,28): error TS2322: `getTheme('dark')` tra `darkTheme` nhung `Theme` chi chap nhan light mode.
```

## Thay doi da fix
- `src/components/common/Card.tsx`: doi `gradientColors` sang tuple readonly `[ColorValue, ColorValue, ...ColorValue[]]` dung voi `expo-linear-gradient`.
- `src/theme/colors.ts`: doi `Theme` thanh union `typeof lightTheme | typeof darkTheme`.
- `src/store/transactionStore.ts`: them action `lockInMemoryData` va dung `encryptedTransactionStorage` thay cho AsyncStorage truc tiep.

## Lenh da chay
```bash
npx tsc --noEmit
```

## Ket qua
```text
Pass, exit code 0, khong co TypeScript error.
```
